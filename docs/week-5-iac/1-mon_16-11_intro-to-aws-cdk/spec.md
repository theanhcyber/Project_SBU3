# Làm quen AWS CDK

| | |
|---|---|
| **Tuần** | 5: IaC (16–22/11) |
| **Ngày** | Thứ 2, 16/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Deploy và xoá được một stack đơn giản bằng CDK viết bằng C#.

## 2. Yêu cầu

- Cài Node.js và CDK CLI (`npm install -g aws-cdk`).
- Trong `infra/`: `cdk init app --language csharp`, `cdk bootstrap` nếu cần.
- Stack thử 1 bucket S3: chạy `cdk synth`, `cdk diff`, `cdk deploy`, `cdk destroy`.

## 3. Output phải nộp

- Project `infra/`, log ghi tác dụng 4 lệnh.

## 4. Tự đánh giá: đạt khi

- [ ] Thấy stack trong CloudFormation sau deploy, mất sau destroy.
- [ ] `cdk diff` hiện thay đổi trước khi deploy.
- [ ] Giải thích được `cdk synth` sinh ra gì.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Cài Node.js và CDK CLI (`npm install -g aws-cdk`).
- [ ] Trong `infra/`: `cdk init app --language csharp`, `cdk bootstrap` nếu cần.
- [ ] Stack thử 1 bucket S3: chạy `cdk synth`, `cdk diff`, `cdk deploy`, `cdk destroy`.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [AWS CDK v2 guide](https://docs.aws.amazon.com/cdk/v2/guide/home.html)
- [CDK với C#](https://docs.aws.amazon.com/cdk/v2/guide/work-with-cdk-csharp.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
