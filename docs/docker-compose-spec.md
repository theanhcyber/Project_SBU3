# Spec: Chạy TestManager API + SQL Server bằng Docker Compose

| | |
|---|---|
| **Mục tiêu** | Chạy API và SQL Server cùng lúc bằng `docker compose` |
| **Ngày** | 21/10/2026 |
| **Phạm vi** | `TestManager-AspNet` |
| **Môi trường** | Docker Desktop 28.1.1 chế độ **Linux containers**, backend **Hyper-V** (máy chưa có WSL2) |
| **Trạng thái** | Đã triển khai T1–T6, T8. **T0 và T7 đang chờ** Docker Desktop khởi động được Linux engine |

### Tiến độ (22/10/2026)

| Task | Trạng thái | Bằng chứng |
|---|---|---|
| T0 Chuyển sang Linux | ⏳ Đang chặn | Đã chuyển cấu hình (`UseWindowsContainers = False`), nhưng máy ảo Linux chưa lên (`no connected VM`). Cần khởi động lại Docker Desktop, nếu vẫn lỗi thì cài WSL2 |
| T1 Connection string `Default` | ✅ | `appsettings.json` và `Program.cs` đã đổi tên. Chạy local 78/78 test pass |
| T2 `/health` | ✅ | Local: `Healthy` 200. Trỏ tới DB không tồn tại qua `ConnectionStrings__Default`: `Unhealthy` 503 sau khoảng 13 giây |
| T3 `.env` | ✅ | `.env.example` đã tạo. `.env` có mật khẩu ngẫu nhiên. `git check-ignore` trả về `.gitignore:382:.env` |
| T4, T5, T6 Compose | ✅ (chưa chạy) | `docker compose config` hợp lệ: 3 service, volume `mssql-data`. Thiếu `.env` thì Compose báo lỗi rõ ràng |
| T7 Kiểm thử compose | ⏳ Chờ T0 | |
| T8 README | ✅ | `README.md` |

> **Lịch sử quyết định:**
> - Ban đầu định giữ Windows containers.
> - Microsoft không còn phát hành image SQL Server chính thức cho Windows. Giữ Windows thì phải tự build image SQL Server, rủi ro cao.
> - Vì vậy chốt **chuyển sang Linux containers** và dùng image chính thức `mcr.microsoft.com/mssql/server:2022-latest`.
> - `Dockerfile` (bản Windows) vẫn giữ lại cho bài trước. Compose dùng `Dockerfile.linux`.

---

## 1. Yêu cầu và tiêu chí đạt (trích đề)

| # | Yêu cầu | Tiêu chí đạt | Output nộp |
|---|---|---|---|
| R1 | Connection string đọc từ biến môi trường `ConnectionStrings__Default` | API trong container kết nối được DB | `docker-compose.yml` |
| R2 | Endpoint `/health` dùng `AddHealthChecks()`, có kiểm tra DB | `/health` trả `Healthy` khi DB chạy, `Unhealthy` sau `docker compose stop db` | Endpoint `/health` |
| R3 | `docker-compose.yml` có service `api`, `db` (SQL Server 2022) và named volume cho dữ liệu | `docker compose up -d` cho 2 container ở trạng thái running. `down` rồi `up` lại thì dữ liệu cũ vẫn còn | `docker-compose.yml` |
| R4 | Mật khẩu SA đọc từ `.env` (không commit), commit kèm `.env.example` | `.env` có trong `.gitignore`, repo không chứa mật khẩu thật | `.env.example` |
| R5 *(nâng cao)* | Healthcheck cho service `db` trong compose | `docker compose ps` hiện `db` ở trạng thái `healthy` | |

---

## 2. Hiện trạng và các điểm cần xử lý

| # | Hiện trạng | Vấn đề | Cần làm |
|---|---|---|---|
| G1 | Docker Desktop đang ở Windows containers, máy chưa có WSL2, Hyper-V đang chạy | Image SQL Server chỉ có bản Linux | Chuyển sang **Linux containers dùng backend Hyper-V**, không cần cài WSL2 (T0) |
| G2 | Ổ C chỉ còn **trống khoảng 4.9 GB** | Image Linux cần tải: mssql 2022 khoảng 1.6 GB, dotnet sdk khoảng 0.9 GB, aspnet khoảng 0.2 GB, cộng ổ đĩa ảo của Docker. Rất dễ đầy ổ | Giải phóng dung lượng **trước khi pull**, mục tiêu ≥ 10 GB (T0) |
| G3 | Connection string tên là `DefaultConnection` | Đề yêu cầu `ConnectionStrings__Default` | Đổi tên key thành `Default` (T1) |
| G4 | Chưa có health check | Thiếu R2 | `AddHealthChecks()` + `MapHealthChecks("/health")` (T2) |
| G5 | `.gitignore` chưa có `.env` | Có thể lỡ commit mật khẩu | Thêm `.env`, tạo `.env.example` (T3) |
| G6 | `database/init.sql` có `DROP TABLE` | Nếu chạy script này mỗi lần `up` thì dữ liệu bị xóa, vi phạm R3 | Service `db-init` chỉ chạy `init.sql` **khi DB chưa tồn tại** (T4) |
| G7 | SQL Server cài trên máy chiếm cổng 1433 | Trùng cổng | Map `14333:1433` (T5) |
| G8 | App dùng Windows Authentication | Không dùng được trong Linux container | SQL Authentication `User Id=sa;Password=...` (T5) |

---

## 3. Kiến trúc mục tiêu

```
                 docker compose (Linux containers, network mặc định)
┌────────────────────────────────────────────────────────────────────────┐
│                                                                        │
│  ┌──────────────────────┐   Server=db,1433   ┌──────────────────────┐  │
│  │  api                 │ ─────────────────▶ │  db                  │  │
│  │  build: Dockerfile.  │                    │  mssql/server:2022   │  │
│  │         linux        │   depends_on:      │  healthcheck: sqlcmd │  │
│  │  :8080               │   service_healthy  │  :1433               │  │
│  │  /swagger  /health   │                    │                      │  │
│  └──────────┬───────────┘                    └──────────┬───────────┘  │
│             │                                ┌──────────┴───────────┐  │
│             │                                │ db-init (chạy 1 lần) │  │
│             │                                │ chạy init.sql nếu DB │  │
│             │                                │ chưa tồn tại         │  │
│             │                                └──────────────────────┘  │
└─────────────┼───────────────────────────────────────────┼──────────────┘
              │ 8080:8080                                 │ volume mssql-data → /var/opt/mssql
        localhost:8080                                    │ 14333:1433 (SSMS từ máy host)
```

Trong mạng compose, các container gọi nhau bằng **tên service** (`db`), không dùng `localhost`. Với SQL Server bản Linux, toàn bộ `/var/opt/mssql`, gồm cả database `master`, nằm trên volume. Vì vậy sau `down` rồi `up` thì database vẫn còn, **không cần attach lại**.

---

## 4. Các việc cần làm

| Task | Nội dung | File thay đổi | Xong khi |
|---|---|---|---|
| **T0** | Chuyển Docker Desktop sang Linux containers (backend Hyper-V). Giải phóng ổ C (≥ 10 GB trống) | Môi trường | `docker version` báo Server `linux/amd64`, `docker run --rm hello-world` chạy được |
| **T1** | Đổi tên connection string `DefaultConnection` thành `Default` | `appsettings.json`, `Program.cs` | Chạy local vẫn kết nối được, 78 test hồi quy pass |
| **T2** | Health check có kiểm tra DB, map `/health` | `Program.cs`, `.csproj` | `/health` trả `Healthy` (200). Tắt DB thì trả `Unhealthy` (503) |
| **T3** | Thêm `.env` vào `.gitignore`. Tạo `.env.example` và `.env` | `.gitignore`, `.env.example`, `.env` | `git check-ignore .env` in ra `.env` |
| **T4** | Service `db-init`: chạy `init.sql` chỉ khi DB chưa có | `docker-compose.yml` | `up` lần 2 không mất dữ liệu |
| **T5** | `docker-compose.yml`: `api`, `db`, `db-init`, volume `mssql-data` | `docker-compose.yml` | `docker compose up -d` cho `api` và `db` running |
| **T6** *(nâng cao)* | Healthcheck cho `db` bằng `sqlcmd`, kết hợp `depends_on: service_healthy` | `docker-compose.yml` | `docker compose ps` hiện `db` (healthy) |
| **T7** | Kiểm thử theo mục 7, lưu log | `docs/docker-compose-log.txt` | Đạt đủ 4 tiêu chí |
| **T8** | Cập nhật README và giáo án | `README.md`, `docs/` | Làm theo README là chạy được |

Thứ tự thực hiện: T0 → T1 → T2 (test local) → T3 → T5 → T4 → T6 → T7 → T8.

---

## 5. Thiết kế chi tiết

### 5.1 Connection string (T1)
`appsettings.json`: đổi key `DefaultConnection` thành `Default`, giá trị giữ nguyên để chạy local. `Program.cs`: `GetConnectionString("Default")`.
Trong container, biến `ConnectionStrings__Default` **ghi đè** giá trị trong appsettings, vì ASP.NET Core đổi `__` thành `:`.

### 5.2 Health check (T2)
Package `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (phiên bản 9.0.x):
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");
...
app.MapHealthChecks("/health");
```
Trả `Healthy` (200) hoặc `Unhealthy` (503). Thêm `Connect Timeout=5` vào connection string để `/health` không bị treo khoảng 30 giây khi DB tắt.

### 5.3 `.env` / `.env.example` / `.gitignore` (T3)
```dotenv
# .env.example: copy thành .env rồi đổi mật khẩu.
# Mật khẩu SA: ≥ 8 ký tự, gồm 3 trong 4 loại: hoa, thường, số, ký tự đặc biệt. Tránh dùng $ và '
MSSQL_SA_PASSWORD=ChangeMe_123!
```
Thêm dòng `.env` vào `.gitignore`. Compose tự đọc `.env` cùng thư mục và thay vào các chỗ `${MSSQL_SA_PASSWORD}`.

### 5.4 `docker-compose.yml` (T4, T5, T6)
```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD:?Thiếu MSSQL_SA_PASSWORD trong .env}
    ports:
      - "14333:1433"
    volumes:
      - mssql-data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -C -b -Q \"SELECT 1\" -o /dev/null"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 30s

  db-init:
    image: mcr.microsoft.com/mssql/server:2022-latest
    depends_on:
      db:
        condition: service_healthy
    environment:
      MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
    volumes:
      - ./database:/scripts:ro
    entrypoint: ["/bin/bash", "-c"]
    command:
      - |
        S="/opt/mssql-tools18/bin/sqlcmd -S db -U sa -P $$MSSQL_SA_PASSWORD -C -b"
        if [ "$$($$S -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='TestManagerDb'")" = "0" ]; then
          echo "Khởi tạo TestManagerDb..."; $$S -f 65001 -i /scripts/init.sql
        else
          echo "TestManagerDb đã tồn tại, bỏ qua."; fi
    restart: "no"

  api:
    build:
      context: .
      dockerfile: Dockerfile.linux
    depends_on:
      db:
        condition: service_healthy
      db-init:
        condition: service_completed_successfully
    environment:
      ConnectionStrings__Default: "Server=db,1433;Database=TestManagerDb;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;Connect Timeout=5"
      ASPNETCORE_ENVIRONMENT: Development
    ports:
      - "8080:8080"

volumes:
  mssql-data:
```
- `$$` là `$` đã escape cho Compose. Biến đó do shell trong container đọc, Compose không thay giá trị.
- Cú pháp `${VAR:?thông báo}`: thiếu biến thì Compose báo lỗi và dừng ngay, thay vì chạy với mật khẩu rỗng.
- `db-init` dùng lại image `mssql/server` vì image này có sẵn `sqlcmd`, không phải pull thêm image nào. Đây là điểm quan trọng khi ổ đĩa hạn chế.

---

## 6. Quyết định thiết kế và phương án thay thế

| Vấn đề | Đề xuất | Phương án khác | Lý do chọn |
|---|---|---|---|
| Container engine | Linux, backend **Hyper-V** | WSL2 (Docker khuyến nghị, nhanh hơn, cần cài và khởi động lại máy). Windows containers kèm image SQL tự build (rủi ro cao) | Hyper-V đã chạy sẵn, chuyển ngay được, không cần khởi động lại máy |
| Health check | `AddDbContextCheck<AppDbContext>()` | `AspNetCore.HealthChecks.SqlServer` (Xabaril) | Dùng lại `DbContext` có sẵn, package chính chủ của Microsoft |
| Tạo schema và dữ liệu mẫu | `db-init` chạy `init.sql` khi DB chưa có | EF Core Migrations kèm `Database.Migrate()` | Dùng lại `init.sql`, kịp trong hôm nay. Migrations là hướng chuẩn lâu dài (mục 9) |
| Mật khẩu | `.env` + interpolation | `env_file`, Docker secrets | Đúng yêu cầu đề |
| Tài khoản DB của app | `sa` | Login riêng `testmanager_app` | Đúng đề. Login riêng là best practice (mục 9) |
| Thứ tự khởi động | `depends_on` + `service_healthy` | `depends_on` thường | SQL Server cần khoảng 20–60 giây mới nhận kết nối |

---

## 7. Kế hoạch kiểm thử (T7)

| # | Tiêu chí | Lệnh | Kết quả mong đợi |
|---|---|---|---|
| 1 | 2 container running | `docker compose up -d --build`, sau đó `docker compose ps` | `db` Up (healthy), `api` Up. `db-init` Exited (0) |
| 2a | `/health` Healthy | `curl.exe http://localhost:8080/health` | `Healthy`, HTTP 200 |
| 2b | Unhealthy khi tắt DB | `docker compose stop db`, rồi `curl.exe -i http://localhost:8080/health` | `Unhealthy`, HTTP 503 |
| 2c | Hồi phục | `docker compose start db`, đợi DB healthy rồi gọi lại `/health` | Lại `Healthy` |
| 3 | Dữ liệu còn sau down/up | `POST /api/subjects` tạo "Docker 101", `docker compose down`, `docker compose up -d`, rồi `GET /api/subjects` | Vẫn thấy "Docker 101". `db-init` in "đã tồn tại, bỏ qua" |
| 4 | Không lộ mật khẩu | `git check-ignore -v .env`, `git status`, `git grep -n "<mật khẩu thật>"` | `.env` bị ignore, không tìm thấy mật khẩu thật |

> ⚠️ `docker compose down -v` **xóa luôn volume**, tức mất dữ liệu. Khi kiểm tiêu chí 3, chỉ dùng `docker compose down`.

---

## 8. Rủi ro và lưu ý

| Rủi ro | Dấu hiệu | Cách xử lý |
|---|---|---|
| **Hết dung lượng ổ C** | `no space left on device` khi pull hoặc build | Dọn ổ C trước. Image Windows cũ không dùng được ở chế độ Linux nhưng vẫn chiếm chỗ trên ổ |
| Mật khẩu SA không đủ mạnh | `db` thoát ngay, log báo *password does not meet SQL Server password policy* | Mật khẩu ≥ 8 ký tự, gồm 3 trong 4 loại |
| Mật khẩu chứa `$` | Compose hiểu `$` là biến | Tránh dùng `$`, hoặc viết `$$` |
| Trùng cổng 1433 | `port is already allocated` | `14333:1433` |
| Đường dẫn sqlcmd | `sqlcmd: not found` | Image 2022 dùng `/opt/mssql-tools18/bin/sqlcmd`, **bắt buộc có `-C`** |
| `/health` chậm khi DB tắt | `curl` treo lâu | `Connect Timeout=5` |
| Lần đầu chạy chậm | `api` chờ lâu | Bình thường: pull image và DB khởi động mất khoảng 30 giây |
| Backend Hyper-V tốn RAM | Máy chậm | Docker Desktop → Settings → Resources: giới hạn RAM khoảng 4 GB. SQL Server cần tối thiểu 2 GB |
| Lỡ commit `.env` | `git log` có `.env` | `git rm --cached .env`, đổi mật khẩu |

---

## 9. Phần nâng cao và bổ sung (ngoài đề)

| Mức | Nội dung | Lợi ích |
|---|---|---|
| ⭐ Đề yêu cầu | Healthcheck `db` kết hợp `depends_on: service_healthy` (đã có trong mục 5.4) | API không khởi động trước khi DB sẵn sàng |
| ⭐⭐ | `/health/live` và `/health/ready` tách bằng `tags` | Đúng chuẩn liveness/readiness |
| ⭐⭐ | `/health` trả JSON chi tiết | Dễ debug |
| ⭐⭐ | `restart: unless-stopped` | Tự chạy lại khi lỗi |
| ⭐⭐ | Login SQL riêng cho app | Nguyên tắc quyền tối thiểu |
| ⭐⭐ | Chuyển backend sang WSL2 khi có điều kiện | Nhanh hơn, ít tốn RAM hơn Hyper-V |
| ⭐⭐⭐ | EF Core Migrations thay `init.sql` | Quản lý schema theo phiên bản, bỏ được `db-init` |
| ⭐⭐⭐ | Docker secrets (`MSSQL_SA_PASSWORD_FILE`) | Mật khẩu không hiện trong `docker inspect` |
| ⭐⭐⭐ | Healthcheck cho `api`: image aspnet không có `curl`, phải cài thêm hoặc viết lệnh nhỏ bằng `dotnet` | Compose biết khi nào API sẵn sàng |

---

## 10. Câu hỏi cần chốt

1. ~~Windows hay Linux?~~ **Đã chốt: Linux, backend Hyper-V.**
2. Dọn ổ C thế nào? Xoá image Windows cũ không còn dùng, hay dọn file khác?
3. App dùng `sa` như đề (mặc định), hay tạo login riêng?
4. Có map cổng DB `14333` ra máy host không (mặc định: có)?
5. Có làm thêm phần nâng cao nào ở mục 9 không?

---

## 11. Kiến thức cần nắm và tài liệu tham khảo

| Chủ đề | Cần hiểu | Tài liệu |
|---|---|---|
| Docker Compose cơ bản | `services`, `build`, `image`, `ports`, `environment`, `depends_on`, gọi nhau bằng tên service | https://docs.docker.com/compose/ · https://docs.docker.com/reference/compose-file/services/ |
| Biến môi trường và `.env` | `${VAR}`, `${VAR:?err}`, `$$` | https://docs.docker.com/compose/how-tos/environment-variables/variable-interpolation/ |
| Thứ tự khởi động | `service_healthy`, `service_completed_successfully` | https://docs.docker.com/compose/how-tos/startup-order/ |
| Volume | Named volume, `down` khác `down -v` | https://docs.docker.com/engine/storage/volumes/ |
| SQL Server trong container | `ACCEPT_EULA`, `MSSQL_SA_PASSWORD`, `/var/opt/mssql`, `sqlcmd -C` | https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker |
| Configuration của ASP.NET Core | Biến môi trường ghi đè appsettings, quy ước `__` | https://learn.microsoft.com/aspnet/core/fundamentals/configuration/ |
| Health checks | `AddHealthChecks`, `AddDbContextCheck`, `MapHealthChecks` | https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks |
| Docker Desktop trên Windows | Backend Hyper-V và WSL2, chuyển Linux/Windows containers | https://docs.docker.com/desktop/setup/install/windows-install/ |
| *(nâng cao)* EF Core Migrations | `dotnet ef migrations add`, `Database.Migrate()` | https://learn.microsoft.com/ef/core/managing-schemas/migrations/ |
