# Khái niệm cloud, đăng nhập sandbox

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 2, 26/10/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Vào được tài khoản sandbox an toàn và hiểu khái niệm nền.

## 2. Yêu cầu

- Đăng nhập console, bật MFA cho user của mình.
- Xem AWS Budgets: ngưỡng cảnh báo và người nhận.
- Phân biệt region, Availability Zone, IaaS / PaaS / SaaS.

## 3. Output phải nộp

- Log: region team dùng và lý do; bảng 2 dịch vụ AWS cho mỗi loại IaaS, PaaS, SaaS.

## 4. Tự đánh giá: đạt khi

- [ ] Trang IAM của user mình hiện MFA đã gán.
- [ ] Giải thích được vì sao hệ thống chạy trên 2 AZ vẫn sống khi 1 AZ sự cố.
- [ ] Biết cảnh báo chi phí gửi tới ai, ở ngưỡng bao nhiêu.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Đăng nhập console, bật MFA cho user của mình.
- [ ] Xem AWS Budgets: ngưỡng cảnh báo và người nhận.
- [ ] Phân biệt region, Availability Zone, IaaS / PaaS / SaaS.

**Câu hỏi cần chốt trước khi làm** (hỏi team/mentor):

1. Sandbox cấp quyền theo cách nào: IAM user (sign-in URL + mật khẩu tạm) hay IAM Identity Center (access portal)? Câu trả lời quyết định cách bật MFA hôm nay và cách cấu hình CLI ngày 27/10.
2. Team dùng region nào? SCP có chặn region khác không? Region đó có App Runner không (cần cho bài 31/10)?
3. User của mình có quyền xem Billing/Budgets không? Nếu không, ai cung cấp thông tin ngưỡng và người nhận cảnh báo?
4. Có quy ước tag bắt buộc (ví dụ `Owner`, `Project`) khi tạo tài nguyên không?

**Áp dụng vào TestManager-AspNet:** bài này chưa sửa code. Phần liên quan là kế hoạch chuyển kiến trúc tuần 1 lên AWS:

| Tuần 1 (Docker Compose) | Tuần 2 (AWS) | Loại dịch vụ |
|---|---|---|
| Container `db` (SQL Server) | RDS SQL Server (29/10) | Managed database (PaaS) |
| Container `api` | App Runner (31/10), image lấy từ ECR (30/10) | PaaS |
| `EnableRetryOnFailure()` trong `Program.cs` | Giúp app vượt qua lúc RDS Multi-AZ failover | |

## 7. Kiến thức cần học và tài liệu

> Chi tiết khái niệm, nguồn học, phân bổ thời gian và checklist tự kiểm tra: [learning.md](learning.md).

- [AWS global infrastructure](https://aws.amazon.com/about-aws/global-infrastructure/)
- [IAM: MFA](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_mfa.html)
- [AWS Budgets](https://docs.aws.amazon.com/cost-management/latest/userguide/budgets-managing-costs.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
