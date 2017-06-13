using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using DedicabUtility.Client.Annotations;
using DedicabUtility.Client.Core;

namespace DedicabUtility.Client.Modules.ErrorPopup
{
    public class ErrorPopupModel : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => this._title;
            set
            {
                if (value == this._title) return;
                this._title = value;
                this.OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => this._message;
            set
            {
                if (value == this._message) return;
                this._message = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _visibility;
        public Visibility Visibility
        {
            get => this._visibility;
            set
            {
                if (value == this._visibility) return;
                this._visibility = value;
                this.OnPropertyChanged();
            }
        }

        private MessageIcon _messageIcon;
        public MessageIcon MessageIcon
        {
            get => this._messageIcon;
            set
            {
                if (value == this._messageIcon) return;
                this._messageIcon = value;
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