using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.Options;
using WebApplication1.Helpers;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Kestrel 設定 
// 換成 iis 的時候需要刪除
builder.WebHost.ConfigureKestrel(options =>
{
    // Handle requests up to 1 GB
    options.Limits.MaxRequestBodySize = 1000 * 1024 * 1024;// 1GB
});

// 配置服務，注入 FileUploadOptions
builder.Services.Configure<FileUploadOptions>(options =>
{
    options.MultipartBoundaryLengthLimit = 128;
    options.ValueCountLimit = 1024;
    options.MultipartBodyLengthLimit = 1000 * 1024 * 1024;  // 1GB
    options.AllowedExtensions = new string[] { ".jpg", ".png", ".pdf", ".mp3", ".m4a" };
});

builder.Services.AddScoped<IMultipartRequestHelper, MultipartRequestHelper>();
builder.Services.AddScoped<IFileStreamingHelper, FileStreamingHelper>();
builder.Services.AddScoped<ISteamUploadServices, SteamUploadServices>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 CORS 服务并配置一个默认策略
builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenCorsPolicy", builder => builder
        .WithOrigins("http://localhost:5173") // 允许来自前端服务器的请求
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // 如果你的请求涉及到凭证，如 Cookies，这是必须的
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 使用 CORS
app.UseCors("OpenCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
