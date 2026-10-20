using System.Reflection;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TestManager.Api.Common.Exceptions;
using TestManager.Api.Common.Mappings;
using TestManager.Api.Data;
using TestManager.Api.Models;
using TestManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Connection string lấy từ appsettings.json, có thể ghi đè bằng biến môi trường
// ConnectionStrings__DefaultConnection (dùng khi chạy Docker)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMappings(builder.Configuration);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IClassRoomService, ClassRoomService>();
builder.Services.AddScoped<IStudentClassService, StudentClassService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IStudentExamService, StudentExamService>();

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

// Bỏ các trường null khỏi JSON trả về (vd. password, isCorrect trong đề phát cho sinh viên)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Hiển thị các comment /// <summary> trên Swagger
    var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xmlFile);
});

var app = builder.Build();

// Kiểm tra toàn bộ AutoMapper Profile ngay khi khởi động: DTO có property chưa map => báo lỗi luôn
app.Services.GetRequiredService<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Mở trang gốc thì chuyển thẳng sang Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
