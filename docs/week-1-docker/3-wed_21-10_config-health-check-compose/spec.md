# Cấu hình, health check, Compose

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 4, 21/10/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | 🟡 Đang làm: code xong, chờ Docker Linux để chạy compose |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Chạy API + SQL Server cùng lúc bằng Docker Compose.

## 2. Yêu cầu

- Connection string đọc từ biến môi trường `ConnectionStrings__Default`.
- Endpoint `/health` dùng `AddHealthChecks()` kèm kiểm tra DB.
- `docker-compose.yml` có service `api`, `db` (SQL Server 2022), named volume cho dữ liệu.
- Mật khẩu SA đọc từ `.env` (không commit), commit kèm `.env.example`.

## 3. Output phải nộp

- `docker-compose.yml`, `.env.example`, endpoint `/health`.

## 4. Tự đánh giá: đạt khi

- [ ] `docker compose up -d` cho 2 container running.
- [ ] `/health` Healthy khi DB chạy, Unhealthy sau `docker compose stop db`.
- [ ] `docker compose down` rồi `up` lại: dữ liệu cũ vẫn còn.
- [ ] `.env` nằm trong `.gitignore`; repo không chứa mật khẩu thật.

## 5. Nâng cao (không bắt buộc)

- Thêm `healthcheck` cho service `db` trong compose.

## 6. Phân tích, hiện trạng và kế hoạch

**Hiện trạng:** code và cấu hình đã xong. **Chưa chạy được `docker compose up`**, vì Docker Desktop chưa khởi động được Linux engine (`no connected VM`). Image SQL Server 2022 chỉ có bản Linux.

| Yêu cầu và tiêu chí | Kết quả |
|---|---|
| Đọc `ConnectionStrings__Default` | ✅ Đã đổi tên key thành `Default` (`appsettings.json`, `Program.cs`). Biến môi trường ghi đè đúng (đã test) |
| `/health` dùng `AddHealthChecks()` kèm kiểm tra DB | ✅ `AddDbContextCheck<AppDbContext>()`. Chạy trên máy: `Healthy` 200. DB chết: `Unhealthy` 503 |
| `docker-compose.yml` có `api`, `db` (SQL Server 2022), named volume | ✅ `docker compose config` hợp lệ: `db`, `db-init`, `api`, volume `mssql-data` |
| `.env` (không commit) và `.env.example` | ✅ `git check-ignore` trả về `.gitignore:382:.env` |
| `docker compose up -d` cho 2 container running | ⏳ Chờ Docker Linux |
| Healthy, rồi Unhealthy sau `stop db` | ⏳ Chờ Docker Linux |
| `down` rồi `up`: dữ liệu còn | ⏳ Chờ Docker Linux |
| *(nâng cao)* healthcheck cho `db` | ✅ Có trong compose (dùng `sqlcmd SELECT 1`) |

**Tài liệu chi tiết:**
- [design.md](design.md): phân tích hiện trạng, kiến trúc, quyết định thiết kế, rủi ro, phần nâng cao.
- [explained.md](explained.md): giải thích từng dòng của `docker-compose.yml`, `Program.cs`, `.env` và hướng dẫn chạy, kiểm tra.

**Việc còn lại:**
1. Khởi động lại Docker Desktop. Nếu vẫn lỗi thì cài WSL2.
2. Chạy kiểm thử theo mục 4.3 của `explained.md`, lưu log vào thư mục này.

## 7. Kiến thức cần học và tài liệu

- [Docker Compose](https://docs.docker.com/compose/)
- [Compose: variable interpolation / .env](https://docs.docker.com/compose/how-tos/environment-variables/variable-interpolation/)
- [ASP.NET Core health checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [SQL Server trong container](https://learn.microsoft.com/sql/linux/quickstart-install-connect-docker)
- [Docker volumes](https://docs.docker.com/engine/storage/volumes/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
