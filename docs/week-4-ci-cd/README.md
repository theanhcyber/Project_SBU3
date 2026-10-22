# Tuần 4: CI/CD (9–15/11)

**Mốc tuần:** Merge vào `main` là tự build, test, deploy staging; production có bước duyệt, rollback trong 10 phút.

## Mục đích

Tự động hoá con đường từ code đến production, để mỗi lần deploy là việc nhỏ, an toàn và lặp lại được.

## Sau tuần này, mỗi người sẽ

- Viết workflow GitHub Actions để build và test mỗi PR.
- Viết unit test và bảo vệ nhánh `main`.
- Kết nối GitHub với AWS qua OIDC, không lưu access key.
- Build, tag image theo commit SHA, tự deploy staging kèm smoke test.
- Quy trình duyệt production và rollback nhanh.

## Cuối tuần: đào sâu thêm

_Làm sau khi lab thứ Bảy đã đạt mốc. Chọn 1–2 mục, ghi điều học được vào log._

- Chiến lược deploy: blue/green, canary, feature flag.
- Reusable workflow và matrix build trong GitHub Actions.
- Bảo mật pipeline: Dependabot, quét secret, ký image.
- Đọc về 4 chỉ số DORA (sách Accelerate).

## Các bài trong tuần

| Ngày | Bài | Thời lượng | Trạng thái |
|---|---|---|---|
| Thứ 2 9/11 | [Workflow CI đầu tiên](1-mon_09-11_first-ci-workflow/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 3 10/11 | [Unit test và bảo vệ nhánh main](2-tue_10-11_unit-tests-and-main-branch-protection/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 4 11/11 | [GitHub Actions → AWS qua OIDC](3-wed_11-11_github-actions-to-aws-via-oidc/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 5 12/11 | [Build và push image trong pipeline](4-thu_12-11_build-and-push-image-in-pipeline/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 6 13/11 | [Tự động deploy staging](5-fri_13-11_auto-deploy-staging/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
| Thứ 7 14/11 | [Lab: duyệt production và rollback](6-sat_14-11_lab-production-approval-and-rollback/spec.md) | 3h | ⬜ Chưa bắt đầu |
| Chủ nhật 15/11 | [Review PR](7-sun_15-11_review-pr/spec.md) | 1,5h | ⬜ Chưa bắt đầu |
