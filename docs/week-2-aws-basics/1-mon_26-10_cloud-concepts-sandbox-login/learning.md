# Kiến thức cần nắm: Khái niệm cloud, đăng nhập sandbox

> Bài: [spec.md](spec.md) · Thứ 2, 26/10/2026 · 1,5h
> Tài liệu này **chỉ định nghĩa những gì cần học và cần làm được**, chưa phải log kết quả.
> Bài đầu tiên của tuần AWS: chưa đụng code TestManager, chỉ dựng nền (tài khoản an toàn, hiểu hạ tầng, biết chi phí) cho các bài 27/10 → 31/10.

---

## 0. Bản đồ: yêu cầu nào cần kiến thức nào

| Yêu cầu / tiêu chí trong spec | Khái niệm cần nắm | Mục |
|---|---|---|
| Đăng nhập console | Account, root user vs IAM user vs Identity Center, sign-in URL | 1 |
| Bật MFA, trang IAM hiện MFA đã gán | MFA, loại thiết bị, chính sách "không MFA thì không làm gì" | 2 |
| Xem AWS Budgets: ngưỡng và người nhận | Budgets, alert actual/forecast, quyền xem billing | 3 |
| Log: region team dùng và lý do | Region, tiêu chí chọn region, dịch vụ global vs regional | 4 |
| Giải thích vì sao 2 AZ vẫn sống khi 1 AZ sự cố | Availability Zone, fault isolation, Multi-AZ | 4, 5 |
| Bảng 2 dịch vụ cho mỗi loại IaaS / PaaS / SaaS | Mô hình dịch vụ, shared responsibility model | 6 |

**Phụ thuộc:** thông tin sandbox do team/mentor cấp (sign-in URL hoặc portal, user, mật khẩu tạm). Chưa có thì hỏi trước giờ học, đây là việc chặn duy nhất của bài.

---

## 1. Tài khoản AWS và các cách đăng nhập

**Cần nắm được:**
- [ ] **AWS account** = đơn vị chứa tài nguyên và hoá đơn, định danh bằng **Account ID** 12 chữ số. Sandbox thường là một account con trong **AWS Organizations** của công ty.
- [ ] 3 loại danh tính có thể đăng nhập console:

| Loại | Đăng nhập ở đâu | Credential | Ai dùng |
|---|---|---|---|
| **Root user** | `signin.aws.amazon.com` bằng email | Email + mật khẩu | Chỉ chủ account, cho vài việc đặc biệt. **Học viên không dùng** |
| **IAM user** | `https://<account-id hoặc alias>.signin.aws.amazon.com/console` | User name + mật khẩu (+ MFA) | Cách sandbox cấp phổ biến, đơn giản |
| **IAM Identity Center** (SSO) | AWS access portal `https://<tên>.awsapps.com/start` | User của Identity Center (+ MFA) | Cách AWS khuyến nghị cho tổ chức, cho phép chọn account + role |

- [ ] Xác định **sandbox cấp theo cách nào**, vì nó quyết định: cách bật MFA (mục 2) và cách cấu hình CLI ngày mai (`aws configure` hay `aws configure sso`).
- [ ] Sau khi đăng nhập: góc phải trên cùng hiện user/role và Account ID, kiểm tra đúng **account sandbox** chứ không phải account khác.
- [ ] Lần đầu đăng nhập IAM user thường bị bắt **đổi mật khẩu** theo password policy của account.

**Bẫy thường gặp:**
- Nhầm trang đăng nhập root với trang IAM user: ở `signin.aws.amazon.com` phải chọn "IAM user" và nhập Account ID/alias.
- Sandbox thường có **SCP** (Service Control Policy) của Organizations chặn một số region hoặc dịch vụ. Bị "AccessDenied" dù thấy mình có quyền thì nghĩ tới SCP (học kỹ ở bài 27/10).

## 2. MFA (Multi-Factor Authentication)

**Là gì:** ngoài mật khẩu (thứ bạn biết), phải có thêm thứ bạn có (điện thoại, khoá bảo mật). Lộ mật khẩu vẫn chưa vào được.

**Cần nắm được:**
- [ ] Các loại thiết bị MFA AWS hỗ trợ:

| Loại | Ví dụ | Ghi chú |
|---|---|---|
| Virtual authenticator app | Google Authenticator, Microsoft Authenticator, Authy | Phổ biến nhất, quét QR, nhập **2 mã liên tiếp** khi gán |
| Passkey / security key (FIDO2) | YubiKey, Windows Hello, passkey điện thoại | Chống phishing tốt nhất |
| Hardware TOTP token | Thiết bị sinh mã riêng | Ít gặp |

- [ ] Cách gán cho **IAM user**: IAM → Users → *user của mình* → tab **Security credentials** → *Multi-factor authentication (MFA)* → **Assign MFA device**. Một user gán được tối đa 8 thiết bị.
- [ ] Với **Identity Center**, MFA đăng ký trong AWS access portal (MFA devices), không nằm ở trang IAM user. Nếu sandbox dùng Identity Center thì tiêu chí "trang IAM hiện MFA" cần chụp màn hình tương đương ở portal, ghi rõ trong log.
- [ ] Vì sao nhiều sandbox **ép MFA**: policy có `Condition` `aws:MultiFactorAuthPresent = true`. Chưa bật MFA thì gần như mọi thao tác bị từ chối, trừ việc tự gán MFA. Đây cũng là ví dụ thực tế đầu tiên của `Condition` (bài 27/10).
- [ ] MFA cho **CLI**: access key dùng với CLI không tự có MFA. Nếu policy ép MFA thì phải lấy credential tạm bằng `aws sts get-session-token --serial-number ... --token-code ...`. Chỉ cần biết để ngày mai không bất ngờ.

**Bẫy thường gặp:** đồng hồ điện thoại lệch giờ → mã TOTP sai. Mất điện thoại mà chỉ có 1 MFA → phải nhờ admin gỡ. Nên gán 2 thiết bị (ví dụ app trên điện thoại + passkey Windows Hello).

## 3. AWS Budgets và chi phí

**Cần nắm được:**
- [ ] **Billing and Cost Management** gồm: Bills, **Cost Explorer** (xem chi phí theo ngày/dịch vụ), **Budgets** (đặt ngưỡng và cảnh báo), Free Tier.
- [ ] Một budget gồm: loại (Cost / Usage / Savings Plans / Reservation), số tiền theo kỳ (thường **Monthly**), và tối đa nhiều **alert threshold**:

| Thành phần alert | Ý nghĩa |
|---|---|
| Threshold | % của budget (ví dụ 50%, 80%, 100%) hoặc số tiền tuyệt đối |
| Trigger | **Actual** (đã tiêu thật) hoặc **Forecasted** (AWS dự đoán sẽ vượt cuối kỳ) |
| Người nhận | Email (tối đa 10), SNS topic, Amazon Chatbot/Slack |
| Budget action (tuỳ chọn) | Tự gán policy chặn tạo tài nguyên, hoặc dừng EC2/RDS khi vượt |

- [ ] Đọc ra được và ghi vào log: **tên budget, số tiền, các ngưỡng (actual/forecast), ai nhận email**. Đó chính là tiêu chí "biết cảnh báo gửi tới ai, ở ngưỡng bao nhiêu".
- [ ] **Quyền xem billing:** IAM user chỉ xem được Billing/Budgets nếu root đã bật *IAM user and role access to Billing information* **và** user có quyền như `budgets:ViewBudget`. Không xem được thì đó là thiết kế của sandbox, không phải lỗi của mình → hỏi admin ngưỡng và người nhận, ghi rõ "thông tin do admin cung cấp".
- [ ] Budget **không chặn tiêu tiền**, chỉ cảnh báo (trừ khi có budget action). Dữ liệu chi phí cập nhật trễ vài giờ → không dựa vào budget để phát hiện tức thời.
- [ ] Các khoản tốn tiền của **tuần này** cần để ý ngay từ đầu:

| Bài | Dịch vụ | Tính tiền khi nào | Thói quen |
|---|---|---|---|
| 29/10 | RDS | Theo **giờ chạy** + dung lượng lưu trữ | Tắt (stop) khi không dùng; stop tối đa 7 ngày rồi tự bật lại |
| 28/10 | NAT Gateway (nếu tạo) | Theo giờ + GB dữ liệu, khá đắt | Tránh tạo nếu không cần |
| 30/10 | ECR | Theo GB lưu image | Xoá image cũ |
| 31/10 | App Runner | Theo vCPU/GB khi chạy + phí bộ nhớ khi idle | Pause service khi không dùng |

- [ ] **Tag** tài nguyên (ví dụ `Owner=anhdt1`, `Project=teamboard`) để lọc chi phí theo người trong Cost Explorer. Hỏi team có quy ước tag bắt buộc không.

## 4. Region và Availability Zone

**Cần nắm được:**
- [ ] **Region**: một vùng địa lý (ví dụ `ap-southeast-1` Singapore), độc lập hoàn toàn với region khác. Dữ liệu không tự rời region.
- [ ] **Availability Zone (AZ)**: một hoặc nhiều data center tách biệt trong region (điện, làm mát, mạng riêng), nối với nhau bằng mạng độ trễ thấp. Tên dạng `ap-southeast-1a`, `-1b`, `-1c`. Mỗi region có tối thiểu 3 AZ.
- [ ] **AZ ID** (`apse1-az1`) khác **AZ name** (`ap-southeast-1a`): tên AZ được ánh xạ khác nhau giữa các account. So sánh giữa 2 account thì dùng AZ ID.
- [ ] Ngoài ra có **Local Zone**, **Edge location** (CloudFront). Chỉ cần biết tên, không dùng trong khoá.
- [ ] **Dịch vụ global vs regional:** IAM, Budgets, Organizations là **global** (console hiện "Global"). EC2, VPC, RDS, ECR, App Runner là **regional** → tạo ở region nào chỉ thấy ở region đó. Bẫy kinh điển: "tài nguyên biến mất" vì đang chọn nhầm region ở góc phải console.

**Tiêu chí chọn region** (dùng để viết "lý do" trong log):

| Tiêu chí | Câu hỏi | Với team |
|---|---|---|
| Độ trễ | Người dùng ở đâu? | Việt Nam → Singapore gần, phổ biến |
| Dịch vụ có sẵn | Region có đủ dịch vụ khoá học cần không? | Kiểm tra **App Runner** có trong region (không phải region nào cũng có) |
| Chi phí | Giá cùng dịch vụ khác nhau theo region | So trên trang pricing |
| Tuân thủ | Dữ liệu có bắt buộc nằm ở quốc gia nào không? | Sandbox học tập: thường không |
| Quy định sandbox | SCP có chặn region khác không? | **Hỏi team region nào được phép**, đây là lý do quan trọng nhất |

## 5. Vì sao hệ thống chạy 2 AZ vẫn sống khi 1 AZ sự cố

Cần tự giải thích được bằng lời của mình, theo khung sau:
- [ ] Mỗi AZ cô lập lỗi (fault isolation): sự cố điện, mạng, thiên tai ở AZ-a **không lan** sang AZ-b.
- [ ] Ứng dụng chạy **≥ 2 bản**, mỗi bản ở 1 AZ; **load balancer** (hoặc dịch vụ managed như App Runner) kiểm tra health check và chỉ gửi request tới bản còn khoẻ.
- [ ] Dữ liệu cũng phải Multi-AZ: **RDS Multi-AZ** có bản standby ở AZ khác, đồng bộ (synchronous replication); AZ chính hỏng thì tự **failover**, DNS endpoint trỏ sang standby (thường 1–2 phút). App cần **retry** để vượt qua khoảng này, chính là `EnableRetryOnFailure()` đã có trong [Program.cs](../../../TestManager.Api/Program.cs).
- [ ] Giới hạn: Multi-AZ **không** chống sự cố cả region, không chống xoá nhầm dữ liệu (cần backup), và tốn tiền gấp đôi phần DB.

**Liên hệ các bài sau:** bài 28/10 tạo subnet ở ít nhất 2 AZ, bài 29/10 RDS bắt buộc **DB subnet group có subnet ở ≥ 2 AZ** (kể cả khi chạy Single-AZ). Hiểu mục này là nền cho cả 2 bài.

## 6. IaaS / PaaS / SaaS và mô hình trách nhiệm chung

**Cần nắm được:**
- [ ] Phân biệt theo câu hỏi "**mình còn phải quản lý gì?**":

| | Bạn quản lý | AWS quản lý | Ví dụ AWS (chọn 2 cho log) | TestManager sẽ dùng |
|---|---|---|---|---|
| **IaaS** | OS, runtime, vá lỗi, app, dữ liệu | Phần cứng, ảo hoá, mạng vật lý | **EC2**, **EBS**, VPC | Nếu tự chạy `docker compose` trên EC2 |
| **PaaS** | Code/container, cấu hình, dữ liệu | Thêm OS, runtime, scale, vá lỗi | **App Runner**, **Elastic Beanstalk**, (RDS thường xếp vào nhóm managed/DBaaS) | App Runner (31/10), RDS (29/10) |
| **SaaS** | Chỉ dữ liệu và người dùng | Toàn bộ phần mềm | **Amazon QuickSight**, **Amazon Connect** | Không |

- [ ] **Shared responsibility model:** AWS chịu trách nhiệm *bảo mật **của** cloud* (phần cứng, data center), khách hàng chịu *bảo mật **trong** cloud* (IAM, security group, dữ liệu, mã hoá, vá app). Càng lên PaaS/SaaS, phần của khách hàng càng nhỏ, nhưng **IAM và dữ liệu luôn là việc của mình**.
- [ ] Nhóm ranh giới (biết để không lúng túng khi bị hỏi): Lambda/Fargate thường gọi là **serverless** hoặc FaaS/CaaS; RDS gọi là managed database. Bảng trong log chỉ cần lý do rõ ràng.
- [ ] So sánh với tuần 1: Docker Compose trên máy mình giống "tự làm IaaS"; tuần này chuyển SQL Server container → RDS và API container → App Runner là chuyển dần lên PaaS.

## 7. Làm quen console (10 phút, không bắt buộc ghi log)

- [ ] Thanh trên cùng: ô tìm dịch vụ, **chọn region**, thông tin account, **CloudShell** (terminal có sẵn AWS CLI và credential của mình, dùng được ngay cả trước khi cài CLI trên máy).
- [ ] Thử `aws sts get-caller-identity` trong CloudShell: thấy Account và ARN của mình. Đây là bước làm nóng cho bài 27/10.
- [ ] Ghim (favorite) các dịch vụ sẽ dùng tuần này: IAM, VPC, RDS, ECR, App Runner, Billing.

---

## 8. Nguồn học (theo thứ tự ưu tiên)

| # | Nguồn | Đọc phần | Thời gian |
|---|---|---|---|
| 1 | [AWS global infrastructure](https://aws.amazon.com/about-aws/global-infrastructure/) | Region, AZ, bản đồ | 5' |
| 2 | [Regions and Zones (docs)](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/using-regions-availability-zones.html) | AZ name vs AZ ID | 10' |
| 3 | [IAM: MFA](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_mfa.html) | Loại thiết bị, gán cho IAM user | 10' |
| 4 | [AWS Budgets](https://docs.aws.amazon.com/cost-management/latest/userguide/budgets-managing-costs.html) | Alert, actual vs forecast, người nhận | 10' |
| 5 | [Shared responsibility model](https://aws.amazon.com/compliance/shared-responsibility-model/) | Toàn trang | 5' |
| 6 | [Types of cloud computing](https://aws.amazon.com/types-of-cloud-computing/) | IaaS / PaaS / SaaS | 5' |
| 7 | [Root user best practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/root-user-best-practices.html) | Vì sao không dùng root | 5' |
| 8 | [RDS Multi-AZ](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/Concepts.MultiAZ.html) | Failover, chuẩn bị cho 29/10 | 10' |
| 9 | [AWS Well-Architected: Reliability, *Use fault isolation*](https://docs.aws.amazon.com/wellarchitected/latest/reliability-pillar/use-fault-isolation-to-protect-your-workload.html) | Đào sâu cuối tuần | tuỳ |
| 10 | [App Runner endpoints and quotas](https://docs.aws.amazon.com/general/latest/gr/apprunner.html) | Region nào có App Runner | 2' |

---

## 9. Phân bổ 1,5h gợi ý

| Thời gian | Việc |
|---|---|
| 0:00–0:15 | Đăng nhập sandbox, đổi mật khẩu nếu bị yêu cầu, kiểm tra đúng Account ID |
| 0:15–0:30 | Gán MFA (mục 2), đăng xuất rồi đăng nhập lại để chắc MFA hoạt động, chụp trang IAM |
| 0:30–0:45 | Mở Budgets (mục 3): ghi tên budget, số tiền, ngưỡng, người nhận. Không xem được thì hỏi admin |
| 0:45–1:10 | Đọc nguồn 1, 2, 5, 6. Viết bảng IaaS/PaaS/SaaS và đoạn giải thích 2 AZ bằng lời của mình |
| 1:10–1:30 | Ghi log: region + lý do; thử CloudShell `aws sts get-caller-identity` |

## 10. Tự kiểm tra trước khi coi là "nắm được"

- [ ] Nói được sandbox đăng nhập bằng IAM user hay Identity Center, và điều đó ảnh hưởng gì tới bài ngày mai.
- [ ] Biết vì sao không dùng root user cho việc hằng ngày.
- [ ] Đăng xuất, đăng nhập lại: bị hỏi mã MFA.
- [ ] Đọc thuộc: budget bao nhiêu, ngưỡng nào, gửi cho ai, là actual hay forecast.
- [ ] Giải thích được region vs AZ, và vì sao IAM không có region còn RDS thì có.
- [ ] Giải thích 2 AZ trong 3 câu: cô lập lỗi, ≥ 2 bản + health check, dữ liệu Multi-AZ có failover.
- [ ] Đưa được 2 ví dụ cho mỗi loại IaaS / PaaS / SaaS và chỉ ra phần mình còn phải quản lý.
- [ ] Kể được 2 dịch vụ trong tuần này tính tiền theo giờ và cách tránh tốn tiền khi không dùng.
