# CDK: RDS, Secrets Manager, App Runner

| | |
|---|---|
| **Tuần** | 5: IaC (16–22/11) |
| **Ngày** | Thứ 4, 18/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Toàn bộ hạ tầng staging nằm trong code.

## 2. Yêu cầu

- RDS trong subnet private, mật khẩu do CDK sinh, lưu Secrets Manager.
- App Runner (L1 `CfnService` hoặc module alpha), VPC connector, instance role, tham chiếu secret.
- Stack output URL của service.

## 3. Output phải nộp

- Code stack hoàn chỉnh cho staging.

## 4. Tự đánh giá: đạt khi

- [ ] Code CDK không chứa mật khẩu.
- [ ] `cdk deploy` in ra URL, `/health` Healthy.
- [ ] Security group RDS chỉ cho phép VPC connector.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] RDS trong subnet private, mật khẩu do CDK sinh, lưu Secrets Manager.
- [ ] App Runner (L1 `CfnService` hoặc module alpha), VPC connector, instance role, tham chiếu secret.
- [ ] Stack output URL của service.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [CDK API reference (RDS, SecretsManager, AppRunner)](https://docs.aws.amazon.com/cdk/api/v2/)
- [App Runner + VPC](https://docs.aws.amazon.com/apprunner/latest/dg/network-vpc.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
