using Attendance.Api.Helpers;
using Attendance.Api.Middlewares;
using Attendance.Api.Models.Request;
using Attendance.Api.Models.Request.Validator;
using Attendance.Infrastructure;
using Attendance.Service;
using Attendance.Service.Interface.Repository;
using Attendance.Service.Interface.Service;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// you can change the default log to other log provider, such as Serilog, NLog, etc.
builder.Services.AddLogging();

builder.Services.AddScoped<IValidator<AttendanceClockRequest>, AttendanceClockRequestValidator>();
builder.Services.AddTransient<IValidationService, ValidationService>();

builder.Services.AddScoped<IAttendanceRecordService, AttendanceRecordService>();
builder.Services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCustomExceptionMiddleware();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();