# Lộ trình học: Xoá mù chữ Cloud (Team SBU3, 19/10 – 1/12/2026)

Tài liệu chia theo **tuần**, rồi theo **ngày**. Mỗi ngày có `spec.md` của bài học: mục tiêu, yêu cầu, output, tiêu chí tự đánh giá (trích nguyên văn pipeline), kèm phân tích, kế hoạch và tài liệu tham khảo.

> **Dự án thực hành:** pipeline viết cho *TeamBoard*. Học viên dùng **TestManager-AspNet** (repo này) thay cho TeamBoard. Image vẫn đặt tag `teamboard-api`. `TeamBoard.Api` (bài 19/10) nằm ở thư mục cạnh repo này.

| Tuần | Chủ đề | Thời gian | Mốc tuần |
|---|---|---|---|
| 1 | [Docker](week-1-docker/README.md) | 19–25/10 | Người khác clone repo, chạy `docker compose up` là dùng được TeamBoard. |
| 2 | [AWS cơ bản](week-2-aws-basics/README.md) | 26/10–1/11 | TeamBoard chạy trên AWS với URL public, dữ liệu lưu trên RDS. |
| 3 | [AWS vận hành](week-3-aws-operations/README.md) | 2–8/11 | Không còn mật khẩu hay access key trong code và cấu hình; có alarm đã test; staging và production tách biệt. |
| 4 | [CI/CD](week-4-ci-cd/README.md) | 9–15/11 | Merge vào `main` là tự build, test, deploy staging; production có bước duyệt, rollback trong 10 phút. |
| 5 | [IaC](week-5-iac/README.md) | 16–22/11 | Xoá staging rồi dựng lại trong 30 phút chỉ bằng code và pipeline, không thao tác console. |
| 6 | [System design](week-6-system-design/README.md) | 23/11–1/12 | Tài liệu thiết kế 3–5 trang cho 100.000 người dùng, bảo vệ được ở demo day 30/11–1/12. |

## Cấu trúc thư mục

```
docs/
├── week-1-docker/
│   ├── README.md
│   ├── 1-mon_19-10_build-teamboard/spec.md
│   ├── 2-tue_20-10_dockerfile-multi-stage/spec.md
│   ├── 3-wed_21-10_config-health-check-compose/spec.md
│   ├── 4-thu_22-10_migration-on-startup-linux-basics/spec.md
│   ├── 5-fri_23-10_redis-cache/spec.md
│   ├── 6-sat_24-10_lab-make-it-runnable-by-others/spec.md
│   ├── 7-sun_25-10_review-pr/spec.md
├── week-2-aws-basics/
│   ├── README.md
│   ├── 1-mon_26-10_cloud-concepts-sandbox-login/spec.md
│   ├── 2-tue_27-10_iam-and-aws-cli/spec.md
│   ├── 3-wed_28-10_vpc-networking/spec.md
│   ├── 4-thu_29-10_database-on-rds/spec.md
│   ├── 5-fri_30-10_push-image-to-ecr/spec.md
│   ├── 6-sat_31-10_lab-app-live-on-app-runner/spec.md
│   ├── 7-sun_01-11_review-pr/spec.md
├── week-3-aws-operations/
│   ├── README.md
│   ├── 1-mon_02-11_secrets-manager/spec.md
│   ├── 2-tue_03-11_iam-role-for-app-runner/spec.md
│   ├── 3-wed_04-11_s3-and-presigned-url/spec.md
│   ├── 4-thu_05-11_cloudwatch-logs-structured-logging/spec.md
│   ├── 5-fri_06-11_alarms-and-notifications/spec.md
│   ├── 6-sat_07-11_lab-split-staging-production/spec.md
│   ├── 7-sun_08-11_review-pr/spec.md
├── week-4-ci-cd/
│   ├── README.md
│   ├── 1-mon_09-11_first-ci-workflow/spec.md
│   ├── 2-tue_10-11_unit-tests-and-main-branch-protection/spec.md
│   ├── 3-wed_11-11_github-actions-to-aws-via-oidc/spec.md
│   ├── 4-thu_12-11_build-and-push-image-in-pipeline/spec.md
│   ├── 5-fri_13-11_auto-deploy-staging/spec.md
│   ├── 6-sat_14-11_lab-production-approval-and-rollback/spec.md
│   ├── 7-sun_15-11_review-pr/spec.md
├── week-5-iac/
│   ├── README.md
│   ├── 1-mon_16-11_intro-to-aws-cdk/spec.md
│   ├── 2-tue_17-11_cdk-ecr-and-s3/spec.md
│   ├── 3-wed_18-11_cdk-rds-secrets-manager-app-runner/spec.md
│   ├── 4-thu_19-11_cdk-in-pipeline/spec.md
│   ├── 5-fri_20-11_safe-database-migrations/spec.md
│   ├── 6-sat_21-11_lab-rebuild-environment-incident-drill/spec.md
│   ├── 7-sun_22-11_review-pr/spec.md
├── week-6-system-design/
│   ├── README.md
│   ├── 1-mon_23-11_scalability-and-capacity-estimation/spec.md
│   ├── 2-tue_24-11_database-and-cache-at-scale/spec.md
│   ├── 3-wed_25-11_messaging-with-sqs-sns/spec.md
│   ├── 4-thu_26-11_monolith-vs-microservices-capstone-brief/spec.md
│   ├── 5-fri_27-11_overall-architecture/spec.md
│   ├── 6-sat_28-11_design-decisions-cost-final-doc/spec.md
│   ├── 7-sun_29-11_cross-review-design-docs/spec.md
```

## Ký hiệu trạng thái

✅ Đã xong · 🟡 Đang làm · ⏳ Hôm nay · ❗ Quá hạn · ⬜ Chưa bắt đầu
