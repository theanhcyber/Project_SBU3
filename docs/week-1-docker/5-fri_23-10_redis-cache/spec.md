# Redis cache

| | |
|---|---|
| **Tuần** | 1: Docker (19–25/10) |
| **Ngày** | Thứ 6, 23/10/2026 |
| **Thời lượng** | 2h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Nội dung các mục 1–5 trích nguyên văn từ pipeline *Xoá mù chữ Cloud*. Mục 6–8 là phần phân tích và ghi chép của học viên.

## 1. Mục tiêu

Cache danh sách task của một board và đo được hiệu quả.

## 2. Yêu cầu

- Thêm service `redis`, dùng `IDistributedCache` (`Microsoft.Extensions.Caching.StackExchangeRedis`).
- Cache GET `/api/boards/{id}/tasks`, TTL 60 giây.
- Xoá cache khi tạo, sửa, xoá task của board đó.
- Đo thời gian phản hồi có/không cache (ví dụ `curl -w "%{time_total}"`).

## 3. Output phải nộp

- Code cache.
- Bảng đo trong log: ít nhất 5 lần gọi mỗi trường hợp.

## 4. Tự đánh giá: đạt khi

- [ ] Lần gọi thứ 2 không chạy câu SQL (xem log EF Core).
- [ ] Tạo task mới, GET ngay thấy task đó.
- [ ] Tắt Redis: ghi lại app xử lý thế nào và đề xuất cách đúng.

## 5. Nâng cao (không bắt buộc)

- Viết 3 dòng về cache stampede.

## 6. Phân tích, hiện trạng và kế hoạch

**Hiện trạng:** chưa làm.

**Áp dụng vào TestManager:** pipeline cache "danh sách task của một board". Endpoint tương đương trong TestManager là danh sách đọc nhiều, ít ghi:

| Pipeline (TeamBoard) | TestManager | Xoá cache khi |
|---|---|---|
| `GET /api/boards/{id}/tasks` | `GET /api/classes/{id}/students` *(đề xuất)* hoặc `GET /api/exams/{id}/questions` | Thêm, sửa, xoá `student-classes` của lớp đó (hoặc thêm, gỡ câu hỏi của đề) |

**Kế hoạch:**
1. Thêm service `redis` (`redis:7-alpine`) vào compose. API dùng `AddStackExchangeRedisCache`, connection string qua biến môi trường.
2. Viết phần cache trong service: key `class:{id}:students`, TTL 60 giây, serialize JSON.
3. Xoá cache (`RemoveAsync`) trong Create, Update, Delete của `StudentClassService`.
4. Đo thời gian bằng `curl -w "%{time_total}"`, mỗi trường hợp 5 lần (có và không có cache). Bật log `Microsoft.EntityFrameworkCore.Database.Command` để chứng minh lần gọi thứ 2 không chạy SQL.
5. Tắt Redis để xem app xử lý thế nào. Mặc định sẽ ném exception dẫn tới 500. Đề xuất: bọc try/catch, khi cache lỗi thì đọc thẳng DB và ghi log cảnh báo.

**Nâng cao:** cache stampede là hiện tượng nhiều request cùng gặp cache vừa hết hạn và cùng đổ vào DB. Cách xử lý: khoá theo key, làm mới sớm, thêm thời gian hết hạn ngẫu nhiên (jitter).

## 7. Kiến thức cần học và tài liệu

- [Distributed cache (Redis) trong ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/distributed)
- [Redis docs](https://redis.io/docs/latest/)
- [Cache-aside pattern](https://learn.microsoft.com/azure/architecture/patterns/cache-aside)

## 8. Kết quả và log

_(ghi lệnh đã chạy, số liệu, ảnh và vấn đề gặp phải; tóm tắt vào `LEARNING_LOG.md`)_
