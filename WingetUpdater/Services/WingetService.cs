using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WingetUpdater.Models;

namespace WingetUpdater.Services
{
    public class WingetService
    {
        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;

        public async Task<bool> IsWingetInstalledAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<PackageInfo>> GetAvailableUpdatesAsync()
        {
            var packages = new List<PackageInfo>();
            var output = new StringBuilder();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "upgrade",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        OutputReceived?.Invoke(this, e.Data);
                        output.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                // Parse the captured output
                packages = ParseWingetOutput(output.ToString());
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error al obtener actualizaciones: {ex.Message}");
            }

            return packages;
        }

        private List<PackageInfo> ParseWingetOutput(string output)
        {
            var packages = new List<PackageInfo>();
            var lines = output.Split('\n');
            bool headerPassed = false;

            foreach (var line in lines)
            {
                // Skip empty lines and headers
                if (string.IsNullOrWhiteSpace(line) || line.Contains("---") || line.Contains("actualizaciones disponibles"))
                {
                    if (line.Contains("---"))
                        headerPassed = true;
                    continue;
                }

                // Skip header line
                if (line.Contains("Nombre") || line.Contains("Name") || !headerPassed)
                    continue;

                // Try to parse the line
                try
                {
                    // Winget output is space-separated with variable spacing
                    var parts = Regex.Split(line.Trim(), @"\s{2,}");
                    
                    if (parts.Length >= 4)
                    {
                        var package = new PackageInfo
                        {
                            Name = parts[0].Trim(),
                            Id = parts[1].Trim(),
                            CurrentVersion = parts[2].Trim(),
                            AvailableVersion = parts[3].Trim(),
                            Status = PackageStatus.Pending
                        };

                        packages.Add(package);
                    }
                }
                catch
                {
                    // Skip lines that can't be parsed
                    continue;
                }
            }

            return packages;
        }

        public async Task<bool> UpdatePackageAsync(PackageInfo package)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = $"upgrade --id \"{package.Id}\" --silent --accept-source-agreements --accept-package-agreements",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error al actualizar {package.Name}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAllPackagesAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "upgrade --all --silent --accept-source-agreements --accept-package-agreements",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error al actualizar todos los paquetes: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get ALL installed packages (not just updates)
        /// Uses winget list command
        /// </summary>
        public async Task<List<PackageInfo>> GetAllInstalledPackagesAsync()
        {
            var packages = new List<PackageInfo>();
            var output = new StringBuilder();

            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = "list",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        OutputReceived?.Invoke(this, e.Data);
                        output.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                // Parse output - same format as upgrade command
                packages = ParseWingetOutput(output.ToString());
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error al listar programas: {ex.Message}");
            }

            return packages;
        }

        /// <summary>
        /// Uninstall a package using winget
        /// Uses official flags: --id, --exact, --silent, --source winget
        /// </summary>
        public async Task<bool> UninstallPackageAsync(string packageId)
        {
            try
            {
                OutputReceived?.Invoke(this, $"Desinstalando {packageId}...");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "winget",
                        Arguments = $"uninstall --id \"{packageId}\" --exact --silent --source winget",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    OutputReceived?.Invoke(this, $"{packageId} desinstalado correctamente");
                    return true;
                }
                else
                {
                    ErrorReceived?.Invoke(this, $"Error al desinstalar {packageId} (código: {process.ExitCode})");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error al desinstalar {packageId}: {ex.Message}");
                return false;
            }
        }
    }
}
