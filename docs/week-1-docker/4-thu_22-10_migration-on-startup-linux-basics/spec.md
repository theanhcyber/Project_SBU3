# Migration khi khởi động, Linux tối thiểu

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 5, 22/10/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⏳ Hôm nay, chưa bắt đầu |

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

**Hiện trạng:** chưa làm.
- DB hiện được tạo bằng service `db-init`, chạy `database/init.sql` khi DB chưa tồn tại.
- Pipeline yêu cầu **app tự chạy EF Core migration khi khởi động**, kèm seed dữ liệu mẫu không trùng.
- Vì vậy bài này sẽ **thay thế** cách làm của bài 21/10.

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

- [EF Core: áp dụng migration](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
- [EF Core: data seeding](https://learn.microsoft.com/ef/core/modeling/data-seeding)
- [Connection resiliency (EnableRetryOnFailure)](https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency)
- [Compose startup order](https://docs.docker.com/compose/how-tos/startup-order/)
- [docker exec](https://docs.docker.com/reference/cli/docker/container/exec/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
