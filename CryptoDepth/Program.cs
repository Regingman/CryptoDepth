using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using CryptoDepth.Domain.Data.Adapters;
using Quartz.Spi;
using Quartz;
using Quartz.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CryptoDepth.Application.Services;
using CryptoDepth.Application;
using CryptoDepth.Application.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

var optionsBuilder = new DbContextOptionsBuilder<CryptoDepthDbContext>();
optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddDbContext<CryptoDepthDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CryptoDepth", Version = "v1" });
    c.EnableAnnotations();
});


#region DI (Dependency Injection)
builder.Services.AddApplicationServices();
builder.Services.AddSingleton<IJobFactory, JobFactory>();
builder.Services.AddSingleton<BackgroundService>();
builder.Services.AddSingleton<IReportService, ReportService>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

builder.Services.AddHostedService<QuartzHostedService>();
builder.Services.AddSingleton(new JobSchedule(
    jobType: typeof(BackgroundService),
    cronExpression: "*/10 * * ? * * *"));
#endregion

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddControllersWithViews()
     .AddNewtonsoftJson(options =>
     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
 );

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoDepth v1");
    c.DisplayRequestDuration();
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});
app.UseHsts();

app.UseSwagger(options =>
{
    options.SerializeAsV2 = true;
});
app.UseSwaggerUI();

app.UseCors(builder => builder
          .AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader()
         );

app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.Run();
