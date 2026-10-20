# TestManager – Quản lý môn học, lớp học & thi trắc nghiệm

ASP.NET Core 9 Web API + EF Core + SQL Server, test qua Swagger.
Convert từ project Spring Boot `ManagerTest-Springboot` (Java 17, JPA, MySQL).

## Chạy project

Yêu cầu: .NET SDK 9, SQL Server (instance mặc định `localhost`, Windows Authentication).

```bash
# 1. Tạo database + bảng + dữ liệu mẫu (chạy lại sẽ reset dữ liệu)
sqlcmd -S localhost -E -C -f 65001 -i database/init.sql

# 2. Chạy API
dotnet run --project TestManager.Api --launch-profile http
```

Swagger: http://localhost:5080/swagger. Tài khoản mẫu: mật khẩu đều là `123456`.

Đổi SQL Server khác: sửa `ConnectionStrings:DefaultConnection` trong `TestManager.Api/appsettings.json`
hoặc đặt biến môi trường `ConnectionStrings__DefaultConnection`.

AutoMapper (>= 15) cần license key cho môi trường production (bản Community miễn phí tại https://luckypennysoftware.com):
đặt `AutoMapper:LicenseKey` trong appsettings hoặc biến môi trường `AUTOMAPPER_LICENSE_KEY`.
Không có key app vẫn chạy, chỉ log cảnh báo.

## Mô hình dữ liệu

```
User 1─1 Manager / Teacher / Student         (xoá User ⇒ xoá role đi kèm)
Subject 1─n Topic 1─n Question 1─n Answer
Semester 1─n Class n─n Student               (qua StudentClass)
Exam n─n Question                            (qua ExamQuestion)
Exam n─n Student                             (qua StudentExam: bài làm + điểm)
```

Mã tự sinh bằng computed column: `TC001` (giáo viên), `ST001` (sinh viên), `SE001` (học kỳ),
`CL001` (lớp), `TP001` (chủ đề), `EX001` (đề thi).

## API

Mọi resource đều có đủ `GET /api/x`, `GET /api/x/{id}`, `POST /api/x`, `PUT /api/x/{id}`, `DELETE /api/x/{id}`
(riêng `users` chỉ xem + đổi mật khẩu, vì tài khoản được tạo cùng manager/teacher/student).

| Resource | Endpoint nghiệp vụ thêm | Bản Java tương ứng |
|---|---|---|
| `managers` | `PATCH /{id}/status` đảo trạng thái | `/save-manager`, `/change` |
| `teachers` | | `/save-teacher` |
| `students` | `GET /{id}/exams?isTaken=`, `GET /{id}/classes` | `/save-student`, `/getlist-exam` |
| `users` | `PUT /{id}/password` | `/change-pass` |
| `subjects` | | `/save-Subject` |
| `semesters` | | `/save-semester` |
| `classes` | `GET /{id}/students` | `/save-class` |
| `student-classes` | | `/save-studentClass` |
| `topics` | | `/save-topic` |
| `questions` | `POST /import` (Excel), `GET /import-template` | `/save-question`, `/save-excel` |
| `exams` | `GET/POST /{id}/questions`, `DELETE /{id}/questions/{qId}`, `GET /{id}/students` | `/save-exam`, `/save-examQuestion`, `/getlist-question` |
| `student-exams` | `POST /batch`, `POST /{id}/submit`, `POST /{id}/mark` | `/save-Studentexam`, `/savelist-exam`, `/savelist-answer`, `/save-mark` |

## Quy tắc nghiệp vụ (giữ từ bản Java)

- **Học kỳ**: bắt đầu < kết thúc, kết thúc ≥ hiện tại, không trùng (kể cả chạm mốc) với học kỳ khác.
- **Lớp học**: bắt đầu < kết thúc, kết thúc ≥ hiện tại, nằm trọn trong học kỳ.
- **Đề thi**: bắt đầu < kết thúc. Chỉ thêm được câu hỏi cùng môn với đề.
- **Đăng ký thi**: không đăng ký trùng đề, không trùng giờ với đề đã đăng ký. Đăng ký nhiều: lỗi 1 thì huỷ hết.
- **Nộp bài**: lưu dạng `questionId:answerId:answerId|...`, chỉ nộp một lần.
- **Chấm điểm**: `điểm = totalScore / tổng số đáp án đúng của đề × số đáp án đúng SV chọn`;
  một câu chỉ được tính khi SV chọn ≥ số đáp án đúng của câu đó.
- **Import Excel** (sheet đầu, dòng 1 là tiêu đề): `Mã môn | Mã chủ đề | Câu hỏi | Độ khó | Đáp án | Đúng (x)`.
  Dòng trống cột "Câu hỏi" là đáp án tiếp theo của câu phía trên.
- **Xoá** bản ghi đang được dùng (vd. giáo viên còn lớp, câu hỏi đang trong đề) ⇒ `409 Conflict`.

## Cấu trúc

```
database/init.sql           Script tạo DB, bảng, dữ liệu mẫu
TestManager.Api/
├── Controllers/            12 controller REST
├── Common/
│   ├── Mappings/           AutoMapper Profile, mỗi entity 1 file XMapping (thay mapper/ bên Java) + AddMappings()
│   └── Exceptions/         AppException + AppExceptionHandler (lỗi nghiệp vụ -> 400/409)
├── Services/
│   ├── Interfaces/         IXService (thay service/ bên Java)
│   └── Implementations/    XService + BaseService (thay service/impl/ bên Java)
├── DTOs/                   Mỗi entity 1 DTO dùng chung request/response (như dto/ bên Java)
├── Models/                 Entity EF Core, mỗi entity 1 file (thay entity JPA)
├── Data/AppDbContext.cs    Mapping EF Core ↔ bảng SQL (thay repository JPA)
└── Program.cs
```
# Project_SBU3
# Project_SBU3
