# Lab: app live trên App Runner

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 7, 31/10/2026 |
| **Thời lượng** | 3h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Đạt mốc tuần 2.

## 2. Yêu cầu

- App Runner service từ image ECR, port 8080, health check `/health`.
- Kết nối RDS qua VPC connector; security group RDS cho phép security group của connector.
- Connection string tạm qua biến môi trường (tuần 3 chuyển sang Secrets Manager).
- Cập nhật `docs/network.png`, mở PR `week-2`.

## 3. Output phải nộp

- URL public, PR tuần 2, chi phí ước tính mỗi ngày.

## 4. Tự đánh giá: đạt khi

- [ ] `<URL>/health` trả Healthy.
- [ ] Tạo board qua URL public, thấy dữ liệu trên RDS.
- [ ] App Runner báo Running, không restart liên tục.
- [ ] RDS vẫn không mở ra internet.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] App Runner service từ image ECR, port 8080, health check `/health`.
- [ ] Kết nối RDS qua VPC connector; security group RDS cho phép security group của connector.
- [ ] Connection string tạm qua biến môi trường (tuần 3 chuyển sang Secrets Manager).
- [ ] Cập nhật `docs/network.png`, mở PR `week-2`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [AWS App Runner](https://docs.aws.amazon.com/apprunner/latest/dg/what-is-apprunner.html)
- [App Runner + VPC](https://docs.aws.amazon.com/apprunner/latest/dg/network-vpc.html)
- [App Runner health check](https://docs.aws.amazon.com/apprunner/latest/dg/manage-configure-healthcheck.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
