# CDK: ECR và S3

| | |
|---|---|
| **Tuần** | 5: IaC (16–22/11) |
| **Ngày** | Thứ 3, 17/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

ECR và bucket đính kèm được quản lý bằng code.

## 2. Yêu cầu

- Stack `TeamBoardStaging`: ECR (scan, lifecycle 10 image) và S3 (Block Public Access, mã hoá, xoá được khi destroy).
- Tag `Project=TeamBoard`, `Env=staging` cho mọi tài nguyên.
- Tên tài nguyên đọc từ cấu hình theo môi trường.

## 3. Output phải nộp

- Code stack, kết quả `cdk diff` trong log.

## 4. Tự đánh giá: đạt khi

- [ ] Mọi tài nguyên có đủ tag.
- [ ] Deploy xong chạy lại `cdk diff`: không còn thay đổi.
- [ ] Bucket không truy cập công khai được.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Stack `TeamBoardStaging`: ECR (scan, lifecycle 10 image) và S3 (Block Public Access, mã hoá, xoá được khi destroy).
- [ ] Tag `Project=TeamBoard`, `Env=staging` cho mọi tài nguyên.
- [ ] Tên tài nguyên đọc từ cấu hình theo môi trường.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [CDK API reference](https://docs.aws.amazon.com/cdk/api/v2/)
- [CDK tagging](https://docs.aws.amazon.com/cdk/v2/guide/tagging.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
