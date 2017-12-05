using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DedicabUtility.Client.Annotations;
using DedicabUtility.Client.Models;

namespace DedicabUtility.Client.Core
{
    public sealed class DedicabDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<SongGroupModel> _songGroups;
        public ObservableCollection<SongGroupModel> SongGroups
        {
            get => _songGroups;
            set
            {
                if (Equals(value, _songGroups)) return;
                _songGroups = value;
                OnPropertyChanged();
            }
        }
    }
}