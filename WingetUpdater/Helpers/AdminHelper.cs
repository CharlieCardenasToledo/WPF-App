using System;
using System.Diagnostics;
using System.Security.Principal;

namespace WingetUpdater.Helpers
{
    /// <summary>
    /// Helper class for managing administrator privileges
    /// Based on Microsoft best practices
    /// </summary>
    public static class AdminHelper
    {
        /// <summary>
        /// Check if the application is running with administrator privileges
        /// </summary>
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Restart the application with administrator privileges
        /// </summary>
        public static void RestartAsAdmin()
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas" // Request elevation
                };

                Process.Start(processInfo);
                
                // Close current instance
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                // User cancelled UAC prompt
                throw new InvalidOperationException("Failed to restart as administrator", ex);
            }
        }

        /// <summary>
        /// Request admin privileges with user explanation
        /// Returns true if user approved, false if cancelled
        /// </summary>
        public static bool RequestAdminPrivileges(string reason)
        {
            try
            {
                // Show explanation first (would be in WPF dialog in real app)
                // For now, just attempt to restart
                RestartAsAdmin();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a specific operation requires admin privileges
        /// </summary>
        public static bool RequiresAdmin(AdminOperation operation)
        {
            return operation switch
            {
                AdminOperation.CleanSystemTemp => true,
                AdminOperation.CleanWindowsCache => true,
                AdminOperation.UninstallProgram => false, // Usually not needed with winget
                AdminOperation.CleanUserTemp => false,
                AdminOperation.EmptyRecycleBin => false,
                AdminOperation.CleanBrowserCache => false,
                _ => false
            };
        }
    }

    public enum AdminOperation
    {
        CleanUserTemp,
        CleanSystemTemp,
        CleanWindowsCache,
        CleanBrowserCache,
        EmptyRecycleBin,
        UninstallProgram,
        RemoveResidualFiles
    }
}
