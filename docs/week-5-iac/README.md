# Tuần 5: IaC (16–22/11)

**Mốc tuần:** Xoá staging rồi dựng lại trong 30 phút chỉ bằng code và pipeline, không thao tác console.

## Mục đích

Mô tả toàn bộ hạ tầng bằng code để dựng lại được bất cứ lúc nào, và tập xử lý sự cố giống như trên dự án thật.

## Sau tuần này, mỗi người sẽ

- Dùng AWS CDK viết bằng C#, hiểu quan hệ với CloudFormation.
- Viết stack cho ECR, S3, RDS, Secrets Manager, App Runner.
- Đưa thay đổi hạ tầng qua PR với `cdk diff` trong pipeline.
- Migration database tương thích ngược, tách khỏi lúc app khởi động.
- Điều tra sự cố bằng log/alarm và viết incident note không đổ lỗi.

## Cuối tuần: đào sâu thêm

_Làm sau khi lab thứ Bảy đã đạt mốc. Chọn 1–2 mục, ghi điều học được vào log._

- Viết lại một phần bằng Terraform và so sánh với CDK.
- Test cho CDK (assertions) và kiểm tra bảo mật bằng cdk-nag.
- Phát hiện drift giữa code và hạ tầng thật.
- Đọc chương Postmortem Culture trong Google SRE Book.

## Các bài trong tuần

| Ngày | Bài | Thời lượng | Trạng thái |
|---|---|---|---|
| Thứ 2 16/11 | [Làm quen AWS CDK](1-mon_16-11_intro-to-aws-cdk/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 3 17/11 | [CDK: ECR và S3](2-tue_17-11_cdk-ecr-and-s3/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 4 18/11 | [CDK: RDS, Secrets Manager, App Runner](3-wed_18-11_cdk-rds-secrets-manager-app-runner/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 5 19/11 | [Chạy CDK trong pipeline](4-thu_19-11_cdk-in-pipeline/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 6 20/11 | [Migration database an toàn](5-fri_20-11_safe-database-migrations/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 7 21/11 | [Lab: dựng lại môi trường, diễn tập sự cố](6-sat_21-11_lab-rebuild-environment-incident-drill/spec.md) | 3h | ⬜ Chưa bắt đầu |
| Chủ nhật 22/11 | [Review PR](7-sun_22-11_review-pr/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
