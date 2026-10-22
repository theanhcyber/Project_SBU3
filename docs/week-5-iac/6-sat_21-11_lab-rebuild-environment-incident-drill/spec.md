# Lab: dựng lại môi trường, diễn tập sự cố

| | |
|---|---|
| **Tuần** | 5: IaC (16–22/11) |
| **Ngày** | Thứ 7, 21/11/2026 |
| **Thời lượng** | 3h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Đạt mốc tuần 5.

## 2. Yêu cầu

- `cdk destroy` staging rồi dựng lại chỉ bằng pipeline/lệnh, bấm giờ.
- Diễn tập sự cố theo kịch bản mentor giao, chỉ dùng log, alarm, pipeline.
- Incident note: timeline, dấu hiệu, nguyên nhân, cách sửa, phòng ngừa.
- Mở PR `week-5`.

## 3. Output phải nộp

- `docs/incident-<ngày>.md`, thời gian dựng lại, PR tuần 5.

## 4. Tự đánh giá: đạt khi

- [ ] Staging dựng lại trong 30 phút, `/health` Healthy, không thao tác console.
- [ ] Tìm ra nguyên nhân bằng log hoặc alarm, không phải đoán.
- [ ] Incident note không đổ lỗi, có ít nhất 1 hành động phòng ngừa cụ thể.

## 5. Nâng cao (không bắt buộc)

- Viết lại một phần hạ tầng bằng Terraform và so sánh.

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] `cdk destroy` staging rồi dựng lại chỉ bằng pipeline/lệnh, bấm giờ.
- [ ] Diễn tập sự cố theo kịch bản mentor giao, chỉ dùng log, alarm, pipeline.
- [ ] Incident note: timeline, dấu hiệu, nguyên nhân, cách sửa, phòng ngừa.
- [ ] Mở PR `week-5`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [Google SRE: Postmortem Culture](https://sre.google/sre-book/postmortem-culture/)
- [Terraform docs](https://developer.hashicorp.com/terraform/docs)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
