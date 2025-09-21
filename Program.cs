using HostTool.Hubs;
using HostTool.Services;
using HostTool.Infrastructure;

namespace HostTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Configure IIS Integration
            builder.Services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()   // cho phép tất cả domain
                        .AllowAnyMethod()   // cho phép tất cả HTTP method: GET, POST, PUT, DELETE...
                        .AllowAnyHeader();  // cho phép tất cả header
                });
            });
            // Add services to the container.
            builder.Services.AddControllers();
            
            // Add SignalR
            builder.Services.AddSignalR();
            
            // Add Notification Service
            builder.Services.AddScoped<INotificationService, NotificationService>();
            
            // Add Device Service
            builder.Services.AddScoped<IDeviceService, DeviceService>();
            
            // Add Entity Framework
            builder.Services.AddDbContext<AppDataContext>();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Configure for IIS
            app.UseForwardedHeaders();
            
            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();

            // Map SignalR Hub
            app.MapHub<NotificationHub>("/notificationHub");

            app.MapControllers();

            app.Run();
        }
    }
}
