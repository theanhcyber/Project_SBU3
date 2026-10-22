# Dựng TeamBoard

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 2, 19/10/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ✅ Đã xong (còn điểm lệch so với pipeline, xem mục 6) |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Có Web API chạy local với CRUD cho Board và Task.

## 2. Yêu cầu

- ASP.NET Core Web API, project `src/TeamBoard.Api`.
- Entity `Board` (Id, Name, CreatedAt) và `TaskItem` (Id, BoardId, Title, Status, DueDate), quan hệ 1 Board – n Task.
- EF Core + SQL Server (LocalDB hoặc bản cài sẵn), migration `InitialCreate`.
- Endpoint `/api/boards`, `/api/boards/{id}/tasks`, `/api/tasks/{id}` đủ GET/POST/PUT/DELETE.
- Swagger bật ở Development.

## 3. Output phải nộp

- Repo `teamboard` trên GitHub, README có cách chạy.
- File `LEARNING_LOG.md` có mục đầu tiên.

## 4. Tự đánh giá: đạt khi

- [ ] `dotnet run` lên, `/swagger` thấy đủ endpoint.
- [ ] Tạo 1 board, 2 task; GET tasks của board trả đúng 2 task.
- [ ] Id không tồn tại trả 404; tạo task thiếu Title trả 400.
- [ ] Xoá board có task: xử lý nhất quán (xoá theo hoặc chặn), lý do ghi trong README.

## 5. Nâng cao (không bắt buộc)

- DTO tách khỏi entity, validation bằng FluentValidation.

## 6. Phân tích, hiện trạng và kế hoạch

**Hiện trạng:**
- Đã dựng `TeamBoard.Api` (thư mục `Project_SBU3/TeamBoard.Api`), gồm Board, TaskItem và Member.
- Sau đó chuyển sang làm dự án chính **TestManager-AspNet** (convert từ Spring Boot, 14 entity, 71 endpoint). Dự án này dùng thay cho TeamBoard ở các bài sau. Image vẫn đặt tag `teamboard-api` cho khớp đề.

**Đối chiếu yêu cầu (TeamBoard.Api):**

| Yêu cầu pipeline | Hiện trạng | Kết quả |
|---|---|---|
| Project `src/TeamBoard.Api` | Đang nằm ở `TeamBoard.Api/`, chưa có thư mục `src/` | ⚠️ Lệch đường dẫn |
| `Board` (Id, Name, CreatedAt) và `TaskItem` (Id, BoardId, Title, Status, DueDate), quan hệ 1–n | Có đủ, `Board` có thêm `Description` | ✅ |
| EF Core + SQL Server, migration `InitialCreate` | Bảng được tạo bằng **script SQL** (`database/init.sql`), **chưa có migration** | ❌ Thiếu |
| `/api/boards`, `/api/boards/{id}/tasks`, `/api/tasks/{id}` đủ GET/POST/PUT/DELETE | Có đủ | ✅ |
| Swagger **chỉ bật ở Development** | Đang bật ở mọi môi trường | ⚠️ |
| README có cách chạy, `LEARNING_LOG.md` | README chỉ có tiêu đề, **chưa có `LEARNING_LOG.md`** | ❌ Thiếu |
| Id không tồn tại trả 404, thiếu Title trả 400 | Có (`NotFound()`, `[Required]`) | ✅ |
| Xoá board có task: xử lý nhất quán, ghi lý do trong README | Có (`ON DELETE CASCADE`), nhưng **lý do chưa ghi trong README** | ⚠️ |
| *(nâng cao)* DTO tách entity, FluentValidation | Có DTO, đang dùng DataAnnotations | ◐ |

**Việc cần bù:**
1. Tạo migration `InitialCreate` bằng `dotnet ef migrations add InitialCreate`. Làm gộp với bài 22/10 (migration khi khởi động).
2. Viết README: cách chạy, lý do chọn xoá theo (cascade).
3. Tạo `LEARNING_LOG.md` ở thư mục gốc repo. Mỗi bài thêm một mục: đã làm gì, gặp lỗi gì, học được gì.
4. Bọc Swagger trong `if (app.Environment.IsDevelopment())`.

## 7. Kiến thức cần học và tài liệu

- [ASP.NET Core Web API](https://learn.microsoft.com/aspnet/core/web-api/)
- [EF Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [Swashbuckle / Swagger](https://learn.microsoft.com/aspnet/core/tutorials/getting-started-with-swashbuckle)
- [FluentValidation](https://docs.fluentvalidation.net/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
