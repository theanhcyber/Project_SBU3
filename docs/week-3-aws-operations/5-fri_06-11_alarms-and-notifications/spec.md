# Alarm và thông báo

| | |
|---|---|
| **Tuần** | 3: AWS vận hành (2–8/11) |
| **Ngày** | Thứ 6, 6/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Nhận được email khi app lỗi nhiều.

## 2. Yêu cầu

- SNS topic gửi email, đã xác nhận đăng ký.
- Alarm trên metric lỗi 5xx của App Runner, vượt ngưỡng trong 5 phút thì báo.
- Endpoint `/api/debug/error` trả 500, chỉ bật ở staging.

## 3. Output phải nộp

- Ảnh email alarm; ngưỡng và lý do trong log.

## 4. Tự đánh giá: đạt khi

- [ ] Gọi endpoint lỗi liên tục: email alarm đến.
- [ ] Ngừng gọi: alarm tự về OK.
- [ ] Endpoint lỗi không tồn tại ở production.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] SNS topic gửi email, đã xác nhận đăng ký.
- [ ] Alarm trên metric lỗi 5xx của App Runner, vượt ngưỡng trong 5 phút thì báo.
- [ ] Endpoint `/api/debug/error` trả 500, chỉ bật ở staging.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [CloudWatch alarm gửi email](https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/AlarmThatSendsEmail.html)
- [Amazon SNS](https://docs.aws.amazon.com/sns/latest/dg/welcome.html)
- [App Runner metrics](https://docs.aws.amazon.com/apprunner/latest/dg/monitor-cw.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
