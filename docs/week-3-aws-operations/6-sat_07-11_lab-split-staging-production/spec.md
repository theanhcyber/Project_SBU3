# Lab: tách staging/production

| | |
|---|---|
| **Tuần** | 3: AWS vận hành (2–8/11) |
| **Ngày** | Thứ 7, 7/11/2026 |
| **Thời lượng** | 3h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Đạt mốc tuần 3.

## 2. Yêu cầu

- 2 service `teamboard-staging` và `teamboard-prod`, mỗi service một secret và một database riêng.
- Auto scaling: min 1, max 2–3, giới hạn concurrency; ghi lý do.
- Vẽ sơ đồ hạ tầng đầy đủ, mở PR `week-3`.

## 3. Output phải nộp

- `docs/architecture.png`, 2 URL, PR tuần 3.

## 4. Tự đánh giá: đạt khi

- [ ] Dữ liệu tạo ở staging không xuất hiện ở production.
- [ ] `git log -p` toàn repo không chứa mật khẩu hay access key.
- [ ] Alarm đã test ở staging; sơ đồ khớp hạ tầng thật.

## 5. Nâng cao (không bắt buộc)

- Chạy cùng image trên ECS Fargate và so sánh.

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] 2 service `teamboard-staging` và `teamboard-prod`, mỗi service một secret và một database riêng.
- [ ] Auto scaling: min 1, max 2–3, giới hạn concurrency; ghi lý do.
- [ ] Vẽ sơ đồ hạ tầng đầy đủ, mở PR `week-3`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [App Runner auto scaling](https://docs.aws.amazon.com/apprunner/latest/dg/manage-autoscaling.html)
- [ECS Fargate](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
