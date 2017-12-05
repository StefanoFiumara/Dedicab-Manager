using System.ComponentModel;
using System.Runtime.CompilerServices;
using DedicabUtility.Client.Annotations;
using DedicabUtility.Client.Models;

namespace DedicabUtility.Client.Modules.SongOverview
{
    public class SongOverviewModel : INotifyPropertyChanged
    {
        private SongGroupModel _selectedSongGroup;
        public SongGroupModel SelectedSongGroup 
        {
            get => _selectedSongGroup;
            set
            {
                if (Equals(value, _selectedSongGroup)) return;
                _selectedSongGroup = value;
                OnPropertyChanged();
            }
        }

        private SongDataModel _selectedSong;
        public SongDataModel SelectedSong
        {
            get => _selectedSong;
            set
            {
                if (Equals(value, _selectedSong)) return;
                _selectedSong = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}