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
            this.ErrorPopupModel = new ErrorPopupModel { Visibility = Visibility.Hidden };
        }
        
        private ErrorPopupModel _errorPopupModel;
        public ErrorPopupModel ErrorPopupModel
        {
            get => this._errorPopupModel;
            private set
            {
                if (this._errorPopupModel == value) return;
                this._errorPopupModel = value;
                this.OnPropertyChanged();
            }
        }

        private string _stepmaniaInstallLocation;
        public string StepmaniaInstallLocation
        {
            get => this._stepmaniaInstallLocation;
            set
            {
                if (this._stepmaniaInstallLocation == value) return;
                this._stepmaniaInstallLocation = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _logoVisibility;
        public Visibility LogoVisibility
        {
            get => this._logoVisibility;
            set
            {
                if (this._logoVisibility == value) return;
                this._logoVisibility = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}