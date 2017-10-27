using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DedicabUtility.Client.Core;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Exceptions;
using DedicabUtility.Client.Models;
using DedicabUtility.Client.Services;
using FanoMvvm.Commands;
using FanoMvvm.Events;
using Ookii.Dialogs.Wpf;
using StepmaniaUtils.Core;

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
        public ICommand RemoveSongPackCommand { get; set; }

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

            this.RemoveSongPackCommand = new RelayCommand<SongGroupModel>(this.OnRemoveSongPack, s => s != null);
        }

        private void OnRemoveSongPack(SongGroupModel song)
        {
            //TODO: Remove the song pack from the model, move the actual pack folder to a deleted files cache in case it needs to be restored

            this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Not Implemented", "You can't delete song packs yet!", MessageIcon.Error));

        }

        private async void OnAddSongs()
        {
            var dialog = new VistaFolderBrowserDialog();

            var dialogResult = dialog.ShowDialog();
            if (dialogResult != true) return;

            var selectedDirectory = new DirectoryInfo(dialog.SelectedPath);

            var smFiles = selectedDirectory.EnumerateFiles("*.sm", SearchOption.AllDirectories).ToList();

            if (selectedDirectory.EnumerateDirectories().Any() == false)
            {
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Not A Song Group?", "There were no subfolders found in the selected folder.\nAre you sure you selected a song group?", MessageIcon.Error));
            }
            else if (smFiles.Any())
            {
                this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Loading Songs..."));

                string newPackName = selectedDirectory.Name;
                var stepmaniaDirLocation = new DirectoryInfo(AppSettings.Get(Setting.StepmaniaInstallLocation));
                var progress = new Progress<string>(i => this.BusyText = $"Loading Songs... \n{i}");

                try
                {
                    var newGroup = await Task.Run(() => this.DataProvider.AddNewSongs(stepmaniaDirLocation, smFiles, newPackName,progress));

                    this.DataProvider.SongGroups.Add(newGroup);
                }
                catch (DuplicateSongPackException)
                {
                    this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Duplicate Pack", "The song pack you selected is already on the machine!", MessageIcon.Error));
                }
                this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
            }
            else
            {
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("No Songs Found", "There were no songs found in the selected folder.", MessageIcon.Error));
            }
        }

        private void OnSongOverviewNavigation()
        {
            
        }

        
    }
}