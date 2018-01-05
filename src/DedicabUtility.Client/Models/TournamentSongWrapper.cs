using System.ComponentModel;
using System.Runtime.CompilerServices;
using DedicabUtility.Client.Annotations;

namespace DedicabUtility.Client.Models
{
    public enum PickState
    {
        None, Picked, Banned
    }

    public class TournamentSongWrapper : INotifyPropertyChanged
    {
        private SongDataModel _songData;
        private PickState _pickState;

        public SongDataModel SongData
        {
            get => _songData;
            set
            {
                _songData = value;
                OnPropertyChanged();
            }
        }

        public PickState PickState
        {
            get => _pickState;
            set
            {
                _pickState = value;
                OnPropertyChanged();
            }
        }

        public TournamentSongWrapper(SongDataModel songData)
        {
            _songData = songData;
            PickState = PickState.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}