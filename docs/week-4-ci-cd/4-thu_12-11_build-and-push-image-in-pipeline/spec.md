# Build và push image trong pipeline

| | |
|---|---|
| **Tuần** | 4: CI/CD (9–15/11) |
| **Ngày** | Thứ 5, 12/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Mỗi commit trên `main` có một image tương ứng trên ECR.

## 2. Yêu cầu

- `deploy.yml` chạy khi push `main`: build, tag commit SHA, push ECR.
- Endpoint `/version` trả commit SHA.
- Role pipeline chỉ được push vào repo `teamboard-api`.

## 3. Output phải nộp

- `deploy.yml`, danh sách image có tag SHA.

## 4. Tự đánh giá: đạt khi

- [ ] 2 commit liên tiếp tạo 2 image với 2 tag SHA khác nhau.
- [ ] Test fail thì không push image.
- [ ] Chạy image local, `/version` trả đúng SHA.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] `deploy.yml` chạy khi push `main`: build, tag commit SHA, push ECR.
- [ ] Endpoint `/version` trả commit SHA.
- [ ] Role pipeline chỉ được push vào repo `teamboard-api`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [aws-actions/amazon-ecr-login](https://github.com/aws-actions/amazon-ecr-login)
- [docker/build-push-action](https://github.com/docker/build-push-action)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
