# GitHub Actions → AWS qua OIDC

| | |
|---|---|
| **Tuần** | 4: CI/CD (9–15/11) |
| **Ngày** | Thứ 4, 11/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Pipeline gọi được AWS mà không lưu access key ở GitHub.

## 2. Yêu cầu

- IAM OIDC provider cho `token.actions.githubusercontent.com`.
- Role `github-actions-teamboard`, trust policy giới hạn đúng repo (điều kiện `sub`).
- Workflow có `permissions: id-token: write`, dùng `aws-actions/configure-aws-credentials`, chạy `aws sts get-caller-identity`.

## 3. Output phải nộp

- Trust policy trong `docs/iam-roles.md`, link lần chạy in ra role ARN.

## 4. Tự đánh giá: đạt khi

- [ ] Job in đúng ARN của role.
- [ ] GitHub Secrets không chứa AWS access key.
- [ ] Trust policy không cho repo khác.
- [ ] Giải thích được vì sao OIDC an toàn hơn lưu key.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] IAM OIDC provider cho `token.actions.githubusercontent.com`.
- [ ] Role `github-actions-teamboard`, trust policy giới hạn đúng repo (điều kiện `sub`).
- [ ] Workflow có `permissions: id-token: write`, dùng `aws-actions/configure-aws-credentials`, chạy `aws sts get-caller-identity`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [GitHub OIDC với AWS](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services)
- [aws-actions/configure-aws-credentials](https://github.com/aws-actions/configure-aws-credentials)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
