# Monolith hay microservices, nhận đề capstone

| | |
|---|---|
| **Tuần** | 6: System design (23/11–1/12) |
| **Ngày** | Thứ 5, 26/11/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Viết được yêu cầu có số đo cho đề capstone.

## 2. Yêu cầu

- Đọc: API gateway, idempotency, retry với backoff, circuit breaker; khi nào chưa nên tách microservice.
- Đề: TeamBoard cho 100.000 người dùng, thông báo realtime, báo cáo.
- Ít nhất 5 yêu cầu chức năng và 4 yêu cầu phi chức năng.

## 3. Output phải nộp

- `docs/design/requirements.md`.

## 4. Tự đánh giá: đạt khi

- [ ] Mỗi yêu cầu phi chức năng có con số đo được (ví dụ p95 dưới 300 ms, sẵn sàng 99,9%).
- [ ] Ghi rõ điều nằm ngoài phạm vi.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Đọc: API gateway, idempotency, retry với backoff, circuit breaker; khi nào chưa nên tách microservice.
- [ ] Đề: TeamBoard cho 100.000 người dùng, thông báo realtime, báo cáo.
- [ ] Ít nhất 5 yêu cầu chức năng và 4 yêu cầu phi chức năng.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [microservices.io](https://microservices.io/)
- [Timeouts, retries, backoff (AWS Builders' Library)](https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
