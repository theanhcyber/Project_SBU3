# Kiến trúc tổng thể

| | |
|---|---|
| **Tuần** | 6: System design (23/11–1/12) |
| **Ngày** | Thứ 6, 27/11/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Có sơ đồ kiến trúc đáp ứng mọi yêu cầu hôm qua.

## 2. Yêu cầu

- Sơ đồ thành phần và dịch vụ AWS cho từng thành phần.
- Luồng dữ liệu cho: tạo task, nhận thông báo realtime.
- Bảng đối chiếu yêu cầu phi chức năng ↔ thành phần đáp ứng.

## 3. Output phải nộp

- `docs/design/architecture.md` + sơ đồ.

## 4. Tự đánh giá: đạt khi

- [ ] Không yêu cầu phi chức năng nào bị bỏ trống trong bảng đối chiếu.
- [ ] Mọi thành phần trong sơ đồ giải thích được vì sao cần.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Sơ đồ thành phần và dịch vụ AWS cho từng thành phần.
- [ ] Luồng dữ liệu cho: tạo task, nhận thông báo realtime.
- [ ] Bảng đối chiếu yêu cầu phi chức năng ↔ thành phần đáp ứng.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [AWS Architecture Center](https://aws.amazon.com/architecture/)
- [API Gateway WebSocket](https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-websocket-api.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
