# S3 và presigned URL

| | |
|---|---|
| **Tuần** | 3: AWS vận hành (2–8/11) |
| **Ngày** | Thứ 4, 4/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Task đính kèm được file, file lưu trên S3 private.

## 2. Yêu cầu

- Bucket private `teamboard-attachments-<tên>`, bật Block Public Access.
- `POST /api/tasks/{id}/attachments/upload-url` trả presigned PUT URL, hết hạn 5–15 phút.
- `GET /api/attachments/{id}/download-url` trả presigned URL tải về.
- Metadata lưu bảng `Attachments`; instance role chỉ có quyền trên bucket này.

## 3. Output phải nộp

- Code 2 endpoint, migration `Attachments`, lệnh curl test trong log.

## 4. Tự đánh giá: đạt khi

- [ ] Upload bằng `curl -X PUT --upload-file` thành công, tải về đúng file.
- [ ] URL hết hạn trả 403.
- [ ] Mở URL object không ký trả Access Denied.

## 5. Nâng cao (không bắt buộc)

- Giới hạn loại file và kích thước tối đa.

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Bucket private `teamboard-attachments-<tên>`, bật Block Public Access.
- [ ] `POST /api/tasks/{id}/attachments/upload-url` trả presigned PUT URL, hết hạn 5–15 phút.
- [ ] `GET /api/attachments/{id}/download-url` trả presigned URL tải về.
- [ ] Metadata lưu bảng `Attachments`; instance role chỉ có quyền trên bucket này.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [S3 presigned URL](https://docs.aws.amazon.com/AmazonS3/latest/userguide/using-presigned-url.html)
- [S3 Block Public Access](https://docs.aws.amazon.com/AmazonS3/latest/userguide/access-control-block-public-access.html)
- [AWS SDK for .NET](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/welcome.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
