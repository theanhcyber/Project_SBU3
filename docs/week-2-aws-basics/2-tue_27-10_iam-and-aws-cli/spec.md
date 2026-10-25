# IAM và AWS CLI

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 3, 27/10/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Dùng AWS CLI với quyền tối thiểu và đọc được một policy.

## 2. Yêu cầu

- Hiểu user, group, role, policy; cấu trúc `Effect`, `Action`, `Resource`, `Condition`.
- Cài AWS CLI v2, cấu hình profile `teamboard` theo cách sandbox cấp.
- Tự viết policy chỉ cho đọc object trong một bucket S3 cụ thể.

## 3. Output phải nộp

- `docs/iam-notes.md`: policy tự viết kèm giải thích từng dòng.

## 4. Tự đánh giá: đạt khi

- [ ] `aws sts get-caller-identity --profile teamboard` trả đúng account và user.
- [ ] Access key (nếu có) chỉ nằm trong `~/.aws`, không có trong repo.
- [ ] Policy không dùng `"*"` cho `Action`, `Resource` là ARN cụ thể.
- [ ] Giải thích được khác nhau giữa user và role.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Hiểu user, group, role, policy; cấu trúc `Effect`, `Action`, `Resource`, `Condition`.
- [ ] Cài AWS CLI v2, cấu hình profile `teamboard` theo cách sandbox cấp.
- [ ] Tự viết policy chỉ cho đọc object trong một bucket S3 cụ thể.

**Hiện trạng:** máy chưa cài AWS CLI, chưa có thư mục `%USERPROFILE%\.aws`.

**Câu hỏi cần chốt trước khi làm:**

1. Sandbox cấp credential cho CLI theo cách nào: access key của IAM user (`aws configure`) hay SSO (`aws configure sso`)? (đã hỏi ở bài 26/10)
2. Policy của sandbox có ép MFA cho CLI không? Nếu có, cần dùng `aws sts get-session-token`.
3. User có quyền tạo policy và bucket S3 không? Nếu không, chỉ kiểm tra policy bằng Access Analyzer và Policy Simulator.
4. Đặt `iam-notes.md` ở `docs/` (đúng spec) hay trong thư mục bài này?

**Áp dụng vào TestManager-AspNet:** chưa sửa code. Policy đọc S3 là bản nháp cho bài 04/11 (S3 và presigned URL). Cách nghĩ user và role là nền cho bài 03/11 (IAM role cho App Runner) và 11/11 (GitHub Actions dùng OIDC). Kiểm tra không có access key trong repo: `git grep -nE "AKIA|ASIA|aws_secret_access_key"`.

## 7. Kiến thức cần học và tài liệu

> Chi tiết khái niệm, bài tập policy, nguồn học, phân bổ thời gian và checklist tự kiểm tra: [learning.md](learning.md).

- [IAM policies](https://docs.aws.amazon.com/IAM/latest/UserGuide/access_policies.html)
- [Cài AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- [Ví dụ S3 policy](https://docs.aws.amazon.com/AmazonS3/latest/userguide/example-policies-s3.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
