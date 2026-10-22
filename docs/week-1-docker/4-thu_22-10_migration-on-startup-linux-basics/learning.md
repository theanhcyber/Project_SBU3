# Kiến thức cần nắm: Migration khi khởi động, Linux tối thiểu

> Bài: [spec.md](spec.md) · Thứ 5, 22/10/2026 · 2h
> Tài liệu này **chỉ định nghĩa những gì cần học và cần làm được**, chưa triển khai code.

---

## 0. Bản đồ: yêu cầu nào cần kiến thức nào

| Yêu cầu / tiêu chí trong spec | Khái niệm cần nắm | Mục |
|---|---|---|
| Tự chạy migration khi khởi động, chỉ ở Development | EF Core migration, `__EFMigrationsHistory`, `Migrate()`, môi trường ASP.NET Core | 1, 2, 3 |
| Xử lý DB chưa sẵn sàng | Thứ tự khởi động Compose, connection resiliency | 4 |
| Seed 1 bản ghi mẫu, không trùng | Data seeding, tính idempotent | 5 |
| `docker compose down -v` rồi `up` chạy sạch | Vòng đời volume, xung đột với `init.sql` cũ | 6 |
| `logs`, `exec`, `ls`, `cat`, `env`, `ps` | Linux trong container: process, filesystem, env, user, log | 7 |
| Vì sao không migrate khi start trên production nhiều instance | Race condition, quyền DDL, chiến lược deploy migration | 8 |

---

## 1. EF Core Migrations: mô hình hoạt động

**Là gì:** migration là một file C# mô tả bước thay đổi schema (tạo bảng, thêm cột…), sinh ra bằng cách so sánh **model hiện tại** (`AppDbContext` + entity) với **snapshot** lần trước.

**Cần nắm được:**
- [ ] 3 thành phần: file migration (`Up()` / `Down()`), `AppDbContextModelSnapshot.cs`, bảng `__EFMigrationsHistory` trong DB.
- [ ] `Migrate()` làm gì: đọc `__EFMigrationsHistory` → tìm migration chưa áp dụng → chạy lần lượt → ghi lịch sử. DB chưa tồn tại thì tạo DB trước.
- [ ] Phân biệt `Migrate()` với `EnsureCreated()`: `EnsureCreated` tạo schema **không có lịch sử migration**, sau này không migrate tiếp được. Không trộn 2 cách.
- [ ] Các lệnh `dotnet ef`: `migrations add`, `migrations list`, `migrations remove`, `database update`, `migrations script`, `dbcontext info`.
- [ ] Công cụ cần: tool `dotnet-ef` (cùng major version với EF Core = 9.x) và package `Microsoft.EntityFrameworkCore.Design` trong csproj.
- [ ] Design-time: `dotnet ef` khởi động `Program.cs` để lấy `AppDbContext`, nên cần connection string hợp lệ lúc chạy lệnh (appsettings đang trỏ `localhost`, đủ dùng).

**Áp dụng vào TestManager:**
- Repo **chưa có thư mục `Migrations/`**. Migration đầu tiên `InitialCreate` sinh từ `AppDbContext` phải tạo ra schema tương đương `database/init.sql` (computed column, unique index, FK `Cascade`/`NoAction`).
- Cách kiểm tra: `dotnet ef migrations script` ra SQL rồi so sánh với `init.sql` (tên bảng, kiểu cột, constraint).

**Bẫy thường gặp:**
- EF Core 9: `Migrate()` **ném lỗi nếu model đã đổi mà chưa `migrations add`** (`PendingModelChangesWarning`). Đây là tính năng, không phải bug: nó chặn chạy schema lệch với code.
- Sửa tay file migration đã áp dụng ở nơi khác → lịch sử lệch. Quy tắc: migration đã chia sẻ thì không sửa, tạo migration mới.

## 2. Gọi migration lúc khởi động app

**Cần nắm được:**
- [ ] Lấy `DbContext` trong `Program.cs` phải qua **scope**: `using var scope = app.Services.CreateScope();` vì `AppDbContext` đăng ký là Scoped.
- [ ] Vị trí gọi: sau `builder.Build()`, trước `app.Run()`. App chỉ nhận request khi migrate xong.
- [ ] `Migrate()` vs `MigrateAsync()`; lỗi migration làm app **không khởi động** (exit code ≠ 0) → container dừng → thấy ngay trong `docker compose ps`.
- [ ] Log rõ ràng: "Applying migrations…", "Seeded sample data" / "Seed skipped", để chứng minh tiêu chí "log không lỗi".

## 3. Môi trường (Environment) trong ASP.NET Core

**Cần nắm được:**
- [ ] `ASPNETCORE_ENVIRONMENT` → `app.Environment.IsDevelopment()`. Giá trị mặc định khi không đặt là **Production**.
- [ ] Compose đang đặt `ASPNETCORE_ENVIRONMENT: Development` cho `api`, nên migration sẽ chạy trong Docker. Trên AWS (tuần 2) không đặt biến này → không migrate, đúng ý đồ.
- [ ] Môi trường còn ảnh hưởng: file `appsettings.Development.json`, trang lỗi chi tiết, Swagger. Không bật Development trên production.

**Câu hỏi tự kiểm tra:** nếu quên đặt biến này trong compose thì điều gì xảy ra lúc `up` trên volume trống?

## 4. Xử lý "DB chưa sẵn sàng": 2 lớp bảo vệ

| Lớp | Cơ chế | Bảo vệ được gì | Không bảo vệ được gì |
|---|---|---|---|
| Hạ tầng (Compose) | `depends_on: db: condition: service_healthy` + `healthcheck` của `db` (đã có) | Lần khởi động đầu: API chỉ start khi SQL Server trả lời `SELECT 1` | DB restart **khi app đang chạy**; môi trường không có Compose (App Runner, ECS, k8s không đọc `depends_on`) |
| Ứng dụng (EF Core) | `UseSqlServer(cs, o => o.EnableRetryOnFailure())` | Lỗi tạm thời (transient) lúc khởi động và lúc chạy: mất kết nối, failover, timeout | Lỗi không tạm thời: sai mật khẩu (18456), sai tên DB, sai schema |

**Cần nắm được:**
- [ ] Transient fault là gì; danh sách lỗi SQL Server được coi là transient do `SqlServerRetryingExecutionStrategy` quyết định.
- [ ] Tham số: `maxRetryCount` (mặc định 6), `maxRetryDelay` (mặc định 30s), backoff tăng dần có jitter.
- [ ] **Hệ quả quan trọng:** khi bật retry, transaction tự mở (`BeginTransaction`) phải bọc trong `db.Database.CreateExecutionStrategy().ExecuteAsync(...)`, nếu không EF ném `InvalidOperationException`. Cần `grep BeginTransaction` trong repo để biết có bị ảnh hưởng không.
- [ ] Vì sao giữ cả 2 lớp: Compose giúp khởi động gọn và log sạch; retry trong app mới là thứ đi theo app lên cloud.

**Liên hệ tương lai:** tuần 2 (RDS) không có `depends_on`, chỉ còn lớp ứng dụng.

## 5. Data seeding và tính idempotent

**Khái niệm:** idempotent = chạy 1 lần hay N lần cho cùng kết quả. Seed phải idempotent để tiêu chí "khởi động lần 2 không sinh bản ghi mẫu thứ hai" đạt.

**3 cách seed trong EF Core, cần biết chọn:**

| Cách | Cơ chế | Hợp với TestManager? |
|---|---|---|
| `HasData()` trong `OnModelCreating` | Dữ liệu nằm **trong migration** | ❌ cho User: `PasswordHasher` sinh hash khác nhau mỗi lần → mỗi lần `migrations add` sinh thay đổi giả. Chỉ hợp với dữ liệu tĩnh, Id cố định |
| `UseSeeding` / `UseAsyncSeeding` (mới trong EF Core 9) | Callback chạy sau `Migrate()` / `EnsureCreated()` | ✅ Cách Microsoft khuyến nghị cho EF 9 |
| Code seed tự viết sau `Migrate()` | Hàm `SeedAsync(db)` gọi thủ công | ✅ Đơn giản, dễ đọc |

**Cần nắm được:**
- [ ] Điều kiện chống trùng: kiểm tra theo **khoá tự nhiên** (ví dụ `Subject.Code`, `User.UserName`, `ClassRoom.Code`) thay vì chỉ `Any()` trên cả bảng. `Any()` sai khi người dùng đã xoá bản ghi mẫu nhưng còn dữ liệu khác, hoặc ngược lại.
- [ ] Thứ tự seed theo FK: `Users` → `Teachers`/`Students` → `Subjects`, `Semesters` → `Classes` → `StudentClasses`.
- [ ] Seed trong 1 `SaveChanges()` để hoặc tất cả, hoặc không gì (nếu chia nhiều lần thì cần transaction + execution strategy, xem mục 4).

**"1 board mẫu" tương đương trong TestManager:** 1 lớp học (`ClassRoom`) đầy đủ: 1 môn, 1 học kỳ, 1 giáo viên, 2–3 sinh viên trong lớp. Bộ này cũng là dữ liệu cho bài Redis ngày mai (`GET /api/classes/{id}/students`).

## 6. Vòng đời volume và xung đột với cách làm cũ

**Cần nắm được:**
- [ ] `docker compose down` giữ volume; `down -v` xoá named volume `mssql-data` → DB mất hoàn toàn. Đây là cách tạo "môi trường sạch" của tiêu chí 1.
- [ ] **Bẫy:** volume hiện tại (nếu đã từng `up`) chứa DB do `init.sql` tạo, **không có** `__EFMigrationsHistory`. `Migrate()` sẽ cố `CREATE TABLE Users` → lỗi "There is already an object named 'Users'". Phải `down -v` một lần khi chuyển sang migration.
- [ ] Bỏ service `db-init` và điều kiện `service_completed_successfully` ở `api`, nếu không sẽ có 2 nơi cùng tạo schema.
- [ ] `docker volume ls`, `docker volume inspect` để xác nhận volume đã bị xoá / tạo lại.

## 7. Linux tối thiểu trong container

Image runtime là `mcr.microsoft.com/dotnet/aspnet:9.0` (Debian 12 slim), chạy bằng **user không phải root** (`USER $APP_UID` trong `Dockerfile.linux`). Hai điều này quyết định lệnh nào dùng được.

### 7.1 Khái niệm

| Khái niệm | Cần hiểu | Ứng dụng thực tế |
|---|---|---|
| Container = process | Container là 1 process được cô lập (namespace), không phải VM. `dotnet TestManager.Api.dll` là **PID 1** | Process PID 1 chết = container dừng. `docker stop` gửi SIGTERM, chờ 10s rồi SIGKILL |
| stdout/stderr = log | Docker thu stdout/stderr của PID 1. Không ghi log ra file trong container | `docker compose logs -f api` chỉ thấy những gì app in ra console |
| Exit code | 0 = bình thường, 1 = lỗi app (ví dụ migration fail), 137 = bị SIGKILL (thường OOM), 139 = segfault | Đọc ở `docker compose ps -a` để biết container chết vì sao |
| Filesystem | Mỗi image có cây thư mục riêng; `/app` là nơi publish. Ghi vào container mất khi xoá container, chỉ volume mới bền | So sánh `/app` (API) với `/var/opt/mssql` (DB, gắn volume) |
| Biến môi trường | Cấu hình đi vào app qua env (12-Factor) | `env` thấy `ConnectionStrings__Default` **kèm mật khẩu dạng chữ thường** → env không phải nơi giữ bí mật an toàn (tuần 3: Secrets Manager) |
| User & quyền | Container API chạy UID `app`, không có `sudo`, không `apt-get` được | `docker exec -u root` khi thật sự cần cài công cụ debug (chỉ ở máy dev) |
| `/proc` | Hệ thống file ảo mô tả process | Image không có `ps`: dùng `ls /proc`, `cat /proc/1/cmdline`, hoặc `docker top` từ máy host |
| DNS nội bộ | Tên service Compose là hostname | `getent hosts db` trong container `api` ra IP của `db` |

### 7.2 Bộ lệnh cần làm được (ghi vào log ≥ 5 lệnh)

| Lệnh | Tác dụng | Dùng khi |
|---|---|---|
| `docker compose ps -a` | Trạng thái, health, exit code từng service | Câu hỏi đầu tiên khi có sự cố |
| `docker compose logs -f --tail 100 api` | Theo dõi log trực tiếp | Xem migration/seed chạy, lỗi khởi động |
| `docker exec -it <container> sh` | Mở shell trong container đang chạy | Debug tại chỗ (`sh`, image không có `bash`) |
| `ls -la /app` | Xem file publish, quyền, chủ sở hữu | Kiểm tra image có đủ file, đúng phiên bản |
| `cat /etc/os-release` | Xem distro gốc của image | Biết dùng được lệnh/gói nào |
| `env` hoặc `docker exec <c> env \| grep -i connection` | Liệt kê biến môi trường | Tiêu chí 3: xác nhận cấu hình đã vào container |
| `id` / `whoami` | User đang chạy | Chứng minh container chạy non-root |
| `ls /proc` + `cat /proc/1/cmdline` | Thay cho `ps` | Xem PID 1 là gì |
| `docker top <container>` | Danh sách process, chạy từ host | Khi image không có `ps` |
| `docker inspect <c> --format '{{.State.Health.Status}}'` | Trạng thái health chi tiết | Debug `depends_on: service_healthy` treo |

**Áp dụng:** trên container `db` (image SQL Server, có `bash`, `ps`) chạy thử các lệnh tương tự và so sánh: image có tooling khác nhau, đó là lý do image runtime càng nhỏ càng ít công cụ debug.

## 8. Câu hỏi bắt buộc: vì sao không migrate khi start trên production nhiều instance

Cần trả lời được ít nhất 5 ý, kèm ví dụ:

1. **Race condition:** 3 instance cùng khởi động, cùng thấy migration X chưa chạy, cùng chạy → lỗi hoặc schema dở dang. EF Core 9 có thêm **khoá migration** nên giảm rủi ro này, nhưng các ý dưới vẫn đúng.
2. **Quyền tối thiểu:** app chạy migration cần quyền DDL (`CREATE/ALTER/DROP`). Lộ connection string là lộ quyền xoá bảng. Tài khoản runtime chỉ nên có quyền đọc/ghi dữ liệu.
3. **Khởi động chậm + health check:** migration lâu (thêm index trên bảng lớn) làm app chưa sẵn sàng → orchestrator coi là chết, kill, khởi động lại → migrate lại → vòng lặp.
4. **Không có điểm kiểm soát:** không review được SQL trước, không có bước duyệt, không có kế hoạch rollback riêng.
5. **Rolling deploy:** trong lúc deploy, phiên bản cũ và mới chạy song song trên cùng schema. Migration phải tương thích ngược (expand → migrate data → contract), không thể "chạy là xong".

**Cách làm đúng (để biết, tuần 4–5 sẽ làm):** tách migration thành bước riêng trong pipeline: `dotnet ef migrations script --idempotent` (SQL review được) hoặc `dotnet ef migrations bundle` (file chạy `efbundle`), chạy 1 lần trước khi deploy app.

---

## 9. Phần nâng cao (không bắt buộc, chọn 1–2)

| Chủ đề | Học gì | Giá trị thực tế |
|---|---|---|
| Migration bundle & idempotent script | `migrations bundle --self-contained`, `migrations script --idempotent` | Chính là cách tuần 5 (ngày 20/11) triển khai |
| Expand / contract migration | Đổi tên cột an toàn qua 3 bước, không downtime | Bài 20/11 "safe database migrations" |
| Readiness vs liveness | Tách `/health/live` và `/health/ready` bằng tag của health check | Load balancer chỉ gửi traffic khi app sẵn sàng (DB + migration xong) |
| Graceful shutdown | SIGTERM, `HostOptions.ShutdownTimeout`, `stop_grace_period` trong compose | Không cắt ngang request khi deploy |
| `dotnet-ef` dạng local tool | `dotnet new tool-manifest` + `dotnet tool install dotnet-ef` | Cả team (và CI) dùng đúng một phiên bản tool |
| Debug image không có shell | `docker debug`, hoặc `docker run --pid container:<c> --network container:<c> busybox` | Cần khi chuyển sang image chiseled/distroless (mục đào sâu cuối tuần) |

---

## 10. Nguồn học (theo thứ tự ưu tiên)

| # | Nguồn | Đọc phần | Thời gian |
|---|---|---|---|
| 1 | [EF Core Migrations overview](https://learn.microsoft.com/ef/core/managing-schemas/migrations/) | Toàn bộ trang | 15' |
| 2 | [Applying migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying) | Mục *Apply migrations at runtime* + cảnh báo production | 10' |
| 3 | [Data seeding](https://learn.microsoft.com/ef/core/modeling/data-seeding) | `UseSeeding`, so sánh với `HasData` | 10' |
| 4 | [Connection resiliency](https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency) | Execution strategy + transaction | 10' |
| 5 | [Compose: control startup order](https://docs.docker.com/compose/how-tos/startup-order/) | `condition: service_healthy` | 5' |
| 6 | [ASP.NET Core environments](https://learn.microsoft.com/aspnet/core/fundamentals/environments) | `IsDevelopment`, giá trị mặc định | 5' |
| 7 | [docker exec](https://docs.docker.com/reference/cli/docker/container/exec/), [docker compose logs](https://docs.docker.com/reference/cli/docker/compose/logs/), [docker top](https://docs.docker.com/reference/cli/docker/container/top/) | Tham số `-it`, `-u`, `-f`, `--tail` | 10' |
| 8 | [What's new in EF Core 9](https://learn.microsoft.com/ef/core/what-is-new/ef-core-9.0/whatsnew) | Mục *Migrations* (khoá concurrent, seeding mới) | 10' |
| 9 | [EF Core CLI reference](https://learn.microsoft.com/ef/core/cli/dotnet) | Tra cứu khi dùng | tra cứu |
| 10 | [12-Factor: Admin processes](https://12factor.net/admin-processes), [Config](https://12factor.net/config) | Vì sao migration là "one-off process" | 5' |
| 11 | [Linux Journey](https://linuxjourney.com/) (*Command Line*, *Processes*), [man proc(5)](https://man7.org/linux/man-pages/man5/proc.5.html) | Nền tảng Linux, tự học thêm | tuỳ |

---

## 11. Phân bổ 2h gợi ý

| Thời gian | Việc |
|---|---|
| 0:00–0:30 | Đọc nguồn 1–4. Vẽ lại bằng tay luồng `Migrate()` → `__EFMigrationsHistory` → seed |
| 0:30–1:10 | Tạo migration, viết `Migrate` + seed (lúc triển khai), `down -v` → `up` |
| 1:10–1:40 | Thực hành mục 7.2 trên cả `api` và `db`, ghi log |
| 1:40–2:00 | Viết câu trả lời mục 8 bằng lời của mình, tick tiêu chí trong spec |

## 12. Tự kiểm tra trước khi coi là "nắm được"

- [ ] Giải thích được `Migrate()` biết migration nào cần chạy bằng cách nào.
- [ ] Nói được vì sao không dùng `HasData` cho user có mật khẩu hash.
- [ ] Biết lớp nào (Compose hay retry) còn tác dụng khi lên AWS.
- [ ] Biết vì sao volume cũ làm `Migrate()` lỗi và cách xử lý.
- [ ] Không có `ps` vẫn liệt kê được process của container theo 2 cách.
- [ ] Trả lời câu hỏi production với ≥ 3 lý do, không nhìn tài liệu.
