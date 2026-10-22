# Unit test và bảo vệ nhánh main

| | |
|---|---|
| **Tuần** | 4: CI/CD (9–15/11) |
| **Ngày** | Thứ 3, 10/11/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Có bộ test tối thiểu, không ai đẩy thẳng vào `main`.

## 2. Yêu cầu

- `tests/TeamBoard.Tests` dùng xUnit, 5–10 test cho phần nghiệp vụ.
- SQLite in-memory hoặc mock cho phần dữ liệu.
- Branch protection `main`: bắt buộc PR, CI pass, ít nhất 1 approval.

## 3. Output phải nộp

- Project test, ảnh cấu hình branch protection.

## 4. Tự đánh giá: đạt khi

- [ ] `dotnet test` pass ở local và CI.
- [ ] Sửa sai một điều kiện: ít nhất 1 test đỏ.
- [ ] `git push` thẳng lên `main` bị từ chối.

## 5. Nâng cao (không bắt buộc)

- Đo code coverage và hiện trong kết quả CI.

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] `tests/TeamBoard.Tests` dùng xUnit, 5–10 test cho phần nghiệp vụ.
- [ ] SQLite in-memory hoặc mock cho phần dữ liệu.
- [ ] Branch protection `main`: bắt buộc PR, CI pass, ít nhất 1 approval.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [Unit test với dotnet test + xUnit](https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test)
- [Testing EF Core](https://learn.microsoft.com/ef/core/testing/)
- [Protected branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
