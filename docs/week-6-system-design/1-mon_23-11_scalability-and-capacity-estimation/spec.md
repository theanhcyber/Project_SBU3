# Scalability và ước lượng tải

| | |
|---|---|
| **Tuần** | 6: System design (23/11–1/12) |
| **Ngày** | Thứ 2, 23/11/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Ước lượng tải của hệ thống bằng số liệu cụ thể.

## 2. Yêu cầu

- Đọc: scale dọc/ngang, stateless service, load balancer.
- Ước lượng cho 100.000 người dùng: tỷ lệ dùng hằng ngày, request mỗi người, QPS trung bình và cao điểm, dung lượng DB và file sau 1 năm.

## 3. Output phải nộp

- `docs/design/capacity.md`: giả định, công thức, kết quả.

## 4. Tự đánh giá: đạt khi

- [ ] Mọi con số có giả định và cách tính.
- [ ] Chỉ ra thành phần nghẽn đầu tiên và vì sao.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Đọc: scale dọc/ngang, stateless service, load balancer.
- [ ] Ước lượng cho 100.000 người dùng: tỷ lệ dùng hằng ngày, request mỗi người, QPS trung bình và cao điểm, dung lượng DB và file sau 1 năm.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [System Design Primer](https://github.com/donnemartin/system-design-primer)
- [ByteByteGo](https://bytebytego.com/)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
