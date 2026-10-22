# Giáo án: Đóng gói TestManager API bằng Docker

**Mục tiêu:** build được image của API và chạy được trong container.
**Thời lượng gợi ý:** khoảng 3 giờ (1 giờ lý thuyết, 2 giờ thực hành và tự đánh giá).

| Phần | Nội dung | Thời gian |
|---|---|---|
| 0 | Chuẩn bị môi trường Docker | 15' |
| 1 | Bốn khái niệm: image, container, layer, registry | 30' |
| 2 | Yêu cầu của đề nằm ở đâu, dùng để làm gì | 15' |
| 3 | Đọc hiểu Dockerfile 2 stage | 30' |
| 4 | Đọc hiểu .dockerignore | 10' |
| 5 | Thực hành: build, run, đo kích thước, kiểm tra cache | 60' |
| 6 | Log nộp bài và tự đánh giá | 10' |
| 7 | Lỗi thường gặp và câu hỏi tự kiểm tra | 10' |

---

## 0. Chuẩn bị môi trường

Image `mcr.microsoft.com/dotnet/aspnet` là **Linux image**, nên Docker Desktop phải chạy ở chế độ **Linux containers**.

1. Mở **Docker Desktop**. Đợi đến khi góc trái dưới hiện *Engine running*.
2. Click chuột phải biểu tượng cá voi ở khay hệ thống:
   - Nếu menu có dòng **Switch to Linux containers…** thì bấm vào đó.
   - Nếu menu ghi *Switch to Windows containers…* thì bạn đã ở Linux, không cần làm gì.
3. Docker Desktop cần WSL2. Nếu được yêu cầu, mở PowerShell **với quyền admin**, chạy `wsl --install` rồi khởi động lại máy.
4. Kiểm tra lại:

```powershell
docker version          # phải có cả Client và Server; Server OS/Arch = linux/amd64
docker run --rm hello-world
```

### 0b. Dùng Windows containers (máy chưa có WSL2)

Nếu Docker Desktop đang ở chế độ **Windows containers** (`docker version` báo Server `windows/amd64`), hãy dùng file [`Dockerfile`](../../../Dockerfile) (bản Windows). File này giống `Dockerfile.linux`, chỉ khác image nền và một vài dòng:

| | `Dockerfile.linux` (Linux) | `Dockerfile` (Windows) |
|---|---|---|
| Image build | `dotnet/sdk:9.0` | `dotnet/sdk:9.0-nanoserver-ltsc2022` |
| Image runtime | `dotnet/aspnet:9.0` | `dotnet/aspnet:9.0-nanoserver-ltsc2022` |
| User chạy app | `USER $APP_UID` | Không cần, image mặc định đã chạy bằng `ContainerUser` |
| Lệnh build | `docker build -f Dockerfile.linux -t teamboard-api .` | `docker build -t teamboard-api .` |

Lệnh `docker run`, cách kiểm tra cache và đo kích thước giữ nguyên.

Hạn chế của Windows containers:
- Image to hơn bản Linux.
- **Không chạy được SQL Server bằng container**, vì `mssql/server` chỉ có bản Linux. Bài docker-compose sau sẽ cần Linux containers.

---

## 1. Bốn khái niệm, giải thích bằng lời

| Khái niệm | Hiểu đơn giản | Ví dụ so sánh | Trong bài này |
|---|---|---|---|
| **Image** | Gói **đóng băng, chỉ đọc**, chứa mọi thứ để chạy app: hệ điều hành rút gọn, .NET runtime, file `.dll` của mình, lệnh khởi động | **Class** trong C#, hay công thức nấu ăn | `teamboard-api` tạo ra từ `docker build` |
| **Container** | Một **phiên chạy** của image: có tiến trình, mạng và ổ ghi riêng. Từ một image chạy được nhiều container | **Object** tạo từ class, hay món ăn nấu theo công thức | Thứ được tạo ra khi `docker run -p 8080:8080 teamboard-api` |
| **Layer** | Image được xếp từ nhiều **lớp**. Mỗi lệnh `FROM`, `COPY`, `RUN` trong Dockerfile tạo một lớp. Lớp nào không đổi thì Docker **dùng lại từ cache**, không build lại | Các bước làm bánh: bước đã làm xong và nguyên liệu không đổi thì không làm lại | `RUN dotnet restore` là một layer. Nhờ cache mà sửa `.cs` vẫn không phải tải lại NuGet |
| **Registry** | **Kho lưu image** trên mạng, để `pull` (tải về) và `push` (đẩy lên) | NuGet cho package, GitHub cho source code | `mcr.microsoft.com` (Microsoft Container Registry) là nơi tải image `dotnet/sdk` và `dotnet/aspnet`. Docker Hub là registry công khai phổ biến nhất |

**Một câu tóm tắt:** Dockerfile là công thức → `docker build` xếp các **layer** thành **image** → `docker run` tạo **container** từ image → `docker push`/`pull` đưa image lên hoặc xuống **registry**.

---

## 2. Yêu cầu của đề nằm ở đâu

| Yêu cầu | Nằm ở đâu | Dùng để làm gì |
|---|---|---|
| Dockerfile 2 stage | [`Dockerfile.linux`](../../../Dockerfile.linux) / [`Dockerfile`](../../../Dockerfile) (Windows): `FROM … sdk:9.0 AS build` và `FROM … aspnet:9.0 AS runtime` | Stage 1 có đủ công cụ để **biên dịch**. Stage 2 chỉ có runtime để **chạy**. Image cuối chỉ là stage 2, nên nhỏ hơn |
| Copy `.csproj` + `dotnet restore` trước | `Dockerfile`: phần ① | Tách bước tải package thành một layer riêng, chỉ chạy lại khi `.csproj` đổi |
| `.dockerignore` | [`.dockerignore`](../../../.dockerignore) | Không gửi `bin/`, `obj/`, `.git`, `.env` vào build context. Context nhẹ hơn, không lẫn file build của Windows, không lộ bí mật |
| Log kích thước image | Mục 6 của file này | Chứng minh image runtime nhỏ hơn image SDK |
| Restore hiện CACHED | Mục 5.4 | Chứng minh layer cache hoạt động |

---

## 3. Đọc hiểu Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build      # (a)
WORKDIR /src                                         # (b)
COPY global.json ./                                  # (c) ①
COPY TestManager.Api/TestManager.Api.csproj TestManager.Api/
RUN dotnet restore TestManager.Api/TestManager.Api.csproj   # (d)
COPY . .                                             # (e) ②
RUN dotnet publish ... -c Release -o /app/publish --no-restore -p:UseAppHost=false   # (f)

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime # (g)
WORKDIR /app
COPY --from=build /app/publish .                     # (h)
EXPOSE 8080                                          # (i)
USER $APP_UID                                        # (j)
ENTRYPOINT ["dotnet", "TestManager.Api.dll"]         # (k)
```

- **(a)** Bắt đầu từ image SDK, đặt tên stage là `build` để stage sau tham chiếu.
- **(b)** Thư mục làm việc bên trong image, giống `cd`.
- **(c)** Chỉ copy `global.json` và `.csproj`. `global.json` ghim phiên bản SDK, `.csproj` liệt kê package NuGet (EF Core, AutoMapper, ClosedXML…).
- **(d)** Tải package. Layer này **chỉ chạy lại khi file ở (c) đổi**.
- **(e)** Lúc này mới copy toàn bộ source, đã được lọc bởi `.dockerignore`. Sửa file `.cs` chỉ làm layer này và các layer sau nó build lại.
- **(f)** Biên dịch ở chế độ Release vào `/app/publish`.
  - `--no-restore`: package đã có từ bước (d), không tải lại.
  - `UseAppHost=false`: không tạo file `.exe` của Windows, vì container chạy bằng lệnh `dotnet xxx.dll`.
- **(g)** Stage mới, **bắt đầu lại từ một image khác**. Mọi thứ của stage `build` (SDK, source, obj) **không** đi theo.
- **(h)** Chỉ lấy thư mục publish từ stage `build` sang.
- **(i)** Ghi chú rằng app lắng nghe cổng 8080. Từ .NET 8, image aspnet mặc định đặt `ASPNETCORE_HTTP_PORTS=8080`. `EXPOSE` không tự mở cổng ra máy host; cổng chỉ được mở khi chạy với `-p`.
- **(j)** Chạy app bằng user thường có sẵn trong image, không dùng root. Đây là thói quen bảo mật tốt.
- **(k)** Lệnh chạy khi container khởi động.

**Vì sao copy `.csproj` trước?** Docker kiểm tra cache từng lệnh, từ trên xuống. Lệnh `COPY` được coi là "không đổi" nếu file copy vào không đổi. Nếu viết `COPY . .` rồi mới `dotnet restore`, thì chỉ cần sửa một dòng `.cs` là bước restore phải chạy lại và tải lại toàn bộ NuGet. Mỗi lần build sẽ chậm thêm vài chục giây đến vài phút.

---

## 4. Đọc hiểu .dockerignore

`docker build -t teamboard-api .`: dấu `.` ở cuối là **build context**, tức thư mục được gửi cho Docker engine. `.dockerignore` lọc bớt thư mục đó trước khi gửi.

| Mục | Lý do loại |
|---|---|
| `**/bin/`, `**/obj/` | Kết quả build trên Windows. Nặng, sai hệ điều hành, và nếu lọt vào image sẽ đè lên kết quả restore ở bước (d) |
| `.git` | Lịch sử git có thể rất nặng, app không cần |
| `.env` | Có thể chứa mật khẩu. **Không bao giờ** được nằm trong image, vì ai có image cũng đọc được |
| `docs/`, `postman/`, `database/`, `*.http` | Không cần để chạy API |

---

## 5. Thực hành

Chạy tất cả lệnh trong thư mục `TestManager-AspNet` (thư mục có `Dockerfile`).

### 5.1 Build
```powershell
docker build -t teamboard-api .
```

### 5.2 Run
```powershell
docker run --rm -p 8080:8080 --name teamboard teamboard-api
```
- `-p 8080:8080`: cổng máy host → cổng trong container.
- `--rm`: tự xoá container khi dừng.

Mở <http://localhost:8080/swagger>.

- Các endpoint gọi DB **sẽ lỗi**. Đó là bình thường, vì trong container, `localhost` là chính container chứ không phải máy bạn. Kết nối SQL Server là bài tiếp theo, dùng docker-compose.
- Nhấn `Ctrl+C` để dừng.

### 5.3 Đo kích thước
```powershell
docker pull mcr.microsoft.com/dotnet/sdk:9.0
docker images --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}" | Select-String "dotnet|teamboard"
docker history teamboard-api      # xem từng layer và dung lượng
```

### 5.4 Kiểm tra cache
1. Sửa một file `.cs`, ví dụ thêm một comment vào `Program.cs`.
2. Build lại và lưu log:
   ```powershell
   # Linux containers (BuildKit)
   docker build --progress=plain -f Dockerfile.linux -t teamboard-api . 2>&1 | Tee-Object build-cache.log
   # Windows containers (builder cũ, không có --progress)
   docker build -t teamboard-api . 2>&1 | Tee-Object build-cache.log
   ```
3. Tìm dòng `RUN dotnet restore` trong log:
   - Linux phải thấy **`CACHED`**.
   - Windows phải thấy **`---> Using cache`**.

   Bước `COPY . .` và `dotnet publish` thì chạy lại.

---

## 6. Log nộp bài

> Đo thật ngày 21/10/2026 trên Docker Desktop 28.1.1, chế độ **Windows containers** (`Dockerfile`, bản Windows). Log đầy đủ: [`build-log.txt`](build-log.txt).

| Image | Kích thước |
|---|---|
| `mcr.microsoft.com/dotnet/sdk:9.0-nanoserver-ltsc2022` (build) | **1.28 GB** |
| `mcr.microsoft.com/dotnet/aspnet:9.0-nanoserver-ltsc2022` (runtime gốc) | **413 MB** |
| `teamboard-api` (runtime + app) | **443 MB** |

- Image cuối **nhỏ hơn image SDK khoảng 3 lần** (443 MB so với 1.28 GB).
- Phần app chỉ thêm khoảng 30 MB: layer `COPY --from=build /app/publish` nặng 29.8 MB.
- SDK, source code và thư mục `obj/` hoàn toàn không có trong image cuối.

| Lần build | Thời gian | Ghi chú |
|---|---|---|
| Lần 1 | ~8 phút | Tải image nền, restore NuGet, publish |
| Lần 2, sau khi sửa `Program.cs` | ~40 giây | `Step 5/13 : RUN dotnet restore` → `---> Using cache` |

### Tự đánh giá

- [x] `docker build -t teamboard-api .` thành công (`Successfully tagged teamboard-api:latest`)
- [x] `docker run -p 8080:8080 teamboard-api` khởi động được app:
  - Log báo `Now listening on: http://[::]:8080`.
  - `/swagger` trả 200.
  - `GET /api/subjects` trả 500 do không kết nối được SQL Server. Đề bài cho phép lỗi này.
- [x] Image runtime nhỏ hơn rõ rệt: 443 MB so với 1.28 GB
- [x] Sửa một file `.cs` rồi build lại: bước `dotnet restore` được lấy từ cache

> **CACHED hay Using cache?** Hai chữ này cùng một ý nghĩa, chỉ khác builder:
> - Linux containers dùng **BuildKit**, log in `CACHED`.
> - Windows containers dùng **builder cũ**, log in `---> Using cache`.
>
> Builder cũ cũng không có cờ `--progress=plain`.

---

## 7. Lỗi thường gặp

| Triệu chứng | Nguyên nhân | Cách sửa |
|---|---|---|
| `open //./pipe/dockerDesktopWindowsEngine` hoặc `cannot connect to the Docker daemon` | Docker Desktop chưa chạy, hoặc đang ở Windows containers | Làm lại mục 0 |
| `no matching manifest for windows/amd64` | Đang ở Windows containers | Chuyển sang Linux containers |
| `A compatible .NET SDK was not found` | `global.json` đòi SDK mà image không có | `global.json` dùng `rollForward: latestFeature` nên SDK 9.0 bất kỳ đều chạy được. Nếu vẫn lỗi, kiểm tra tag image `sdk:9.0` |
| `unknown flag: --progress` | Windows containers dùng builder cũ, không phải BuildKit | Bỏ cờ `--progress=plain`, chạy `docker build -t teamboard-api . 2>&1 \| Tee-Object build.log` |
| `port is already allocated` | Cổng 8080 đang bị chiếm | Chạy với `-p 8081:8080`, hoặc `docker ps` rồi `docker stop <id>` |
| Gọi API bị lỗi kết nối SQL | Trong container, `localhost` không phải máy host, và Windows Authentication không dùng được trong Linux container | Bình thường ở bài này. Bài sau: chạy SQL Server bằng container và đặt `ConnectionStrings__DefaultConnection` |
| Log cảnh báo *AutoMapper license* | AutoMapper ≥ 15 cần license khi chạy production | Không ảnh hưởng. Có thể truyền key: `-e AutoMapper__LicenseKey=...` |

### Câu hỏi tự kiểm tra

1. Image khác container ở điểm nào? Một image chạy được bao nhiêu container?
2. Vì sao image cuối không chứa .NET SDK, dù Dockerfile có `FROM …/sdk`?
3. Nếu đổi thứ tự thành `COPY . .` rồi mới `dotnet restore`, chuyện gì xảy ra khi sửa một file `.cs`?
4. Thêm package NuGet mới vào `.csproj` thì bước restore có CACHED không? Vì sao?
5. Vì sao `.env` phải nằm trong `.dockerignore`, dù app không đọc file đó?
6. `EXPOSE 8080` có tự mở cổng ra máy host không? Cái gì thực sự mở cổng?
7. Registry dùng để làm gì? Image `dotnet/aspnet` được tải từ registry nào?

<details>
<summary>Gợi ý đáp án</summary>

1. Image là gói chỉ đọc. Container là phiên chạy của image. Một image chạy được nhiều container.
2. Image cuối chỉ là stage cuối cùng (`runtime`). Stage `build` chỉ dùng tạm để biên dịch, sau đó bị bỏ.
3. Layer `COPY . .` đổi, nên mọi layer sau nó, gồm cả restore, phải chạy lại và tải lại toàn bộ NuGet.
4. Không. `.csproj` đổi, nên layer `COPY *.csproj` đổi, kéo theo restore chạy lại. Đây là hành vi đúng.
5. Mọi file trong image đều đọc được bởi bất kỳ ai có image (`docker save`, `docker run … cat`).
6. Không. `EXPOSE` chỉ là ghi chú. Cờ `-p host:container` khi `docker run` mới thật sự mở cổng.
7. Registry là kho lưu và phân phối image. Image `dotnet/aspnet` được tải từ `mcr.microsoft.com`.
</details>

---

## Bước tiếp theo

`docker-compose.yml` gồm 2 service, `api` và `sqlserver` (`mcr.microsoft.com/mssql/server`):
- API kết nối bằng `Server=sqlserver;User Id=sa;Password=...`, truyền qua biến môi trường `ConnectionStrings__DefaultConnection`.
- Có thể chuyển từ `init.sql` sang EF Core Migrations để container tự tạo schema.
