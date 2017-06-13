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
            get => this._selectedSongGroup;
            set
            {
                if (Equals(value, this._selectedSongGroup)) return;
                this._selectedSongGroup = value;
                this.OnPropertyChanged();
            }
        }

        private SongDataModel _selectedSong;
        public SongDataModel SelectedSong
        {
            get => this._selectedSong;
            set
            {
                if (Equals(value, this._selectedSong)) return;
                this._selectedSong = value;
                this.OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}