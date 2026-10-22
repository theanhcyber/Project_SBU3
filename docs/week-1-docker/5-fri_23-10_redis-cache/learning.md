# Kiến thức cần nắm: Redis cache

> Bài: [spec.md](spec.md) · Thứ 6, 23/10/2026 · 2h
> Tài liệu này **chỉ định nghĩa những gì cần học và cần làm được**, chưa triển khai code.
> Phụ thuộc bài 22/10: cần dữ liệu mẫu (1 lớp có vài sinh viên) để có thứ mà cache.

---

## 0. Bản đồ: yêu cầu nào cần kiến thức nào

| Yêu cầu / tiêu chí trong spec | Khái niệm cần nắm | Mục |
|---|---|---|
| Thêm service `redis`, dùng `IDistributedCache` | Redis cơ bản, Redis trong Compose, `AddStackExchangeRedisCache` | 2, 3 |
| Cache GET danh sách, TTL 60s | Cache-aside, TTL, thiết kế key, serialize | 1, 4 |
| Xoá cache khi tạo/sửa/xoá | Cache invalidation | 5 |
| Đo thời gian có/không cache, ≥ 5 lần | Đo hiệu năng đúng cách | 6 |
| Lần gọi 2 không chạy SQL | Log SQL của EF Core | 6 |
| Tắt Redis: app xử lý thế nào, đề xuất cách đúng | Failure mode, cache là phụ thuộc "mềm" | 7 |
| *(nâng cao)* cache stampede | Thundering herd, single-flight, jitter | 9 |

**Endpoint áp dụng trong TestManager:** `GET /api/classes/{id}/students` (`ClassesController.GetStudents` → `StudentClassService.GetAllAsync(classId)`), key đề xuất `class:{id}:students`.

---

## 1. Caching căn bản

**Cần nắm được:**
- [ ] Cache hit / miss, **hit ratio** (= hit / tổng request). Cache chỉ có lợi khi hit ratio cao: dữ liệu đọc nhiều, ít ghi.
- [ ] **TTL** và **staleness**: TTL 60s = chấp nhận dữ liệu cũ tối đa 60s nếu invalidation bị sót. TTL là lưới an toàn, không phải cơ chế chính.
- [ ] 4 chiến lược và khi nào dùng:

| Chiến lược | Luồng | Dùng trong bài? |
|---|---|---|
| **Cache-aside** (lazy loading) | App đọc cache → miss → đọc DB → ghi cache | ✅ Đây là cách của bài |
| Read-through | Thư viện cache tự đọc DB khi miss | Biết khái niệm (HybridCache gần giống) |
| Write-through | Ghi DB đồng thời ghi cache | Không |
| Write-behind | Ghi cache trước, ghi DB sau (bất đồng bộ) | Không, rủi ro mất dữ liệu |

- [ ] Absolute expiration vs sliding expiration. Bài yêu cầu TTL 60s → dùng **absolute** (`AbsoluteExpirationRelativeToNow`). Sliding có thể khiến key "sống mãi" nếu được đọc liên tục.
- [ ] In-memory cache (`IMemoryCache`) vs distributed cache (`IDistributedCache`): khi chạy nhiều instance (tuần 2+), memory cache mỗi instance một bản → xoá ở instance A, instance B vẫn trả dữ liệu cũ. Đây là lý do dùng Redis.

## 2. Redis cơ bản

**Cần nắm được:**
- [ ] Redis là key-value store **trong RAM**, xử lý lệnh tuần tự (single-threaded cho lệnh) nên từng lệnh là nguyên tử.
- [ ] Các kiểu dữ liệu: string, **hash**, list, set, sorted set. Bài này gặp string và hash (xem mục 3).
- [ ] Hết RAM thì sao: `maxmemory` + chính sách eviction (`allkeys-lru`, `volatile-ttl`, `noeviction`…). Với cache, nên dùng `allkeys-lru`.
- [ ] Persistence (RDB snapshot, AOF): với vai trò cache thuần, mất dữ liệu khi restart là chấp nhận được → không cần volume.

**Lệnh `redis-cli` cần làm được** (`docker compose exec redis redis-cli`):

| Lệnh | Tác dụng | Dùng khi |
|---|---|---|
| `PING` | Kiểm tra sống | Healthcheck compose |
| `SCAN 0 MATCH testmanager:* COUNT 100` | Liệt kê key an toàn | Xem key đã được tạo chưa. **Không dùng `KEYS *` trên production** (chặn server) |
| `TYPE <key>` | Kiểu dữ liệu của key | Phát hiện key là hash, không phải string |
| `HGETALL <key>` | Xem nội dung hash | Đọc dữ liệu `IDistributedCache` đã ghi |
| `TTL <key>` | Số giây còn sống (-1 không hết hạn, -2 không tồn tại) | Chứng minh TTL 60s |
| `DEL <key>` | Xoá key | Thử invalidation bằng tay |
| `MONITOR` | Xem mọi lệnh tới Redis theo thời gian thực | Thấy app gọi `HGET` / `HSET` / `DEL` lúc nào. Chỉ dùng ở dev |
| `INFO stats` | `keyspace_hits`, `keyspace_misses` | Tính hit ratio |
| `DBSIZE`, `FLUSHALL` | Đếm key / xoá tất cả | Reset khi đo. `FLUSHALL` nguy hiểm ngoài dev |

## 3. `IDistributedCache` với Redis trong ASP.NET Core

**Cần nắm được:**
- [ ] Package: `Microsoft.Extensions.Caching.StackExchangeRedis` (cùng major version 9.x với app).
- [ ] Đăng ký: `AddStackExchangeRedisCache(o => { o.Configuration = ...; o.InstanceName = "testmanager:"; })`. `InstanceName` là **tiền tố** gắn vào mọi key → nhiều app dùng chung Redis không đụng nhau.
- [ ] Connection string đi qua biến môi trường như bài 21/10, ví dụ `ConnectionStrings__Redis=redis:6379`. Không hard-code.
- [ ] API: `GetAsync` / `GetStringAsync`, `SetAsync` / `SetStringAsync` + `DistributedCacheEntryOptions`, `RemoveAsync`, `RefreshAsync`. Tất cả làm việc với `byte[]`/`string` → **tự serialize** (System.Text.Json).
- [ ] Lifetime: `IDistributedCache` là singleton, inject được vào service Scoped (`StudentClassService`).
- [ ] **Bẫy:** implementation Redis lưu mỗi entry dưới dạng **hash** (các field `data`, `absexp`, `sldexp`), nên `GET <key>` trong redis-cli báo `WRONGTYPE`. Phải dùng `HGETALL`.

**Thiết kế key (cần tự định nghĩa trước khi code):**

| Nội dung | Key đầy đủ trong Redis | TTL |
|---|---|---|
| Danh sách sinh viên của lớp 5 | `testmanager:class:5:students` | 60s absolute |

Quy tắc: tiền tố app → loại entity → id → loại dữ liệu. Nếu sau này đổi cấu trúc DTO, thêm version (`v2:`) để không đọc nhầm dữ liệu cũ đã cache.

## 4. Serialize và đặt cache ở đâu

**Cần nắm được:**
- [ ] Cache **DTO** (`List<StudentClassDto>`), không cache entity EF (có navigation, có thể vòng tham chiếu).
- [ ] Đặt logic cache ở **service** (`StudentClassService`), không ở controller: controller không cần biết có cache; invalidation cũng nằm cùng chỗ với Create/Update/Delete.
- [ ] Phân biệt "lớp không tồn tại" (controller trả 404 trước khi gọi danh sách) với "lớp không có sinh viên" (danh sách rỗng, vẫn cache được).

## 5. Cache invalidation: phần khó nhất của bài

**Nguyên tắc:** xoá cache **sau khi** `SaveChangesAsync` thành công. Xoá trước thì một request đọc chen giữa sẽ nạp lại dữ liệu cũ vào cache.

**Các thao tác làm danh sách `class:{id}:students` thay đổi** (cần liệt kê đủ trước khi code):

| Thao tác | Key cần xoá | Lưu ý từ code hiện tại |
|---|---|---|
| `StudentClassService.CreateAsync` | `class:{dto.ClassId}:students` | Dễ |
| `StudentClassService.UpdateAsync` | **Cả lớp cũ và lớp mới** | Update có thể đổi `ClassId` (chuyển sinh viên sang lớp khác). Phải lấy `ClassId` cũ trước khi gán |
| `StudentClassService.DeleteAsync` | `class:{classId}:students` | `DeleteByIdAsync` dùng `ExecuteDeleteAsync` theo `id`, **không biết `ClassId`** → phải đọc `ClassId` trước khi xoá |
| Sửa `ClassRoom` (Name, Code) | `class:{id}:students` | DTO chứa `ClassName`, `ClassCode` (dữ liệu denormalize) |
| Xoá `ClassRoom` | `class:{id}:students` | FK `Cascade` xoá luôn `StudentClasses` |
| Sửa `Student` (StudentCode) / xoá `Student`/`User` | Mọi lớp sinh viên đó học | Khó xoá chính xác → **chấp nhận cũ tối đa 60s** và ghi rõ quyết định này trong log |

**Cần nắm được:**
- [ ] Vì sao "There are only two hard things in Computer Science: cache invalidation and naming things".
- [ ] Race condition còn lại: request A đọc DB (dữ liệu cũ) → request B ghi DB + xoá cache → A ghi dữ liệu cũ vào cache. TTL giới hạn thời gian sai tối đa 60s. Biết điều này là đủ, không cần xử lý trong bài.
- [ ] Phân biệt *invalidate* (xoá, lần đọc sau nạp lại) với *update cache* (ghi đè giá trị mới). Bài dùng invalidate vì đơn giản và ít sai.

## 6. Đo hiệu năng và chứng minh không chạy SQL

**Bật log SQL của EF Core** (không cần sửa code, đặt qua biến môi trường trong compose):
`Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Information`
→ mỗi câu SQL in ra dòng `Executed DbCommand (3ms) ...`. Lần gọi 2 (hit) không có dòng này = đạt tiêu chí 1.

**Đo bằng curl:**
```
curl -s -o /dev/null -w "%{http_code} %{time_starttransfer} %{time_total}\n" http://localhost:8080/api/classes/1/students
```

**Cần nắm được để số liệu có nghĩa:**
- [ ] **Warm-up:** request đầu tiên sau khi app start chậm do JIT, mở connection pool, khởi tạo EF model. Bỏ request đầu khỏi bảng, hoặc ghi riêng.
- [ ] Đo "không cache" đúng cách: `DEL` key (hoặc `FLUSHALL`) trước **mỗi** lần đo miss; đo "có cache" là các lần liên tiếp trong 60s.
- [ ] Báo cáo **trung vị (median)** và min/max, không chỉ trung bình. 5 lần là tối thiểu, 10–20 lần tốt hơn.
- [ ] Hiểu kết quả thật: DB local + ít dữ liệu → chênh lệch có thể nhỏ (vài ms). Muốn thấy rõ thì seed thêm nhiều sinh viên vào lớp. Ghi trung thực, kể cả khi cache không nhanh hơn nhiều.
- [ ] Biết đo ở phía server (`Stopwatch`, log thời gian request) khác đo ở client (`curl`, có cả mạng).

**Mẫu bảng log:**

| Lần | Miss (ms) | Hit (ms) |
|---|---|---|
| 1..5 | | |
| Median | | |

## 7. Khi Redis chết: phụ thuộc "mềm"

**Hành vi mặc định cần quan sát và ghi lại** (`docker compose stop redis`):
- `GetAsync` ném `RedisConnectionException` / `RedisTimeoutException` → `AppExceptionHandler` biến thành **500**.
- Mỗi request có thể **chờ hết connect timeout** (mặc định khoảng 5s) rồi mới lỗi → app vừa lỗi vừa chậm.
- Khi Redis bật lại, thư viện tự kết nối lại (cần kiểm chứng và ghi lại).

**Cách đúng (đề xuất, nắm khái niệm):**
- [ ] Cache là **tối ưu hoá**, không phải nguồn dữ liệu → lỗi cache **không được** làm hỏng request. Bọc đọc/ghi/xoá cache trong try/catch, lỗi thì đọc thẳng DB và log `Warning`.
- [ ] Timeout ngắn: `connectTimeout`, `syncTimeout`/`asyncTimeout` cỡ vài trăm ms; `abortConnect=false` để app vẫn khởi động được khi Redis chưa lên.
- [ ] Health check: Redis lỗi → `Degraded`, không phải `Unhealthy` (app vẫn phục vụ được). Nếu báo `Unhealthy`, load balancer sẽ rút app khỏi traffic chỉ vì mất cache.
- [ ] Nâng hơn: circuit breaker (Polly) để ngừng gọi Redis một lúc sau nhiều lỗi liên tiếp.
- [ ] Rủi ro ngược: invalidation lỗi (Redis chập chờn lúc ghi) → cache cũ tồn tại tới hết TTL. Lại là lý do cần TTL.

## 8. Redis trong Docker Compose

**Cần nắm được:**
- [ ] Image `redis:7-alpine`, cổng 6379, không cần mật khẩu ở dev (sang AWS sẽ khác).
- [ ] `healthcheck: redis-cli ping`, và **quyết định**: `api` có `depends_on: redis: condition: service_healthy` không? Nếu cache là phụ thuộc mềm (mục 7) thì app không nên bắt buộc chờ Redis. Ghi lại lựa chọn và lý do.
- [ ] Có cần volume cho Redis không (mục 2: không, với vai trò cache).
- [ ] Không publish cổng 6379 ra host nếu không cần; `redis-cli` dùng qua `docker compose exec`.

---

## 9. Phần nâng cao

### 9.1 Cache stampede (yêu cầu nâng cao của spec: viết 3 dòng)

Cần hiểu đủ để tự viết, không chép:
- **Là gì:** key "nóng" hết hạn → hàng trăm request cùng miss trong cùng lúc → cùng truy vấn DB → DB quá tải (còn gọi là thundering herd / dog-piling).
- **Cách xử lý:** (1) khoá theo key / single-flight: chỉ 1 request nạp lại, số còn lại chờ; (2) làm mới sớm trước khi hết hạn (probabilistic early expiration); (3) thêm **jitter** vào TTL (ví dụ 60s ± 10s) để các key không hết hạn cùng lúc.
- **Trong .NET 9:** `HybridCache` (`Microsoft.Extensions.Caching.Hybrid`) có sẵn chống stampede, kết hợp cache L1 (memory) + L2 (Redis).

### 9.2 Chủ đề khác (chọn 1 nếu còn thời gian)

| Chủ đề | Học gì | Giá trị thực tế |
|---|---|---|
| HybridCache | `GetOrCreateAsync`, tag-based invalidation, L1/L2 | Cách hiện đại thay cho tự viết cache-aside |
| Output caching với Redis | `AddOutputCache` + `AddStackExchangeRedisOutputCache` | Cache cả response HTTP, không cần sửa service |
| Negative caching | Cache cả kết quả "không tìm thấy" (TTL ngắn) | Chặn request dồn vào DB với id không tồn tại |
| Eviction & memory | `maxmemory`, `maxmemory-policy`, `INFO memory` | Cấu hình Redis đúng vai trò cache |
| Redis trên AWS | ElastiCache (Redis OSS / Valkey), ElastiCache Serverless | Tuần 6 (24/11): database và cache ở quy mô lớn |
| Quan sát cache | Đếm hit/miss trong app, log, metric | Chứng minh cache có giá trị ở production |

---

## 10. Nguồn học (theo thứ tự ưu tiên)

| # | Nguồn | Đọc phần | Thời gian |
|---|---|---|---|
| 1 | [Distributed caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/distributed) | Phần Redis + `IDistributedCache` | 15' |
| 2 | [Cache-aside pattern](https://learn.microsoft.com/azure/architecture/patterns/cache-aside) | Toàn bộ, đặc biệt *Issues and considerations* | 10' |
| 3 | [Amazon Builders' Library: Caching challenges and strategies](https://aws.amazon.com/builders-library/caching-challenges-and-strategies/) | Toàn bộ, bài hay nhất về rủi ro của cache | 20' |
| 4 | [Redis: data types](https://redis.io/docs/latest/develop/data-types/) và [commands](https://redis.io/docs/latest/commands/) | String, Hash; tra `SCAN`, `TTL`, `INFO` | 15' |
| 5 | [StackExchange.Redis: Configuration](https://stackexchange.github.io/StackExchange.Redis/Configuration) | `abortConnect`, `connectTimeout`, `syncTimeout` | 10' |
| 6 | [EF Core simple logging](https://learn.microsoft.com/ef/core/logging-events-diagnostics/simple-logging) | Log câu SQL | 5' |
| 7 | [Redis image trên Docker Hub](https://hub.docker.com/_/redis) | Cách chạy, cấu hình | 5' |
| 8 | [HybridCache in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid) | Phần nâng cao, stampede protection | 10' |
| 9 | [Redis: key eviction](https://redis.io/docs/latest/develop/reference/eviction/) | Phần nâng cao | 10' |
| 10 | [AWS whitepaper: Database caching strategies using Redis](https://docs.aws.amazon.com/whitepapers/latest/database-caching-strategies-using-redis/welcome.html) | Đọc trước cho tuần 6 | tuỳ |
| 11 | [Everything curl: `--write-out`](https://everything.curl.dev/usingcurl/verbose/writeout) | Các biến thời gian của curl | 5' |

---

## 11. Phân bổ 2h gợi ý

| Thời gian | Việc |
|---|---|
| 0:00–0:25 | Đọc nguồn 1–3. Tự vẽ luồng cache-aside cho `GET /api/classes/{id}/students` |
| 0:25–0:35 | Chạy Redis, thực hành bảng lệnh `redis-cli` ở mục 2 |
| 0:35–1:15 | Triển khai cache + invalidation (lúc triển khai), dùng bảng mục 5 làm checklist |
| 1:15–1:40 | Đo theo mục 6, điền bảng |
| 1:40–2:00 | Tắt Redis theo mục 7, ghi hành vi + đề xuất; viết 3 dòng stampede |

## 12. Tự kiểm tra trước khi coi là "nắm được"

- [ ] Vẽ được luồng cache-aside, chỉ ra chỗ hit, miss, ghi cache.
- [ ] Giải thích vì sao bài dùng Redis thay cho `IMemoryCache`.
- [ ] Biết vì sao `GET` trên key của `IDistributedCache` báo `WRONGTYPE`.
- [ ] Kể đủ các thao tác làm thay đổi danh sách sinh viên của lớp, gồm cả trường hợp `Update` đổi lớp và `Delete` không biết `ClassId`.
- [ ] Giải thích vì sao phải xoá cache sau `SaveChanges`, không phải trước.
- [ ] Đọc được bảng đo và giải thích vì sao chênh lệch có thể nhỏ.
- [ ] Trình bày được: Redis chết thì mặc định chuyện gì xảy ra và cách đúng là gì.
- [ ] Nói được cache stampede trong 3 câu, không nhìn tài liệu.
