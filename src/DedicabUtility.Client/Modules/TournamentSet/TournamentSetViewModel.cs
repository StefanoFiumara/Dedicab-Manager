using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DedicabUtility.Client.Core;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Models;
using DedicabUtility.Client.Services;
using Fano.Events.Core;
using Fano.Logging.Core;
using Fano.Mvvm.Commands;
using StepmaniaUtils.Enums;

namespace DedicabUtility.Client.Modules.TournamentSet
{
    public class TournamentSetViewModel : DedicabUtilityBaseViewModel
    {
        private SongGroupModel _selectedSongGroup;
        private TournamentSongWrapper _selectedSong;
        private int _turnIndicator;
        private int _maxDifficulty;
        private int _minDifficulty;
        private bool _isFinalsRound;

        public TournamentSetViewModel(IEventAggregator eventAggregator, ILogger log, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, log, dataService, dataModel)
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

        public int TurnIndicator
        {
            get => _turnIndicator;
            set
            {
                _turnIndicator = value;
                OnPropertyChanged();
            }
        }

        public List<int> SelectableDifficultyRanges { get; } = new List<int>(Enumerable.Range(1, 25));

        public int MinDifficulty
        {
            get => _minDifficulty;
            set
            {
                _minDifficulty = value;
                OnPropertyChanged();
            }
        }

        public int MaxDifficulty
        {
            get => _maxDifficulty;
            set
            {
                _maxDifficulty = value;
                OnPropertyChanged();
            }
        }

        public bool IsFinalsRound
        {
            get => _isFinalsRound;
            set
            {
                _isFinalsRound = value;
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
            PickSongCommand = new RelayCommand(OnPickSong, CanPickSong);
            BanSongCommand = new RelayCommand(OnBanSong, CanBanSong);
            ResetPicksCommand = new RelayCommand(OnResetPicks);

            TurnIndicator = 0;
            MinDifficulty = 9;
            MaxDifficulty = 13;
        }

        private bool CanBanSong()
        {
            return SelectedSong?.PickState == PickState.None &&
                   new[] {0,1,4,5,8,9}.Contains(TurnIndicator);
        }

        private bool CanPickSong()
        {
            return  SelectedSong?.PickState == PickState.None && 
                    new[] {2,3,6,7}.Contains(TurnIndicator);
        }

        private void OnResetPicks()
        {
            PickedSongs.Clear();
            TurnIndicator = 0;
            foreach (var song in SetSongs)
            {
                song.PickState = PickState.None;
            }
        }

        private void OnPickSong()
        {
            SelectedSong.PickState = PickState.Picked;
            PickedSongs.Add(SelectedSong);

            int songsToPick = IsFinalsRound ? 5 : 3;

            if (PickedSongs.Count == songsToPick)
            {
                foreach (var song in SetSongs.Where(s => s.PickState == PickState.None))
                {
                    song.PickState = PickState.Banned;
                }
            }

            TurnIndicator++;
        }

        private void OnBanSong()
        {
            SelectedSong.PickState = PickState.Banned;
            var finalTurn = IsFinalsRound ? 5 : 9;

            if(TurnIndicator == finalTurn)
            {
                var lastSong = SetSongs.Single(s => s.PickState == PickState.None);
                lastSong.PickState = PickState.Picked;
                PickedSongs.Add(lastSong);
            }

            TurnIndicator++;
        }

        private void OnGenerateSongSet()
        {
            SetSongs.Clear();
            PickedSongs.Clear();

            TurnIndicator = 0;
            int songsToDraw = IsFinalsRound ? 11 : 7;
            var rnd = new Random();

            var songs = SelectedSongGroup.Songs.ToList();

            if (songs.Count(s =>
                    s.DifficultySingles.ContainsKey(SongDifficulty.Challenge) &&
                    IsInDifficultyRange(s.DifficultySingles[SongDifficulty.Challenge])) < songsToDraw)
            {
                EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Not Enough Songs", "There aren't enough songs in this song pack to generate the set.", MessageIcon.Error));
                return;
            }
            while (SetSongs.Count < songsToDraw)
            {
                int randomIndex = rnd.Next(0, songs.Count);
                var song = songs[randomIndex];

                if (song.DifficultySingles.ContainsKey(SongDifficulty.Challenge))
                {
                    int rating = song.DifficultySingles[SongDifficulty.Challenge];

                    if (IsInDifficultyRange(rating))
                    {
                        SetSongs.Add(new TournamentSongWrapper(song));
                        songs.RemoveAt(randomIndex);
                    }
                }
            }
        }

        private bool IsInDifficultyRange(int rating)
        {
            return rating >= MinDifficulty && rating <= MaxDifficulty;
        }
    }
}