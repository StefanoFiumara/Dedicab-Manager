using System.Windows.Input;
using DedicabUtility.Client.Core;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Models;
using DedicabUtility.Client.Services;
using FanoMvvm.Commands;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Modules.SongOverview
{
    public class SongOverviewViewModel : DedicabUtilityBaseViewModel
    {
        private SongOverviewModel _model;
        public SongOverviewModel Model
        {
            get => this._model;
            set {
                if (Equals(value, this._model)) return;
                this._model = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand AddSongsCommand { get; set; }
        public ICommand RemoveSongCommand { get; set; }

        public SongOverviewViewModel(IEventAggregator eventAggregator, DedicabDataProvider dataProvider) 
            : base(eventAggregator, dataProvider)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Model = new SongOverviewModel();

            this.EventAggregator.Subscribe<SongOverviewNavigationEvent>(OnSongOverviewNavigation);

            this.AddSongsCommand = new RelayCommand(this.OnAddSongs);

            this.RemoveSongCommand = new RelayCommand<SongDataModel>(this.OnRemoveSong, s => s != null);
        }

        private void OnRemoveSong(SongDataModel song)
        {
            //TODO: Remove the song from the model, move the actual song folder to a deleted files cache

            this.EventAggregator.Publish<ShowPopupEvent, ShowPopupEventArgs>(new ShowPopupEventArgs("Not Implemented", "You can't delete songs yet!", MessageIcon.Error));

        }

        private void OnAddSongs()
        {
            //TODO: File/Folder picker that filters to .SM files

            //TODO: Prompt user to add to an existing group or create a new one.
        }

        private void OnSongOverviewNavigation()
        {
            
        }

        
    }
}