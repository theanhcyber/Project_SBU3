# Chạy CDK trong pipeline

| | |
|---|---|
| **Tuần** | 5: IaC (16–22/11) |
| **Ngày** | Thứ 5, 19/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Mọi thay đổi hạ tầng đi qua PR và pipeline.

## 2. Yêu cầu

- Job `infra` chạy `cdk diff` trên PR có thay đổi trong `infra/`.
- Merge vào `main`: `cdk deploy` staging trước bước deploy app.
- Role OIDC riêng cho infra.

## 3. Output phải nộp

- Workflow cập nhật, link lần chạy có `cdk diff` trên PR.

## 4. Tự đánh giá: đạt khi

- [ ] Đổi 1 thuộc tính qua PR: thấy diff ở PR, merge xong hạ tầng đổi đúng.
- [ ] Role infra và role app là 2 role khác nhau.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Job `infra` chạy `cdk diff` trên PR có thay đổi trong `infra/`.
- [ ] Merge vào `main`: `cdk deploy` staging trước bước deploy app.
- [ ] Role OIDC riêng cho infra.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [CDK CLI (diff, deploy)](https://docs.aws.amazon.com/cdk/v2/guide/cli.html)
- [GitHub Actions: paths filter](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#onpushpull_requestpull_request_targetpathspaths-ignore)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
