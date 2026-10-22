# Messaging với SQS/SNS

| | |
|---|---|
| **Tuần** | 6: System design (23/11–1/12) |
| **Ngày** | Thứ 4, 25/11/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Xử lý bất đồng bộ qua queue, chịu được lỗi và message trùng.

## 2. Yêu cầu

- Thêm `AssigneeEmail` cho task; giao task thì API gửi message vào SQS.
- Worker `BackgroundService` (chạy local) đọc message, ghi log “đã gửi thông báo”.
- DLQ sau 3 lần lỗi; xử lý idempotent.

## 3. Output phải nộp

- Code worker, kịch bản test và kết quả trong log.

## 4. Tự đánh giá: đạt khi

- [ ] Tắt worker, gửi 10 message, bật lại: xử lý đủ 10.
- [ ] Message lỗi nằm trong DLQ sau 3 lần thử.
- [ ] Gửi lặp cùng message: chỉ 1 thông báo.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Thêm `AssigneeEmail` cho task; giao task thì API gửi message vào SQS.
- [ ] Worker `BackgroundService` (chạy local) đọc message, ghi log “đã gửi thông báo”.
- [ ] DLQ sau 3 lần lỗi; xử lý idempotent.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [Amazon SQS](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/welcome.html)
- [SQS dead-letter queues](https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-dead-letter-queues.html)
- [BackgroundService](https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
