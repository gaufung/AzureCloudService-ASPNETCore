using Microsoft.Web.Administration;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace WebRole;

public class WebRoleCoreLauncher : IDisposable
{
    private const string OnStartCompletedConsoleIndicator = "WebRoleCore started.";
    private const string OnStopCompletedConsoleIndicator = "WebRoleCore stopped.";
    private const string IISSiteName = "WebRole";
    private const int ASPNETCorePort = 8080;

    private static string AssemblyDirectory => Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

    private readonly ManualResetEventSlim _onStopCompleted = new ManualResetEventSlim(false);
    private readonly ManualResetEventSlim _onStartCompleted = new ManualResetEventSlim(false);
    private readonly Process _process;

    public WebRoleCoreLauncher()
    {
        var filePath = Path.Combine(Path.GetPathRoot(AssemblyDirectory), "sitesroot", "0", "WebRoleCore", "WebRoleCore", "WebRoleCore.exe");
        var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(
            ip => ip.AddressFamily == AddressFamily.InterNetwork
                && ip.ToString() != "127.0.0.1");

        _process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = filePath,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(filePath),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                EnvironmentVariables =
                    {
                        { "ASPNETCORE_ADDRESS", ipAddress.ToString() },
                        { "ASPNETCORE_PORT", $"{ASPNETCorePort}" },
                    },
            },
            EnableRaisingEvents = true,
        };

        _process.Exited += (sender, e) =>
        {
            _onStartCompleted.Set();
            _onStopCompleted.Set();
        };

        _process.OutputDataReceived += (sender, e) =>
        {
            var data = e.Data ?? string.Empty;
            if (data.Contains(OnStartCompletedConsoleIndicator))
            {
                _onStartCompleted.Set();
            }

            if (data.Contains(OnStopCompletedConsoleIndicator))
            {
                _onStopCompleted.Set();
            }
        };
    }

    public void Run()
    {
        if (HasAdministratorPrivileges)
        {
            TryRemoveIISSitePortBinding();
            SpawnASPNETCoreProcess();
        }
        else
        {
            throw new UnauthorizedAccessException("Administrator privilege is required to start the ASP.NET Core process.");
        }
    }

    private bool HasAdministratorPrivileges =>
                new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);

    private void TryRemoveIISSitePortBinding()
    {
        using var serverManager = new ServerManager();
        var site = serverManager.Sites.FirstOrDefault(s => s.Name.StartsWith(IISSiteName, StringComparison.OrdinalIgnoreCase));
        if (site != null)
        {
            var binding = site.Bindings.FirstOrDefault(b => b.EndPoint?.Port == ASPNETCorePort);
            if (binding != null)
            {
                site.Bindings.Remove(binding);
                serverManager.CommitChanges();
            }
        }
        else
        {
            throw new InvalidOperationException("IIS site is not found or not started.");
        }
    }

    private void SpawnASPNETCoreProcess()
    {
        _process.Start();
        _process.BeginOutputReadLine();
        _onStartCompleted.Wait();
    }

    

    public void Dispose()
    {
        _process?.StandardInput.WriteLine();
        _onStopCompleted.Wait();
        _onStopCompleted.Dispose();
        _onStartCompleted.Dispose();
        _process?.Dispose();
    }
}