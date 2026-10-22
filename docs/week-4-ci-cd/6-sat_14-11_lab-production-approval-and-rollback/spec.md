# Lab: duyệt production và rollback

| | |
|---|---|
| **Tuần** | 4: CI/CD (9–15/11) |
| **Ngày** | Thứ 7, 14/11/2026 |
| **Thời lượng** | 3h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Đạt mốc tuần 4.

## 2. Yêu cầu

- GitHub Environment `production` có required reviewer.
- Workflow rollback (`workflow_dispatch`) nhận tag SHA và deploy lại image đó.
- Diễn tập: deploy bản lỗi, phát hiện, rollback, bấm giờ.
- Mở PR `week-4`.

## 3. Output phải nộp

- PR tuần 4, link pipeline có bước duyệt, thời gian rollback.

## 4. Tự đánh giá: đạt khi

- [ ] Production không đổi khi chưa ai duyệt.
- [ ] Rollback xong trong 10 phút, `/version` trả SHA cũ.
- [ ] Không thao tác trên console AWS.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] GitHub Environment `production` có required reviewer.
- [ ] Workflow rollback (`workflow_dispatch`) nhận tag SHA và deploy lại image đó.
- [ ] Diễn tập: deploy bản lỗi, phát hiện, rollback, bấm giờ.
- [ ] Mở PR `week-4`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [Environments: required reviewers](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)
- [workflow_dispatch](https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows#workflow_dispatch)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
