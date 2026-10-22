# Giải thích chi tiết: Docker Compose cho TestManager API

Tài liệu này giải thích **từng file đã thêm hoặc sửa** cho bài *"Chạy API + SQL Server bằng Docker Compose"*. Mỗi file được đi qua từng dòng cấu hình, sau đó là phần hướng dẫn chạy và kiểm tra.

> Spec và tiến độ: [design.md](design.md) · [spec.md](spec.md)

---

## 1. Tổng quan các file

| File | Thêm hay sửa | Vai trò | Yêu cầu đề |
|---|---|---|---|
| `TestManager.Api/appsettings.json` | Sửa | Đổi tên connection string thành `Default` | R1 |
| `TestManager.Api/Program.cs` | Sửa | Đọc connection string `Default`, thêm health check và endpoint `/health` | R1, R2 |
| `TestManager.Api/TestManager.Api.csproj` | Sửa | Thêm package health check cho EF Core | R2 |
| `docker-compose.yml` | **Thêm** | Khai báo 3 service `db`, `db-init`, `api` và volume `mssql-data` | R3, R5 |
| `.env.example` | **Thêm** | Mẫu biến môi trường, được commit | R4 |
| `.env` | **Thêm** (không commit) | Mật khẩu SA thật | R4 |
| `.gitignore` | Sửa | Thêm `.env`, để git không bao giờ theo dõi file mật khẩu | R4 |
| `README.md` | **Thêm** | Hướng dẫn chạy ngắn gọn | |
| `docs/week-1-docker/3-wed_21-10_config-health-check-compose/spec.md`, `design.md`, `explained.md` | **Thêm** | Spec bài học, thiết kế chi tiết, giải thích (file này) | |
| `Dockerfile.linux` | Dùng lại, không sửa | Build image API cho Linux | R3 |
| `.dockerignore` | Dùng lại, không sửa | Đã loại `.env` khỏi build context | R4 |
| `database/init.sql` | Dùng lại, không sửa | Tạo schema và dữ liệu mẫu lần đầu | R3 |

---

## 2. Bức tranh tổng thể: mật khẩu và connection string đi qua những đâu

```
 .env                         docker-compose.yml                         Container
┌─────────────────────┐      ┌──────────────────────────────────┐
│MSSQL_SA_PASSWORD=xxx│─────▶│ ${MSSQL_SA_PASSWORD}  (Compose    │
└─────────────────────┘      │  tự thay lúc đọc file)            │
                             │                                  │
                             │ db.environment.MSSQL_SA_PASSWORD ─┼──▶ SQL Server đặt mật khẩu sa
                             │                                  │
                             │ api.environment.                 │
                             │  ConnectionStrings__Default =    │
                             │  "Server=db,1433;...;            ─┼──▶ ASP.NET Core đọc thành
                             │   Password=${MSSQL_SA_PASSWORD}" │    Configuration["ConnectionStrings:Default"]
                             └──────────────────────────────────┘    → GetConnectionString("Default")
                                                                       → ghi đè giá trị trong appsettings.json
```

**Tóm tắt:**
- Mật khẩu **chỉ nằm trong `.env`**.
- Compose chép mật khẩu vào biến môi trường của từng container.
- API không hề biết có file `.env`. API chỉ đọc biến môi trường `ConnectionStrings__Default`.

---

## 3. Giải thích từng file

### 3.1 `TestManager.Api/appsettings.json` (sửa dòng 3)

```json
2  "ConnectionStrings": {
3    "Default": "Server=localhost;Database=TestManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
4  },
```

| Dòng | Giải thích |
|---|---|
| 2 | Mục `ConnectionStrings` là nơi ASP.NET Core quy ước đặt các chuỗi kết nối |
| 3 | **Trước đây key tên là `DefaultConnection`, giờ đổi thành `Default`.** Đề yêu cầu biến môi trường tên `ConnectionStrings__Default`, nên key trong appsettings phải là `Default` thì biến môi trường mới ghi đè đúng chỗ |
| 3 (giá trị) | Giá trị này chỉ dùng khi **chạy trực tiếp trên máy** (`dotnet run`): `localhost` là SQL Server cài trên máy, `Trusted_Connection=True` là đăng nhập bằng tài khoản Windows. Khi chạy trong Docker, giá trị này bị biến môi trường thay thế |

**Vì sao biến môi trường ghi đè được?** `WebApplication.CreateBuilder` nạp cấu hình theo thứ tự `appsettings.json` → `appsettings.{Environment}.json` → **biến môi trường** → tham số dòng lệnh. Nguồn nạp sau ghi đè nguồn nạp trước. Trong tên biến môi trường, `__` (2 dấu gạch dưới) được hiểu là dấu `:`, nên `ConnectionStrings__Default` chính là key `ConnectionStrings:Default`.

### 3.2 `TestManager.Api/Program.cs` (sửa dòng 15, thêm dòng 17–18 và 57)

```csharp
14 builder.Services.AddDbContext<AppDbContext>(options =>
15     options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
16
17 builder.Services.AddHealthChecks()
18     .AddDbContextCheck<AppDbContext>("database");
...
56 app.MapControllers();
57 app.MapHealthChecks("/health");
```

| Dòng | Giải thích |
|---|---|
| 14 | Đăng ký `AppDbContext` (EF Core) vào DI container |
| 15 | `GetConnectionString("Default")` đọc `ConnectionStrings:Default`. Trên máy, giá trị lấy từ appsettings. Trong Docker, giá trị lấy từ biến `ConnectionStrings__Default`. **Trước đây là `"DefaultConnection"`** |
| 17 | `AddHealthChecks()` bật dịch vụ health check có sẵn của ASP.NET Core |
| 18 | `AddDbContextCheck<AppDbContext>("database")` thêm một bài kiểm tra tên `database`. Mỗi lần có người gọi `/health`, bài này gọi `CanConnectAsync()` của EF Core, tức là thử mở kết nối tới SQL Server. Mở được thì `Healthy`, không mở được thì `Unhealthy` |
| 56 | Map các controller API (đã có từ trước) |
| 57 | `MapHealthChecks("/health")` tạo endpoint `GET /health`. Kết quả trả về là chữ thường (plain text): `Healthy` với **HTTP 200**, hoặc `Unhealthy` với **HTTP 503 Service Unavailable**. Docker, load balancer và Kubernetes đều dựa vào status code này |

> `/health` không có `/api` ở đầu và **không hiện trong Swagger**. Đây là endpoint hạ tầng, không phải API nghiệp vụ.

### 3.3 `TestManager.Api/TestManager.Api.csproj` (thêm dòng 15)

```xml
15 <PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="9.0.21" />
```

| Dòng | Giải thích |
|---|---|
| 15 | Package chính chủ của Microsoft, cung cấp `AddDbContextCheck<T>()`. Phiên bản **9.0.21** để **cùng phiên bản với EF Core** (dòng 14). Lệnh đã dùng: `dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 9.0.21` |

### 3.4 `docker-compose.yml` (file mới, 56 dòng)

#### Service `db`: SQL Server 2022 (dòng 1–16)

```yaml
 1 services:
 2   db:
 3     image: mcr.microsoft.com/mssql/server:2022-latest
 4     environment:
 5       ACCEPT_EULA: "Y"
 6       MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD:?Thiếu MSSQL_SA_PASSWORD trong .env (copy từ .env.example)}
 7     ports:
 8       - "14333:1433"
 9     volumes:
10       - mssql-data:/var/opt/mssql
11     healthcheck:
12       test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"$$MSSQL_SA_PASSWORD\" -C -b -Q \"SELECT 1\" -o /dev/null"]
13       interval: 10s
14       timeout: 5s
15       retries: 10
16       start_period: 30s
```

| Dòng | Giải thích |
|---|---|
| 1 | `services:` liệt kê các container mà Compose quản lý |
| 2 | Tên service là `db`. **Tên này cũng là hostname trong mạng nội bộ**, nên API kết nối bằng `Server=db` |
| 3 | Image SQL Server 2022 chính thức của Microsoft, tải từ registry `mcr.microsoft.com`. Image này chỉ có bản Linux, nên Docker phải ở chế độ Linux containers |
| 5 | `ACCEPT_EULA: "Y"` là bắt buộc. Thiếu dòng này thì SQL Server không chịu khởi động |
| 6 | Đặt mật khẩu cho tài khoản `sa`. `${MSSQL_SA_PASSWORD}` được Compose **thay bằng giá trị trong `.env`** lúc đọc file. Phần `:?…` nghĩa là nếu thiếu biến thì **dừng ngay và in thông báo**, không chạy với mật khẩu rỗng |
| 7–8 | Map cổng theo dạng `máy_host:container`. Trên máy host dùng cổng **14333** vì cổng 1433 đã bị SQL Server cài trên máy chiếm. Dùng cổng này để kết nối SSMS vào `localhost,14333`. API **không** đi qua cổng này, API dùng mạng nội bộ `db:1433` |
| 9–10 | Gắn **named volume** `mssql-data` vào `/var/opt/mssql`, thư mục SQL Server lưu toàn bộ dữ liệu, kể cả database `master`. Xóa container thì volume vẫn còn, nên **dữ liệu còn sau `down` rồi `up`** |
| 11 | `healthcheck` để Docker tự kiểm tra container có "khỏe" không. Đây là phần **nâng cao (R5)** |
| 12 | Lệnh kiểm tra: dùng `sqlcmd` có sẵn trong image để chạy `SELECT 1`. `CMD-SHELL` nghĩa là chạy qua shell. `$$` là `$` đã escape, nên **shell trong container** mới là bên đọc biến `MSSQL_SA_PASSWORD`, Compose không thay giá trị lúc parse. `-C` để tin chứng chỉ tự ký (bắt buộc với mssql-tools18). `-b` để trả mã lỗi khi SQL lỗi. `-o /dev/null` để bỏ phần output |
| 13 | Kiểm tra mỗi 10 giây |
| 14 | Lệnh nào chạy quá 5 giây thì tính là thất bại |
| 15 | Thất bại 10 lần liên tiếp thì Docker đánh dấu `unhealthy` |
| 16 | 30 giây đầu là thời gian SQL Server khởi động, thất bại trong lúc này không bị tính |

#### Service `db-init`: tạo dữ liệu lần đầu (dòng 18–37)

```yaml
18   db-init:
19     image: mcr.microsoft.com/mssql/server:2022-latest
20     depends_on:
21       db:
22         condition: service_healthy
23     environment:
24       MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
25     volumes:
26       - ./database:/scripts:ro
27     entrypoint: ["/bin/bash", "-c"]
28     command:
29       - |
30         S="/opt/mssql-tools18/bin/sqlcmd -S db -U sa -P $$MSSQL_SA_PASSWORD -C -b"
31         if [ "$$($$S -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name='TestManagerDb'")" = "0" ]; then
32           echo "Khoi tao TestManagerDb tu init.sql..."
33           $$S -f 65001 -i /scripts/init.sql
34         else
35           echo "TestManagerDb da ton tai, bo qua init.sql."
36         fi
37     restart: "no"
```

| Dòng | Giải thích |
|---|---|
| 18 | Service phụ, **chạy một lần rồi thoát**. Lý do cần nó: SQL Server mới tinh chưa có database `TestManagerDb` |
| 19 | Dùng **cùng image** với `db` vì image này có sẵn `sqlcmd`, không phải tải thêm image nào |
| 20–22 | Chỉ chạy khi `db` đã **healthy**, tức SQL Server đã nhận được kết nối |
| 24 | Truyền mật khẩu vào để `sqlcmd` đăng nhập |
| 26 | Bind mount thư mục `./database` trên máy vào `/scripts` trong container. `:ro` nghĩa là chỉ đọc. Nhờ vậy container đọc được `init.sql` |
| 27 | Ghi đè lệnh khởi động mặc định (vốn là chạy SQL Server) bằng `bash -c` để chạy đoạn script ở dòng 29–36 |
| 30 | Biến `S` là lệnh `sqlcmd` dùng chung. `-S db` kết nối sang container `db` qua mạng nội bộ |
| 31 | Đếm xem đã có database `TestManagerDb` chưa. `-h -1 -W` để bỏ tiêu đề và khoảng trắng, chỉ in ra con số |
| 32–33 | Nếu kết quả là `0` (chưa có), chạy `init.sql`. `-f 65001` để đọc file theo UTF-8, giữ đúng tiếng Việt |
| 34–35 | Nếu đã có thì **bỏ qua**. Đây là điểm mấu chốt: `init.sql` có lệnh `DROP TABLE`. Chạy lại mỗi lần `up` là **mất dữ liệu** |
| 37 | Không tự khởi động lại sau khi thoát. Container `db-init` có trạng thái `Exited (0)` là đúng |

#### Service `api` và volume (dòng 39–56)

```yaml
39   api:
40     build:
41       context: .
42       dockerfile: Dockerfile.linux
43     image: teamboard-api:linux
44     depends_on:
45       db:
46         condition: service_healthy
47       db-init:
48         condition: service_completed_successfully
49     environment:
50       ConnectionStrings__Default: "Server=db,1433;Database=TestManagerDb;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;Connect Timeout=5"
51       ASPNETCORE_ENVIRONMENT: Development
52     ports:
53       - "8080:8080"
54
55 volumes:
56   mssql-data:
```

| Dòng | Giải thích |
|---|---|
| 40–42 | Build image API từ source. `context: .` gửi cả thư mục (đã lọc bởi `.dockerignore`). Dùng `Dockerfile.linux` vì file `Dockerfile` hiện là bản Windows |
| 43 | Đặt tên cho image build ra là `teamboard-api:linux`, để không trùng với image Windows `teamboard-api:latest` của bài trước |
| 44–48 | API chỉ khởi động khi `db` đã **healthy** và `db-init` đã **chạy xong không lỗi**. Nếu không đợi, API sẽ lên trước SQL Server và gọi DB bị lỗi |
| 50 | **R1**: biến `ConnectionStrings__Default` ghi đè connection string trong appsettings. Từng phần của chuỗi: `Server=db,1433` là tên service `db`, cổng 1433 trong mạng nội bộ. `User Id=sa;Password=…` là SQL Authentication, vì container không dùng được Windows Authentication. `TrustServerCertificate=True` để chấp nhận chứng chỉ tự ký của SQL Server. `Connect Timeout=5` để khi DB tắt, `/health` báo `Unhealthy` sau vài giây thay vì treo khoảng 30 giây |
| 51 | Môi trường `Development` |
| 53 | Mở cổng 8080 ra máy host: http://localhost:8080 |
| 55–56 | Khai báo **named volume** `mssql-data`. Docker tự tạo và quản lý volume này. Volume chỉ mất khi chạy `docker compose down -v` hoặc `docker volume rm` |

### 3.5 `.env.example` (file mới, có commit)

```dotenv
1 # Environment variables for TestManager-AspNet
2 MSSQL_SA_PASSWORD=ChangeMe_123!
```

| Dòng | Giải thích |
|---|---|
| 1 | Comment mô tả file |
| 2 | **Giá trị mẫu, không phải mật khẩu thật.** Người mới clone repo chạy `copy .env.example .env` rồi đổi giá trị. Mật khẩu SA phải có ít nhất 8 ký tự, gồm 3 trong 4 loại: hoa, thường, số, ký tự đặc biệt. Nên tránh `$` (Compose hiểu là biến) và `'` |

### 3.6 `.env` (file mới, KHÔNG commit)

```dotenv
MSSQL_SA_PASSWORD=<20 ký tự ngẫu nhiên>
```
- Compose **tự đọc** file `.env` nằm cùng thư mục với `docker-compose.yml`. Không cần khai báo `env_file`.
- Mật khẩu được sinh ngẫu nhiên, đủ 4 loại ký tự, không có `$` hoặc `'`.
- Xem mật khẩu: mở file `.env` bằng VS Code. Đổi mật khẩu: sửa file, chạy `docker compose down -v` (xóa dữ liệu, vì SQL Server chỉ nhận mật khẩu ở lần khởi tạo đầu tiên), rồi `up` lại.

### 3.7 `.gitignore` (thêm 2 dòng cuối)

```gitignore
381 # Docker Compose secrets
382 .env
```
| Dòng | Giải thích |
|---|---|
| 382 | Git bỏ qua file `.env`, nên `git add .` cũng không đưa mật khẩu vào repo. Kiểm tra bằng `git check-ignore -v .env`, kết quả phải là `.gitignore:382:.env` |

`.dockerignore` đã có sẵn `.env` và `**/.env` từ bài trước, nên mật khẩu cũng không lọt vào image API.

---

## 4. Hướng dẫn chạy

### 4.1 Chuẩn bị (một lần)

1. **Docker Desktop ở chế độ Linux containers.** Click chuột phải biểu tượng cá voi. Nếu menu có *Switch to Linux containers…* thì bấm vào. Kiểm tra lại:
   ```powershell
   docker version --format "Server: {{.Server.Os}}/{{.Server.Arch}}"   # phải là linux/amd64
   ```
2. **Tạo `.env`** nếu chưa có:
   ```powershell
   cd C:\Users\anhdt1\Desktop\Intern\Project_SBU3\TestManager-AspNet
   copy .env.example .env        # rồi sửa mật khẩu trong .env
   ```
3. Kiểm tra cú pháp compose (không cần engine chạy):
   ```powershell
   docker compose config --quiet   # không in gì = hợp lệ
   ```

### 4.2 Khởi động

```powershell
docker compose up -d --build
```
Docker sẽ làm theo thứ tự:
1. Build image `teamboard-api:linux`.
2. Pull image `mssql/server:2022` (khoảng 1.6 GB, chỉ tải lần đầu).
3. Khởi động `db` và đợi healthy (khoảng 30 giây).
4. Chạy `db-init`: tạo DB và dữ liệu mẫu.
5. Khởi động `api`.

```powershell
docker compose ps
```
Kết quả mong đợi:
```
NAME                         SERVICE   STATUS
testmanager-aspnet-db-1      db        Up (healthy)
testmanager-aspnet-api-1     api       Up
```
`db-init` không hiện vì đã thoát. Xem nó bằng `docker compose ps -a`, phải ở trạng thái `Exited (0)`.

### 4.3 Kiểm tra từng tiêu chí của đề

| # | Tiêu chí | Lệnh | Kết quả đúng |
|---|---|---|---|
| 1 | 2 container running | `docker compose ps` | `db` Up (healthy), `api` Up |
| 2a | Healthy khi DB chạy | `curl.exe http://localhost:8080/health` | `Healthy` |
| 2b | Unhealthy khi tắt DB | `docker compose stop db`, sau đó `curl.exe -i http://localhost:8080/health` | `HTTP/1.1 503`, `Unhealthy` (mất khoảng 5–15 giây) |
| 2c | Bật lại DB | `docker compose start db`, đợi khoảng 30 giây, gọi lại `/health` | `Healthy` |
| 3 | Dữ liệu còn sau down/up | Tạo dữ liệu (lệnh bên dưới), `docker compose down`, `docker compose up -d`, `GET /api/subjects` | Vẫn thấy môn "Docker 101" |
| 4 | Không lộ mật khẩu | `git check-ignore -v .env`, rồi `git status` | `.env` bị ignore, không xuất hiện trong `git status` |

Tạo dữ liệu thử cho tiêu chí 3:
```powershell
curl.exe -X POST http://localhost:8080/api/subjects -H "Content-Type: application/json" -d '{\"subjectName\":\"Docker 101\",\"code\":\"DK01\"}'
docker compose down          # KHÔNG thêm -v
docker compose up -d
curl.exe http://localhost:8080/api/subjects   # vẫn có DK01
docker compose logs db-init  # in "TestManagerDb da ton tai, bo qua init.sql."
```

### 4.4 Lệnh thường dùng

| Lệnh | Tác dụng |
|---|---|
| `docker compose up -d --build` | Build lại API và chạy nền |
| `docker compose ps` / `ps -a` | Xem trạng thái (`-a` hiện cả container đã thoát) |
| `docker compose logs -f api` | Xem log API theo thời gian thực |
| `docker compose logs db-init` | Xem `init.sql` có chạy hay đã bỏ qua |
| `docker compose stop db` / `start db` | Tắt hoặc bật riêng DB (để test `/health`) |
| `docker compose down` | Xóa container và network, **giữ dữ liệu** |
| `docker compose down -v` | Xóa container **và volume**, **mất toàn bộ dữ liệu** |
| `docker volume ls` | Xem volume `testmanager-aspnet_mssql-data` |

### 4.5 Chạy trực tiếp trên máy, không dùng Docker

```powershell
sqlcmd -S localhost -E -C -f 65001 -i database/init.sql
dotnet run --project TestManager.Api --launch-profile http
curl.exe http://localhost:5080/health      # Healthy
```
Khi chạy trên máy, app dùng connection string `Default` trong `appsettings.json` (SQL Server trên máy, Windows Authentication).

---

## 5. Kết quả đã kiểm chứng và phần còn chờ

| Hạng mục | Kết quả |
|---|---|
| Build | Thành công, 0 warning |
| 78 test hồi quy (chạy trên máy) | 78/78 pass sau khi đổi tên connection string |
| `/health`, DB chạy | `Healthy`, HTTP 200 |
| `/health`, trỏ tới DB không tồn tại qua `ConnectionStrings__Default` | `Unhealthy`, HTTP 503, khoảng 13 giây. Biến môi trường ghi đè appsettings đúng |
| `docker compose config` | Hợp lệ: 3 service, 1 volume |
| Thiếu `.env` | Compose dừng và báo `required variable MSSQL_SA_PASSWORD is missing a value` |
| `.env` có bị ignore không | Có, `.gitignore:382:.env` |
| **Chạy `docker compose up` thật** | ⏳ **Chưa chạy được.** Docker Desktop chưa khởi động xong Linux engine (`no connected VM`). Cần Quit rồi mở lại Docker Desktop, nếu vẫn lỗi thì cài WSL2 |

---

## 6. Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| `no matching manifest for windows/amd64` | Docker đang ở Windows containers | Switch to Linux containers |
| `docker version` treo, log `no connected VM` | Máy ảo Linux chưa lên | Quit rồi mở lại Docker Desktop. Nếu vẫn lỗi thì cài WSL2: `wsl --install --no-distribution` (quyền admin) và khởi động lại máy |
| `required variable MSSQL_SA_PASSWORD is missing` | Chưa có `.env` | `copy .env.example .env` |
| `db` thoát ngay, log báo *password … policy* | Mật khẩu yếu | Đặt mật khẩu ≥ 8 ký tự, gồm 3 trong 4 loại |
| `db` mãi không healthy sau khi đổi mật khẩu | SQL Server chỉ đặt mật khẩu ở **lần khởi tạo đầu**, volume cũ vẫn giữ mật khẩu cũ | `docker compose down -v` rồi `up` lại (mất dữ liệu) |
| `port is already allocated` | Cổng 14333 hoặc 8080 đang bị chiếm | Đổi cổng bên trái trong `ports`, hoặc tắt tiến trình đang chiếm cổng |
| `api` báo lỗi kết nối DB | Dùng `localhost` thay vì `db` | Trong compose phải dùng `Server=db,1433` |
