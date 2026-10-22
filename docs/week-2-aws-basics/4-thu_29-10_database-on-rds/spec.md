# Database trên RDS

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 5, 29/10/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

TeamBoard có database trên RDS.

## 2. Yêu cầu

- Tạo RDS SQL Server Express, instance nhỏ nhất sandbox cho phép, trong subnet private.
- Kết nối từ máy mình theo hướng dẫn của mentor (bastion hoặc cách sandbox hỗ trợ).
- Chạy migration từ local vào RDS.
- Stop instance khi xong (RDS stop sẽ tự bật lại sau 7 ngày).

## 3. Output phải nộp

- Log: endpoint RDS (không ghi mật khẩu), ảnh rule security group, chi phí ước tính mỗi giờ.

## 4. Tự đánh giá: đạt khi

- [ ] Kết nối được bằng SSMS hoặc Azure Data Studio, thấy bảng `Boards` và `TaskItems`.
- [ ] Security group không có rule `0.0.0.0/0`.
- [ ] Instance ở trạng thái Stopped sau buổi học.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Tạo RDS SQL Server Express, instance nhỏ nhất sandbox cho phép, trong subnet private.
- [ ] Kết nối từ máy mình theo hướng dẫn của mentor (bastion hoặc cách sandbox hỗ trợ).
- [ ] Chạy migration từ local vào RDS.
- [ ] Stop instance khi xong (RDS stop sẽ tự bật lại sau 7 ngày).

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [RDS for SQL Server](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_SQLServer.html)
- [Stop RDS instance](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/USER_StopInstance.html)
- [EF Core Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
