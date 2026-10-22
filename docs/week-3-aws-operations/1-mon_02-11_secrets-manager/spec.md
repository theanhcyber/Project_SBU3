# Secrets Manager

| | |
|---|---|
| **Tuần** | 3: AWS vận hành (2–8/11) |
| **Ngày** | Thứ 2, 2/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Thông tin kết nối DB nằm trong Secrets Manager thay vì cấu hình.

## 2. Yêu cầu

- Tạo secret `teamboard/staging/db` chứa connection string.
- Đọc secret bằng `aws secretsmanager get-secret-value`.
- Hiểu rotation và vì sao không copy giá trị secret ra chỗ khác.

## 3. Output phải nộp

- ARN của secret trong log (không ghi giá trị).

## 4. Tự đánh giá: đạt khi

- [ ] Đọc được secret bằng CLI với profile của mình.
- [ ] Log và repo không chứa giá trị secret.
- [ ] Giải thích được rotation trong 3 dòng.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Tạo secret `teamboard/staging/db` chứa connection string.
- [ ] Đọc secret bằng `aws secretsmanager get-secret-value`.
- [ ] Hiểu rotation và vì sao không copy giá trị secret ra chỗ khác.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [AWS Secrets Manager](https://docs.aws.amazon.com/secretsmanager/latest/userguide/intro.html)
- [Rotation](https://docs.aws.amazon.com/secretsmanager/latest/userguide/rotating-secrets.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
