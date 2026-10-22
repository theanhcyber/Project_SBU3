# Tuần 3: AWS vận hành (2–8/11)

**Mốc tuần:** Không còn mật khẩu hay access key trong code và cấu hình; có alarm đã test; staging và production tách biệt.

## Mục đích

Biến app “chạy được” thành app “vận hành được”: không lộ secret, lưu file đúng cách, nhìn thấy lỗi và được báo ngay khi có sự cố.

## Sau tuần này, mỗi người sẽ

- Quản lý secret bằng Secrets Manager; app dùng IAM role thay cho access key.
- Lưu file trên S3 private và cấp quyền tạm bằng presigned URL.
- Structured logging và truy vấn log bằng CloudWatch Logs Insights.
- Dựng alarm + SNS, chọn ngưỡng cảnh báo có lý do.
- Tách staging/production và cấu hình auto scaling.

## Cuối tuần: đào sâu thêm

_Làm sau khi lab thứ Bảy đã đạt mốc. Chọn 1–2 mục, ghi điều học được vào log._

- Tracing phân tán với OpenTelemetry / AWS X-Ray.
- Chạy thử trên ECS Fargate và so sánh với App Runner.
- S3 nâng cao: versioning, lifecycle, CloudFront.
- Đọc về SLI/SLO trong Google SRE Book (chương Service Level Objectives).

## Các bài trong tuần

| Ngày | Bài | Thời lượng | Trạng thái |
|---|---|---|---|
| Thứ 2 2/11 | [Secrets Manager](1-mon_02-11_secrets-manager/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 3 3/11 | [IAM role cho App Runner](2-tue_03-11_iam-role-for-app-runner/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 4 4/11 | [S3 và presigned URL](3-wed_04-11_s3-and-presigned-url/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 5 5/11 | [CloudWatch Logs, structured logging](4-thu_05-11_cloudwatch-logs-structured-logging/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 6 6/11 | [Alarm và thông báo](5-fri_06-11_alarms-and-notifications/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 7 7/11 | [Lab: tách staging/production](6-sat_07-11_lab-split-staging-production/spec.md) | 3h | ⬜ Chưa bắt đầu |
| Chủ nhật 8/11 | [Review PR](7-sun_08-11_review-pr/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
