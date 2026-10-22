# Đưa image lên ECR

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 6, 30/10/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Image TeamBoard nằm trên ECR, sẵn sàng deploy.

## 2. Yêu cầu

- Tạo repository ECR `teamboard-api`, bật scan on push.
- Đăng nhập Docker vào ECR bằng `aws ecr get-login-password`, tag theo version, push.
- Lifecycle policy giữ 10 image gần nhất.
- Máy Mac chip Apple: build với `--platform linux/amd64`.

## 3. Output phải nộp

- URI image trong log; số lỗ hổng High/Critical từ kết quả scan.

## 4. Tự đánh giá: đạt khi

- [ ] `aws ecr describe-images --repository-name teamboard-api` thấy image vừa push.
- [ ] Image có tag version, không chỉ `latest`.
- [ ] Đã đọc kết quả scan và ghi số liệu vào log.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Tạo repository ECR `teamboard-api`, bật scan on push.
- [ ] Đăng nhập Docker vào ECR bằng `aws ecr get-login-password`, tag theo version, push.
- [ ] Lifecycle policy giữ 10 image gần nhất.
- [ ] Máy Mac chip Apple: build với `--platform linux/amd64`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [Amazon ECR](https://docs.aws.amazon.com/AmazonECR/latest/userguide/what-is-ecr.html)
- [ECR image scanning](https://docs.aws.amazon.com/AmazonECR/latest/userguide/image-scanning.html)
- [ECR lifecycle policies](https://docs.aws.amazon.com/AmazonECR/latest/userguide/LifecyclePolicies.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
