# Mạng VPC

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 4, 28/10/2026 |
| **Thời lượng** | 1,5h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Hiểu mạng mà app và database sẽ chạy bên trong.

## 2. Yêu cầu

- Xem VPC sandbox: CIDR, subnet public/private, route table, internet gateway.
- Phân biệt security group và network ACL.
- Vẽ sơ đồ: VPC, 2 AZ, subnet public/private, vị trí RDS, đường App Runner kết nối vào.

## 3. Output phải nộp

- `docs/network.png` (kèm file gốc) và 5 dòng giải thích.

## 4. Tự đánh giá: đạt khi

- [ ] Sơ đồ có CIDR, ít nhất 2 AZ, subnet public/private, internet gateway.
- [ ] Security group của RDS chỉ mở cổng 1433 cho nguồn cần thiết.
- [ ] Nói được subnet public và private khác nhau ở đâu.

## 5. Nâng cao (không bắt buộc)

_Không có._

## 6. Phân tích, hiện trạng và kế hoạch

**Việc cần làm** (tách từ yêu cầu, đánh dấu khi xong):

- [ ] Xem VPC sandbox: CIDR, subnet public/private, route table, internet gateway.
- [ ] Phân biệt security group và network ACL.
- [ ] Vẽ sơ đồ: VPC, 2 AZ, subnet public/private, vị trí RDS, đường App Runner kết nối vào.

**Câu hỏi cần chốt trước khi làm:** _(điền khi bắt đầu bài)_

**Áp dụng vào TestManager-AspNet:** _(ghi endpoint, file sẽ thêm hoặc sửa)_

## 7. Kiến thức cần học và tài liệu

- [Amazon VPC](https://docs.aws.amazon.com/vpc/latest/userguide/what-is-amazon-vpc.html)
- [Security groups](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-security-groups.html)
- [Network ACLs](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-network-acls.html)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
