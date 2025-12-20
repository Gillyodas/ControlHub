using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;

namespace ControlHub.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================================
            // 1. HOST CONFIGURATION (Logging, Metrics, Tracing)
            // Phần này thuộc về "Ứng dụng chứa" (Host App). 
            // Người dùng thư viện có thể muốn dùng NLog thay vì Serilog, hoặc Jaeger thay vì Prometheus.
            // Nên để họ tự quyết định ở đây.
            // =========================================================================

            // Config Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ControlHub.API")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("Logs/log-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Config OpenTelemetry
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://otel-collector:4317");
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                })
                .WithMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://otel-collector:4318");
                        });
                });

            // =========================================================================
            // 2. CONTROL HUB LIBRARY (CORE LOGIC)
            // Đây là dòng quan trọng nhất. Toàn bộ logic nghiệp vụ, DB, Auth nằm ở đây.
            // =========================================================================

            builder.Services.AddControlHub(builder.Configuration);

            // =========================================================================
            // 3. API DOCUMENTATION (Swagger)
            // Swagger là công cụ của tầng API (Presentation). 
            // Tuy nhiên, phần cấu hình Security Definition (Bearer) nên được gợi ý hoặc cung cấp sẵn.
            // Ở đây giữ lại để App có thể tùy chỉnh Title/Version.
            // =========================================================================

            

            // =========================================================================
            // 4. BUILD & PIPELINE
            // =========================================================================

            var app = builder.Build();

            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.MapMetrics(); // Prometheus Endpoint

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Authentication & Authorization Middleware phải được gọi ở Host App
            // để đảm bảo đúng thứ tự trong Pipeline của họ.
            app.UseAuthentication();
            app.UseAuthorization();

            // Kích hoạt ControlHub (Auto Migration & Seed Data)
            app.UseControlHub();

            app.MapControllers();

            app.Run();
        }
    }
}