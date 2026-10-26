# Kiến thức cần nắm (v2): Tổng quan AWS Networking

> Bài: [spec-v2.md](spec-v2.md) · Thứ 2, 26/10/2026 · +1h (học sau [learning.md](learning.md))
> Tài liệu này **chỉ định nghĩa những gì cần học và cần làm được**, chưa phải log kết quả.
> Lý thuyết đầy đủ (sơ đồ, bảng route, bảng SG, packet walk) nằm ở [spec-v2.md mục 6](spec-v2.md#6-lý-thuyết). File này không chép lại phần đó mà biến nó thành checklist, bài tập và cách tự kiểm tra.

> **Phạm vi:** hôm nay chỉ **hiểu và đọc** được mạng AWS, không tạo resource nào tốn tiền. Bài 28/10 mới xem VPC kỹ và vẽ `docs/network.png`, bài 29/10 mới đặt RDS vào subnet thật.

---

## 0. Bản đồ: yêu cầu nào cần kiến thức nào

| Yêu cầu / tiêu chí trong spec-v2 | Cần hiểu | Mục |
|---|---|---|
| Giải thích 3 nhóm Subnet / Routing / Security | VPC, CIDR, subnet, route table | 2, 3, 4 |
| Phân biệt public / private subnet | Route `0.0.0.0/0` trỏ tới đâu | 3, 4 |
| Phân biệt IGW / NAT | Chiều traffic, vị trí đặt | 4 |
| Phân biệt SG / NACL, chuỗi SG không dùng `0.0.0.0/0` | Stateful / stateless, SG tham chiếu SG | 5 |
| Đặt ALB, EC2, NAT, RDS đúng subnet | Kiến trúc 3-tier | 6 |
| Ánh xạ Docker Compose → AWS | So sánh với tuần 1 | 1, 7 |
| Kể 2 thành phần mạng tính tiền theo giờ | NAT, public IPv4, ALB | 8 |

**Cần chuẩn bị:** đã đăng nhập được bằng danh tính admin (xong [learning.md](learning.md) mục 3), đã chọn region (learning.md mục 5), có giấy bút để tự vẽ sơ đồ.

---

## 1. Cầu nối từ tuần 1: bạn đã biết networking rồi

Docker Compose tuần 1 đã dùng gần đủ các ý tưởng của VPC, chỉ khác tên gọi:

| Đã quen ở Docker | Tương đương trên AWS | Khác nhau ở đâu |
|---|---|---|
| Network bridge mà Compose tự tạo (`<project>_default`) | **VPC** | VPC do bạn tự chọn dải IP, trải qua nhiều AZ |
| Dải IP của network (`172.18.0.0/16`) | **CIDR** của VPC / subnet | Trên AWS phải chọn sao cho không trùng với các mạng sẽ nối vào |
| Container gọi `db:1433` bằng tên service | Gọi RDS bằng **DNS endpoint** | Endpoint do AWS cấp, không tự đặt tên được |
| `ports: "8080:8080"` (mở ra máy host) | **IGW + public subnet + ALB** | Trên AWS phải đi qua nhiều lớp mới ra được Internet |
| Service `db` không có `ports:` nên bên ngoài không gọi được | **Private subnet + SG** | AWS tách 2 lớp: không có đường đi (route) và không có quyền (SG) |
| Docker tự sinh rule iptables | **Security Group** | SG do bạn khai báo, mặc định chặn mọi inbound |

- [ ] Mở `docker-compose.yml` của TestManager, chỉ ra từng dòng ứng với thành phần AWS nào trong bảng trên.

## 2. VPC và CIDR

**Cần nắm được:**
- [ ] VPC là **regional**: một VPC nằm trong 1 region và trải qua mọi AZ của region đó. Đổi region ở góc phải console thì thấy danh sách VPC khác.
- [ ] Đọc được ký hiệu CIDR: `/16` nghĩa là 16 bit đầu cố định, còn lại 2^(32−16) = 65.536 IP. Mỗi lần tăng thêm 1 bit thì số IP giảm một nửa.
- [ ] Mỗi subnet mất **5 IP** do AWS giữ (`.0 .1 .2 .3` và IP cuối). Subnet `/24` chỉ dùng được 251 IP.
- [ ] VPC dùng được từ `/16` (lớn nhất) đến `/28` (nhỏ nhất). Sau khi tạo VPC thì **không đổi được CIDR chính**, chỉ thêm được CIDR phụ, nên phải chọn rộng ngay từ đầu.
- [ ] Hai mạng có CIDR **trùng nhau** thì không peering hay VPN với nhau được. Default VPC của mọi account đều là `172.31.0.0/16`, nên VPC tự tạo nên chọn dải `10.x`.

**Bài tập CIDR** (làm bằng tay, có thể dùng trang tính CIDR online để kiểm tra lại):

1. VPC `10.0.0.0/16` chia được tối đa bao nhiêu subnet `/24`?
2. Subnet `/20` dùng được bao nhiêu IP?
3. `10.0.1.0/24` có nằm trong `10.0.0.0/23` không?
4. Dự kiến 3 tầng × 2 AZ, mỗi subnet tối đa 200 máy. Chọn CIDR cho VPC và cho 6 subnet.

<details>
<summary>Đáp án</summary>

1. 2^(24−16) = 256 subnet.
2. 2^12 − 5 = 4.091 IP.
3. Có. `10.0.0.0/23` bao gồm dải từ `10.0.0.0` đến `10.0.1.255`.
4. Ví dụ VPC `10.0.0.0/16` với các subnet `/24` (251 IP > 200): public `10.0.1.0/24`, `10.0.2.0/24`; app `10.0.11.0/24`, `10.0.12.0/24`; db `10.0.21.0/24`, `10.0.22.0/24`. Vẫn còn rất nhiều dải trống để mở rộng sau này.

</details>

## 3. Subnet public và private

**Cần nắm được:**
- [ ] Subnet nằm trọn trong **1 AZ**. Muốn chịu được sự cố 1 AZ thì mỗi tầng cần ≥ 2 subnet ở 2 AZ (liên hệ [learning.md mục 6](learning.md#6-vì-sao-hệ-thống-chạy-2-az-vẫn-sống-khi-1-az-sự-cố)).
- [ ] Một subnet là "public" khi **route table của nó có `0.0.0.0/0 → igw-…`**. Ngoài điều đó ra, AWS không có thuộc tính nào khác quyết định subnet là public.
- [ ] *Auto-assign public IPv4* chỉ quyết định máy có được cấp IP public hay không. Máy có IP public nhưng nằm trong subnet không có route tới IGW thì **vẫn không ra được Internet**.
- [ ] Muốn Internet gọi được vào một máy thì phải có **đủ 4 điều kiện**: subnet có route tới IGW, máy có IP public (hoặc đứng sau ALB), SG cho phép, NACL cho phép.

**Hiểu nhầm thường gặp:**

| Hiểu nhầm | Thực tế |
|---|---|
| "Private subnet thì không ra Internet được" | Ra được nếu có NAT. Private chỉ có nghĩa là **Internet không gọi vào được** |
| "Đặt ở public subnet là bị lộ" | Không lộ nếu không có public IP hoặc SG chặn. Tuy vậy vẫn nên giảm bề mặt tấn công bằng cách chỉ đặt ALB/NAT ở public |
| "Default VPC có subnet private" | Không có. Mọi subnet của default VPC đều là public |

## 4. Routing: route table, IGW, NAT

**Cần nắm được:**
- [ ] Mỗi subnet gắn **đúng 1** route table. Nếu không gắn rõ thì subnet dùng *main route table* của VPC.
- [ ] Route `local` (CIDR của VPC) luôn có sẵn và **không xoá được**, nên mọi subnet trong cùng VPC luôn nói chuyện được với nhau ở tầng route. Muốn chặn thì phải dùng SG hoặc NACL.
- [ ] Route có prefix **dài nhất** thắng: `10.0.0.0/16 → local` thắng `0.0.0.0/0 → igw` khi đích là `10.0.5.7`.
- [ ] **IGW**: cho traffic vào và ra, mỗi VPC 1 IGW, miễn phí, tự scale.
- [ ] **NAT Gateway**: chỉ cho chiều đi ra, **đặt trong subnet public**, có Elastic IP, tính tiền theo giờ và theo GB.
- [ ] Tại sao NAT nên đặt **1 cái mỗi AZ**: nếu AZ chứa NAT duy nhất bị sự cố thì mọi subnet private ở các AZ khác cũng mất đường ra Internet. Với tài khoản học thì 1 NAT là đủ, hoặc tốt hơn là không tạo NAT.
- [ ] Biết VPC endpoint là phương án thay NAT khi chỉ cần gọi dịch vụ AWS: **Gateway endpoint** (S3, DynamoDB) miễn phí; **Interface endpoint** (ECR, Secrets Manager…) tính tiền theo giờ.

**Bài tập đọc route table:** cho route table sau, trả lời 3 câu hỏi.

| Destination | Target |
|---|---|
| `10.0.0.0/16` | `local` |
| `0.0.0.0/0` | `nat-0abc` |

1. Subnet gắn route table này là public hay private?
2. Máy `10.0.11.5` gọi `10.0.21.8:1433` thì gói tin đi qua route nào?
3. Máy `10.0.11.5` gọi `api.github.com` thì phía GitHub thấy IP nào?

<details>
<summary>Đáp án</summary>

1. Private, vì `0.0.0.0/0` trỏ tới NAT chứ không phải IGW.
2. `local`, vì đây là prefix dài hơn và đích nằm trong VPC.
3. Elastic IP của `nat-0abc`, không phải `10.0.11.5`.

</details>

## 5. Security Group và Network ACL

**Cần nắm được:**
- [ ] Phân biệt được qua 4 trục: gắn ở đâu (ENI hay subnet), loại rule (chỉ allow hay allow và deny), trạng thái (stateful hay stateless), cách xét rule (tất cả hay theo thứ tự). Bảng chi tiết ở [spec-v2 mục 6.5](spec-v2.md#65-security-security-group-và-network-acl).
- [ ] **SG là lớp bảo vệ chính.** Trong thực tế hầu như chỉ dùng SG, còn NACL để mặc định.
- [ ] **SG tham chiếu SG**: nguồn của rule là một SG khác chứ không phải IP. Máy mới scale lên tự có quyền, không phải sửa rule.
- [ ] Một resource gắn được **nhiều SG**, và quyền là **hợp** của các SG đó. Không có cách nào dùng SG để "chặn" một nguồn đã được SG khác cho phép.
- [ ] SG mới tạo: **chặn mọi inbound, cho mọi outbound**. SG `default` của VPC: cho phép inbound từ chính nó.
- [ ] NACL stateless nên mở inbound 443 thì phải mở thêm **outbound ephemeral `1024–65535`** cho chiều trả về.

**Bài tập thiết kế SG cho TestManager** (kiến trúc App Runner ở [spec-v2 mục 7.2](spec-v2.md#72-kiến-trúc-thật-của-khoá-học-app-runner)):

Viết bảng inbound cho `sg-apprunner-connector` và `sg-rds`. Yêu cầu: RDS chỉ nhận kết nối từ App Runner, và bạn được kết nối SSMS từ IP nhà mình trong buổi lab.

<details>
<summary>Đáp án gợi ý</summary>

| SG | Inbound | Nguồn | Ghi chú |
|---|---|---|---|
| `sg-apprunner-connector` | _(không có)_ | | App Runner chỉ **gọi ra** RDS qua connector, không ai gọi vào ENI này |
| `sg-rds` | TCP 1433 | `sg-apprunner-connector` | Rule cố định |
| `sg-rds` | TCP 1433 | `<IP-nhà>/32` | **Tạm thời**, và chỉ có tác dụng nếu RDS đặt *Publicly accessible* và nằm ở subnet có route tới IGW. Cách an toàn hơn là bastion hoặc SSM port forwarding. Xoá rule ngay sau buổi lab |

Outbound của `sg-apprunner-connector` để mặc định (all), hoặc siết lại chỉ còn TCP 1433 tới `sg-rds`.

</details>

## 6. Kiến trúc 3-tier: ai nằm ở đâu

**Cần nắm được:**
- [ ] Nguyên tắc: **chỉ những thứ bắt buộc phải nhận traffic từ Internet mới nằm ở public**, gồm ALB và NAT (bastion nếu có).
- [ ] App (EC2, ECS task, App Runner connector) và dữ liệu (RDS, ElastiCache) nằm ở **private**.
- [ ] ALB bắt buộc phải có subnet ở **≥ 2 AZ**. RDS bắt buộc có **DB subnet group ≥ 2 AZ**, kể cả khi chạy Single-AZ.
- [ ] Tự sửa được 2 lỗi của sơ đồ ban đầu (NAT đặt trong private, RDS nối với public) và giải thích lý do bằng lời của mình.
- [ ] Tự vẽ lại sơ đồ [spec-v2 mục 6.1](spec-v2.md#61-sơ-đồ-tổng-quan-đã-sửa) **mà không nhìn tài liệu**, rồi so sánh với bản gốc. Đây là output phải nộp.

## 7. Áp dụng: kiến trúc TestManager trên AWS

**Cần nắm được:**
- [ ] Khoá học dùng **App Runner** thay cho ALB + EC2, nên bạn **không phải tự quản** public subnet, IGW hay ALB cho phần app. AWS lo những thứ này trong VPC của họ.
- [ ] **VPC connector** đặt ENI của App Runner vào private subnet trong VPC của bạn, nhờ đó app gọi được RDS private.
- [ ] Khi đã gắn VPC connector, **mọi traffic đi ra** của app đều đi qua VPC của bạn. Nếu sau này TestManager cần gọi API ngoài (gửi email, webhook…) thì subnet đó phải có NAT.
- [ ] Connection string trên AWS: `Server=<rds-endpoint>,1433;...` thay cho `Server=db,1433;...`, và nên để ở biến môi trường hoặc Secrets Manager chứ không ghi vào code.
- [ ] `EnableRetryOnFailure()` trong [Program.cs](../../../TestManager.Api/Program.cs) giúp app chịu được RDS failover và các lỗi mạng tạm thời.

**Bảng xử lý sự cố** (học thuộc thứ tự kiểm tra, vì bài 29/10 và 31/10 rất hay gặp):

| Triệu chứng | Kiểm tra theo thứ tự |
|---|---|
| App timeout khi kết nối RDS (không có lỗi login) | 1. `sg-rds` có cho 1433 từ SG của app không → 2. App và RDS có cùng VPC không → 3. NACL → 4. Endpoint và port trong connection string |
| Lỗi `Login failed for user` | Mạng đã thông. Lỗi nằm ở username/password/database, không còn là chuyện networking |
| Container private không pull image hay gọi API ngoài được | Route `0.0.0.0/0` của subnet → NAT có nằm ở subnet public không → subnet của NAT có route tới IGW không |
| SSMS trên laptop không kết nối RDS | RDS không public (đúng thiết kế). Dùng bastion / SSM, hoặc tạm mở theo [mục 5](#5-security-group-và-network-acl) |

> **Mẹo phân biệt nhanh:** *timeout* thường do mạng (route, SG, NACL), còn *connection refused* hoặc *login failed* nghĩa là đã tới được máy đích.

## 8. Chi phí mạng

**Cần nắm được:**
- [ ] **Miễn phí:** VPC, subnet, route table, IGW, SG, NACL, Gateway endpoint.
- [ ] **Tính tiền theo giờ, kể cả khi không có traffic:** NAT Gateway, public IPv4 (kể cả Elastic IP đang không gắn vào đâu), ALB, Interface endpoint.
- [ ] **Tính tiền theo GB:** dữ liệu xử lý qua NAT, dữ liệu đi ra Internet, dữ liệu đi giữa các AZ.
- [ ] Liên hệ [learning.md mục 4](learning.md#4-budgets-và-kiểm-soát-chi-phí): nếu zero spend budget báo động mà bạn chưa tạo RDS, hãy kiểm tra **NAT Gateway và Elastic IP** trước tiên, ở **mọi region**.
- [ ] Tra giá thật trên [Amazon VPC pricing](https://aws.amazon.com/vpc/pricing/) cho region của bạn và ghi vào log: NAT bao nhiêu $/giờ, public IPv4 bao nhiêu $/giờ.

## 9. Thực hành trên console (15 phút, không tốn tiền)

Làm theo [spec-v2 mục 8](spec-v2.md#8-thực-hành-nhỏ-chỉ-xem-không-tạo-resource-không-tốn-tiền), rồi ghi vào log:

- [ ] CIDR của default VPC, số subnet, AZ của từng subnet.
- [ ] Route table chính có `0.0.0.0/0 → igw-…` không. Từ đó kết luận subnet là public hay private.
- [ ] Inbound rule của SG `default`.
- [ ] NACL mặc định: rule số mấy cho phép, rule `*` làm gì.
- [ ] Mở *VPC → Resource map* (sơ đồ tự vẽ của AWS) và so sánh với sơ đồ bạn tự vẽ.

---

## 10. Nguồn học (theo thứ tự ưu tiên)

| # | Nguồn | Đọc phần | Thời gian |
|---|---|---|---|
| 1 | [How Amazon VPC works](https://docs.aws.amazon.com/vpc/latest/userguide/how-it-works.html) | VPC, subnet, route, IGW | 10' |
| 2 | [Subnets for your VPC](https://docs.aws.amazon.com/vpc/latest/userguide/configure-subnets.html) | Public / private, IP được giữ lại | 5' |
| 3 | [Route tables](https://docs.aws.amazon.com/vpc/latest/userguide/VPC_Route_Tables.html) | Main route table, longest prefix match | 5' |
| 4 | [Security groups](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-security-groups.html) và [Compare SG and NACL](https://docs.aws.amazon.com/vpc/latest/userguide/infrastructure-security.html) | Stateful / stateless, SG tham chiếu SG | 10' |
| 5 | [NAT gateways](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-nat-gateway.html) | Vị trí đặt, giá | 5' |
| 6 | [App Runner: enabling VPC access](https://docs.aws.amazon.com/apprunner/latest/dg/network-vpc.html) | VPC connector, traffic đi ra | 5' |
| 7 | [Example: VPC with servers in private subnets and NAT](https://docs.aws.amazon.com/vpc/latest/userguide/vpc-example-private-subnets-nat.html) | Kiến trúc mẫu giống sơ đồ 6.1 | tuỳ |
| 8 | [Amazon VPC pricing](https://aws.amazon.com/vpc/pricing/) | NAT, IPv4, endpoint | 3' |

---

## 11. Phân bổ 1h gợi ý

| Thời gian | Việc |
|---|---|
| 0:00–0:10 | Mục 1: liên hệ với `docker-compose.yml`. Đọc nguồn 1 |
| 0:10–0:25 | Mục 2–4: làm bài tập CIDR và bài tập đọc route table |
| 0:25–0:40 | Mục 5–6: bài tập thiết kế SG, tự vẽ lại sơ đồ 3-tier |
| 0:40–0:50 | Mục 7–8: bảng xử lý sự cố, tra giá NAT/IPv4 |
| 0:50–1:00 | Mục 9 trên console, ghi log |

## 12. Tự kiểm tra trước khi coi là "nắm được"

- [ ] Vẽ được sơ đồ VPC 2 AZ × 3 tầng mà không nhìn tài liệu, có CIDR, IGW, NAT đặt đúng chỗ.
- [ ] Nói được trong 1 câu: "subnet public là subnet có route `0.0.0.0/0` tới IGW".
- [ ] Giải thích vì sao NAT phải nằm ở subnet public, và vì sao private subnet vẫn ra được Internet.
- [ ] Tính nhanh số IP dùng được của một subnet `/24`, `/26`, `/28`.
- [ ] Phân biệt SG / NACL qua 4 trục, và biết khi nào cần mở ephemeral port.
- [ ] Viết được chuỗi SG ALB → app → RDS (hoặc App Runner connector → RDS) không có `0.0.0.0/0` ở app và RDS.
- [ ] Gặp "timeout tới RDS" thì biết kiểm tra những gì và theo thứ tự nào.
- [ ] Kể được 3 thứ trong mạng AWS tính tiền theo giờ dù không có traffic.
- [ ] Trả lời được 5 câu hỏi ở [spec-v2 mục 10](spec-v2.md#10-câu-hỏi-tự-kiểm-tra) mà không xem đáp án.
