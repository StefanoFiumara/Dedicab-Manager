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
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        private Visibility _visibility;
        public Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                OnPropertyChanged();
            }
        }

        private MessageIcon _messageIcon;
        public MessageIcon MessageIcon
        {
            get => _messageIcon;
            set
            {
                if (value == _messageIcon) return;
                _messageIcon = value;
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