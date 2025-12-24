using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingetUpdater.Services
{
    /// <summary>
    /// Service for clean uninstallation of programs including residual file removal
    /// Based on Microsoft best practices - does NOT clean registry automatically
    /// </summary>
    public class UninstallService
    {
        private readonly WingetService _wingetService;

        public event EventHandler<string> LogMessage;

        public UninstallService()
        {
            _wingetService = new WingetService();
            _wingetService.OutputReceived += (s, e) => LogMessage?.Invoke(this, e);
            _wingetService.ErrorReceived += (s, e) => LogMessage?.Invoke(this, e);
        }

        /// <summary>
        /// Preview what will be removed during uninstallation (safe, read-only)
        /// </summary>
        public async Task<UninstallPreview> PreviewUninstallAsync(string packageId, string programName)
        {
            var preview = new UninstallPreview
            {
                PackageId = packageId,
                ProgramName = programName
            };

            await Task.Run(() =>
            {
                try
                {
                    // Find residual files
                    preview.ResidualFiles = FindResidualFiles(programName);
                    
                    // Calculate total size
                    preview.TotalSize = preview.ResidualFiles.Sum(f => f.Size);
                    
                    LogMessage?.Invoke(this, $"Found {preview.ResidualFiles.Count} residual items ({FormatBytes(preview.TotalSize)})");
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke(this, $"Error previewing uninstall: {ex.Message}");
                }
            });

            return preview;
        }

        /// <summary>
        /// Perform clean uninstallation with specified options
        /// </summary>
        public async Task<UninstallResult> UninstallCleanAsync(string packageId, string programName, UninstallOptions options)
        {
            var result = new UninstallResult
            {
                PackageId = packageId,
                ProgramName = programName
            };

            try
            {
                LogMessage?.Invoke(this, $"Starting clean uninstall of {programName}...");

                // Step 1: Uninstall using winget
                LogMessage?.Invoke(this, "Running winget uninstall...");
                
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
                        StandardOutputEncoding = Encoding.UTF8
                    }
                };

                var output = new StringBuilder();
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogMessage?.Invoke(this, e.Data);
                        output.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                    {
                        LogMessage?.Invoke(this, e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    result.Success = false;
                    result.Errors.Add($"Winget uninstall failed with code {process.ExitCode}");
                    return result;
                }

                result.UninstalledSuccessfully = true;
                LogMessage?.Invoke(this, "Winget uninstall completed successfully");

                // Step 2: Remove residual files if requested
                if (options.RemoveAppData || options.RemoveLocalAppData || options.RemoveProgramData)
                {
                    LogMessage?.Invoke(this, "Removing residual files...");
                    var residualFiles = FindResidualFiles(programName);

                    foreach (var file in residualFiles)
                    {
                        bool shouldRemove = false;

                        if (file.Type == ResidualFileType.AppData && options.RemoveAppData)
                            shouldRemove = true;
                        else if (file.Type == ResidualFileType.LocalAppData && options.RemoveLocalAppData)
                            shouldRemove = true;
                        else if (file.Type == ResidualFileType.ProgramData && options.RemoveProgramData)
                            shouldRemove = true;
                        else if (file.Type == ResidualFileType.Shortcut && options.RemoveShortcuts)
                            shouldRemove = true;

                        if (shouldRemove)
                        {
                            try
                            {
                                if (file.IsDirectory)
                                {
                                    Directory.Delete(file.Path, true);
                                }
                                else
                                {
                                    File.Delete(file.Path);
                                }

                                result.RemovedFiles.Add(file.Path);
                                result.SpaceFreed += file.Size;
                                LogMessage?.Invoke(this, $"Removed: {file.Path}");
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Could not remove {file.Path}: {ex.Message}");
                                LogMessage?.Invoke(this, $"Error removing {file.Path}: {ex.Message}");
                            }
                        }
                    }
                }

                result.Success = true;
                LogMessage?.Invoke(this, $"Clean uninstall complete. Removed {result.RemovedFiles.Count} items, freed {FormatBytes(result.SpaceFreed)}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Uninstall failed: {ex.Message}");
                LogMessage?.Invoke(this, $"Error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Find residual files for a program (safe, read-only operation)
        /// Uses Environment.SpecialFolder for safe path resolution
        /// </summary>
        public List<ResidualFile> FindResidualFiles(string programName)
        {
            var residualFiles = new List<ResidualFile>();

            if (string.IsNullOrWhiteSpace(programName))
                return residualFiles;

            try
            {
                // AppData (Roaming)
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                CheckFolder(appData, programName, ResidualFileType.AppData, residualFiles);

                // LocalAppData
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                CheckFolder(localAppData, programName, ResidualFileType.LocalAppData, residualFiles);

                // ProgramData (All Users)
                var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                CheckFolder(programData, programName, ResidualFileType.ProgramData, residualFiles);

                // Desktop shortcuts
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                CheckShortcuts(desktop, programName, residualFiles);

                // Start Menu shortcuts
                var startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                CheckShortcuts(startMenu, programName, residualFiles);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Error finding residual files: {ex.Message}");
            }

            return residualFiles;
        }

        private void CheckFolder(string basePath, string programName, ResidualFileType type, List<ResidualFile> results)
        {
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                return;

            try
            {
                var programFolder = Path.Combine(basePath, programName);
                
                if (Directory.Exists(programFolder))
                {
                    var dirInfo = new DirectoryInfo(programFolder);
                    long size = GetDirectorySize(programFolder);

                    results.Add(new ResidualFile
                    {
                        Path = programFolder,
                        Type = type,
                        IsDirectory = true,
                        Size = size,
                        LastModified = dirInfo.LastWriteTime
                    });
                }
            }
            catch
            {
                // May not have permissions to access folder
            }
        }

        private void CheckShortcuts(string basePath, string programName, List<ResidualFile> results)
        {
            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
                return;

            try
            {
                var shortcuts = Directory.GetFiles(basePath, $"*{programName}*.lnk", SearchOption.AllDirectories);
                
                foreach (var shortcut in shortcuts)
                {
                    var fileInfo = new FileInfo(shortcut);
                    results.Add(new ResidualFile
                    {
                        Path = shortcut,
                        Type = ResidualFileType.Shortcut,
                        IsDirectory = false,
                        Size = fileInfo.Length,
                        LastModified = fileInfo.LastWriteTime
                    });
                }
            }
            catch
            {
                // May not have permissions
            }
        }

        private long GetDirectorySize(string path)
        {
            if (!Directory.Exists(path))
                return 0;

            try
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                return files.Sum(file =>
                {
                    try
                    {
                        return new FileInfo(file).Length;
                    }
                    catch
                    {
                        return 0;
                    }
                });
            }
            catch
            {
                return 0;
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }

    // Data Models
    public class UninstallPreview
    {
        public string PackageId { get; set; }
        public string ProgramName { get; set; }
        public List<ResidualFile> ResidualFiles { get; set; } = new List<ResidualFile>();
        public long TotalSize { get; set; }
    }

    public class UninstallResult
    {
        public bool Success { get; set; }
        public string PackageId { get; set; }
        public string ProgramName { get; set; }
        public bool UninstalledSuccessfully { get; set; }
        public List<string> RemovedFiles { get; set; } = new List<string>();
        public long SpaceFreed { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class UninstallOptions
    {
        public bool RemoveAppData { get; set; } = true;
        public bool RemoveLocalAppData { get; set; } = true;
        public bool RemoveProgramData { get; set; } = false; // More dangerous
        public bool RemoveShortcuts { get; set; } = true;
        public bool CreateRestorePoint { get; set; } = true; // Recommended
        public bool CleanRegistry { get; set; } = false; // NOT recommended by Microsoft
    }

    public class ResidualFile
    {
        public string Path { get; set; }
        public ResidualFileType Type { get; set; }
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
    }

    public enum ResidualFileType
    {
        AppData,
        LocalAppData,
        ProgramData,
        Shortcut,
        Registry
    }
}
