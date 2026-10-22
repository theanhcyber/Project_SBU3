# Tự động deploy staging

| | |
|---|---|
| **Tuần** | 4: CI/CD (9–15/11) |
| **Ngày** | Thứ 6, 13/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Merge là staging chạy code mới, không thao tác tay.

## 2. Yêu cầu

- Job deploy cập nhật image của `teamboard-staging` và chờ deploy xong.
- Smoke test `/health` và `/version`; sai SHA hoặc không Healthy thì job fail.

## 3. Output phải nộp

- Link một lần pipeline chạy trọn từ merge đến staging.

## 4. Tự đánh giá: đạt khi

- [ ] Merge PR, trong 15 phút `/version` staging trả SHA mới.
- [ ] Cố ý làm `/health` lỗi: pipeline đỏ ở bước smoke test.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Job deploy cập nhật image của `teamboard-staging` và chờ deploy xong.
- [ ] Smoke test `/health` và `/version`; sai SHA hoặc không Healthy thì job fail.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [aws apprunner start-deployment](https://docs.aws.amazon.com/cli/latest/reference/apprunner/start-deployment.html)
- [GitHub environments](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
