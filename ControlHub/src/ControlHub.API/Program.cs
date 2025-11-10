using System.Security.Claims;
using System.Text;
using ControlHub.API.Configurations;
using ControlHub.API.Middlewares;
using ControlHub.Application.Common.Behaviors;
<<<<<<< HEAD
<<<<<<< Updated upstream
=======
using ControlHub.Application.Tokens;
using ControlHub.Infrastructure.Permissions.AuthZ;
>>>>>>> Stashed changes
=======
using ControlHub.Infrastructure.Permissions.AuthZ;
>>>>>>> feature/auth/claims-enrichment
using ControlHub.Infrastructure.Tokens;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;
using Serilog;
using ControlHub.Application.Tokens;

namespace ControlHub.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Register AutoMapper ---
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(ControlHub.Application.AssemblyReference).Assembly);
                cfg.AddMaps(typeof(ControlHub.Infrastructure.AssemblyReference).Assembly);
            });

            builder.Services.AddMemoryCache();

            // Add Authentication + JWT config
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

                        // THIS IS THE KEY
                        RoleClaimType = AppClaimTypes.Role // map claim "role" trong JWT thành ClaimTypes.Role
                    };
                });

            builder.Services.AddTransient<IClaimsTransformation, PermissionClaimsTransformation>();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            builder.Services.AddAuthorization();

            builder.Services.Configure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            // Vẫn giữ Serilog (dùng cho môi trường tốt)
                            Log.Error(context.Exception, "🔴 [AUTH-FAIL-SERILOG] JWT authentication failed: {Message}", context.Exception.Message);

                            // BƯỚC QUAN TRỌNG: GHI THẲNG VÀO CONSOLE (không dùng Serilog)
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n=======================================================");
                            Console.WriteLine($"!!! LỖI XÁC THỰC JWT CẤP ĐỘ GỐC !!!");
                            Console.WriteLine($"MESSAGE: {context.Exception.Message}");
                            Console.WriteLine($"TYPE: {context.Exception.GetType().FullName}");
                            Console.WriteLine($"STACK TRACE: {context.Exception.StackTrace}");
                            Console.WriteLine("=======================================================\n");
                            Console.ResetColor();

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Log.Information("✅ JWT token validated successfully for {Sub}",
                                context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown");
                            return Task.CompletedTask;
                        }
                    };
                });


            // Config Serilog
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "ControlHub.API")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("Logs/log-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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
                        options.Endpoint = new Uri("http://otel-collector:4317"); // dùng HTTP OTLP
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


            // Config MediatR
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(ControlHub.Application.AssemblyReference.Assembly));
            builder.Services.AddValidatorsFromAssembly(ControlHub.Application.AssemblyReference.Assembly);
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Load extra config files BEFORE services use them
            builder.Configuration
                .AddJsonFile("Configurations/DBSettings.json", optional: true, reloadOnChange: true);

            // Add services to the container.

            // 1. API level ***************************************************************************************

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ControlHub API", Version = "v1" });

                // Khai báo scheme Bearer
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header. Nhập token theo dạng: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Áp dụng scheme cho toàn bộ API
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            //*****************************************************************************************************



            // 2. Infrastructure **********************************************************************************

            // Register Infrastructure service identifiers
            builder.Services.AddInfrastructure();

            // Register DbContext
            builder.Services.AddDatabase(builder.Configuration);

            // Register TokenSettings
            builder.Services.Configure<TokenSettings>(
                builder.Configuration.GetSection("TokenSettings"));

            //*****************************************************************************************************


            var app = builder.Build();

            // Middlewares
            app.UseMiddleware<GlobalExceptionMiddleware>();

            // Metrics endpoint cho Prometheus
            app.MapMetrics();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseGlobalExceptionMiddleware();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
