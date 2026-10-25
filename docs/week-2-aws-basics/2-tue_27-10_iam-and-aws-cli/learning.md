# Kiến thức cần nắm: IAM và AWS CLI

> Bài: [spec.md](spec.md) · Thứ 3, 27/10/2026 · 1,5h
> Tài liệu này **chỉ định nghĩa những gì cần học và cần làm được**, chưa phải log kết quả.
> Phụ thuộc bài 26/10: đã đăng nhập được sandbox, đã bật MFA, biết sandbox cấp quyền bằng IAM user hay Identity Center.

---

## 0. Bản đồ: yêu cầu nào cần kiến thức nào

| Yêu cầu / tiêu chí trong spec | Khái niệm cần nắm | Mục |
|---|---|---|
| Hiểu user, group, role, policy | Danh tính IAM, credential dài hạn vs tạm thời, STS | 1 |
| Giải thích khác nhau giữa user và role | Assume role, trust policy | 1 |
| Cấu trúc `Effect`, `Action`, `Resource`, `Condition` | Ngữ pháp policy JSON, ARN | 2, 3 |
| *(ngầm)* vì sao bị AccessDenied | Các loại policy, cách AWS đánh giá | 4 |
| Tự viết policy chỉ đọc object trong 1 bucket | S3 action, ARN bucket vs object, quyền tối thiểu | 5 |
| Policy không dùng `"*"` cho `Action`, `Resource` là ARN cụ thể | Least privilege, cách kiểm tra policy | 5, 6 |
| Cài CLI v2, profile `teamboard` | AWS CLI, file `config`/`credentials`, SSO | 7 |
| `aws sts get-caller-identity --profile teamboard` đúng | Thứ tự tìm credential của CLI | 7 |
| Access key chỉ nằm trong `~/.aws` | Vệ sinh secret, quét repo | 8 |
| Output `docs/iam-notes.md` | Giải thích từng dòng policy | 5, 10 |

**Hiện trạng máy:** chưa cài AWS CLI (`aws` không có trong PATH ở cả Git Bash và PowerShell). Thư mục `%USERPROFILE%\.aws` chưa tồn tại.

---

## 1. Danh tính trong IAM

**Cần nắm được:**
- [ ] **Principal** = thực thể gửi request tới AWS. Mọi request đều được ký bằng credential của một principal.

| Khái niệm | Là gì | Credential | Ví dụ trong khoá |
|---|---|---|---|
| **User** | Một người hoặc ứng dụng cố định | **Dài hạn**: mật khẩu console, access key (`AKIA...`) | User của mình trong sandbox |
| **Group** | Tập hợp user để gán policy chung. **Không phải principal**, không đăng nhập được | Không có | Group `developers` |
| **Role** | Danh tính **không gắn với ai**, ai được tin tưởng thì **assume** để nhận quyền | **Tạm thời** (`ASIA...` + session token, hết hạn 15 phút → 12 giờ) | Role App Runner (tuần 3), role GitHub Actions OIDC (tuần 4), role của Identity Center |
| **Policy** | Tài liệu JSON mô tả quyền | | Policy đọc S3 hôm nay |

- [ ] **User vs Role** (tiêu chí bắt buộc), cần nói được theo 4 ý:
  1. User gắn với 1 danh tính cố định; role ai được phép cũng assume được.
  2. User có credential dài hạn, lộ là dùng được tới khi bị thu hồi; role chỉ có credential tạm thời, tự hết hạn.
  3. Role có 2 policy: **trust policy** (*ai* được assume, có `Principal`) và **permission policy** (assume xong *làm được gì*).
  4. Ứng dụng chạy trên AWS (App Runner, EC2, Lambda) và CI/CD nên dùng role, không dùng access key của user.
- [ ] **STS** (Security Token Service) là dịch vụ cấp credential tạm thời: `AssumeRole`, `GetSessionToken`, `AssumeRoleWithWebIdentity` (OIDC, tuần 4). `GetCallerIdentity` cho biết "tôi đang là ai".
- [ ] Nếu sandbox dùng **Identity Center**, bạn thực chất đăng nhập rồi **assume một role** (permission set). ARN trả về sẽ dạng `arn:aws:sts::<account>:assumed-role/AWSReservedSSO_.../anhdt1`, không phải `:user/...`.

## 2. Cấu trúc policy JSON

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "TenGoiNhoDuoc",
      "Effect": "Allow",
      "Action": ["service:Operation"],
      "Resource": ["arn:aws:..."],
      "Condition": { "Operator": { "condition-key": "value" } }
    }
  ]
}
```

| Phần tử | Bắt buộc | Ý nghĩa | Cần biết |
|---|---|---|---|
| `Version` | Nên có | Phiên bản ngôn ngữ policy | Luôn `2012-10-17`. Thiếu thì biến `${aws:username}` không hoạt động |
| `Statement` | Có | Danh sách quy tắc | Một policy nhiều statement |
| `Sid` | Không | Tên statement | Giúp đọc và giải thích |
| `Effect` | Có | `Allow` hoặc `Deny` | |
| `Action` | Có | `dịch-vụ:TênAPI`, ví dụ `s3:GetObject` | Có wildcard `s3:Get*`, nhưng bài này cấm `"*"` |
| `Resource` | Có (identity policy) | ARN mà action tác động | Mỗi action chỉ áp dụng cho một số **resource type** (mục 5) |
| `Condition` | Không | Điều kiện thêm | Operator (`StringEquals`, `Bool`, `IpAddress`, `StringLike`...) + key (`aws:SourceIp`, `aws:SecureTransport`, `aws:MultiFactorAuthPresent`, `s3:prefix`...) |
| `Principal` | Chỉ resource-based policy | Ai được áp dụng | **Không** có trong policy gắn vào user/role. Có trong bucket policy và trust policy |
| `NotAction`, `NotResource` | Không | Phủ định | Biết để đọc, tránh tự dùng vì dễ cấp thừa quyền |

**Logic trong 1 statement:** nhiều giá trị trong cùng `Action` hoặc `Resource` là **OR**; nhiều key trong `Condition` là **AND**; nhiều giá trị trong cùng 1 key là **OR**.

## 3. ARN (Amazon Resource Name)

- [ ] Dạng chung: `arn:partition:service:region:account-id:resource`.
- [ ] Ví dụ cần đọc được:

| Tài nguyên | ARN | Ghi chú |
|---|---|---|
| S3 bucket | `arn:aws:s3:::teamboard-demo` | **Không có region và account** vì tên bucket là duy nhất toàn cầu |
| Object trong bucket | `arn:aws:s3:::teamboard-demo/*` | `/*` = mọi object; `/uploads/*` = chỉ thư mục `uploads/` |
| IAM user | `arn:aws:iam::123456789012:user/anhdt1` | IAM global → không có region |
| RDS instance | `arn:aws:rds:ap-southeast-1:123456789012:db:teamboard-db` | Regional |
| Secret (tuần 3) | `arn:aws:secretsmanager:ap-southeast-1:123456789012:secret:teamboard-db-AbCdEf` | Có hậu tố ngẫu nhiên 6 ký tự |

## 4. AWS quyết định Allow hay Deny thế nào

**Cần nắm được:**
- [ ] 3 quy tắc cốt lõi:
  1. **Mặc định là Deny** (implicit deny): không có gì cho phép thì bị từ chối.
  2. Có một `Allow` phù hợp thì được phép...
  3. ...trừ khi có **`Deny` tường minh** ở bất kỳ đâu. **Explicit Deny luôn thắng.**
- [ ] Các loại policy tham gia đánh giá:

| Loại | Gắn vào | Vai trò | Gặp trong khoá |
|---|---|---|---|
| Identity-based (AWS managed, customer managed, inline) | User, group, role | Cấp quyền | Hôm nay, tuần 3 |
| Resource-based | Tài nguyên (S3 bucket policy, trust policy của role, ECR repo policy) | Cấp quyền, có `Principal` | Tuần 3 (S3), tuần 4 (OIDC trust) |
| Permissions boundary | User, role | **Giới hạn trần**, không cấp quyền | Đào sâu cuối tuần |
| SCP (Organizations) | Account / OU | **Giới hạn trần** cho cả account | Sandbox gần như chắc chắn có |
| Session policy | Lúc assume role | Giới hạn thêm cho phiên | Biết tên |

- [ ] Quyền thực tế = **giao** của các lớp giới hạn (SCP ∩ boundary ∩ session) với phần được cấp (identity ∪ resource), trừ mọi Deny. Bị `AccessDenied` thì đọc kỹ thông báo lỗi: AWS thường ghi rõ *"...because no identity-based policy allows..."* hoặc *"...with an explicit deny in a service control policy"*.
- [ ] **AWS managed vs customer managed vs inline:** managed dùng lại được nhiều nơi và có version; inline gắn chặt 1 danh tính, xoá danh tính là mất. Bài tập hôm nay nên tạo dạng **customer managed** (nếu sandbox cho tạo).

## 5. Bài tập: policy chỉ đọc object trong 1 bucket S3

Phần này định nghĩa **yêu cầu** và **bẫy**, bạn tự viết policy (đó là output của bài).

**Policy phải thoả:**
- [ ] Chỉ `Allow`, chỉ action đọc object, liệt kê **tên action cụ thể** (không `"*"`, hạn chế cả `s3:*` hay `s3:Get*`).
- [ ] `Resource` là ARN đầy đủ của **đúng 1 bucket**.
- [ ] Mỗi dòng có lý do khi đưa vào `docs/iam-notes.md`.

**Cần tra và quyết định trước khi viết:**

| Câu hỏi | Gợi ý tra cứu |
|---|---|
| Action nào để tải nội dung object? | Bảng *Actions defined by Amazon S3*, cột *Resource types* phải là `object` |
| `Resource` cho action đó là ARN bucket hay ARN object? | Bẫy số 1: action trên object cần `arn:aws:s3:::<bucket>/*`. Ghi ARN bucket (không `/*`) thì policy **không có tác dụng** dù cú pháp đúng |
| "Đọc object" có cần **liệt kê** object không? | `s3:ListBucket` là action trên **bucket** (ARN không `/*`). Quyết định có thêm hay không và ghi lý do. Không có `ListBucket` thì `GetObject` một key không tồn tại trả 403 thay vì 404 |
| Có giới hạn thêm không? | Thêm `Condition` làm điểm cộng: chỉ cho phép qua HTTPS (`aws:SecureTransport`), hoặc giới hạn prefix (`s3:prefix` cho `ListBucket`) |
| Có cần quyền đọc phiên bản cũ, tag, ACL không? | `GetObjectVersion`, `GetObjectTagging`, `GetObjectAcl` là action khác, chỉ thêm nếu có lý do |
| Bucket mã hoá bằng KMS key riêng thì sao? | Cần thêm `kms:Decrypt` trên ARN key. Chỉ ghi nhận, sandbox thường dùng SSE-S3 mặc định |

**Bẫy thường gặp:**
- Gộp ARN bucket và ARN object vào cùng statement với cả `ListBucket` và `GetObject`: chạy được nhưng mỗi action chỉ khớp một ARN → khó đọc. Tách 2 statement có `Sid` rõ ràng thì giải thích dễ hơn.
- Viết `Principal` trong policy gắn vào user → lỗi validate.
- Quên rằng bucket policy có thể `Deny` ngược lại, hoặc Block Public Access không liên quan tới quyền IAM nội bộ.

**Áp dụng vào TestManager:** hiện app chưa dùng S3. Policy hôm nay là bản nháp cho bài **04/11 (S3 và presigned URL)**: role của App Runner sẽ cần quyền đọc/ghi object trong đúng bucket của TestManager. Nên đặt tên bucket theo quy ước của team ngay từ bây giờ, ví dụ `teamboard-<env>-<tên>`.

## 6. Kiểm tra policy trước khi tin

| Công cụ | Làm gì | Khi nào |
|---|---|---|
| **Policy editor** trong console (IAM → Policies → Create) | Báo lỗi cú pháp, cảnh báo bảo mật ngay khi gõ | Luôn |
| **IAM Access Analyzer: policy validation** | Hơn 100 kiểm tra: lỗi, cảnh báo, gợi ý (ví dụ action không khớp resource type) | Trước khi nộp. CLI: `aws accessanalyzer validate-policy --policy-type IDENTITY_POLICY --policy-document file://policy.json` |
| **IAM Policy Simulator** | Giả lập: action X trên resource Y có được phép không | Chứng minh `GetObject` được, `PutObject`/`DeleteObject` bị từ chối |
| Thử thật bằng CLI | `aws s3api get-object` / `put-object` với profile đã gắn policy | Chỉ khi sandbox cho tạo bucket và gắn policy |

Sandbox có thể không cho tạo policy hoặc bucket. Khi đó dùng validation + simulator và ghi rõ trong log.

## 7. AWS CLI v2

**Cài đặt trên Windows:**
- [ ] Tải MSI `AWSCLIV2.msi` (link trong nguồn 3) hoặc `winget install Amazon.AWSCLI`. Mở lại terminal, `aws --version` phải ra `aws-cli/2.x`.
- [ ] Không cài nhầm CLI v1 qua `pip install awscli`.

**Cấu hình profile `teamboard`, chọn theo cách sandbox cấp:**

| Sandbox cấp | Lệnh | Ghi vào đâu |
|---|---|---|
| IAM user + access key | `aws configure --profile teamboard` (nhập Access Key ID, Secret, region, output) | Key vào `~/.aws/credentials`, region/output vào `~/.aws/config` |
| Identity Center (SSO) | `aws configure sso --profile teamboard` (nhập start URL, SSO region, chọn account + role), sau đó `aws sso login --profile teamboard` mỗi khi phiên hết hạn | Chỉ `~/.aws/config`, **không có access key nào** trên máy → an toàn hơn |

- [ ] Trên Windows, `~/.aws` là `C:\Users\pc\.aws\`. Đọc được 2 file này và hiểu từng dòng: `[profile teamboard]` trong `config` nhưng `[teamboard]` trong `credentials`.
- [ ] Thứ tự CLI tìm credential (ưu tiên từ trên xuống), giải thích được vì sao "chạy nhầm account":
  1. Tham số dòng lệnh (`--profile`, `--region`)
  2. Biến môi trường `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` / `AWS_SESSION_TOKEN`, rồi `AWS_PROFILE`
  3. File `credentials` và `config` (profile `default` nếu không chỉ định)
  4. Credential của container/EC2 (role, dùng khi chạy trên AWS)
- [ ] Đặt `$env:AWS_PROFILE="teamboard"` (PowerShell) hoặc `export AWS_PROFILE=teamboard` (Bash) để khỏi gõ `--profile` mỗi lệnh. Nhớ biến này chỉ sống trong cửa sổ terminal đó.

**Lệnh cần làm được:**

| Lệnh | Tác dụng |
|---|---|
| `aws sts get-caller-identity --profile teamboard` | Tiêu chí 1: đọc `Account` (đúng sandbox) và `Arn` (đúng user/role của mình) |
| `aws configure list --profile teamboard` | Xem credential đang lấy từ nguồn nào (file, env...) |
| `aws configure list-profiles` | Liệt kê profile |
| `aws iam list-attached-user-policies --user-name <user>` | Xem policy đang gắn (nếu có quyền) |
| `aws s3 ls --profile teamboard` | Thử một lệnh đọc thật |
| `--query`, `--output table` | Lọc kết quả (JMESPath), ví dụ `--query Account --output text` |
| `aws <lệnh> help` | Tài liệu ngay trong terminal |

**Bẫy thường gặp:**
- `Unable to locate credentials`: quên `--profile`, hoặc profile tên sai.
- `ExpiredToken`: phiên SSO hoặc session token hết hạn → `aws sso login` lại.
- Policy sandbox ép MFA → access key gọi gì cũng `AccessDenied` trừ khi dùng `aws sts get-session-token` với mã MFA (bài 26/10, mục 2).
- Thiếu region → lỗi `You must specify a region`. Region đặt trong profile theo region team đã chốt hôm qua.

## 8. Giữ access key ngoài repo

**Cần nắm được:**
- [ ] Access key ID dạng `AKIA...` (dài hạn) hoặc `ASIA...` (tạm thời). Bot quét GitHub tìm key bị lộ **trong vài phút** sau khi push công khai; AWS cũng tự quét và gắn policy cách ly cho key lộ.
- [ ] Key chỉ nằm ở `~/.aws/credentials` (ngoài repo). Không đặt vào `appsettings*.json`, `.env`, `docker-compose.yml`, `launchSettings.json`, script hay tài liệu `docs/`.
- [ ] Kiểm tra repo trước khi nộp: `git grep -nE "AKIA|ASIA|aws_secret_access_key"` và kiểm tra cả lịch sử `git log -p | grep -E "AKIA|ASIA"`. Hiện `.gitignore` đã chặn `.env`, nhưng không có gì chặn key nếu dán vào file khác.
- [ ] Lỡ commit key: **vô hiệu hoá và xoá key ngay** trong IAM (xoá khỏi lịch sử git là chưa đủ), tạo key mới, báo admin.
- [ ] Hướng tốt nhất: không có access key dài hạn nào cả (Identity Center cho người, role cho app và CI). Đây là mốc của tuần 3: "không còn access key trong code và cấu hình".
- [ ] Ghi chú trong `docs/iam-notes.md` bằng ARN có Account ID **che bớt** hoặc dùng `123456789012`, không dán credential.

## 9. Nguyên tắc quyền tối thiểu (least privilege)

- [ ] Chỉ cấp **đúng action**, trên **đúng resource**, kèm **điều kiện** nếu có thể, trong **thời gian cần thiết**.
- [ ] Cách thực tế: bắt đầu từ ít quyền, gặp `AccessDenied` thì đọc thông báo và thêm đúng action đó. Đừng bắt đầu bằng `AdministratorAccess` rồi định "siết sau".
- [ ] Công cụ hỗ trợ (biết tên): IAM **Last accessed** (dịch vụ nào chưa từng dùng → bỏ quyền), Access Analyzer **policy generation** từ CloudTrail.

---

## 10. Gợi ý khung `docs/iam-notes.md` (output)

1. Thông tin môi trường: cách sandbox cấp (IAM user / SSO), region, profile `teamboard`, kết quả `get-caller-identity` (che Account ID).
2. User vs role: 4 ý ở mục 1, kèm ví dụ role sẽ dùng ở tuần 3 và 4.
3. Policy JSON tự viết.
4. Bảng giải thích **từng dòng**: dòng → ý nghĩa → vì sao cần → nếu bỏ đi thì sao.
5. Bằng chứng kiểm tra: Access Analyzer không có lỗi/cảnh báo; Policy Simulator cho phép đọc, từ chối ghi/xoá.
6. Những gì cố ý **không** cấp và lý do (ví dụ không `ListBucket`, không `PutObject`).

Spec ghi đường dẫn `docs/iam-notes.md`. Trong repo này có thể đặt ở `docs/iam-notes.md` đúng spec, hoặc trong thư mục bài và trỏ link từ spec. Chốt một chỗ và giữ nguyên cho bài `docs/iam-roles.md` tuần 3.

---

## 11. Nguồn học (theo thứ tự ưu tiên)

| # | Nguồn | Đọc phần | Thời gian |
|---|---|---|---|
| 1 | [IAM identities](https://docs.aws.amazon.com/IAM/latest/UserGuide/id.html) | User, group, role; khi nào tạo user vs role | 10' |
| 2 | [IAM policies](https://docs.aws.amazon.com/IAM/latest/UserGuide/access_policies.html) | Các loại policy, cấu trúc JSON | 10' |
| 3 | [Cài AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) | Mục Windows | 5' |
| 4 | [CLI: configuration and credential files](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-files.html) và [SSO](https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-sso.html) | Theo cách sandbox cấp | 10' |
| 5 | [Ví dụ S3 policy](https://docs.aws.amazon.com/AmazonS3/latest/userguide/example-policies-s3.html) | Ví dụ đọc object, `ListBucket` theo prefix | 10' |
| 6 | [Actions, resources, condition keys for Amazon S3](https://docs.aws.amazon.com/service-authorization/latest/reference/list_amazons3.html) | Tra cột *Resource types* của action đã chọn | 5' |
| 7 | [IAM JSON policy elements reference](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_elements.html) | `Effect`, `Action`, `Resource`, `Condition` | tra cứu |
| 8 | [Policy evaluation logic](https://docs.aws.amazon.com/IAM/latest/UserGuide/reference_policies_evaluation-logic.html) | Sơ đồ quyết định | 10' |
| 9 | [IAM Access Analyzer policy validation](https://docs.aws.amazon.com/IAM/latest/UserGuide/access-analyzer-policy-validation.html) | Cách chạy | 5' |
| 10 | [IAM security best practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html) | Đọc cuối tuần | tuỳ |
| 11 | [Permissions boundaries](https://docs.aws.amazon.com/IAM/latest/UserGuide/access_policies_boundaries.html) | Đào sâu cuối tuần | tuỳ |

---

## 12. Phân bổ 1,5h gợi ý

| Thời gian | Việc |
|---|---|
| 0:00–0:15 | Cài AWS CLI v2, cấu hình profile `teamboard`, chạy `get-caller-identity` (tiêu chí 1) |
| 0:15–0:40 | Đọc nguồn 1, 2, 8. Tự viết 4 ý user vs role |
| 0:40–1:05 | Tra nguồn 5, 6, viết policy đọc S3 theo mục 5 |
| 1:05–1:15 | Kiểm tra bằng Access Analyzer + Policy Simulator (mục 6) |
| 1:15–1:30 | Viết `docs/iam-notes.md`, chạy `git grep` kiểm tra không lộ key |

## 13. Tự kiểm tra trước khi coi là "nắm được"

- [ ] Giải thích user vs role trong 4 ý, không nhìn tài liệu.
- [ ] Nói được group có phải principal không, và vì sao.
- [ ] Đọc một policy bất kỳ và nói ra: ai, làm gì, trên tài nguyên nào, với điều kiện gì.
- [ ] Biết `Principal` xuất hiện trong loại policy nào và không xuất hiện trong loại nào.
- [ ] Giải thích quy tắc: mặc định Deny, Allow để cho phép, explicit Deny luôn thắng; SCP và boundary chỉ giới hạn, không cấp quyền.
- [ ] Biết vì sao `s3:GetObject` cần ARN `bucket/*` còn `s3:ListBucket` cần ARN bucket.
- [ ] Chỉ ra được credential của CLI đang lấy từ đâu (`aws configure list`) và thứ tự ưu tiên.
- [ ] Phân biệt `AKIA` và `ASIA`, biết phải làm gì ngay khi lỡ commit key.
