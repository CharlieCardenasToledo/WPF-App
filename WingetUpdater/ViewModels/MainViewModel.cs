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
        private readonly Services.CleanupService _cleanupService;
        private bool _isLoading;
        private bool _isUpdating;
        private double _progress;
        private string _statusMessage = "Listo";
        private string _logs = string.Empty;
        private bool _showLogs = false;
        private string _searchText = string.Empty;
        private string _totalCleanableSpace = "0 B";
        private string _tempFilesSize = "0 B";
        private string _recycleBinSize = "0 B";
        private string _browserCacheSize = "0 B";

        public MainViewModel()
        {
            _wingetService = new WingetService();
            _wingetService.OutputReceived += OnOutputReceived;
            _wingetService.ErrorReceived += OnErrorReceived;

            _uninstallService = new Services.UninstallService();
            _uninstallService.LogMessage += OnOutputReceived;

            _cleanupService = new Services.CleanupService();
            _cleanupService.LogMessage += OnOutputReceived;

            Packages = new ObservableCollection<PackageInfo>();
            UpdatesPackages = new ObservableCollection<PackageInfo>();
            AllPackages = new ObservableCollection<PackageInfo>();
            FilteredPackages = new ObservableCollection<PackageInfo>();

            RefreshCommand = new RelayCommand(async _ => await RefreshPackagesAsync(), _ => !IsLoading && !IsUpdating);
            UpdateSelectedCommand = new RelayCommand(async _ => await UpdateSelectedPackagesAsync(), _ => !IsUpdating && Packages.Any(p => p.IsSelected));
            UpdateAllCommand = new RelayCommand(async _ => await UpdateAllPackagesAsync(), _ => !IsUpdating && Packages.Any());
            SelectAllCommand = new RelayCommand(_ => SelectAll(), _ => Packages.Any());
            DeselectAllCommand = new RelayCommand(_ => DeselectAll(), _ => Packages.Any());
            ToggleLogsCommand = new RelayCommand(_ => ShowLogs = !ShowLogs);
            UninstallSelectedCommand = new RelayCommand(async _ => await UninstallSelectedPackagesAsync(), _ => !IsUpdating && Packages.Any(p => p.IsSelected));
            UpdatePackageCommand = new RelayCommand(async param => await UpdateSinglePackageAsync(param as PackageInfo), _ => !IsUpdating);
            UninstallPackageCommand = new RelayCommand(async param => await UninstallSinglePackageAsync(param as PackageInfo), _ => !IsUpdating);
            SearchCommand = new RelayCommand(_ => FilterPrograms());
            AnalyzeSystemCommand = new RelayCommand(async _ => await AnalyzeSystemAsync(), _ => !IsLoading);
            CleanTempFilesCommand = new RelayCommand(async _ => await CleanTempFilesAsync(), _ => !IsUpdating);
            EmptyRecycleBinCommand = new RelayCommand(async _ => await EmptyRecycleBinAsync(), _ => !IsUpdating);
            CleanBrowserCacheCommand = new RelayCommand(async _ => await CleanBrowserCacheAsync(), _ => !IsUpdating);
            CleanAllCommand = new RelayCommand(async _ => await CleanAllAsync(), _ => !IsUpdating);

            // Auto-check for winget and load packages on startup
            _ = InitializeAsync();
        }

        public ObservableCollection<PackageInfo> Packages { get; }
        public ObservableCollection<PackageInfo> UpdatesPackages { get; }
        public ObservableCollection<PackageInfo> AllPackages { get; }
        public ObservableCollection<PackageInfo> FilteredPackages { get; }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand UpdateSelectedCommand { get; }
        public RelayCommand UpdateAllCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand DeselectAllCommand { get; }
        public RelayCommand ToggleLogsCommand { get; }
        public RelayCommand UninstallSelectedCommand { get; }
        public RelayCommand UpdatePackageCommand { get; }
        public RelayCommand UninstallPackageCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand AnalyzeSystemCommand { get; }
        public RelayCommand CleanTempFilesCommand { get; }
        public RelayCommand EmptyRecycleBinCommand { get; }
        public RelayCommand CleanBrowserCacheCommand { get; }
        public RelayCommand CleanAllCommand { get; }

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

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterPrograms();
            }
        }

        public string TotalCleanableSpace
        {
            get => _totalCleanableSpace;
            set
            {
                _totalCleanableSpace = value;
                OnPropertyChanged();
            }
        }

        public string TempFilesSize
        {
            get => _tempFilesSize;
            set
            {
                _tempFilesSize = value;
                OnPropertyChanged();
            }
        }

        public string RecycleBinSize
        {
            get => _recycleBinSize;
            set
            {
                _recycleBinSize = value;
                OnPropertyChanged();
            }
        }

        public string BrowserCacheSize
        {
            get => _browserCacheSize;
            set
            {
                _browserCacheSize = value;
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
            UpdatesPackages.Clear();
            AllPackages.Clear();
            FilteredPackages.Clear();

            AddLog("Cargando todos los programas instalados...");

            try
            {
                // Get ALL installed packages
                var allPackages = await _wingetService.GetAllInstalledPackagesAsync();
                
                // Get packages with updates available
                var updatesPackages = await _wingetService.GetAvailableUpdatesAsync();
                
                // Populate collections
                foreach (var package in allPackages)
                {
                    Packages.Add(package);
                    AllPackages.Add(package);
                }

                foreach (var package in updatesPackages)
                {
                    UpdatesPackages.Add(package);
                }

                // Initialize filtered collection with all packages
                FilterPrograms();

                StatusMessage = $"{AllPackages.Count} programa(s) instalado(s) | {UpdatesPackages.Count} actualización(es) disponible(s)";
                AddLog($"Se encontraron {AllPackages.Count} programa(s) instalado(s).");
                AddLog($"Hay {UpdatesPackages.Count} actualización(es) disponible(s).");
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

        private void FilterPrograms()
        {
            FilteredPackages.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var package in AllPackages)
                {
                    FilteredPackages.Add(package);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                foreach (var package in AllPackages)
                {
                    if (package.Name.ToLower().Contains(searchLower) ||
                        package.Id.ToLower().Contains(searchLower))
                    {
                        FilteredPackages.Add(package);
                    }
                }
            }
        }

        private async Task AnalyzeSystemAsync()
        {
            IsLoading = true;
            StatusMessage = "Analizando sistema...";
            Progress = 50;

            AddLog("Analizando espacio recuperable...");

            try
            {
                var analysis = await _cleanupService.AnalyzeSystemAsync();

                TempFilesSize = FormatBytes(analysis.TemporaryFilesSize + analysis.SystemTempSize);
                RecycleBinSize = FormatBytes(analysis.RecycleBinSize);
                BrowserCacheSize = FormatBytes(analysis.BrowserCacheSize);
                TotalCleanableSpace = FormatBytes(analysis.TotalSize);

                StatusMessage = $"Análisis completo: {TotalCleanableSpace} recuperables";
                AddLog($"Espacio total recuperable: {TotalCleanableSpace}");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al analizar sistema";
                AddLog($"ERROR: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                Progress = 0;
            }
        }

        private async Task CleanTempFilesAsync()
        {
            var result = MessageBox.Show(
                "¿Deseas limpiar los archivos temporales?\\n\\nEsta acción es segura y no afectará tus programas.",
                "Confirmar limpieza",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            StatusMessage = "Limpiando archivos temporales...";
            Progress = 50;

            AddLog("Limpiando archivos temporales...");

            try
            {
                var cleanResult = await _cleanupService.CleanUserTemporaryFilesAsync();

                if (cleanResult.Success)
                {
                    AddLog($"✓ Limpieza completada: {cleanResult.FilesDeleted} archivos, {FormatBytes(cleanResult.SpaceFreed)} liberados");
                    StatusMessage = "Limpieza completada";
                }
                else
                {
                    AddLog($"✗ Error en limpieza");
                    StatusMessage = "Error en limpieza";
                }

                // Refresh analysis
                await AnalyzeSystemAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error en limpieza";
                AddLog($"ERROR: {ex.Message}");
            }
            finally
            {
                IsUpdating = false;
                Progress = 0;
            }
        }

        private async Task EmptyRecycleBinAsync()
        {
            var result = MessageBox.Show(
                "¿Deseas vaciar la papelera de reciclaje?\\n\\nEsta acción NO se puede deshacer.",
                "Confirmar vaciado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            StatusMessage = "Vaciando papelera...";
            Progress = 50;

            AddLog("Vaciando papelera de reciclaje...");

            try
            {
                var cleanResult = await _cleanupService.EmptyRecycleBinAsync(silent: true);

                if (cleanResult.Success)
                {
                    AddLog($"✓ Papelera vaciada: {FormatBytes(cleanResult.SpaceFreed)} liberados");
                    StatusMessage = "Papelera vaciada";
                }
                else
                {
                    AddLog($"✗ Error al vaciar papelera");
                    StatusMessage = "Error al vaciar papelera";
                }

                // Refresh analysis
                await AnalyzeSystemAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error al vaciar papelera";
                AddLog($"ERROR: {ex.Message}");
            }
            finally
            {
                IsUpdating = false;
                Progress = 0;
            }
        }

        private async Task CleanBrowserCacheAsync()
        {
            var result = MessageBox.Show(
                "¿Deseas limpiar el caché de los navegadores?\\n\\nEsto cerrará las sesiones activas en los navegadores.",
                "Confirmar limpieza",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            StatusMessage = "Limpiando caché de navegadores...";
            Progress = 50;

            AddLog("Limpiando caché de navegadores...");

            try
            {
                var cleanResult = await _cleanupService.CleanBrowserCacheAsync();

                if (cleanResult.Success)
                {
                    AddLog($"✓ Caché limpiado: {cleanResult.FilesDeleted} archivos, {FormatBytes(cleanResult.SpaceFreed)} liberados");
                    StatusMessage = "Caché limpiado";
                }
                else
                {
                    AddLog($"✗ Error en limpieza de caché");
                    StatusMessage = "Error en limpieza";
                }

                // Refresh analysis
                await AnalyzeSystemAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "Error en limpieza de caché";
                AddLog($"ERROR: {ex.Message}");
            }
            finally
            {
                IsUpdating = false;
                Progress = 0;
            }
        }

        private async Task CleanAllAsync()
        {
            var result = MessageBox.Show(
                "¿Deseas ejecutar una limpieza completa del sistema?\\n\\n" +
                "Esto incluye:\\n" +
                "• Archivos temporales\\n" +
                "• Papelera de reciclaje\\n" +
                "• Caché de navegadores\\n\\n" +
                "Algunas acciones NO se pueden deshacer.",
                "Confirmar limpieza completa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsUpdating = true;
            AddLog("Iniciando limpieza completa del sistema...");

            // Clean temp files
            StatusMessage = "Limpiando archivos temporales...";
            Progress = 25;
            await _cleanupService.CleanUserTemporaryFilesAsync();
            AddLog("✓ Archivos temporales limpiados");

            // Empty recycle bin
            StatusMessage = "Vaciando papelera...";
            Progress = 50;
            await _cleanupService.EmptyRecycleBinAsync(silent: true);
            AddLog("✓ Papelera vaciada");

            // Clean browser cache
            StatusMessage = "Limpiando caché de navegadores...";
            Progress = 75;
            await _cleanupService.CleanBrowserCacheAsync();
            AddLog("✓ Caché de navegadores limpiado");

            Progress = 100;
            StatusMessage = "Limpieza completa finalizada";
            AddLog("✓ Limpieza completa del sistema finalizada");

            // Refresh analysis
            await AnalyzeSystemAsync();

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
