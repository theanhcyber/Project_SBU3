# Lab: hoàn thiện để người khác chạy được

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 7, 24/10/2026 |
| **Thời lượng** | 5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Đạt mốc tuần 1.

## 2. Yêu cầu

- README đủ: phần mềm cần cài, các bước chạy, biến môi trường, sơ đồ các container.
- Nhờ 1 người trong team clone và chạy thử, sửa mọi lỗi phát sinh.
- Dọn code, mở PR `week-1` → `main`.

## 3. Output phải nộp

- PR tuần 1: đã làm gì, ảnh `/health` và Swagger, vấn đề đã gặp.

## 4. Tự đánh giá: đạt khi

- [ ] Người khác chạy `cp .env.example .env` rồi `docker compose up`, dùng được app trong 10 phút, không phải hỏi.
- [ ] Lịch sử git không chứa mật khẩu thật.
- [ ] Tiêu chí bắt buộc của T2–T6 đều Xanh.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Hiện trạng:** chưa làm. Bài này gom các bài 20–23/10 lại thành mốc tuần 1.

**Checklist trước khi nhờ người khác chạy thử:**
- [ ] README có đủ: phần mềm cần cài (Docker Desktop **chế độ Linux**, .NET 9 SDK nếu chạy trên máy), các bước `cp .env.example .env` rồi `docker compose up`, bảng biến môi trường, sơ đồ container (`api`, `db`, `redis`).
- [ ] Tiêu chí bắt buộc của các bài 20–23/10 đều Xanh (xem `spec.md` từng bài).
- [ ] Kiểm tra lịch sử git không chứa mật khẩu: `git log -p | Select-String "MSSQL_SA_PASSWORD="` chỉ được thấy giá trị mẫu. Nâng cao hơn thì dùng `gitleaks detect`.
- [ ] Dọn code: bỏ file log tạm (`build-*.log`, `docker-sizes.log` ở thư mục gốc, chuyển vào `docs/` nếu cần nộp). Bỏ comment thừa.
- [ ] Mở PR `week-1` → `main`, kèm ảnh `/health` và Swagger, cùng danh sách vấn đề đã gặp (Windows/Linux containers, cổng 1433, `init.sql` có `DROP TABLE`…).

**Lưu ý:** người chạy thử phải dùng Docker ở chế độ Linux containers. Đây là lỗi dễ gặp nhất trên Windows, nên ghi rõ trong README.

## 7. Kiến thức cần học và tài liệu

- [Make a README](https://www.makeareadme.com/)
- [gitleaks (quét secret)](https://github.com/gitleaks/gitleaks)
- [12-Factor App](https://12factor.net/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
