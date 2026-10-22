# Workflow CI đầu tiên

| | |
|---|---|
| **Tuần** | 4: CI/CD (9–15/11) |
| **Ngày** | Thứ 2, 9/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Mỗi PR tự động được build và test.

## 2. Yêu cầu

- Hiểu workflow, job, step, runner, trigger.
- `.github/workflows/ci.yml` chạy khi `pull_request`: checkout, setup-dotnet, restore, build, test.
- Cache NuGet package.

## 3. Output phải nộp

- `ci.yml`, link một lần chạy thành công.

## 4. Tự đánh giá: đạt khi

- [ ] PR hiển thị check xanh.
- [ ] Cố ý làm code không build được: check chuyển đỏ.
- [ ] Lần chạy thứ 2 có cache hit, ghi thời gian 2 lần.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Hiểu workflow, job, step, runner, trigger.
- [ ] `.github/workflows/ci.yml` chạy khi `pull_request`: checkout, setup-dotnet, restore, build, test.
- [ ] Cache NuGet package.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [GitHub Actions](https://docs.github.com/en/actions)
- [actions/setup-dotnet](https://github.com/actions/setup-dotnet)
- [Caching dependencies](https://docs.github.com/en/actions/using-workflows/caching-dependencies-to-speed-up-workflows)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
