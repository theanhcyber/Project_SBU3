# Migration khi khởi động, Linux tối thiểu

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 5, 22/10/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | 🟡 Code xong, đã test trên SQL Server local; chờ Docker Linux để chạy compose và lệnh Linux |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Trên môi trường sạch (máy chưa có image, container, volume hay database nào của TeamBoard), chỉ cần `docker compose up` là app dùng được ngay.

## 2. Yêu cầu

- Tự chạy migration khi khởi động, chỉ ở Development.
- Xử lý DB chưa sẵn sàng: `depends_on` + `condition: service_healthy`, hoặc `EnableRetryOnFailure`.
- Seed 1 board mẫu, không seed trùng ở lần sau.
- Dùng được `docker compose logs -f api`, `docker exec -it <container> sh`, `ls`, `cat`, `env`, `ps`.

## 3. Output phải nộp

- Code migration + seed.
- Log ghi 5 lệnh Docker/Linux đã dùng và tác dụng.

## 4. Tự đánh giá: đạt khi

- [ ] `docker compose down -v` rồi `up`: app tự tạo DB, có board mẫu, log không lỗi.
- [ ] Khởi động lần 2 không sinh board mẫu thứ hai.
- [ ] Lấy được biến môi trường của container bằng `docker exec … env`.
- [ ] Trả lời được: vì sao không tự chạy migration khi start trên production nhiều instance.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Hiện trạng (22/10):** code xong. Đã chạy thật trên SQL Server local (instance `THEANH`, DB test riêng, đã xoá sau khi test). Chưa chạy được bằng Docker vì máy chưa có Docker và WSL.

| Yêu cầu / tiêu chí | Kết quả |
|---|---|
| Tự chạy migration khi khởi động, chỉ ở Development | ✅ `Program.cs`: `if (app.Environment.IsDevelopment())` → `MigrateAsync()` + seed. Test ở `Production`: không có dòng log migrate |
| Xử lý DB chưa sẵn sàng | ✅ Cả 2 lớp: `EnableRetryOnFailure()` trong `UseSqlServer` + `depends_on: db: condition: service_healthy` (giữ từ bài 21/10) |
| Seed 1 lớp mẫu, không trùng | ✅ `Data/DbSeeder.cs`: lớp `C# - K1` + môn, học kỳ, giáo viên, 3 sinh viên. Kiểm tra trùng theo `Subject.Code = CS01`. Lần 1: `Seed: đã tạo lớp mẫu`. Lần 2: `Seed: dữ liệu mẫu đã có, bỏ qua`. DB: 1 subject, 1 class, 3 students, 4 users |
| DB trống → app tự tạo DB | ✅ Log lần 1: `CREATE DATABASE` → `Applying migration '…_InitialCreate'` → seed. `/health` = `Healthy` 200 |
| `docker compose down -v` rồi `up` | ⏳ Chờ Docker Linux |
| `docker exec … env`, 5 lệnh Docker/Linux | ⏳ Chờ Docker Linux (danh sách lệnh ở mục 8) |
| Trả lời câu hỏi production | ✅ Mục 8 |

**Giải thích từng file, từng lệnh, luồng chạy và cách build/run (terminal, Visual Studio 2022, VS Code, Docker):** [explained.md](explained.md)

**Thay đổi:**

| File | Thay đổi |
|---|---|
| `TestManager.Api/Data/Migrations/*` | **Mới.** Migration `InitialCreate` + snapshot, sinh bằng `dotnet ef migrations add InitialCreate --output-dir Data/Migrations` |
| `TestManager.Api/Data/DbSeeder.cs` | **Mới.** Seed lớp mẫu, idempotent, mật khẩu `123456` hash bằng `PasswordHasher` |
| `TestManager.Api/Program.cs` | `EnableRetryOnFailure()`; migrate + seed khi Development |
| `TestManager.Api/Services/Implementations/StudentExamService.cs` | `RegisterManyAsync` bọc trong `CreateExecutionStrategy().ExecuteAsync(...)`. Bật retry mà tự mở transaction thì EF ném `InvalidOperationException`. Đã test `POST /api/student-exams/batch`: lô đúng → 200; lô có phần tử lỗi → 409 và rollback cả lô |
| `TestManager.Api/TestManager.Api.csproj` | Thêm `Microsoft.EntityFrameworkCore.Design` 9.0.21 (bắt buộc cho `dotnet ef`) |
| `.config/dotnet-tools.json` | **Mới.** `dotnet-ef` 9.0.21 dạng local tool: cả team chạy `dotnet tool restore` là có đúng phiên bản |
| `docker-compose.yml` | Bỏ service `db-init` và điều kiện `service_completed_successfully` |

**Ghi chú:**
- Schema từ migration **khác `init.sql` ở kiểu cột**: model không khai báo `MaxLength` nên cột thường là `nvarchar(max)`, cột có index là `nvarchar(450)`, và không có `DEFAULT`. Chức năng không đổi. Muốn giống hẳn thì thêm `HasMaxLength` vào `AppDbContext` rồi tạo migration mới.
- `init.sql` không còn được dùng khi chạy bằng Docker. **DB nào đã tạo bằng `init.sql` thì không có `__EFMigrationsHistory`**, nên `Migrate()` sẽ lỗi "There is already an object named 'Users'". Lần đầu chuyển sang migration phải `docker compose down -v` (hoặc xoá DB local).
- EF Core 9 tự lấy khoá `sp_getapplock '__EFMigrationsLock'` trước khi migrate (thấy trong log), giúp nhiều instance không migrate chồng lên nhau.

**Kế hoạch:**

| # | Việc | Ghi chú |
|---|---|---|
| 1 | Cài `dotnet-ef`, tạo migration `InitialCreate` từ `AppDbContext` (bù yêu cầu bài 19/10) | `AppDbContext` đã cấu hình đủ computed column, khoá ngoại và unique index, nên migration sinh ra sẽ giống `init.sql` |
| 2 | Trong `Program.cs`: nếu `app.Environment.IsDevelopment()` thì `db.Database.Migrate()` | Chỉ chạy ở Development, đúng yêu cầu |
| 3 | `UseSqlServer(..., o => o.EnableRetryOnFailure())` | Giữ thêm `depends_on: service_healthy` làm lớp bảo vệ thứ hai |
| 4 | Seed dữ liệu mẫu: chỉ thêm khi bảng đang rỗng (`if (!db.Subjects.Any())`) | Khởi động lần 2 không sinh trùng. Dùng lại dữ liệu mẫu trong `init.sql`, mật khẩu hash bằng `PasswordHasher` |
| 5 | Bỏ service `db-init` khỏi compose | Không còn cần `init.sql` khi chạy bằng Docker. `init.sql` vẫn giữ để chạy trên máy nếu muốn |
| 6 | Thực hành lệnh Linux trong container, ghi 5 lệnh vào log | `docker compose logs -f api`, `docker exec -it <container> sh` (image aspnet có `sh`), `ls`, `cat`, `env`, `ps` (aspnet không có `ps`: dùng `ls /proc` hoặc cài thêm, ghi chú trong log) |

**Câu hỏi phải trả lời được:** vì sao không tự chạy migration khi start trên production có nhiều instance?
Gợi ý: nhiều instance cùng chạy migration một lúc sẽ tranh khoá, có thể migrate dở dang. App cần quyền DDL, rủi ro bảo mật cao hơn. Migration lỗi khiến app không khởi động được. Không có bước duyệt hay rollback riêng. Pipeline tuần 5 sẽ tách thành bước `migrations bundle` riêng.

**Phụ thuộc:** Docker Desktop phải chạy được Linux containers, giống bài 21/10.

## 7. Kiến thức cần học và tài liệu

> Chi tiết khái niệm, phần nâng cao, nguồn học và checklist tự kiểm tra: [learning.md](learning.md).

- [EF Core: áp dụng migration](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
- [EF Core: data seeding](https://learn.microsoft.com/ef/core/modeling/data-seeding)
- [Connection resiliency (EnableRetryOnFailure)](https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency)
- [Compose startup order](https://docs.docker.com/compose/how-tos/startup-order/)
- [docker exec](https://docs.docker.com/reference/cli/docker/container/exec/)

## 8. Kết quả và log

### 8.1 Đã chạy (22/10, không Docker)

```powershell
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 9.0.21
dotnet add TestManager.Api package Microsoft.EntityFrameworkCore.Design --version 9.0.21
dotnet ef migrations add InitialCreate --project TestManager.Api --output-dir Data/Migrations
dotnet ef migrations has-pending-model-changes --project TestManager.Api   # No changes
# Chạy app 2 lần, ASPNETCORE_ENVIRONMENT=Development, DB test trống
```

Log lần 1 (rút gọn): `Applying EF Core migrations...` → `CREATE DATABASE` → `Acquiring an exclusive lock for migration application` → `Applying migration '20261022161533_InitialCreate'` → `Seed: đã tạo lớp mẫu 'C# - K1' với 3 sinh viên.`
Log lần 2: `Seed: dữ liệu mẫu đã có (Subject CS01), bỏ qua.`

**Vấn đề gặp phải:** máy chỉ có .NET SDK 8 trong khi `global.json` yêu cầu 9.0.201. Đã cài SDK 9.0.319 vào `%USERPROFILE%\.dotnet` (không cần admin) bằng `dotnet-install.ps1 -Channel 9.0`. Máy chưa có Docker Desktop và WSL.

### 8.2 Việc còn lại khi có Docker Linux

```powershell
docker compose down -v                   # xoá volume cũ do init.sql tạo
docker compose up -d --build
docker compose logs -f api               # thấy migrate + seed, không lỗi
docker compose restart api; docker compose logs api | Select-String Seed   # lần 2: "bỏ qua"
```

Bảng 5+ lệnh Docker/Linux cần chạy và ghi tác dụng thực tế:

| # | Lệnh | Tác dụng | Kết quả thực tế |
|---|---|---|---|
| 1 | `docker compose logs -f --tail 50 api` | Theo dõi log API | |
| 2 | `docker exec -it <api> sh` | Mở shell trong container (image không có `bash`) | |
| 3 | `ls -la /app` | File publish, quyền, chủ sở hữu | |
| 4 | `cat /etc/os-release` | Distro gốc của image | |
| 5 | `docker exec <api> env` | Biến môi trường, thấy `ConnectionStrings__Default` (kèm mật khẩu dạng chữ thường) | |
| 6 | `id` | Chứng minh chạy non-root (`USER $APP_UID`) | |
| 7 | `ps` → không có; thay bằng `ls /proc`, `cat /proc/1/cmdline`, hoặc `docker top <api>` từ host | Xem process | |

### 8.3 Câu hỏi: vì sao không tự migrate khi start trên production nhiều instance

1. **Tranh chấp:** nhiều instance khởi động cùng lúc đều thấy migration chưa chạy. EF Core 9 có khoá `__EFMigrationsLock` nên không chạy chồng, nhưng các instance phải xếp hàng chờ, làm chậm khởi động.
2. **Quyền quá rộng:** app cần quyền DDL (`CREATE/ALTER/DROP`). Lộ connection string là lộ quyền xoá bảng. Tài khoản runtime chỉ nên có quyền đọc/ghi dữ liệu.
3. **Migration lỗi = app chết:** app không khởi động được, health check fail, orchestrator khởi động lại liên tục mà lần nào cũng migrate lại.
4. **Không kiểm soát được:** không review SQL trước, không có bước duyệt, không có kế hoạch rollback riêng cho schema.
5. **Rolling deploy:** bản cũ và bản mới chạy song song trên cùng schema, nên migration phải tương thích ngược (expand → contract), không thể gắn cứng vào lúc app khởi động.

→ Production: migration là bước riêng trong pipeline (`dotnet ef migrations bundle` hoặc `migrations script --idempotent`), chạy 1 lần trước khi deploy app (tuần 5, bài 20/11).
