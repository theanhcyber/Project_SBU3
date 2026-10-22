# CloudWatch Logs, structured logging

| | |
|---|---|
| **Tuần** | 3: AWS vận hành (2–8/11) |
| **Ngày** | Thứ 5, 5/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Đọc và truy vấn được log của app trên CloudWatch.

## 2. Yêu cầu

- Log dạng JSON ra console (Serilog hoặc JSON console logger).
- Mỗi request log method, path, status code, thời gian xử lý, trace id.
- 2 câu truy vấn Logs Insights: 5 endpoint chậm nhất; số lỗi 5xx theo giờ.

## 3. Output phải nộp

- `docs/observability.md` có 2 câu truy vấn và ảnh kết quả.

## 4. Tự đánh giá: đạt khi

- [ ] Log trên CloudWatch là JSON, lọc được theo `StatusCode`.
- [ ] 2 câu truy vấn chạy ra kết quả đúng.
- [ ] Log không chứa connection string, token hay nội dung file.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Log dạng JSON ra console (Serilog hoặc JSON console logger).
- [ ] Mỗi request log method, path, status code, thời gian xử lý, trace id.
- [ ] 2 câu truy vấn Logs Insights: 5 endpoint chậm nhất; số lỗi 5xx theo giờ.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [CloudWatch Logs Insights](https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/AnalyzingLogData.html)
- [.NET JSON console logging](https://learn.microsoft.com/dotnet/core/extensions/console-log-formatter)
- [Serilog](https://serilog.net/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
