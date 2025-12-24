using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WingetUpdater.Models
{
    public enum PackageStatus
    {
        Pending,
        Updating,
        Completed,
        Error
    }

    public class PackageInfo : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _id = string.Empty;
        private string _currentVersion = string.Empty;
        private string _availableVersion = string.Empty;
        private PackageStatus _status = PackageStatus.Pending;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                _currentVersion = value;
                OnPropertyChanged();
            }
        }

        public string AvailableVersion
        {
            get => _availableVersion;
            set
            {
                _availableVersion = value;
                OnPropertyChanged();
            }
        }

        public PackageStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string StatusText => Status switch
        {
            PackageStatus.Pending => "Pendiente",
            PackageStatus.Updating => "Actualizando...",
            PackageStatus.Completed => "Completado",
            PackageStatus.Error => "Error",
            _ => "Desconocido"
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
