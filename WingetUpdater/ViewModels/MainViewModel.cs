using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using WingetUpdater.Commands;
using WingetUpdater.Models;
using WingetUpdater.Services;

namespace WingetUpdater.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly WingetService _wingetService;
        private readonly Services.UninstallService _uninstallService;
        private bool _isLoading;
        private bool _isUpdating;
        private double _progress;
        private string _statusMessage = "Listo";
        private string _logs = string.Empty;
        private bool _showLogs = false;

        public MainViewModel()
        {
            _wingetService = new WingetService();
            _wingetService.OutputReceived += OnOutputReceived;
            _wingetService.ErrorReceived += OnErrorReceived;

            _uninstallService = new Services.UninstallService();
            _uninstallService.LogMessage += OnOutputReceived;

            Packages = new ObservableCollection<PackageInfo>();

            RefreshCommand = new RelayCommand(async _ => await RefreshPackagesAsync(), _ => !IsLoading && !IsUpdating);
            UpdateSelectedCommand = new RelayCommand(async _ => await UpdateSelectedPackagesAsync(), _ => !IsUpdating && Packages.Any(p => p.IsSelected));
            UpdateAllCommand = new RelayCommand(async _ => await UpdateAllPackagesAsync(), _ => !IsUpdating && Packages.Any());
            SelectAllCommand = new RelayCommand(_ => SelectAll(), _ => Packages.Any());
            DeselectAllCommand = new RelayCommand(_ => DeselectAll(), _ => Packages.Any());
            ToggleLogsCommand = new RelayCommand(_ => ShowLogs = !ShowLogs);
            UninstallSelectedCommand = new RelayCommand(async _ => await UninstallSelectedPackagesAsync(), _ => !IsUpdating && Packages.Any(p => p.IsSelected));
            UpdatePackageCommand = new RelayCommand(async param => await UpdateSinglePackageAsync(param as PackageInfo), _ => !IsUpdating);
            UninstallPackageCommand = new RelayCommand(async param => await UninstallSinglePackageAsync(param as PackageInfo), _ => !IsUpdating);

            // Auto-check for winget and load packages on startup
            _ = InitializeAsync();
        }

        public ObservableCollection<PackageInfo> Packages { get; }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand UpdateSelectedCommand { get; }
        public RelayCommand UpdateAllCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand DeselectAllCommand { get; }
        public RelayCommand ToggleLogsCommand { get; }
        public RelayCommand UninstallSelectedCommand { get; }
        public RelayCommand UpdatePackageCommand { get; }
        public RelayCommand UninstallPackageCommand { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                _isUpdating = value;
                OnPropertyChanged();
                RefreshCommand.RaiseCanExecuteChanged();
                UpdateSelectedCommand.RaiseCanExecuteChanged();
                UpdateAllCommand.RaiseCanExecuteChanged();
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string Logs
        {
            get => _logs;
            set
            {
                _logs = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLogs
        {
            get => _showLogs;
            set
            {
                _showLogs = value;
                OnPropertyChanged();
            }
        }

        private async Task InitializeAsync()
        {
            AddLog("Verificando instalación de winget...");
            
            var isInstalled = await _wingetService.IsWingetInstalledAsync();
            
            if (!isInstalled)
            {
                StatusMessage = "Winget no está instalado";
                AddLog("ERROR: Winget no está instalado en el sistema.");
                AddLog("Por favor, instala winget desde Microsoft Store (App Installer).");
                MessageBox.Show("Winget no está instalado en el sistema.\n\nPor favor, instala 'App Installer' desde Microsoft Store para usar esta aplicación.",
                    "Winget no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddLog("Winget detectado correctamente.");
            await RefreshPackagesAsync();
        }

        private async Task RefreshPackagesAsync()
        {
            IsLoading = true;
            StatusMessage = "Cargando programas instalados...";
            Progress = 0;
            Packages.Clear();

            AddLog("Cargando todos los programas instalados...");

            try
            {
                // Get ALL installed packages, not just updates
                var packages = await _wingetService.GetAllInstalledPackagesAsync();
                
                foreach (var package in packages)
                {
                    Packages.Add(package);
                }

                StatusMessage = $"{Packages.Count} programa(s) instalado(s)";
                AddLog($"Se encontraron {Packages.Count} programa(s) instalado(s).");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al cargar programas";
                AddLog($"ERROR: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                Progress = 0;
            }
        }

        private async Task UpdateSelectedPackagesAsync()
        {
            var selectedPackages = Packages.Where(p => p.IsSelected).ToList();
            
            if (!selectedPackages.Any())
            {
                MessageBox.Show("Por favor, selecciona al menos un paquete para actualizar.",
                    "Sin selección", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsUpdating = true;
            StatusMessage = $"Actualizando {selectedPackages.Count} paquete(s)...";
            
            int completed = 0;
            int total = selectedPackages.Count;

            foreach (var package in selectedPackages)
            {
                package.Status = PackageStatus.Updating;
                AddLog($"Actualizando {package.Name}...");

                var success = await _wingetService.UpdatePackageAsync(package);

                if (success)
                {
                    package.Status = PackageStatus.Completed;
                    AddLog($"✓ {package.Name} actualizado correctamente.");
                }
                else
                {
                    package.Status = PackageStatus.Error;
                    AddLog($"✗ Error al actualizar {package.Name}.");
                }

                completed++;
                Progress = (double)completed / total * 100;
                StatusMessage = $"Actualizando {completed}/{total}...";
            }

            StatusMessage = "Actualización completada";
            AddLog("Proceso de actualización finalizado.");
            IsUpdating = false;
            Progress = 0;

            // Refresh the list
            await RefreshPackagesAsync();
        }

        private async Task UpdateAllPackagesAsync()
        {
            var result = MessageBox.Show(
                $"¿Deseas actualizar todos los {Packages.Count} paquete(s)?\n\nEsto puede tardar varios minutos.",
                "Confirmar actualización",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            StatusMessage = "Actualizando todos los paquetes...";
            Progress = 50;

            AddLog("Iniciando actualización de todos los paquetes...");

            var success = await _wingetService.UpdateAllPackagesAsync();

            if (success)
            {
                AddLog("✓ Todos los paquetes actualizados correctamente.");
                StatusMessage = "Actualización completada";
            }
            else
            {
                AddLog("✗ Algunos paquetes no se pudieron actualizar.");
                StatusMessage = "Actualización completada con errores";
            }

            IsUpdating = false;
            Progress = 0;

            // Refresh the list
            await RefreshPackagesAsync();
        }

        private void SelectAll()
        {
            foreach (var package in Packages)
            {
                package.IsSelected = true;
            }
        }

        private void DeselectAll()
        {
            foreach (var package in Packages)
            {
                package.IsSelected = false;
            }
        }

        private async Task UninstallSelectedPackagesAsync()
        {
            var selectedPackages = Packages.Where(p => p.IsSelected).ToList();
            
            if (!selectedPackages.Any())
            {
                MessageBox.Show("Por favor, selecciona al menos un paquete para des instalar.",
                    "Sin selección", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"¿Deseas desinstalar {selectedPackages.Count} paquete(s)?\\n\\n" +
                "Esta acción eliminará el programa y sus archivos residuales.\\n\\n" +
                "¿Continuar?",
                "Confirmar desinstalación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            StatusMessage = $"Desinstalando {selectedPackages.Count} paquete(s)...";
            
            int completed = 0;
            int total = selectedPackages.Count;

            foreach (var package in selectedPackages)
            {
                package.Status = PackageStatus.Updating;
                AddLog($"Desinstalando {package.Name}...");

                // Use UninstallService for clean uninstall
                var options = new Services.UninstallOptions
                {
                    RemoveAppData = true,
                    RemoveLocalAppData = true,
                    RemoveProgramData = false, // Safer
                    RemoveShortcuts = true,
                    CreateRestorePoint = false, // Skip for now
                    CleanRegistry = false // Microsoft doesn't recommend
                };

                var uninstallResult = await _uninstallService.UninstallCleanAsync(
                    package.Id, 
                    package.Name, 
                    options);

                if (uninstallResult.Success)
                {
                    package.Status = PackageStatus.Completed;
                    AddLog($"✓ {package.Name} desinstalado correctamente.");
                    if (uninstallResult.RemovedFiles.Count > 0)
                    {
                        AddLog($"  Archivos residuales eliminados: {uninstallResult.RemovedFiles.Count}");
                    }
                    
                    // Remove from list
                    Packages.Remove(package);
                }
                else
                {
                    package.Status = PackageStatus.Error;
                    AddLog($"✗ Error al desinstalar {package.Name}.");
                    if (uninstallResult.Errors.Any())
                    {
                        foreach (var error in uninstallResult.Errors)
                        {
                            AddLog($"  {error}");
                        }
                    }
                }

                completed++;
                Progress = (double)completed / total * 100;
                StatusMessage = $"Desinstalando {completed}/{total}...";
            }

            StatusMessage = "Desinstalación completada";
            AddLog("Proceso de desinstalación finalizado.");
            IsUpdating = false;
            Progress = 0;
        }

        private async Task UpdateSinglePackageAsync(PackageInfo? package)
        {
            if (package == null) return;

            IsUpdating = true;
            StatusMessage = $"Actualizando {package.Name}...";
            Progress = 50;

            package.Status = PackageStatus.Updating;
            AddLog($"Actualizando {package.Name}...");

            var success = await _wingetService.UpdatePackageAsync(package);

            if (success)
            {
                package.Status = PackageStatus.Completed;
                AddLog($"✓ {package.Name} actualizado correctamente.");
                StatusMessage = "Actualización completada";
            }
            else
            {
                package.Status = PackageStatus.Error;
                AddLog($"✗ Error al actualizar {package.Name}.");
                StatusMessage = "Error en actualización";
            }

            IsUpdating = false;
            Progress = 0;

            // Refresh after a short delay
            await Task.Delay(500);
            await RefreshPackagesAsync();
        }

        private async Task UninstallSinglePackageAsync(PackageInfo? package)
        {
            if (package == null) return;

            var result = MessageBox.Show(
                $"¿Deseas desinstalar {package.Name}?\\n\\n" +
                "Esta acción eliminará el programa y sus archivos residuales.\\n\\n" +
                "¿Continuar?",
                "Confirmar desinstalación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            StatusMessage = $"Desinstalando {package.Name}...";
            Progress = 50;

            package.Status = PackageStatus.Updating;
            AddLog($"Desinstalando {package.Name}...");

            var options = new Services.UninstallOptions
            {
                RemoveAppData = true,
                RemoveLocalAppData = true,
                RemoveProgramData = false,
                RemoveShortcuts = true,
                CreateRestorePoint = false,
                CleanRegistry = false
            };

            var uninstallResult = await _uninstallService.UninstallCleanAsync(
                package.Id,
                package.Name,
                options);

            if (uninstallResult.Success)
            {
                package.Status = PackageStatus.Completed;
                AddLog($"✓ {package.Name} desinstalado correctamente.");
                if (uninstallResult.RemovedFiles.Count > 0)
                {
                    AddLog($"  Archivos residuales eliminados: {uninstallResult.RemovedFiles.Count}");
                }

                StatusMessage = "Desinstalación completada";

                // Remove from list
                Packages.Remove(package);
            }
            else
            {
                package.Status = PackageStatus.Error;
                AddLog($"✗ Error al desinstalar {package.Name}.");
                if (uninstallResult.Errors.Any())
                {
                    foreach (var error in uninstallResult.Errors)
                    {
                        AddLog($"  {error}");
                    }
                }
                StatusMessage = "Error en desinstalación";
            }

            IsUpdating = false;
            Progress = 0;
        }

        private void OnOutputReceived(object? sender, string output)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddLog(output);
            });
        }

        private void OnErrorReceived(object? sender, string error)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddLog($"ERROR: {error}");
            });
        }

        private void AddLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            Logs += $"[{timestamp}] {message}\n";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
