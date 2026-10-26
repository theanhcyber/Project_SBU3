# Khái niệm cloud (v2): Tổng quan AWS Networking

| | |
|---|---|
| **Tuần** | 2: AWS cơ bản (26/10–1/11) |
| **Ngày** | Thứ 2, 26/10/2026 (học bổ sung sau [spec.md](spec.md)) |
| **Thời lượng** | +1h |
| **Trạng thái** | ⬜ Chưa bắt đầu |

> Bản v2 **bổ sung** cho [spec.md](spec.md), không thay thế. Toàn bộ nội dung do học viên tự soạn, không có trong pipeline *Xoá mù chữ Cloud*.
> Mục đích: có bức tranh tổng thể về mạng AWS ngay từ ngày đầu, để các bài sau (IAM 27/10, VPC 28/10, RDS 29/10, App Runner 31/10) không còn là các mảnh rời. Phần thực hành sâu (xem VPC thật, vẽ `docs/network.png`) vẫn làm ở bài [28/10](../3-wed_28-10_vpc-networking/spec.md).

## 1. Mục tiêu

Hiểu một request từ Internet đi qua những thành phần mạng nào để tới app và database trên AWS, và mỗi thành phần chịu trách nhiệm gì.

## 2. Yêu cầu

- Giải thích được 3 nhóm của AWS networking: **Subnet**, **Routing**, **Security**.
- Phân biệt subnet public và private, Internet Gateway và NAT Gateway, Security Group và Network ACL.
- Biết đặt ALB, EC2, NAT, RDS vào subnet nào và lý do.
- Ánh xạ được kiến trúc Docker Compose tuần 1 của TestManager-AspNet sang các thành phần mạng AWS.

## 3. Output phải nộp

- Ghi vào log: sơ đồ ASCII (hoặc ảnh) đã sửa ở mục 6.1, vẽ lại bằng tay, kèm 5 câu trả lời ở mục 10.

## 4. Tự đánh giá: đạt khi

- [ ] Nói được "subnet public khác subnet private **chỉ ở route table**".
- [ ] Giải thích được vì sao NAT Gateway nằm ở subnet public chứ không phải private.
- [ ] Viết được chuỗi Security Group ALB → app → RDS mà **không dùng `0.0.0.0/0`** cho app và RDS.
- [ ] Kể được ít nhất 2 thành phần mạng tính tiền theo giờ.

## 5. Nâng cao (không bắt buộc)

- VPC endpoint (Gateway cho S3, Interface cho ECR) để thay một phần NAT.
- VPC peering và Transit Gateway: nối nhiều VPC với nhau.

---

## 6. Lý thuyết

### 6.1 Sơ đồ tổng quan (đã sửa)

Sơ đồ ban đầu chia nhóm đúng (Subnet / Routing / Security), nhưng sai 2 chỗ về vị trí:

| Sơ đồ gốc | Đúng là | Lý do |
|---|---|---|
| NAT nằm dưới Private, sau EC2 | NAT Gateway **nằm trong subnet public** | NAT cần route ra Internet Gateway thì mới chuyển tiếp được traffic. Subnet private chỉ **trỏ route** tới NAT |
| RDS nối cả nhánh Public lẫn Private | RDS **chỉ nằm ở subnet private** | Database không bao giờ nhận kết nối trực tiếp từ Internet |

Sơ đồ đúng, có 2 AZ (đây là chuẩn tối thiểu cho production):

```text
                         Internet
                            │
                   ┌────────▼────────┐
                   │ Internet Gateway│        (gắn vào VPC, miễn phí)
                   └────────┬────────┘
VPC 10.0.0.0/16             │
┌───────────────────────────┼───────────────────────────────┐
│   AZ a                    │               AZ b             │
│ ┌───────────────────┐     │     ┌───────────────────┐      │
│ │ Public 10.0.1.0/24│◄────┴────►│ Public 10.0.2.0/24│      │
│ │  ALB (node a)     │           │  ALB (node b)     │      │
│ │  NAT Gateway a    │           │  NAT Gateway b    │      │
│ └─────────┬─────────┘           └─────────┬─────────┘      │
│           │ ALB → app :8080               │                │
│ ┌─────────▼─────────┐           ┌─────────▼─────────┐      │
│ │ Private 10.0.11.0/24          │ Private 10.0.12.0/24     │
│ │  EC2 / container  │           │  EC2 / container  │      │
│ │  (ra Internet qua NAT a)      │  (ra Internet qua NAT b) │
│ └─────────┬─────────┘           └─────────┬─────────┘      │
│           │ app → RDS :1433               │                │
│ ┌─────────▼─────────┐           ┌─────────▼─────────┐      │
│ │ DB 10.0.21.0/24   │           │ DB 10.0.22.0/24   │      │
│ │  RDS primary      │──replica─►│  RDS standby      │      │
│ └───────────────────┘           └───────────────────┘      │
│                                                            │
│  Route table: public / private-a / private-b / db          │
│  Security: SG gắn trên từng resource, NACL gắn trên subnet │
└────────────────────────────────────────────────────────────┘
```

Ba tầng (public → app → db) là kiến trúc **3-tier** kinh điển. Mỗi tầng chỉ nói chuyện với tầng kế bên.

### 6.2 VPC và CIDR

**Là gì:** VPC (Virtual Private Cloud) là một mạng riêng ảo, cô lập, nằm trong **1 region** và trải qua mọi AZ của region đó. Mọi thứ có IP (EC2, RDS, ALB, NAT) đều nằm trong một VPC.

**CIDR** quy định dải IP của VPC và subnet:

| CIDR | Số IP | Dùng cho |
|---|---|---|
| `10.0.0.0/16` | 65.536 | Cả VPC |
| `10.0.1.0/24` | 256 (dùng được **251**) | Một subnet |
| `10.0.1.0/28` | 16 (dùng được 11) | Subnet nhỏ nhất AWS cho phép |

- [ ] AWS **giữ lại 5 IP** đầu và cuối của mỗi subnet: `.0` (network), `.1` (VPC router), `.2` (DNS), `.3` (dự phòng), `.255` (broadcast).
- [ ] Dùng dải private theo RFC 1918: `10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`. **Không để trùng** với mạng công ty hoặc VPC khác nếu sau này cần nối (peering, VPN).
- [ ] Mỗi region có sẵn một **default VPC** `172.31.0.0/16` với **toàn subnet public**. Dùng để thử nhanh thì tiện, nhưng không nên dùng cho production vì không có subnet private.

**Ví dụ chia dải:** VPC `10.0.0.0/16`. Dải `10.0.1–9.x` cho public, `10.0.11–19.x` cho app, `10.0.21–29.x` cho DB. Chữ số cuối là AZ (1 = a, 2 = b). Nhìn IP là biết ngay máy thuộc tầng nào, AZ nào.

### 6.3 Subnet: public và private

**Là gì:** subnet là một dải con của VPC và **nằm trọn trong 1 AZ**. Muốn chịu được sự cố 1 AZ thì mỗi tầng cần ít nhất 2 subnet ở 2 AZ.

| | Public subnet | Private subnet |
|---|---|---|
| Route `0.0.0.0/0` trỏ tới | **Internet Gateway** | **NAT Gateway** (hoặc không có route nào) |
| Internet gọi vào được không | Được, nếu resource có public IP và SG cho phép | **Không** |
| Resource gọi ra Internet | Trực tiếp | Qua NAT (chỉ chiều đi ra) |
| Đặt gì ở đây | ALB, NAT Gateway, bastion host | EC2/container chạy app, RDS, cache |

> **Điểm mấu chốt:** AWS không có checkbox "public subnet". Subnet là public **chỉ vì** route table của nó có route tới Internet Gateway.

### 6.4 Routing: Route Table, Internet Gateway, NAT Gateway

**Route table** là bảng quy tắc "đi tới dải IP X thì gửi qua cửa Y". Mỗi subnet gắn đúng 1 route table. Route khớp **cụ thể nhất** (prefix dài nhất) sẽ thắng.

Route table của subnet public:

| Destination | Target | Ý nghĩa |
|---|---|---|
| `10.0.0.0/16` | `local` | Mọi IP trong VPC đi thẳng với nhau (tự có, không xoá được) |
| `0.0.0.0/0` | `igw-xxxx` | Mọi IP còn lại đi ra Internet |

Route table của subnet private (AZ a):

| Destination | Target | Ý nghĩa |
|---|---|---|
| `10.0.0.0/16` | `local` | Như trên |
| `0.0.0.0/0` | `nat-aaaa` | Ra Internet thì đi qua NAT Gateway ở **cùng AZ** |

Route table của subnet DB: **chỉ có route `local`**. RDS không cần ra Internet.

| Thành phần | Chiều traffic | Vị trí | Chi phí |
|---|---|---|---|
| **Internet Gateway** | Vào và ra | Gắn vào VPC (1 VPC có 1 IGW) | Miễn phí |
| **NAT Gateway** | **Chỉ chiều ra** (private → Internet), Internet không gọi ngược vào được | Đặt trong subnet public, có Elastic IP | **Tính theo giờ + theo GB** |

**Ví dụ thực tế cần NAT:** container ở subnet private cần `docker pull`, gọi API bên thứ ba, gửi email qua SendGrid, hoặc tải NuGet lúc khởi động. Không có NAT (hoặc VPC endpoint) thì các lệnh này bị timeout.

### 6.5 Security: Security Group và Network ACL

| | **Security Group (SG)** | **Network ACL (NACL)** |
|---|---|---|
| Gắn vào | ENI (EC2, RDS, ALB, Lambda trong VPC…) | Subnet |
| Loại rule | **Chỉ Allow** | Allow **và** Deny |
| Trạng thái | **Stateful**: đã cho request vào thì response tự được cho ra | **Stateless**: phải mở cả chiều về (ephemeral port `1024–65535`) |
| Thứ tự rule | Xét tất cả rule | Xét theo số thứ tự, khớp rule đầu tiên là dừng |
| Mặc định | Chặn mọi inbound, cho mọi outbound | NACL mặc định cho phép tất cả |
| Dùng khi | **Hằng ngày, lớp bảo vệ chính** | Chặn nhanh một dải IP xấu ở cấp subnet, hoặc khi có yêu cầu compliance |

**Kỹ thuật quan trọng nhất: SG tham chiếu SG.** Thay vì mở theo IP, khai báo nguồn là **một security group khác**:

| SG | Inbound | Nguồn |
|---|---|---|
| `sg-alb` | 443, 80 | `0.0.0.0/0` (Internet) |
| `sg-app` | 8080 | `sg-alb` |
| `sg-rds` | 1433 | `sg-app` |

Kết quả: chỉ ALB gọi được app, chỉ app gọi được DB. Khi scale thêm EC2, các máy mới gắn `sg-app` và tự động có quyền, không phải sửa IP.

> **Ví dụ sai thường gặp:** mở `sg-rds` cổng 1433 cho `0.0.0.0/0` để "test cho nhanh" từ laptop. Bot quét cổng 1433 liên tục trên Internet, và chỉ vài phút sau log đã có brute-force `sa`. Muốn kết nối từ laptop thì mở **đúng IP của mình** (`x.x.x.x/32`), rồi xoá rule khi xong việc.

### 6.6 ALB, EC2, RDS: ai nằm ở đâu

| Thành phần | Vai trò | Subnet | Ghi chú |
|---|---|---|---|
| **ALB** (Application Load Balancer) | Cửa ngõ HTTP/HTTPS, terminate TLS, chia tải, health check | Public, **≥ 2 AZ** (bắt buộc) | Là thứ duy nhất có DNS public |
| **EC2 / container** | Chạy app ASP.NET | Private | Không cần public IP |
| **NAT Gateway** | Cho tầng private ra Internet | Public | Nên đặt 1 NAT mỗi AZ để không phụ thuộc chéo AZ |
| **RDS** | SQL Server managed | Private (DB subnet group ≥ 2 AZ) | Đặt `Publicly accessible = No` |

### 6.7 Packet walk: một request đi như thế nào

Ví dụ người dùng gọi `GET https://api.example.com/tests`:

1. DNS trả về IP của ALB. Request đi qua **Internet Gateway** vào VPC.
2. **NACL** của subnet public cho phép 443 vào. **SG `sg-alb`** cho phép 443.
3. ALB chọn một target khoẻ (health check OK), có thể ở AZ a hoặc AZ b, rồi gửi tới `10.0.11.x:8080`. Route `local` lo việc chuyển gói.
4. **SG `sg-app`** chỉ nhận 8080 từ `sg-alb`, nên request hợp lệ.
5. App truy vấn `10.0.21.x:1433`. **SG `sg-rds`** chỉ nhận từ `sg-app`, nên hợp lệ.
6. Response đi ngược lại. Vì SG stateful nên không cần rule outbound riêng. NACL stateless thì phải mở ephemeral port.

Nếu app cần gọi API ngoài: `10.0.11.x` → route `0.0.0.0/0` → **NAT a** (subnet public) → **IGW** → Internet. Phía bên kia chỉ thấy Elastic IP của NAT.

---

## 7. Áp dụng thực tế

### 7.1 Ánh xạ TestManager-AspNet: Docker Compose → AWS

| Tuần 1 (`docker-compose.yml`) | Trên AWS | Bài |
|---|---|---|
| Network mặc định của Compose (bridge) | VPC + subnet | 28/10 |
| Container gọi nhau bằng tên service (`db`) | Gọi bằng DNS endpoint của RDS (`xxx.rds.amazonaws.com`) | 29/10 |
| `ports: "8080:8080"` publish ra máy host | ALB ở public subnet, hoặc URL public của App Runner | 31/10 |
| `db` không publish cổng là an toàn nhất | RDS ở private subnet, `sg-rds` chỉ mở 1433 cho app | 29/10 |
| Connection string trong `.env` | Secrets Manager / biến môi trường App Runner | 31/10 |

### 7.2 Kiến trúc thật của khoá học (App Runner)

Khoá học **không dùng ALB và EC2**, mà dùng App Runner (PaaS). AWS tự lo phần load balancer và máy chủ:

```text
Internet ──► App Runner (VPC do AWS quản lý, có sẵn HTTPS)
                 │ VPC connector (ENI đặt trong private subnet của bạn, gắn sg-apprunner)
                 ▼
           RDS SQL Server (private subnet, sg-rds: 1433 ← sg-apprunner)
```

- [ ] Khi bật VPC connector, **traffic đi ra** của App Runner đi qua VPC của bạn. Nếu app cần gọi Internet thì subnet đó phải có NAT. TestManager hiện chỉ gọi RDS, nên chưa cần NAT. **Đây là cách tiết kiệm tiền lớn nhất của tuần này.**
- [ ] Kiến trúc này có cùng tư duy với sơ đồ 6.1, chỉ khác là App Runner thay cho ALB + EC2.

### 7.3 Các tình huống hay gặp

| Tình huống | Giải pháp mạng |
|---|---|
| App trong private subnet không `docker pull` / gọi API ngoài được | Thiếu route `0.0.0.0/0 → NAT`, hoặc thiếu VPC endpoint |
| Dev cần dùng SSMS kết nối RDS private | Bastion host hoặc SSM Session Manager port forwarding. **Không** bật public RDS |
| App timeout khi kết nối RDS | 90% là do SG: `sg-rds` chưa cho phép SG của app ở cổng 1433 |
| Muốn kéo image từ ECR mà không tốn tiền NAT | VPC endpoint cho ECR (Interface) và S3 (Gateway) |
| Một AZ sự cố | ALB/App Runner chuyển traffic sang AZ còn lại, RDS Multi-AZ failover sang standby. `EnableRetryOnFailure()` giúp app vượt qua vài chục giây gián đoạn |

### 7.4 Chi phí cần biết (tham khảo `ap-southeast-1`, kiểm tra lại trên trang giá)

| Thành phần | Tính tiền? |
|---|---|
| VPC, subnet, route table, IGW, SG, NACL | Miễn phí |
| **NAT Gateway** | ~$0,059/giờ (≈ $43/tháng) **+ phí theo GB** xử lý. Đây là khoản dễ quên nhất |
| **Public IPv4** (EC2, NAT, ALB…) | $0,005/giờ mỗi IP, kể cả Elastic IP đang không gắn vào đâu |
| ALB | Khoảng $0,02–0,03/giờ + phí LCU |
| Data transfer giữa 2 AZ | Khoảng $0,01/GB mỗi chiều |

> Lab xong thì **xoá NAT Gateway và giải phóng Elastic IP**. Hai thứ này vẫn tính tiền dù không có traffic.

---

## 8. Thực hành nhỏ (chỉ xem, không tạo resource, không tốn tiền)

- [ ] Console → **VPC** → *Your VPCs*: tìm default VPC, ghi lại CIDR.
- [ ] *Subnets*: đếm số subnet và mỗi subnet thuộc AZ nào. Kiểm tra cột *Auto-assign public IPv4*.
- [ ] *Route tables*: mở route table chính, xác nhận có `0.0.0.0/0 → igw-…`. Đây là bằng chứng các subnet của default VPC đều là public.
- [ ] *Security groups*: mở SG `default`, đọc inbound rule (nguồn là chính nó).
- [ ] (Sau bài 27/10, khi đã có CLI) chạy lại bằng lệnh:

```bash
aws ec2 describe-vpcs --query "Vpcs[].{Id:VpcId,Cidr:CidrBlock,Default:IsDefault}" --output table
aws ec2 describe-subnets --query "Subnets[].{Id:SubnetId,Az:AvailabilityZone,Cidr:CidrBlock,PublicIp:MapPublicIpOnLaunch}" --output table
aws ec2 describe-route-tables --query "RouteTables[].Routes[]" --output table
```

## 9. Phân bổ thời gian (1h)

| Phút | Nội dung |
|---|---|
| 0–25 | Đọc mục 6.1–6.4, tự vẽ lại sơ đồ 6.1 |
| 25–40 | Mục 6.5–6.7: SG/NACL, packet walk |
| 40–50 | Mục 7: ánh xạ TestManager, chi phí |
| 50–60 | Mục 8 trên console và trả lời mục 10 |

## 10. Câu hỏi tự kiểm tra

1. Subnet `10.0.11.0/24` có route `0.0.0.0/0 → nat-…`. Đây là subnet public hay private? Máy trong đó có nhận request từ Internet được không?
2. Vì sao đặt NAT Gateway vào subnet private thì không hoạt động?
3. RDS báo timeout khi app kết nối. Bạn kiểm tra những gì, theo thứ tự nào?
4. NACL đã cho phép inbound 443 nhưng client vẫn không nhận được response. Thiếu gì?
5. Vì sao TestManager trên App Runner (31/10) chưa cần NAT Gateway?

<details>
<summary>Đáp án gợi ý</summary>

1. Private. Không, NAT chỉ cho phép chiều đi ra.
2. NAT phải ra được Internet thông qua route tới IGW, và chỉ subnet public mới có route đó.
3. SG của RDS (1433 từ SG của app), RDS và app có cùng VPC không, route `local`, NACL, endpoint/port trong connection string, `Publicly accessible`.
4. Outbound rule cho ephemeral port `1024–65535`, vì NACL là stateless.
5. App chỉ gọi RDS ở trong VPC, không gọi ra Internet qua VPC connector. Traffic vào thì đi qua endpoint public do AWS quản lý.

</details>

## 11. Tài liệu

- [How Amazon VPC works](https://docs.aws.amazon.com/vpc/latest/userguide/how-it-works.html)
- [Subnets for your VPC](https://docs.aws.amazon.com/vpc/latest/userguide/configure-subnets.html)
- [Route tables](https://docs.aws.amazon.com/vpc/latest/userguide/VPC_Route_Tables.html)
- [NAT gateways](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-nat-gateway.html)
- [Security groups](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-security-groups.html) · [Network ACLs](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-network-acls.html)
- [App Runner: VPC networking](https://docs.aws.amazon.com/apprunner/latest/dg/network-vpc.html)
- [Amazon VPC pricing](https://aws.amazon.com/vpc/pricing/)

## 12. Kết quả và log

_(dán sơ đồ đã vẽ lại, câu trả lời mục 10, ảnh chụp route table của default VPC; tóm tắt vào `LEARNING_LOG.md`)_
