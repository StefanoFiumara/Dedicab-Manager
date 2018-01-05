using System;
using System.Collections.ObjectModel;
using System.Linq;
using DedicabUtility.Client.Core;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Models;
using DedicabUtility.Client.Services;
using FanoMvvm.Commands;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Modules.TournamentSet
{
    public class TournamentSetViewModel : DedicabUtilityBaseViewModel
    {
        private SongGroupModel _selectedSongGroup;
        private TournamentSongWrapper _selectedSong;

        public TournamentSetViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, dataService, dataModel)
        {
            Initialize();
        }

        public SongGroupModel SelectedSongGroup
        {
            get => _selectedSongGroup;
            set
            {
                _selectedSongGroup = value;
                OnPropertyChanged();
            }
        }

        public TournamentSongWrapper SelectedSong
        {
            get => _selectedSong;
            set
            {
                _selectedSong = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TournamentSongWrapper> SetSongs { get; set; }
        public ObservableCollection<TournamentSongWrapper> PickedSongs { get; set; }

        public RelayCommand GenerateSongSetCommand { get; set; }
        public RelayCommand PickSongCommand { get; set; }
        public RelayCommand BanSongCommand { get; set; }
        public RelayCommand ResetPicksCommand { get; set; }

        private void Initialize()
        {
            SetSongs = new ObservableCollection<TournamentSongWrapper>();
            PickedSongs = new ObservableCollection<TournamentSongWrapper>();

            GenerateSongSetCommand = new RelayCommand(OnGenerateSongSet, () => SelectedSongGroup != null);
            PickSongCommand = new RelayCommand(OnPickSong, () => SelectedSong?.PickState == PickState.None && PickedSongs.Count < 3);
            BanSongCommand = new RelayCommand(OnBanSong, () => SelectedSong?.PickState == PickState.None);
            ResetPicksCommand = new RelayCommand(OnResetPicks);
        }

        private void OnResetPicks()
        {
            PickedSongs.Clear();
            foreach (var song in SetSongs)
            {
                song.PickState = PickState.None;
            }
        }

        private void OnBanSong()
        {
            SelectedSong.PickState = PickState.Banned;
            if (SetSongs.Count(s => s.PickState == PickState.Banned) == 4)
            {
                foreach (var song in SetSongs.Where(s => s.PickState == PickState.None))
                {
                    song.PickState = PickState.Picked;
                    PickedSongs.Add(song);
                }
            }
        }

        private void OnPickSong()
        {
            SelectedSong.PickState = PickState.Picked;
            PickedSongs.Add(SelectedSong);

            if (PickedSongs.Count == 3)
            {
                foreach (var song in SetSongs.Where(s => s.PickState == PickState.None))
                {
                    song.PickState = PickState.Banned;
                }
            }
        }

        private void OnGenerateSongSet()
        {
            SetSongs.Clear();
            PickedSongs.Clear();

            var rnd = new Random();

            var songs = SelectedSongGroup.Songs.ToList();

            for (int i = 0; i < 7; i++)
            {
                int randomIndex = rnd.Next(0, songs.Count);
                SetSongs.Add( new TournamentSongWrapper(songs[randomIndex]) );
                songs.RemoveAt(randomIndex);
            }
        }
    }
}