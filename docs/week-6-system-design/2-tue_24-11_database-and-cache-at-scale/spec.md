# Database và cache ở quy mô lớn

| | |
|---|---|
| **Tuần** | 6: System design (23/11–1/12) |
| **Ngày** | Thứ 3, 24/11/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Biết khi nào dùng index, read replica, chia dữ liệu và cache.

## 2. Yêu cầu

- Đề xuất 3 index cho truy vấn chính (task theo board, theo status, theo hạn).
- Execution plan trước/sau index trên DB local có ~100.000 task giả.
- Chiến lược cache cho 2 endpoint: TTL, xoá cache, xử lý khi cache sập.

## 3. Output phải nộp

- `docs/design/data.md` kèm ảnh execution plan.

## 4. Tự đánh giá: đạt khi

- [ ] Execution plan chuyển từ scan sang seek, có số đo thời gian.
- [ ] Nêu được vấn đề của read replica với dữ liệu vừa ghi và cách xử lý.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Đề xuất 3 index cho truy vấn chính (task theo board, theo status, theo hạn).
- [ ] Execution plan trước/sau index trên DB local có ~100.000 task giả.
- [ ] Chiến lược cache cho 2 endpoint: TTL, xoá cache, xử lý khi cache sập.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [SQL Server indexes](https://learn.microsoft.com/sql/relational-databases/indexes/indexes)
- [Execution plan](https://learn.microsoft.com/sql/relational-databases/performance/display-an-actual-execution-plan)
- [RDS read replicas](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_ReadRepl.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
