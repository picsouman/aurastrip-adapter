using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace aurastripadapter.luncher
{
    public class Program
    {
        public const string ServiceName = "AuraStripAdapter";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(6971);
            });

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddWindowsService();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.MapGet("/start", () =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        StartWindowsService(ServiceName);
                    }
                    else
                    {
                        StartLinuxService(ServiceName);
                    }
                }
                catch(Exception)
                {
                }
            });

            app.MapGet("/stop", () =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        StopWindowsService(ServiceName);
                    }
                    else
                    {
                        StopLinuxService(ServiceName);
                    }
                }
                catch(Exception)
                { 
                }
            });

            app.MapGet("/restart", () =>
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        StopWindowsService(ServiceName);
                        StartWindowsService(ServiceName);
                    }
                    else
                    {
                        StopLinuxService(ServiceName);
                        StartLinuxService(ServiceName);
                    }
                }
                catch(Exception)
                {
                }
            });

            app.Run();
        }


        static void StartWindowsService(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            if(sc is null)
            {
                return;
            }

            if (sc.Status == ServiceControllerStatus.Stopped)
            {
                sc.Start();
            }
        }

        static void StopWindowsService(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            if(sc is null)
            {
                return;
            }

            if (sc.Status == ServiceControllerStatus.Running)
            {
                sc.Stop();
            }
        }

        static void StartLinuxService(string serviceName)
        {
            ExecuteShellCommand($"sudo systemctl start {serviceName}");
        }

        static void StopLinuxService(string serviceName)
        {
            ExecuteShellCommand($"sudo systemctl stop {serviceName}");
        }

        static void ExecuteShellCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);

            if(process is null)
            {
                return;
            }

            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            Console.WriteLine(result);
        }
    }
}
