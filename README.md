# TestManager API

ASP.NET Core 9 Web API + EF Core + SQL Server — quản lý môn học, lớp và thi trắc nghiệm.

## Chạy bằng Docker Compose (API + SQL Server 2022)

Yêu cầu: Docker Desktop ở chế độ **Linux containers**.

```powershell
copy .env.example .env        # rồi đổi MSSQL_SA_PASSWORD trong .env
docker compose up -d --build
docker compose ps             # db (healthy), api (running), db-init (exited 0)
```

| URL | |
|---|---|
| http://localhost:8080/swagger | Swagger |
| http://localhost:8080/health | `Healthy` / `Unhealthy` (503) |
| `localhost,14333` (user `sa`) | Kết nối SQL Server bằng SSMS / Azure Data Studio |

- Lần đầu `db-init` chạy `database/init.sql` tạo schema + dữ liệu mẫu; các lần sau bỏ qua, dữ liệu nằm trong volume `mssql-data`.
- `docker compose down` giữ dữ liệu; `docker compose down -v` **xoá cả dữ liệu**.
- `.env` chứa mật khẩu thật, đã nằm trong `.gitignore` — chỉ commit `.env.example`.

## Chạy trực tiếp trên máy (không Docker)

```powershell
sqlcmd -S localhost -E -C -f 65001 -i database/init.sql
dotnet run --project TestManager.Api --launch-profile http   # http://localhost:5080/swagger
```

Connection string `ConnectionStrings:Default` trong `appsettings.json`; ghi đè bằng biến môi trường `ConnectionStrings__Default`.

## Tài liệu

- [docs/README.md](docs/README.md) — lộ trình 6 tuần, mỗi tuần một thư mục, mỗi ngày một `spec.md`
- Tuần 1 · 20/10 [Dockerfile multi-stage](docs/week-1-docker/2-tue_20-10_dockerfile-multi-stage/spec.md) — kèm `lesson.md`, `build-log.txt`
- Tuần 1 · 21/10 [Cấu hình, health check, Compose](docs/week-1-docker/3-wed_21-10_config-health-check-compose/spec.md) — kèm `design.md`, `explained.md`
