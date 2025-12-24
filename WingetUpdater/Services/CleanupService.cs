using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WingetUpdater.Services
{
    /// <summary>
    /// Service for cleaning up system files using official Windows APIs
    /// Based on Microsoft documentation and best practices
    /// </summary>
    public class CleanupService
    {
        // P/Invoke for SHEmptyRecycleBin - Official Windows API
        [DllImport("shell32.dll")]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);

        [Flags]
        public enum RecycleFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,  // No confirmation dialog
            SHERB_NOPROGRESSUI = 0x00000002,    // No progress tracking UI
            SHERB_NOSOUND = 0x00000004          // No sound on completion
        }

        public event EventHandler<string> LogMessage;

        /// <summary>
        /// Analyze system for cleanable space without deleting anything
        /// </summary>
        public async Task<CleanupAnalysis> AnalyzeSystemAsync()
        {
            var analysis = new CleanupAnalysis();

            await Task.Run(() =>
            {
                try
                {
                    // User temporary files
                    analysis.TemporaryFilesSize = GetDirectorySize(Path.GetTempPath());
                    
                    // System temporary files (if accessible)
                    string systemTemp = @"C:\Windows\Temp";
                    if (Directory.Exists(systemTemp))
                    {
                        try
                        {
                            analysis.SystemTempSize = GetDirectorySize(systemTemp);
                        }
                        catch
                        {
                            // May require admin privileges
                            analysis.SystemTempSize = 0;
                        }
                    }

                    // Browser caches
                    analysis.BrowserCacheSize = GetBrowserCacheSize();

                    // Windows cache (thumbnails, etc.)
                    analysis.WindowsCacheSize = GetWindowsCacheSize();

                    // Recycle bin
                    analysis.RecycleBinSize = GetRecycleBinSize();

                    analysis.TotalSize = analysis.TemporaryFilesSize + 
                                       analysis.SystemTempSize +
                                       analysis.BrowserCacheSize + 
                                       analysis.WindowsCacheSize +
                                       analysis.RecycleBinSize;
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke(this, $"Error analyzing system: {ex.Message}");
                }
            });

            return analysis;
        }

        /// <summary>
        /// Clean user temporary files (doesn't require admin)
        /// </summary>
        public async Task<CleanupResult> CleanUserTemporaryFilesAsync()
        {
            var result = new CleanupResult();
            
            await Task.Run(() =>
            {
                try
                {
                    string tempPath = Path.GetTempPath();
                    LogMessage?.Invoke(this, $"Cleaning user temp folder: {tempPath}");

                    var files = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories);
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            long size = fileInfo.Length;
                            
                            File.Delete(file);
                            
                            result.FilesDeleted++;
                            result.SpaceFreed += size;
                        }
                        catch (Exception ex)
                        {
                            // File may be in use, skip it
                            result.Errors.Add($"Could not delete {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }

                    result.Success = true;
                    LogMessage?.Invoke(this, $"Cleaned {result.FilesDeleted} files, freed {FormatBytes(result.SpaceFreed)}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add($"Error cleaning temp files: {ex.Message}");
                    LogMessage?.Invoke(this, $"Error: {ex.Message}");
                }
            });

            return result;
        }

        /// <summary>
        /// Clean system temporary files (requires admin)
        /// </summary>
        public async Task<CleanupResult> CleanSystemTemporaryFilesAsync()
        {
            var result = new CleanupResult();
            
            await Task.Run(() =>
            {
                try
                {
                    string systemTemp = @"C:\Windows\Temp";
                    
                    if (!Directory.Exists(systemTemp))
                    {
                        result.Success = false;
                        result.Errors.Add("System temp folder not found");
                        return;
                    }

                    LogMessage?.Invoke(this, $"Cleaning system temp folder: {systemTemp}");

                    var files = Directory.GetFiles(systemTemp, "*.*", SearchOption.AllDirectories);
                    
                    foreach (var file in files)
                    {
                        try
                        {
                            var fileInfo = new FileInfo(file);
                            long size = fileInfo.Length;
                            
                            File.Delete(file);
                            
                            result.FilesDeleted++;
                            result.SpaceFreed += size;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Could not delete {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }

                    result.Success = true;
                    LogMessage?.Invoke(this, $"Cleaned {result.FilesDeleted} system temp files");
                }
                catch (UnauthorizedAccessException)
                {
                    result.Success = false;
                    result.Errors.Add("Administrator privileges required to clean system temp files");
                    LogMessage?.Invoke(this, "Error: Administrator privileges required");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add($"Error cleaning system temp: {ex.Message}");
                    LogMessage?.Invoke(this, $"Error: {ex.Message}");
                }
            });

            return result;
        }

        /// <summary>
        /// Empty recycle bin using official Windows API
        /// </summary>
        public async Task<CleanupResult> EmptyRecycleBinAsync(bool silent = true)
        {
            var result = new CleanupResult();

            await Task.Run(() =>
            {
                try
                {
                    LogMessage?.Invoke(this, "Emptying recycle bin...");

                    RecycleFlags flags = RecycleFlags.SHERB_NOSOUND;
                    if (silent)
                    {
                        flags |= RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI;
                    }

                    // Get size before emptying
                    long sizeBefore = GetRecycleBinSize();

                    int hResult = SHEmptyRecycleBin(IntPtr.Zero, null, flags);

                    if (hResult == 0)
                    {
                        result.Success = true;
                        result.SpaceFreed = sizeBefore;
                        LogMessage?.Invoke(this, $"Recycle bin emptied, freed {FormatBytes(sizeBefore)}");
                    }
                    else
                    {
                        result.Success = false;
                        result.Errors.Add($"Failed to empty recycle bin (error code: {hResult})");
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add($"Error emptying recycle bin: {ex.Message}");
                    LogMessage?.Invoke(this, $"Error: {ex.Message}");
                }
            });

            return result;
        }

        /// <summary>
        /// Clean browser caches (Chrome, Edge, Firefox)
        /// </summary>
        public async Task<CleanupResult> CleanBrowserCacheAsync()
        {
            var result = new CleanupResult();

            await Task.Run(() =>
            {
                try
                {
                    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    var cacheFolders = new List<string>
                    {
                        // Chrome
                        Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache"),
                        Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Code Cache"),
                        
                        // Edge
                        Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache"),
                        Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Code Cache"),
                        
                        // Firefox cache2
                        Path.Combine(localAppData, @"Mozilla\Firefox\Profiles")
                    };

                    foreach (var folder in cacheFolders)
                    {
                        if (Directory.Exists(folder))
                        {
                            try
                            {
                                var deleteResult = DeleteFolderContents(folder);
                                result.FilesDeleted += deleteResult.filesDeleted;
                                result.SpaceFreed += deleteResult.spaceFreed;
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Could not clean {folder}: {ex.Message}");
                            }
                        }
                    }

                    result.Success = true;
                    LogMessage?.Invoke(this, $"Browser cache cleaned: {result.FilesDeleted} files, {FormatBytes(result.SpaceFreed)}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Errors.Add($"Error cleaning browser cache: {ex.Message}");
                    LogMessage?.Invoke(this, $"Error: {ex.Message}");
                }
            });

            return result;
        }

        /// <summary>
        /// Get total cleanable space across all categories
        /// </summary>
        public async Task<long> GetTotalCleanableSpaceAsync()
        {
            var analysis = await AnalyzeSystemAsync();
            return analysis.TotalSize;
        }

        // Helper methods
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

        private long GetBrowserCacheSize()
        {
            long totalSize = 0;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var cacheFolders = new[]
            {
                Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache"),
                Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache")
            };

            foreach (var folder in cacheFolders)
            {
                totalSize += GetDirectorySize(folder);
            }

            return totalSize;
        }

        private long GetWindowsCacheSize()
        {
            long totalSize = 0;
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Thumbnails cache
            string thumbnailsPath = Path.Combine(localAppData, @"Microsoft\Windows\Explorer");
            totalSize += GetDirectorySize(thumbnailsPath);

            return totalSize;
        }

        private long GetRecycleBinSize()
        {
            long totalSize = 0;

            try
            {
                // Get recycle bin for all drives
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        string recycleBin = Path.Combine(drive.Name, "$Recycle.Bin");
                        if (Directory.Exists(recycleBin))
                        {
                            totalSize += GetDirectorySize(recycleBin);
                        }
                    }
                }
            }
            catch
            {
                // May not have permissions
            }

            return totalSize;
        }

        private (int filesDeleted, long spaceFreed) DeleteFolderContents(string folderPath)
        {
            int filesDeleted = 0;
            long spaceFreed = 0;

            if (!Directory.Exists(folderPath))
                return (0, 0);

            try
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        long size = fileInfo.Length;
                        
                        File.Delete(file);
                        
                        filesDeleted++;
                        spaceFreed += size;
                    }
                    catch
                    {
                        // File in use, skip
                    }
                }
            }
            catch
            {
                // Folder access denied
            }

            return (filesDeleted, spaceFreed);
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

    // Data models
    public class CleanupAnalysis
    {
        public long TemporaryFilesSize { get; set; }
        public long SystemTempSize { get; set; }
        public long RecycleBinSize { get; set; }
        public long BrowserCacheSize { get; set; }
        public long WindowsCacheSize { get; set; }
        public long TotalSize { get; set; }
        public int FileCount { get; set; }
    }

    public class CleanupResult
    {
        public bool Success { get; set; }
        public long SpaceFreed { get; set; }
        public int FilesDeleted { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
