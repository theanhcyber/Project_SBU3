/*
    TestManager - Quản lý môn học, lớp học, đề thi & bài thi
    (Convert từ project Spring Boot ManagerTest-Springboot sang ASP.NET Core + SQL Server)

    Chạy:  sqlcmd -S localhost -E -C -f 65001 -i database\init.sql
    Lưu ý: script DROP và tạo lại toàn bộ bảng => mất dữ liệu cũ.
    Tài khoản mẫu: mật khẩu của tất cả user là 123456
*/

IF DB_ID(N'TestManagerDb') IS NULL
    CREATE DATABASE TestManagerDb;
GO

USE TestManagerDb;
GO

-- Bắt buộc cho computed column PERSISTED / unique index (sqlcmd mặc định tắt QUOTED_IDENTIFIER)
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Xoá theo thứ tự ngược quan hệ khoá ngoại
DROP TABLE IF EXISTS dbo.StudentExams, dbo.ExamQuestions, dbo.Exams, dbo.Answers, dbo.Questions, dbo.Topics,
                     dbo.StudentClasses, dbo.Classes, dbo.Semesters, dbo.Students, dbo.Teachers, dbo.Managers,
                     dbo.Subjects, dbo.Users;
GO

/* ============================ Tài khoản ============================ */

CREATE TABLE dbo.Users
(
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Email        NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    UserName     NVARCHAR(50)  NOT NULL CONSTRAINT UQ_Users_UserName UNIQUE,
    FirstName    NVARCHAR(50)  NOT NULL,
    MiddleName   NVARCHAR(50)  NOT NULL CONSTRAINT DF_Users_MiddleName DEFAULT N'',
    LastName     NVARCHAR(50)  NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Status       BIT           NOT NULL CONSTRAINT DF_Users_Status DEFAULT 1
);

CREATE TABLE dbo.Subjects
(
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Subjects PRIMARY KEY,
    SubjectName NVARCHAR(150) NOT NULL,
    Code        NVARCHAR(20)  NOT NULL CONSTRAINT UQ_Subjects_Code UNIQUE
);

-- Manager / Teacher / Student: mỗi người gắn 1-1 với một User, xoá User => xoá theo
CREATE TABLE dbo.Managers
(
    Id     BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Managers PRIMARY KEY,
    Name   NVARCHAR(150) NOT NULL,
    Status BIT           NOT NULL,
    Code   NVARCHAR(20)  NOT NULL CONSTRAINT UQ_Managers_Code UNIQUE,
    UserId BIGINT        NOT NULL CONSTRAINT UQ_Managers_UserId UNIQUE
        CONSTRAINT FK_Managers_Users REFERENCES dbo.Users(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.Teachers
(
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Teachers PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    TeacherCode AS (CONCAT(N'TC', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)) PERSISTED,
    Status      BIT           NOT NULL,
    SubjectId   BIGINT        NULL CONSTRAINT FK_Teachers_Subjects REFERENCES dbo.Subjects(Id),
    UserId      BIGINT        NOT NULL CONSTRAINT UQ_Teachers_UserId UNIQUE
        CONSTRAINT FK_Teachers_Users REFERENCES dbo.Users(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.Students
(
    Id          BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Students PRIMARY KEY,
    Name        NVARCHAR(150) NOT NULL,
    StudentCode AS (CONCAT(N'ST', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)) PERSISTED,
    Status      BIT           NOT NULL,
    UserId      BIGINT        NOT NULL CONSTRAINT UQ_Students_UserId UNIQUE
        CONSTRAINT FK_Students_Users REFERENCES dbo.Users(Id) ON DELETE CASCADE
);
GO

/* ============================ Học kỳ & lớp học ============================ */

CREATE TABLE dbo.Semesters
(
    Id        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Semesters PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL,
    StartTime DATETIME2     NOT NULL,
    EndTime   DATETIME2     NOT NULL,
    Code      AS (CONCAT(N'SE', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)) PERSISTED,
    CONSTRAINT CK_Semesters_Time CHECK (StartTime < EndTime)
);

CREATE TABLE dbo.Classes
(
    Id         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Classes PRIMARY KEY,
    Name       NVARCHAR(100) NOT NULL,
    StartTime  DATETIME2     NOT NULL,
    EndTime    DATETIME2     NOT NULL,
    Code       AS (CONCAT(N'CL', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)) PERSISTED,
    SubjectId  BIGINT NOT NULL CONSTRAINT FK_Classes_Subjects  REFERENCES dbo.Subjects(Id),
    SemesterId BIGINT NOT NULL CONSTRAINT FK_Classes_Semesters REFERENCES dbo.Semesters(Id),
    TeacherId  BIGINT NOT NULL CONSTRAINT FK_Classes_Teachers  REFERENCES dbo.Teachers(Id),
    CONSTRAINT CK_Classes_Time CHECK (StartTime < EndTime)
);

-- Sinh viên đăng ký lớp (bảng nối n-n)
CREATE TABLE dbo.StudentClasses
(
    Id        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StudentClasses PRIMARY KEY,
    ClassId   BIGINT NOT NULL CONSTRAINT FK_StudentClasses_Classes  REFERENCES dbo.Classes(Id)  ON DELETE CASCADE,
    StudentId BIGINT NOT NULL CONSTRAINT FK_StudentClasses_Students REFERENCES dbo.Students(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_StudentClasses UNIQUE (StudentId, ClassId)
);
GO

/* ============================ Ngân hàng câu hỏi ============================ */

CREATE TABLE dbo.Topics
(
    Id        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Topics PRIMARY KEY,
    Name      NVARCHAR(150) NOT NULL,
    Code      AS (CONCAT(N'TP', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)) PERSISTED,
    SubjectId BIGINT NOT NULL CONSTRAINT FK_Topics_Subjects REFERENCES dbo.Subjects(Id),
    TeacherId BIGINT NOT NULL CONSTRAINT FK_Topics_Teachers REFERENCES dbo.Teachers(Id)
);

CREATE TABLE dbo.Questions
(
    Id           BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Questions PRIMARY KEY,
    QuestionText NVARCHAR(1000) NOT NULL,
    Difficulty   INT            NULL,
    TopicId      BIGINT         NOT NULL CONSTRAINT FK_Questions_Topics REFERENCES dbo.Topics(Id)
);

CREATE TABLE dbo.Answers
(
    Id         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Answers PRIMARY KEY,
    AnswerText NVARCHAR(500) NOT NULL,
    IsCorrect  BIT           NOT NULL,
    QuestionId BIGINT        NOT NULL CONSTRAINT FK_Answers_Questions REFERENCES dbo.Questions(Id) ON DELETE CASCADE
);
GO

/* ============================ Đề thi & bài thi ============================ */

CREATE TABLE dbo.Exams
(
    Id         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Exams PRIMARY KEY,
    Name       NVARCHAR(150) NOT NULL,
    TotalScore REAL          NOT NULL,
    StartTime  DATETIME2     NOT NULL,
    EndTime    DATETIME2     NOT NULL,
    Code       AS (CONCAT(N'EX', CASE WHEN Id < 1000 THEN RIGHT(CONCAT(N'00', Id), 3) ELSE CAST(Id AS NVARCHAR(20)) END)) PERSISTED,
    SubjectId  BIGINT NOT NULL CONSTRAINT FK_Exams_Subjects REFERENCES dbo.Subjects(Id),
    TeacherId  BIGINT NOT NULL CONSTRAINT FK_Exams_Teachers REFERENCES dbo.Teachers(Id),
    CONSTRAINT CK_Exams_Time CHECK (StartTime < EndTime)
);

-- Câu hỏi trong đề (n-n). Xoá đề => xoá liên kết; câu hỏi đang nằm trong đề thì không xoá được
CREATE TABLE dbo.ExamQuestions
(
    Id         BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExamQuestions PRIMARY KEY,
    ExamId     BIGINT NOT NULL CONSTRAINT FK_ExamQuestions_Exams     REFERENCES dbo.Exams(Id) ON DELETE CASCADE,
    QuestionId BIGINT NOT NULL CONSTRAINT FK_ExamQuestions_Questions REFERENCES dbo.Questions(Id),
    CONSTRAINT UQ_ExamQuestions UNIQUE (ExamId, QuestionId)
);

-- Sinh viên đăng ký thi. Answer lưu dạng "questionId:answerId:answerId|questionId:answerId"
CREATE TABLE dbo.StudentExams
(
    Id        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StudentExams PRIMARY KEY,
    Mark      REAL          NULL,
    IsTaken   BIT           NOT NULL CONSTRAINT DF_StudentExams_IsTaken DEFAULT 0,
    Answer    NVARCHAR(MAX) NULL,
    ExamId    BIGINT NOT NULL CONSTRAINT FK_StudentExams_Exams    REFERENCES dbo.Exams(Id),
    StudentId BIGINT NOT NULL CONSTRAINT FK_StudentExams_Students REFERENCES dbo.Students(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_StudentExams UNIQUE (StudentId, ExamId)
);
GO

/* ============================ Dữ liệu mẫu ============================ */

-- Hash của mật khẩu "123456" (ASP.NET Core PasswordHasher)
DECLARE @pwd NVARCHAR(200) = N'AQAAAAIAAYagAAAAELcYpNwtWTmZxwWFa5kFCieXZJuF3ugpW/PSI2CmYvyoFCeCEp6cERtMCEws3y+Hrw==';

INSERT INTO dbo.Users (Email, UserName, FirstName, MiddleName, LastName, PasswordHash) VALUES
    (N'admin@example.com',    N'admin',    N'Quản',  N'',     N'Trị',  @pwd),  -- 1
    (N'hoa.nt@example.com',   N'hoant',    N'Nguyễn', N'Thị',  N'Hoa',  @pwd),  -- 2
    (N'minh.tv@example.com',  N'minhtv',   N'Trần',  N'Văn',  N'Minh', @pwd),  -- 3
    (N'an.lv@example.com',    N'anlv',     N'Lê',    N'Văn',  N'An',   @pwd),  -- 4
    (N'binh.pt@example.com',  N'binhpt',   N'Phạm',  N'Thị',  N'Bình', @pwd),  -- 5
    (N'cuong.hd@example.com', N'cuonghd',  N'Hoàng', N'Đức',  N'Cường', @pwd); -- 6

INSERT INTO dbo.Subjects (SubjectName, Code) VALUES
    (N'Lập trình C#',   N'CS01'),
    (N'Cơ sở dữ liệu',  N'DB01');

INSERT INTO dbo.Managers (Name, Status, Code, UserId) VALUES (N'Quản Trị', 1, N'MG001', 1);

INSERT INTO dbo.Teachers (Name, Status, SubjectId, UserId) VALUES
    (N'Nguyễn Thị Hoa', 1, 1, 2),
    (N'Trần Văn Minh',  1, 2, 3);

INSERT INTO dbo.Students (Name, Status, UserId) VALUES
    (N'Lê Văn An',      1, 4),
    (N'Phạm Thị Bình',  1, 5),
    (N'Hoàng Đức Cường', 1, 6);

INSERT INTO dbo.Semesters (Name, StartTime, EndTime) VALUES
    (N'Học kỳ 1 2026-2027', '2026-09-01', '2027-01-15'),
    (N'Học kỳ 2 2026-2027', '2027-02-01', '2027-06-30');

INSERT INTO dbo.Classes (Name, StartTime, EndTime, SubjectId, SemesterId, TeacherId) VALUES
    (N'C# - K1',   '2026-09-05', '2026-12-20', 1, 1, 1),
    (N'CSDL - K1', '2026-09-05', '2026-12-20', 2, 1, 2);

INSERT INTO dbo.StudentClasses (ClassId, StudentId) VALUES (1, 1), (1, 2), (2, 2), (2, 3);

INSERT INTO dbo.Topics (Name, SubjectId, TeacherId) VALUES
    (N'Lập trình hướng đối tượng', 1, 1),
    (N'LINQ',                      1, 1),
    (N'SQL cơ bản',                2, 2);

INSERT INTO dbo.Questions (QuestionText, Difficulty, TopicId) VALUES
    (N'Từ khoá nào dùng để kế thừa class trong C#?', 1, 1),
    (N'Những tính chất nào là tính chất của OOP?',  2, 1),
    (N'Phương thức LINQ nào dùng để lọc dữ liệu?',  1, 2),
    (N'Câu lệnh SQL nào dùng để lấy dữ liệu?',      1, 3);

INSERT INTO dbo.Answers (AnswerText, IsCorrect, QuestionId) VALUES
    (N'extends', 0, 1), (N':', 1, 1), (N'implements', 0, 1),
    (N'Đóng gói', 1, 2), (N'Kế thừa', 1, 2), (N'Biên dịch', 0, 2), (N'Đa hình', 1, 2),
    (N'Select', 0, 3), (N'Where', 1, 3), (N'OrderBy', 0, 3),
    (N'SELECT', 1, 4), (N'INSERT', 0, 4), (N'UPDATE', 0, 4);

INSERT INTO dbo.Exams (Name, TotalScore, StartTime, EndTime, SubjectId, TeacherId) VALUES
    (N'Giữa kỳ C#',    10, '2026-11-02 08:00', '2026-11-02 09:00', 1, 1),
    (N'Cuối kỳ CSDL',  10, '2026-12-15 08:00', '2026-12-15 09:30', 2, 2);

INSERT INTO dbo.ExamQuestions (ExamId, QuestionId) VALUES (1, 1), (1, 2), (1, 3), (2, 4);

INSERT INTO dbo.StudentExams (ExamId, StudentId) VALUES (1, 1), (1, 2), (2, 2);
GO

SELECT 'Users' AS [Table], COUNT(*) AS [Rows] FROM dbo.Users
UNION ALL SELECT 'Teachers',  COUNT(*) FROM dbo.Teachers
UNION ALL SELECT 'Students',  COUNT(*) FROM dbo.Students
UNION ALL SELECT 'Classes',   COUNT(*) FROM dbo.Classes
UNION ALL SELECT 'Questions', COUNT(*) FROM dbo.Questions
UNION ALL SELECT 'Exams',     COUNT(*) FROM dbo.Exams;
GO
