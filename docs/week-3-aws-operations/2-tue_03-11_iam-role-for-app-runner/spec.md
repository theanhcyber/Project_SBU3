# IAM role cho App Runner

| | |
|---|---|
| **Tuần** | 3: AWS vận hành (2–8/11) |
| **Ngày** | Thứ 3, 3/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

App lấy secret bằng role, không cần access key nào.

## 2. Yêu cầu

- Instance role có `secretsmanager:GetSecretValue` chỉ trên ARN của secret.
- Biến `ConnectionStrings__Default` của App Runner tham chiếu tới secret.
- Xoá mọi connection string dạng chữ thường còn sót.

## 3. Output phải nộp

- `docs/iam-roles.md`: role, policy, lý do từng quyền.

## 4. Tự đánh giá: đạt khi

- [ ] Cấu hình App Runner không có mật khẩu dạng chữ thường.
- [ ] Đổi mật khẩu trong secret và RDS, deploy lại: app vẫn chạy.
- [ ] `Resource` của policy là ARN cụ thể.
- [ ] Tìm `AKIA` trong repo không ra kết quả.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Instance role có `secretsmanager:GetSecretValue` chỉ trên ARN của secret.
- [ ] Biến `ConnectionStrings__Default` của App Runner tham chiếu tới secret.
- [ ] Xoá mọi connection string dạng chữ thường còn sót.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [App Runner + IAM](https://docs.aws.amazon.com/apprunner/latest/dg/security_iam_service-with-iam.html)
- [App Runner env & secrets](https://docs.aws.amazon.com/apprunner/latest/dg/env-variable.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
