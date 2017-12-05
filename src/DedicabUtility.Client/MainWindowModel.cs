using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using DedicabUtility.Client.Annotations;
using DedicabUtility.Client.Modules.ErrorPopup;

namespace DedicabUtility.Client
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        public MainWindowModel()
        {
            ErrorPopupModel = new ErrorPopupModel { Visibility = Visibility.Hidden };
        }
        
        private ErrorPopupModel _errorPopupModel;
        public ErrorPopupModel ErrorPopupModel
        {
            get => _errorPopupModel;
            private set
            {
                if (_errorPopupModel == value) return;
                _errorPopupModel = value;
                OnPropertyChanged();
            }
        }

        private string _stepmaniaInstallLocation;
        public string StepmaniaInstallLocation
        {
            get => _stepmaniaInstallLocation;
            set
            {
                if (_stepmaniaInstallLocation == value) return;
                _stepmaniaInstallLocation = value;
                OnPropertyChanged();
            }
        }

        private Visibility _logoVisibility;
        public Visibility LogoVisibility
        {
            get => _logoVisibility;
            set
            {
                if (_logoVisibility == value) return;
                _logoVisibility = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}