# Dockerfile multi-stage

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 3, 20/10/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ✅ Đã xong (Windows containers) |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Build được image của API và chạy trong container.

## 2. Yêu cầu

- Giải thích bằng lời của mình: image, container, layer, registry.
- Dockerfile 2 stage: build bằng `dotnet/sdk`, chạy bằng `dotnet/aspnet`.
- Copy `.csproj` và `dotnet restore` trước khi copy toàn bộ source.
- `.dockerignore` loại `bin/`, `obj/`, `.git`, `.env`.

## 3. Output phải nộp

- `Dockerfile`, `.dockerignore`.
- Log ghi kích thước image SDK và image runtime.

## 4. Tự đánh giá: đạt khi

- [ ] `docker build -t teamboard-api .` thành công.
- [ ] `docker run -p 8080:8080 teamboard-api` khởi động được app (endpoint cần DB lỗi là bình thường).
- [ ] Image runtime nhỏ hơn rõ rệt, có số MB trong log.
- [ ] Sửa 1 file `.cs` rồi build lại: bước restore hiện `CACHED`.

## 5. Nâng cao (không bắt buộc)

- Chạy container bằng user không phải root; thử image chiseled/alpine và so kích thước.

## 6. Phân tích, hiện trạng và kế hoạch

**Hiện trạng:** đã xong trên Docker Desktop chế độ **Windows containers** (image Nano Server). Bản Linux (`Dockerfile.linux`) đã viết sẵn, chưa build vì Docker chưa chạy được ở chế độ Linux.

| Tiêu chí | Kết quả |
|---|---|
| `docker build -t teamboard-api .` | ✅ `Successfully tagged teamboard-api:latest` |
| `docker run -p 8080:8080 teamboard-api` | ✅ Log báo `Now listening on: http://[::]:8080`, `/swagger` trả 200, API cần DB trả 500 |
| Image runtime nhỏ hơn rõ rệt | ✅ SDK **1.28 GB**, aspnet 413 MB, `teamboard-api` **443 MB** |
| Restore được lấy từ cache | ✅ `Step 5/13 : RUN dotnet restore ---> Using cache`. Builder cũ của Windows in `Using cache` thay cho chữ `CACHED`. Build lại chỉ 40 giây, lần đầu khoảng 8 phút |

**File liên quan:**
- `Dockerfile` (Windows), `Dockerfile.linux`, `.dockerignore`.
- [lesson.md](lesson.md): giáo án image, container, layer, registry và giải thích Dockerfile từng dòng.
- [build-log.txt](build-log.txt): log nộp bài (kích thước image, 2 lần build).

**Nâng cao:**
- `Dockerfile.linux` đã có `USER $APP_UID` để chạy bằng user không phải root, nhưng chưa build.
- Chưa thử image chiseled hoặc alpine.

## 7. Kiến thức cần học và tài liệu

- [Docker: build best practices](https://docs.docker.com/build/building/best-practices/)
- [Multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
- [.NET container images](https://learn.microsoft.com/dotnet/core/docker/introduction)
- [.dockerignore](https://docs.docker.com/build/concepts/context/#dockerignore-files)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
