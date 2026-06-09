using FresherMisa2026.Application;
using FresherMisa2026.Application.Extensions;
using FresherMisa2026.Entities.FileUpload;
using FresherMisa2026.Infrastructure;
using FresherMisa2026.WebAPI.Middlewares;

// log tiếng việt
Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DI
builder.Services.AddApplicationDI();
builder.Services.AddInfrastructure();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// File upload — BasePath được resolve từ WebRootPath tại runtime
builder.Services.Configure<FileUploadSettings>(options =>
{
    builder.Configuration.GetSection("FileUpload").Bind(options);
    options.BasePath = builder.Environment.WebRootPath;
});


var app = builder.Build();

// Tạo thư mục uploads nếu chưa tồn tại
Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "uploads"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Config sql load
SQLExtension.Initialize();

// Serve static files (wwwroot/uploads/...)
app.UseStaticFiles();

app.UseCors();

//Middlewares
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
