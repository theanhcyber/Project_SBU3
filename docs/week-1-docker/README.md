# Tuần 1: Docker (19–25/10)

**Mốc tuần:** Người khác clone repo, chạy `docker compose up` là dùng được TeamBoard.

## Mục đích

Đóng gói TeamBoard thành container để app chạy giống nhau trên mọi máy. Đây là nền cho cả khoá: tuần sau đưa lên AWS và tự động deploy đều dùng chính image này.

## Sau tuần này, mỗi người sẽ

- Hiểu image, container, layer; viết Dockerfile multi-stage gọn và build nhanh nhờ cache.
- Chạy nhiều service bằng Docker Compose: network, volume, health check.
- Quản lý cấu hình bằng biến môi trường, không để mật khẩu trong repo.
- Thêm cache Redis và đo được hiệu quả bằng số liệu.
- Dùng lệnh Docker và Linux cơ bản để tự debug container.

## Cuối tuần: đào sâu thêm

_Làm sau khi lab thứ Bảy đã đạt mốc. Chọn 1–2 mục, ghi điều học được vào log._

- Tối ưu image: chiseled/distroless, chạy non-root, quét lỗ hổng bằng `docker scout` hoặc Trivy.
- Docker networking sâu hơn: bridge network, DNS nội bộ giữa các container.
- Đọc 12-Factor App, tập trung các mục Config, Logs, Disposability.

## Các bài trong tuần

| Ngày | Bài | Thời lượng | Trạng thái |
|---|---|---|---|
| Thứ 2 19/10 | [Dựng TeamBoard](1-mon_19-10_build-teamboard/spec.md) | 2h | ✅ Đã xong (còn điểm lệch so với pipeline, xem mục 6) |
| Thứ 3 20/10 | [Dockerfile multi-stage](2-tue_20-10_dockerfile-multi-stage/spec.md) | 2h | ✅ Đã xong (Windows containers) |
| Thứ 4 21/10 | [Cấu hình, health check, Compose](3-wed_21-10_config-health-check-compose/spec.md) | 2h | 🟡 Đang làm: code xong, chờ Docker Linux để chạy compose |
| Thứ 5 22/10 | [Migration khi khởi động, Linux tối thiểu](4-thu_22-10_migration-on-startup-linux-basics/spec.md) | 2h | ⏳ Hôm nay, chưa bắt đầu |
| Thứ 6 23/10 | [Redis cache](5-fri_23-10_redis-cache/spec.md) | 2h | ⬜ Chưa bắt đầu |
| Thứ 7 24/10 | [Lab: hoàn thiện để người khác chạy được](6-sat_24-10_lab-make-it-runnable-by-others/spec.md) | 5h | ⬜ Chưa bắt đầu |
| Chủ nhật 25/10 | [Review PR](7-sun_25-10_review-pr/spec.md) | 2h | ⬜ Chưa bắt đầu |
