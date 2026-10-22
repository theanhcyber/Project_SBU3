# Migration database an toàn

| | |
|---|---|
| **Tuần** | 5: IaC (16–22/11) |
| **Ngày** | Thứ 6, 20/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Migration chạy thành bước riêng, không chạy lúc app khởi động ở staging/production.

## 2. Yêu cầu

- `dotnet ef migrations bundle`, chạy trong pipeline trước deploy app.
- Tắt tự chạy migration khi start ngoài Development.
- Thêm cột `Priority` theo kiểu tương thích ngược.

## 3. Output phải nộp

- Bước migration trong pipeline; log giải thích cách xử lý khi migration lỗi giữa chừng.

## 4. Tự đánh giá: đạt khi

- [ ] Log khởi động staging không còn dòng chạy migration.
- [ ] Bản app cũ vẫn chạy được với DB mới trong lúc chuyển.
- [ ] Nêu được 1 ví dụ migration không rollback được và cách xử lý.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] `dotnet ef migrations bundle`, chạy trong pipeline trước deploy app.
- [ ] Tắt tự chạy migration khi start ngoài Development.
- [ ] Thêm cột `Priority` theo kiểu tương thích ngược.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [EF Core migration bundles](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying#bundles)
- [Expand–contract (parallel change)](https://martinfowler.com/bliki/ParallelChange.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
