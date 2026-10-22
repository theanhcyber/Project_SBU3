# Giải thích chi tiết: Migration khi khởi động + Seed dữ liệu mẫu

Tài liệu này đi qua **từng file đã thêm hoặc sửa** trong bài 22/10, **từng lệnh đã chạy thực tế**, **luồng chạy** của app, rồi hướng dẫn build/run bằng terminal, **Visual Studio 2022**, VS Code và Docker Compose.

> Spec và kết quả: [spec.md](spec.md) · Kiến thức nền: [learning.md](learning.md)

---

## 1. Tổng quan các file

| File | Thêm hay sửa | Vai trò | Yêu cầu đề |
|---|---|---|---|
| `TestManager.Api/TestManager.Api.csproj` | Sửa | Thêm package `Microsoft.EntityFrameworkCore.Design` để tool `dotnet ef` chạy được | Migration |
| `.config/dotnet-tools.json` | **Thêm** | Khai báo tool `dotnet-ef` 9.0.21 cho cả repo (local tool) | Migration |
| `TestManager.Api/Data/Migrations/20261022161533_InitialCreate.cs` | **Thêm** (sinh tự động) | Lệnh tạo / xoá toàn bộ bảng, khoá, index | Migration |
| `TestManager.Api/Data/Migrations/20261022161533_InitialCreate.Designer.cs` | **Thêm** (sinh tự động) | Ảnh chụp model tại thời điểm tạo migration này | Migration |
| `TestManager.Api/Data/Migrations/AppDbContextModelSnapshot.cs` | **Thêm** (sinh tự động) | Ảnh chụp model **mới nhất**, dùng để so sánh khi tạo migration sau | Migration |
| `TestManager.Api/Data/DbSeeder.cs` | **Thêm** | Seed 1 lớp mẫu, không seed trùng | Seed |
| `TestManager.Api/Program.cs` | Sửa | Bật retry khi kết nối DB lỗi; tự migrate + seed khi Development | Migration, DB chưa sẵn sàng |
| `TestManager.Api/Services/Implementations/StudentExamService.cs` | Sửa | Bọc transaction trong execution strategy (bắt buộc khi bật retry) | Hệ quả của retry |
| `docker-compose.yml` | Sửa | Bỏ service `db-init` | Migration thay `init.sql` |
| `README.md`, `docs/week-1-docker/README.md`, `spec.md` | Sửa | Hướng dẫn chạy mới, trạng thái, kết quả | |
| `database/init.sql` | Giữ, không dùng nữa | Chỉ còn để tham khảo | |

---

## 2. Bức tranh tổng thể

### 2.1 Hai "thời điểm" khác nhau: lúc viết code và lúc chạy app

```
 LÚC VIẾT CODE (design-time, chạy trên máy dev)            LÚC CHẠY APP (runtime, máy dev hoặc container)
┌───────────────────────────────────────────────┐        ┌──────────────────────────────────────────────────┐
│ Sửa Model / AppDbContext                       │        │ dotnet TestManager.Api.dll                        │
│        │                                       │        │        │                                          │
│        ▼                                       │        │        ▼                                          │
│ dotnet ef migrations add <Ten>                 │        │ builder.Build()                                   │
│   ├─ build project                             │        │        │                                          │
│   ├─ chạy Program.cs tới builder.Build()       │        │        ▼                                          │
│   │  để lấy AppDbContext (rồi DỪNG)            │        │ IsDevelopment()? ──không──▶ bỏ qua, chạy app      │
│   ├─ so model hiện tại  vs  ModelSnapshot.cs   │        │        │ có                                       │
│   └─ sinh ra 3 file trong Data/Migrations/  ───┼──────▶ │        ▼                                          │
│        (commit vào git, build vào DLL)         │  DLL   │ MigrateAsync()                                    │
└───────────────────────────────────────────────┘        │   ├─ DB chưa có? → CREATE DATABASE                │
                                                          │   ├─ khoá __EFMigrationsLock                      │
                                                          │   ├─ đọc __EFMigrationsHistory                    │
                                                          │   ├─ chạy migration chưa có trong lịch sử         │
                                                          │   └─ ghi lịch sử                                  │
                                                          │        ▼                                          │
                                                          │ DbSeeder.SeedAsync()                              │
                                                          │   ├─ đã có Subject 'CS01'? → bỏ qua              │
                                                          │   └─ chưa có → thêm lớp mẫu, 1 lần SaveChanges   │
                                                          │        ▼                                          │
                                                          │ app.Run()  → nhận request                         │
                                                          └──────────────────────────────────────────────────┘
```

**Ý chính:**
- Migration là **code C#**, được sinh ra ở máy dev, commit vào git và **build vào trong DLL**. Container không cần tool `dotnet ef`, vì `MigrateAsync()` đọc migration có sẵn trong DLL.
- Khi chạy `dotnet ef`, tool dừng ở `builder.Build()`, nên khối migrate/seed (sau `Build()`) **không chạy** lúc tạo migration.

### 2.2 Luồng khởi động bằng Docker Compose

```
docker compose up
   │
   ├─▶ db (SQL Server) khởi động ── healthcheck "SELECT 1" mỗi 10s ── healthy
   │                                                                     │
   └─▶ api CHỜ (depends_on: condition: service_healthy) ◀───────────────┘   ← lớp bảo vệ 1 (hạ tầng)
          │
          ▼
       ASPNETCORE_ENVIRONMENT=Development → MigrateAsync()
          │  lỗi kết nối tạm thời? → EnableRetryOnFailure thử lại tối đa 6 lần   ← lớp bảo vệ 2 (ứng dụng)
          ▼
       Seed → app.Run() → http://localhost:8080
```

Trước bài này, luồng là `db` → `db-init` (chạy `init.sql`) → `api`. Giờ `api` tự lo schema, nên `db-init` bị bỏ.

---

## 3. Giải thích từng file

### 3.1 `TestManager.Api/TestManager.Api.csproj` (thêm dòng 14–17)

```xml
14 <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.21">
15   <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
16   <PrivateAssets>all</PrivateAssets>
17 </PackageReference>
```

| Dòng | Giải thích |
|---|---|
| 14 | Package chứa phần "design-time" của EF Core: code sinh migration, so sánh model. Tool `dotnet ef` **bắt buộc** project phải có package này, thiếu thì báo *"Your startup project doesn't reference Microsoft.EntityFrameworkCore.Design"*. Phiên bản **9.0.21 trùng với EF Core SqlServer** (dòng 18) |
| 15–16 | `PrivateAssets=all`: package chỉ dùng lúc phát triển, **không** bị kéo theo nếu project khác tham chiếu project này, và không cần ở runtime. Lệnh `dotnet add package` tự thêm 2 dòng này |

Lệnh đã dùng:
```powershell
dotnet add TestManager.Api package Microsoft.EntityFrameworkCore.Design --version 9.0.21
```

### 3.2 `.config/dotnet-tools.json` (file mới)

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "dotnet-ef": {
      "version": "9.0.21",
      "commands": [ "dotnet-ef" ],
      "rollForward": false
    }
  }
}
```

| Phần | Giải thích |
|---|---|
| File này là gì | **Tool manifest**: danh sách công cụ .NET dùng riêng cho repo này (giống `package.json` cho tool). Tool được cài theo repo, không cài global cho cả máy |
| `isRoot: true` | Dừng tìm manifest ở thư mục này, không tìm lên thư mục cha |
| `dotnet-ef` 9.0.21 | Mọi người (và CI ở tuần 4) dùng **đúng một phiên bản** tool, cùng phiên bản với EF Core. Tool lệch phiên bản dễ sinh migration sai |
| `rollForward: false` | Không tự dùng phiên bản mới hơn |

Lệnh đã dùng:
```powershell
dotnet new tool-manifest                          # tạo .config/dotnet-tools.json
dotnet tool install dotnet-ef --version 9.0.21    # thêm dotnet-ef vào manifest
```
Người khác clone repo chỉ cần chạy `dotnet tool restore` là có tool.

> **Global tool vs local tool:** `dotnet tool install -g dotnet-ef` cài cho cả máy, phiên bản ai cài người nấy. Local tool nằm trong repo, đúng phiên bản cho mọi người. Cách gọi giống nhau: `dotnet ef ...`.

### 3.3 Thư mục `TestManager.Api/Data/Migrations/` (sinh tự động, 3 file)

Sinh bằng:
```powershell
dotnet ef migrations add InitialCreate --project TestManager.Api --output-dir Data/Migrations
```

| Tham số | Ý nghĩa |
|---|---|
| `migrations add` | So sánh model hiện tại với snapshot, sinh migration mới |
| `InitialCreate` | Tên migration (đặt theo nội dung thay đổi, ví dụ `AddExamDuration`) |
| `--project TestManager.Api` | Project chứa `AppDbContext`. Chạy từ thư mục gốc repo nên phải chỉ rõ |
| `--output-dir Data/Migrations` | Đặt migration cạnh `AppDbContext` (mặc định là `Migrations/` ở gốc project). Namespace sinh ra là `TestManager.Api.Data.Migrations` |

#### a) `20261022161533_InitialCreate.cs`: nội dung thay đổi

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Semesters",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ...
                Code = table.Column<string>(type: "nvarchar(max)", nullable: false,
                    computedColumnSql: "CONCAT(N'SE', ...)", stored: true)
            },
            constraints: table => { table.PrimaryKey("PK_Semesters", x => x.Id); });
        ... // 14 CreateTable, 23 CreateIndex
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Answers");
        ... // DropTable theo thứ tự ngược khoá ngoại
    }
}
```

| Phần | Giải thích |
|---|---|
| Tên file `20261022161533_` | Dấu thời gian lúc tạo (UTC, `yyyyMMddHHmmss`). EF **sắp xếp migration theo tên**, nên tiền tố này quyết định thứ tự chạy |
| `Up()` | Đi tới: tạo bảng, cột, khoá chính, khoá ngoại, index. `MigrateAsync()` gọi hàm này |
| `Down()` | Quay lui: xoá những gì `Up()` đã tạo. Dùng khi `dotnet ef database update <migration trước>` |
| `Annotation("SqlServer:Identity", "1, 1")` | Cột `IDENTITY(1,1)`, tương ứng `long Id` |
| `computedColumnSql: ..., stored: true` | Từ `HasComputedColumnSql(CodeSql("SE"), stored: true)` trong `AppDbContext`: cột tính toán `PERSISTED` (`SE001`, `CL001`…) |
| `nvarchar(450)` vs `nvarchar(max)` | Cột có index (`Email`, `UserName`, `Code`) được EF cho dài 450 vì SQL Server không cho index trên `nvarchar(max)`. Cột khác không khai báo `HasMaxLength` nên là `max`. **Đây là điểm khác `init.sql`** (`NVARCHAR(150)`, `NVARCHAR(50)`…) |
| `onDelete: ReferentialAction.Cascade / NoAction` | Lấy từ `OnDelete(DeleteBehavior...)` trong `AppDbContext` |

#### b) `20261022161533_InitialCreate.Designer.cs`: metadata của migration

```csharp
[DbContext(typeof(AppDbContext))]
[Migration("20261022161533_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) { ... }
}
```

| Phần | Giải thích |
|---|---|
| `[DbContext(typeof(AppDbContext))]` | Migration này thuộc `AppDbContext` (một project có thể có nhiều DbContext) |
| `[Migration("20261022161533_InitialCreate")]` | **ID của migration**. Đây chính là giá trị được ghi vào cột `MigrationId` của bảng `__EFMigrationsHistory` |
| `BuildTargetModel` | Model tại thời điểm migration này được tạo |
| `partial class` | Cùng class với file (a), tách ra để file (a) dễ đọc |

#### c) `AppDbContextModelSnapshot.cs`: ảnh chụp model mới nhất

| Phần | Giải thích |
|---|---|
| Vai trò | Lần `migrations add` sau, EF so **model trong code** với **snapshot này** để biết cái gì đã đổi |
| `HasAnnotation("ProductVersion", "9.0.21")` | Phiên bản EF đã sinh snapshot |
| Quy tắc | **Không sửa tay.** Muốn bỏ migration cuối cùng (chưa áp dụng ở đâu) thì dùng `dotnet ef migrations remove`, lệnh này tự sửa lại snapshot |

> **Không sửa file migration đã được áp dụng ở DB của người khác.** Muốn thay đổi, tạo migration mới. Lý do: `__EFMigrationsHistory` của họ đã ghi ID đó là "đã chạy", nên nội dung sửa sẽ không bao giờ được chạy lại.

### 3.4 `TestManager.Api/Program.cs` (sửa dòng 14–16, thêm dòng 52–60)

#### Đăng ký DbContext với retry (dòng 14–16)

```csharp
14 builder.Services.AddDbContext<AppDbContext>(options =>
15     options.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
16         sql => sql.EnableRetryOnFailure()));
```

| Dòng | Giải thích |
|---|---|
| 15 | Như cũ: dùng SQL Server, connection string lấy từ `ConnectionStrings:Default` (appsettings hoặc biến môi trường `ConnectionStrings__Default`) |
| 16 | **Mới.** Tham số thứ 2 cấu hình riêng cho SQL Server. `EnableRetryOnFailure()` bật `SqlServerRetryingExecutionStrategy`: lỗi được coi là **tạm thời** (mất kết nối, timeout, deadlock, DB đang khởi động…) sẽ được **thử lại tối đa 6 lần**, khoảng chờ tăng dần, tối đa 30 giây mỗi lần. Lỗi không tạm thời (sai mật khẩu, sai tên bảng) thì báo lỗi ngay |

**Ảnh hưởng tới toàn app:** từ giờ, mọi chỗ **tự mở transaction** (`BeginTransaction`) phải chạy trong execution strategy (xem mục 3.6). Các lệnh `SaveChanges`, `ToListAsync`, `ExecuteDeleteAsync` thông thường thì không cần sửa gì.

#### Migrate + seed khi khởi động (dòng 52–60)

```csharp
48 var app = builder.Build();
..
52 // Chỉ tự migrate + seed ở Development. Production chạy migration thành bước riêng trong pipeline.
53 if (app.Environment.IsDevelopment())
54 {
55     using var scope = app.Services.CreateScope();
56     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
57     app.Logger.LogInformation("Applying EF Core migrations...");
58     await db.Database.MigrateAsync();
59     await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>(), app.Logger);
60 }
..
   app.Run();
```

| Dòng | Giải thích |
|---|---|
| 48 | Sau `Build()` mới có `app.Services` (DI container) và `app.Environment`. Tool `dotnet ef` dừng **tại đây**, nên khối dưới không chạy khi tạo migration |
| 53 | `IsDevelopment()` đúng khi `ASPNETCORE_ENVIRONMENT=Development`. Không đặt biến → mặc định **Production** → bỏ qua khối này. Nguồn của biến: `launchSettings.json` (khi F5 / `dotnet run`), `environment:` trong `docker-compose.yml` (khi chạy container) |
| 55 | `AppDbContext` đăng ký **Scoped** (mỗi request một instance). Ngoài request thì không có scope, nên phải tự tạo. `using var` đảm bảo scope và DbContext bị huỷ khi ra khỏi khối `if` |
| 56 | Lấy `AppDbContext` từ scope vừa tạo |
| 57 | Log mốc bắt đầu, để thấy được trong `docker compose logs -f api` |
| 58 | **Lệnh chính của bài.** `MigrateAsync()`: tạo DB nếu chưa có → lấy khoá `__EFMigrationsLock` → tạo `__EFMigrationsHistory` nếu chưa có → chạy các migration chưa có trong lịch sử → ghi lịch sử. Lỗi thì ném exception → app **không khởi động** (container exit code ≠ 0), không chạy dở dang trong im lặng |
| 59 | Seed sau khi schema đã có. `IPasswordHasher<User>` lấy từ DI (đã đăng ký Singleton ở dòng 22) để hash mật khẩu giống hệt lúc tạo user qua API |
| `await` ở top-level | `Program.cs` dùng top-level statements nên `await` trực tiếp được, không cần `Main` async |

**Vì sao đặt trước `app.Run()`?** Để app chỉ nhận request khi DB đã đúng schema. Đặt sau `Run()` thì request đầu có thể gặp bảng chưa tồn tại.

### 3.5 `TestManager.Api/Data/DbSeeder.cs` (file mới)

```csharp
10 private const string SampleSubjectCode = "CS01";
11 private const string SamplePassword = "123456";
13 public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher, ILogger logger)
```

| Dòng | Giải thích |
|---|---|
| 8 | `static class`: không có trạng thái, không cần đăng ký DI, gọi thẳng `DbSeeder.SeedAsync(...)` |
| 10 | Mã môn mẫu, dùng làm **khoá tự nhiên** để nhận biết "đã seed rồi". `Subject.Code` có unique index nên mỗi giá trị chỉ có 1 dòng |
| 11 | Mật khẩu mẫu, giống `init.sql` cũ |
| 13 | Nhận DbContext, hasher, logger qua tham số (không tự tạo), dễ test |

#### Chống seed trùng (dòng 15–20)

```csharp
16 if (await db.Subjects.AnyAsync(s => s.Code == SampleSubjectCode))
17 {
18     logger.LogInformation("Seed: dữ liệu mẫu đã có (Subject {Code}), bỏ qua.", SampleSubjectCode);
19     return;
20 }
```

| Dòng | Giải thích |
|---|---|
| 16 | Sinh SQL `SELECT CASE WHEN EXISTS (SELECT 1 FROM Subjects WHERE Code = N'CS01') ...`. Có thì dừng. Đây là điều kiện để **seed idempotent** (chạy mấy lần cũng ra cùng kết quả) |
| 18 | `{Code}` là **structured logging**: giá trị được lưu thành field riêng, không nối chuỗi. Sau này đẩy log lên CloudWatch (tuần 3) có thể lọc theo field |
| Vì sao không dùng `db.Subjects.Any()`? | `Any()` trên cả bảng sai trong 2 trường hợp: người dùng đã thêm môn khác nhưng xoá môn mẫu (không seed lại, đúng ra phải seed), hoặc ngược lại. Kiểm tra theo đúng bản ghi mẫu thì chính xác hơn |

#### Hàm tạo user (dòng 22–34)

```csharp
22 User NewUser(string userName, string firstName, string middleName, string lastName)
23 {
24     var user = new User { Email = $"{userName}@example.com", UserName = userName, ... };
32     user.PasswordHash = hasher.HashPassword(user, SamplePassword);
33     return user;
34 }
```

| Dòng | Giải thích |
|---|---|
| 22 | **Local function**: hàm nằm trong hàm, chỉ dùng ở đây, đọc được biến `hasher` bên ngoài |
| 32 | `PasswordHasher` hash bằng PBKDF2 + **salt ngẫu nhiên**, nên cùng `123456` mỗi lần cho ra chuỗi khác. Đây là lý do **không dùng `HasData()`** (seed nằm trong migration): mỗi lần `migrations add`, EF sẽ thấy hash "thay đổi" và sinh `UpdateData` vô nghĩa |

#### Tạo đồ thị đối tượng (dòng 36–59)

```csharp
36 var subject  = new Subject  { SubjectName = "Lập trình C#", Code = SampleSubjectCode };
37 var semester = new Semester { Name = "Học kỳ 1 2026-2027", ... };
38 var teacher  = new Teacher  { ..., Subject = subject, User = NewUser("hoant", ...) };
43 Student[] students = [ new() { ..., User = NewUser("anlv", ...) }, ... ];
50 db.Classes.Add(new ClassRoom
51 {
55     Subject = subject, Semester = semester, Teacher = teacher,
58     StudentClasses = students.Select(s => new StudentClass { Student = s }).ToList()
59 });
```

| Dòng | Giải thích |
|---|---|
| 36–48 | Tạo đối tượng và **nối với nhau bằng navigation property** (`Subject = subject`), không gán `SubjectId` bằng số. Lúc này chưa có Id vì Id do DB sinh (`IDENTITY`) |
| 43 | `Student[] students = [...]`: cú pháp collection expression (C# 12) |
| 50 | Chỉ `Add` **một** đối tượng gốc là `ClassRoom`. EF tự duyệt cả đồ thị: thấy `Subject`, `Semester`, `Teacher`, `Teacher.User`, `StudentClasses`, `Student`, `Student.User` đều mới nên đánh dấu tất cả là `Added` |
| 58 | Bảng nối `StudentClasses`: mỗi sinh viên một dòng nối với lớp |
| Không gán `Code`, `StudentCode` | Đây là cột tính toán (`HasComputedColumnSql`), EF không chèn giá trị mà đọc lại sau khi insert (`CL001`, `ST001`…) |

#### Lưu (dòng 61–63)

```csharp
62 await db.SaveChangesAsync();
63 logger.LogInformation("Seed: đã tạo lớp mẫu 'C# - K1' với {Count} sinh viên.", students.Length);
```

| Dòng | Giải thích |
|---|---|
| 62 | EF sắp thứ tự `INSERT` theo khoá ngoại (Users → Subjects/Semesters → Teachers/Students → Classes → StudentClasses), lấy Id vừa sinh gán cho khoá ngoại tiếp theo. Tất cả nằm trong **một transaction**: lỗi giữa chừng thì không có dòng nào được lưu, lần khởi động sau seed lại từ đầu. Vì không tự mở transaction nên không vướng luật của execution strategy |

**Dữ liệu sau khi seed:**

| Bảng | Dòng |
|---|---|
| Users | 4 (`hoant`, `anlv`, `binhpt`, `cuonghd`, mật khẩu `123456`) |
| Subjects | 1 (`CS01` Lập trình C#) |
| Semesters | 1 (`SE001`) |
| Teachers | 1 (`TC001` Nguyễn Thị Hoa) |
| Students | 3 (`ST001`–`ST003`) |
| Classes | 1 (`CL001` C# - K1) |
| StudentClasses | 3 |

→ `GET /api/classes/1/students` trả 3 sinh viên. Bài Redis ngày mai sẽ cache chính endpoint này.

### 3.6 `StudentExamService.cs`: `RegisterManyAsync` (dòng 33–47)

**Trước:**
```csharp
public async Task<List<StudentExamDto>> RegisterManyAsync(List<StudentExamDto> dtos)
{
    await using var transaction = await Db.Database.BeginTransactionAsync();
    ...
    await transaction.CommitAsync();
    return result;
}
```

**Sau:**
```csharp
33 // EnableRetryOnFailure không cho tự mở transaction ngoài execution strategy: retry phải chạy lại cả khối
34 public Task<List<StudentExamDto>> RegisterManyAsync(List<StudentExamDto> dtos) =>
35     Db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
36     {
37         Db.ChangeTracker.Clear();
38         await using var transaction = await Db.Database.BeginTransactionAsync();
39         var result = new List<StudentExamDto>();
40         for (var i = 0; i < dtos.Count; i++) { ... RegisterAsync(dtos[i]) ... }
45         await transaction.CommitAsync();
46         return result;
47     });
```

**Vì sao phải sửa:** bật retry (mục 3.4) xong, nếu giữ code cũ thì gọi `POST /api/student-exams/batch` sẽ lỗi ngay:
> *InvalidOperationException: The configured execution strategy 'SqlServerRetryingExecutionStrategy' does not support user-initiated transactions.*

Lý do: giả sử mất kết nối ở phần tử thứ 3. EF không thể chỉ thử lại câu SQL cuối, vì 2 phần tử đầu nằm trong transaction đã bị huỷ theo kết nối. Muốn đúng thì phải **chạy lại cả khối** từ đầu, và chỉ có code của mình mới biết "cả khối" là gì.

| Dòng | Giải thích |
|---|---|
| 35 | `CreateExecutionStrategy()` lấy strategy retry đã cấu hình. `ExecuteAsync(lambda)`: chạy lambda, gặp lỗi tạm thời thì **chạy lại toàn bộ lambda** |
| 37 | **Quan trọng khi chạy lại.** Lần thử trước đã `Add` các `StudentExam` vào change tracker. Không `Clear()` thì lần thử sau sẽ insert lặp |
| 38–45 | Logic cũ giữ nguyên: mở transaction, đăng ký từng phần tử, lỗi nghiệp vụ (`AppException`) thì ném ra (transaction bị huỷ khi `await using` dispose, tức là rollback), đủ hết thì commit |
| 34 | Đổi từ `async` method sang `=>` trả thẳng `Task` từ `ExecuteAsync`, kết quả như cũ |

**Đã test:** lô 2 phần tử hợp lệ trả `200`. Lô `[hợp lệ, trùng]` trả `409 "Phần tử thứ 2: ..."`, và phần tử 1 **không** được lưu (rollback đúng).

> Lỗi nghiệp vụ (`AppException`) **không** phải lỗi tạm thời nên strategy không retry, ném thẳng ra ngoài như trước.

### 3.7 `docker-compose.yml` (xoá 21 dòng)

**Đã xoá:** toàn bộ service `db-init` (chạy `sqlcmd -i /scripts/init.sql` khi DB chưa có), và điều kiện chờ nó trong `api`:

```yaml
# ĐÃ XOÁ khỏi api.depends_on:
      db-init:
        condition: service_completed_successfully
```

**Còn lại trong `api` (dòng 23–28):**

```yaml
23     depends_on:
24       db:
25         condition: service_healthy
26     environment:
27       ConnectionStrings__Default: "Server=db,1433;Database=TestManagerDb;User Id=sa;Password=${MSSQL_SA_PASSWORD};..."
28       ASPNETCORE_ENVIRONMENT: Development
```

| Dòng | Giải thích |
|---|---|
| 23–25 | Lớp bảo vệ 1: chỉ start `api` khi healthcheck của `db` (`SELECT 1`) đã thành công |
| 27 | `Database=TestManagerDb`: DB chưa có thì `MigrateAsync()` tự `CREATE DATABASE`. Tài khoản `sa` có quyền tạo DB (ở production sẽ không dùng `sa`, xem mục 8.3 của spec) |
| 28 | **Điều kiện để migrate chạy.** Bỏ dòng này thì app chạy ở Production, không migrate, DB trống → mọi API lỗi "Invalid object name" |

**Vì sao xoá `db-init`?** Nếu giữ thì có 2 nơi cùng tạo schema: `init.sql` tạo bảng nhưng không có `__EFMigrationsHistory`, sau đó `MigrateAsync()` cố tạo lại bảng và lỗi *"There is already an object named 'Users'"*.

---

## 4. Các lệnh đã dùng thực tế

Chạy từ thư mục gốc repo (`Project_SBU3`).

| # | Lệnh | Tác dụng | Kết quả lúc làm bài |
|---|---|---|---|
| 1 | `dotnet --list-sdks` | Xem SDK đã cài | Chỉ có 8.0.100 → không build được (`global.json` cần 9.0.201) |
| 2 | `.\dotnet-install.ps1 -Channel 9.0 -InstallDir "$env:USERPROFILE\.dotnet"` | Cài SDK 9 vào thư mục user, không cần admin | Cài 9.0.319 (`rollForward: latestFeature` trong `global.json` chấp nhận 9.0.3xx) |
| 3 | `dotnet new tool-manifest` | Tạo `.config/dotnet-tools.json` | |
| 4 | `dotnet tool install dotnet-ef --version 9.0.21` | Thêm `dotnet-ef` vào manifest | |
| 5 | `dotnet add TestManager.Api package Microsoft.EntityFrameworkCore.Design --version 9.0.21` | Thêm package vào csproj | |
| 6 | `dotnet build TestManager.Api` | Build | 0 warning, 0 error |
| 7 | `dotnet ef migrations add InitialCreate --project TestManager.Api --output-dir Data/Migrations` | Sinh migration | 3 file trong `Data/Migrations/` |
| 8 | `dotnet ef migrations script --project TestManager.Api -o initial.sql` | Xuất SQL mà migration sẽ chạy, để **đọc và so với `init.sql`** | 211 dòng, 14 bảng, đủ FK và unique index |
| 9 | `dotnet ef migrations has-pending-model-changes --project TestManager.Api` | Kiểm tra model có thay đổi chưa tạo migration không | `No changes have been made to the model since the last migration.` |
| 10 | Chạy app với `ASPNETCORE_ENVIRONMENT=Development`, connection string trỏ tới DB test | Test migrate + seed (2 lần) | Lần 1 tạo DB + seed, lần 2 "bỏ qua" |
| 11 | `sqlcmd -S localhost\THEANH -E -C -d <db> -Q "SELECT ... COUNT(*) ..."` | Đếm dòng để xác nhận không trùng | 1 subject, 1 class, 3 students, 4 users |
| 12 | Chạy app với `ASPNETCORE_ENVIRONMENT=Production` | Xác nhận không migrate ở Production | 0 dòng "Applying EF Core migrations" |

**Các lệnh `dotnet ef` khác nên biết:**

| Lệnh | Khi nào dùng |
|---|---|
| `dotnet ef migrations list` | Xem danh sách migration, cái nào đã áp dụng vào DB (`(Pending)` = chưa) |
| `dotnet ef migrations remove` | Xoá migration **cuối cùng** khi nó **chưa** áp dụng ở đâu (tạo nhầm) |
| `dotnet ef database update` | Áp dụng migration vào DB bằng tay (thay vì chờ app khởi động) |
| `dotnet ef database update <TenMigration>` | Quay về đúng migration đó (chạy `Down()` của các migration sau nó) |
| `dotnet ef database update 0` | Gỡ hết migration (chạy mọi `Down()`) |
| `dotnet ef database drop` | Xoá DB (có hỏi xác nhận; `--force` để bỏ hỏi) |
| `dotnet ef migrations script --idempotent` | SQL chạy được nhiều lần an toàn, dùng cho production (tuần 5) |
| `dotnet ef migrations bundle` | Đóng gói migration thành file `efbundle.exe` chạy độc lập (tuần 5) |
| `dotnet ef dbcontext info` | Xem DbContext đang dùng provider, DB, connection nào |

> Các lệnh có đụng DB (`database update`, `migrations list`, `dbcontext info`) dùng connection string của `appsettings.json` / biến môi trường **trên máy bạn**, không phải của container.

---

## 5. Build và chạy

### 5.0 Chuẩn bị một lần

| Việc | Lệnh / cách làm |
|---|---|
| Có .NET SDK 9 | `dotnet --list-sdks` phải có dòng `9.0.x`. Nếu SDK nằm ở `%USERPROFILE%\.dotnet` (như máy này), thêm thư mục đó vào **đầu** biến môi trường `PATH` (Windows: *Edit the system environment variables* → *Environment Variables* → `Path` của user), hoặc cài SDK 9 bằng installer chính thức |
| Có `dotnet-ef` | `dotnet tool restore` (đọc `.config/dotnet-tools.json`) |
| Connection string trên máy | `appsettings.json` đang là `Server=localhost` (instance mặc định). Máy dùng named instance (ví dụ `localhost\THEANH`) thì ghi đè bằng **user-secrets**, không sửa appsettings đã commit: |

```powershell
dotnet user-secrets init --project TestManager.Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost\THEANH;Database=TestManagerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True" --project TestManager.Api
```
User-secrets lưu ở `%APPDATA%\Microsoft\UserSecrets\`, ngoài repo, và **chỉ được nạp ở Development**.

> **DB cũ tạo bằng `init.sql`:** xoá một lần (`dotnet ef database drop` hoặc dùng SSMS), hoặc đổi sang tên DB khác (`Database=TestManagerDb_Ef`). Nếu không, app sẽ lỗi "There is already an object named 'Users'".

### 5.1 Terminal (PowerShell), dùng được ở mọi IDE

```powershell
dotnet restore                                                # tải package
dotnet build TestManager.Api                                   # build (Debug)
dotnet run --project TestManager.Api --launch-profile http     # chạy, http://localhost:5080/swagger
```

| Lệnh | Giải thích |
|---|---|
| `dotnet run` | Tự `restore` + `build` rồi chạy, nên thường chỉ cần lệnh này |
| `--launch-profile http` | Lấy cấu hình profile `http` trong `Properties/launchSettings.json`: cổng 5080, `ASPNETCORE_ENVIRONMENT=Development` → **có** migrate + seed |
| `dotnet watch --project TestManager.Api` | Chạy và tự build lại khi sửa code (hot reload) |
| `dotnet build -c Release` | Build bản Release (giống trong Dockerfile) |

Chạy thử ở Production (không migrate):
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project TestManager.Api --no-launch-profile
Remove-Item Env:ASPNETCORE_ENVIRONMENT
```

### 5.2 Visual Studio 2022

Yêu cầu: **Visual Studio 2022 17.12 trở lên** (bản đầu tiên hỗ trợ .NET 9), workload *ASP.NET and web development*. Kiểm tra: *Help → About*.

**Mở và chạy**

| Việc | Thao tác |
|---|---|
| Mở project | *File → Open → Project/Solution* → `TestManager.sln` |
| Chọn profile | Thanh công cụ, nút ▶ có danh sách: chọn **`http`** (hoặc `https`). Cả hai đặt `ASPNETCORE_ENVIRONMENT=Development` |
| Build | `Ctrl+Shift+B` (*Build → Build Solution*) |
| Chạy có debug | `F5`. Trình duyệt mở `/swagger`, có thể đặt breakpoint |
| Chạy không debug | `Ctrl+F5` |
| Xem log migrate/seed | Cửa sổ console của app (bật cùng khi chạy), hoặc *View → Output* → *Show output from: ASP.NET Core Web Server* |
| Dừng | `Shift+F5` |

**Debug luồng migrate/seed:** đặt breakpoint ở `Program.cs` dòng 58 (`MigrateAsync`) và `DbSeeder.cs` dòng 16, nhấn `F5`, rồi dùng `F10` (bước qua) và `F11` (bước vào) để xem đúng thứ tự ở mục 2.1. Lần chạy thứ 2, `F10` ở dòng 16 sẽ thấy nhảy vào nhánh `return`.

**Sửa biến môi trường cho một lần chạy:** chuột phải project → *Properties → Debug → Open debug launch profiles UI* → mục *Environment variables*. Thay đổi được ghi vào `launchSettings.json`, nên không ghi mật khẩu vào đây (dùng user-secrets: chuột phải project → **Manage User Secrets**, VS mở file `secrets.json`).

**Chạy lệnh `dotnet ef` trong Visual Studio**

*Cách 1 (khuyên dùng): Terminal tích hợp.* *View → Terminal* (`Ctrl+\``) mở Developer PowerShell ở thư mục solution, chạy đúng các lệnh ở mục 4.

*Cách 2: Package Manager Console* (*Tools → NuGet Package Manager → Package Manager Console*). Cách này dùng lệnh dạng PowerShell và **cần thêm package `Microsoft.EntityFrameworkCore.Tools`** (repo hiện **chưa** có, vì đã dùng `dotnet ef`):

```powershell
Install-Package Microsoft.EntityFrameworkCore.Tools -Version 9.0.21 -ProjectName TestManager.Api
```

| `dotnet ef` | Package Manager Console |
|---|---|
| `dotnet ef migrations add X --output-dir Data/Migrations` | `Add-Migration X -OutputDir Data/Migrations` |
| `dotnet ef migrations remove` | `Remove-Migration` |
| `dotnet ef migrations list` | `Get-Migration` |
| `dotnet ef database update` | `Update-Database` |
| `dotnet ef database update X` | `Update-Database X` |
| `dotnet ef migrations script --idempotent` | `Script-Migration -Idempotent` |
| `dotnet ef database drop` | `Drop-Database` |

Trong PMC, ô *Default project* phải chọn `TestManager.Api`.

**Xem DB ngay trong VS:** *View → SQL Server Object Explorer* → *Add SQL Server* → `localhost\THEANH` (Windows Authentication) → mở `TestManagerDb` → *Tables*: thấy `__EFMigrationsHistory` và các bảng. Chuột phải bảng → *View Data*.

**Docker trong VS 2022:** repo **chưa** cấu hình *Container Orchestrator Support* (project `docker-compose.dcproj`). Cách đơn giản: chạy Compose bằng Terminal tích hợp (mục 5.4). Cửa sổ *View → Other Windows → Containers* vẫn xem được container, log, biến môi trường, file của container đang chạy.

### 5.3 VS Code

Cần extension **C# Dev Kit** (kèm C#, .NET Install Tool).

| Việc | Thao tác |
|---|---|
| Build | `Ctrl+Shift+P` → *.NET: Build*, hoặc terminal `dotnet build TestManager.Api` |
| Chạy có debug | `F5` → chọn *C#* → chọn project `TestManager.Api` và launch profile `http`. C# Dev Kit đọc `launchSettings.json` |
| Terminal | `` Ctrl+` `` → dùng các lệnh ở mục 4 và 5.1 |
| Breakpoint | Giống VS 2022: `Program.cs` dòng 58, `DbSeeder.cs` dòng 16; `F10`/`F11` |
| Xem DB | Extension *SQL Server (mssql)* → kết nối `localhost\THEANH` hoặc `localhost,14333` (container) |
| Docker | Extension *Container Tools* (*Docker*): xem container, chuột phải → *View Logs*, *Attach Shell* (tương đương `docker exec -it ... sh`) |

### 5.4 Docker Compose (mục tiêu chính của bài)

```powershell
copy .env.example .env                  # lần đầu; đổi MSSQL_SA_PASSWORD
docker compose down -v                   # LẦN ĐẦU sau bài này: xoá volume cũ do init.sql tạo
docker compose up -d --build             # build image api (Dockerfile.linux) + chạy db, api
docker compose ps                        # db: healthy, api: running (không còn db-init)
docker compose logs -f api               # xem migrate + seed, Ctrl+C để thoát
```

| Lệnh | Giải thích |
|---|---|
| `down -v` | Dừng, xoá container **và named volume** `mssql-data` → DB mất, tạo "môi trường sạch" |
| `up -d --build` | `--build` build lại image vì code đã đổi (có `Data/Migrations` mới). `-d` chạy nền |
| `logs -f api` | Theo dõi log; nên thấy đúng thứ tự: `Applying EF Core migrations...` → `Applying migration '20261022161533_InitialCreate'` → `Seed: đã tạo lớp mẫu...` → `Now listening on: http://[::]:8080` |

Image có migration không? Có. `Dockerfile.linux` chạy `COPY . .` rồi `dotnet publish`, nên `Data/Migrations/*.cs` được build vào `TestManager.Api.dll`. `.dockerignore` không loại thư mục này. Container **không** cần `dotnet-ef`.

---

## 6. Kiểm tra theo tiêu chí của spec

| Tiêu chí | Lệnh kiểm tra | Mong đợi |
|---|---|---|
| `down -v` rồi `up`: tự tạo DB, có lớp mẫu, log không lỗi | `docker compose down -v; docker compose up -d --build; docker compose logs api` rồi `curl http://localhost:8080/api/classes` | Log có migrate + seed, không có `fail:`; trả về lớp `C# - K1`, `studentCount: 3` |
| Lần 2 không sinh lớp mẫu thứ hai | `docker compose restart api; docker compose logs api \| Select-String Seed` rồi gọi lại `/api/classes` | `Seed: dữ liệu mẫu đã có (Subject CS01), bỏ qua.`; vẫn chỉ 1 lớp |
| Lấy env của container | `docker compose exec api env` (hoặc `docker exec <container> env`) | Thấy `ASPNETCORE_ENVIRONMENT=Development`, `ConnectionStrings__Default=...` |
| Lịch sử migration trong DB | `docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<mật khẩu>" -C -d TestManagerDb -Q "SELECT * FROM __EFMigrationsHistory"` | 1 dòng `20261022161533_InitialCreate`, `9.0.21` |
| `/health` | `curl http://localhost:8080/health` | `Healthy` |

Trên máy không Docker (đã làm 22/10): chạy app 2 lần ở mục 5.1 trên DB trống rồi kiểm tra như trên với cổng 5080.

---

## 7. Lỗi thường gặp

| Thông báo | Nguyên nhân | Cách xử lý |
|---|---|---|
| `A compatible .NET SDK was not found. Requested SDK version: 9.0.201` | Máy chưa có SDK 9 | Mục 5.0 |
| `Could not execute because the specified command or file was not found` khi gõ `dotnet ef` | Chưa restore tool | `dotnet tool restore` |
| `Your startup project 'TestManager.Api' doesn't reference Microsoft.EntityFrameworkCore.Design` | Thiếu package | Mục 3.1 |
| `There is already an object named 'Users' in the database` | DB được tạo bằng `init.sql` / `EnsureCreated`, không có `__EFMigrationsHistory` | `docker compose down -v` hoặc xoá DB local |
| `The model for context 'AppDbContext' has pending changes` (EF 9) | Đã sửa Model/`AppDbContext` nhưng chưa `migrations add` | `dotnet ef migrations add <Ten> --project TestManager.Api --output-dir Data/Migrations` |
| `does not support user-initiated transactions` | Tự `BeginTransaction` mà không bọc execution strategy | Làm như mục 3.6 |
| `A network-related or instance-specific error ... (error: 26)` khi `dotnet run` | Sai tên instance SQL Server (`localhost` vs `localhost\THEANH`) hoặc SQL Server chưa chạy | User-secrets (mục 5.0); kiểm tra service `MSSQL$...` đang Running |
| App khởi động rất lâu rồi mới lỗi | Retry đang thử lại (6 lần, tăng dần) | Đọc log: lỗi tạm thời thì chờ DB lên; lỗi cấu hình thì sửa connection string |
| Seed không chạy dù DB trống | Không ở Development | Kiểm tra `ASPNETCORE_ENVIRONMENT` (launch profile / compose) |
| Log tiếng Việt thành `?` trong PowerShell | Console Windows không dùng UTF-8, dữ liệu trong DB vẫn đúng | `[Console]::OutputEncoding = [Text.Encoding]::UTF8` |

---

## 8. Quy trình từ giờ khi đổi schema

```
1. Sửa Model (TestManager.Api/Models/*.cs) hoặc AppDbContext
2. dotnet ef migrations add <TenMoTaThayDoi> --project TestManager.Api --output-dir Data/Migrations
3. Đọc file migration vừa sinh (Up/Down) + dotnet ef migrations script <MigrationTruoc> <MigrationMoi>
   → kiểm tra có DropColumn/DropTable ngoài ý muốn không (mất dữ liệu!)
4. Chạy app (F5 / dotnet run / docker compose up --build) → MigrateAsync tự áp dụng
5. Commit cả 3 thứ: file migration, file .Designer.cs, AppDbContextModelSnapshot.cs
```

**Không** làm: sửa tay migration đã commit/đã áp dụng; xoá snapshot; tạo bảng bằng SQL tay song song với migration.
