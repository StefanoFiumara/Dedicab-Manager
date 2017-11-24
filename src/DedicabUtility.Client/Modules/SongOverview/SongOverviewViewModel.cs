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

        public SongOverviewViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, dataService, dataModel)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Model = new SongOverviewModel();
            this.AddSongsCommand = new RelayCommand(this.OnAddSongs);
            this.RemoveSongPackCommand = new RelayCommand<SongGroupModel>(this.OnRemoveSongPack, s => s != null);
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
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Not A Song Group?", "There were no subfolders found in the selected folder.\nAre you sure you selected a song group?", MessageIcon.Warning));
            }
            else if (!smFiles.Any())
            {
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("No Songs Found", "There were no songs found in the selected folder.", MessageIcon.Warning));
            }
            else
            {
                this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Loading Songs..."));

                string newPackName = selectedDirectory.Name;
                var stepmaniaDirLocation = new DirectoryInfo(AppSettings.Get(Setting.StepmaniaInstallLocation));
               

                try
                {
                    var newGroup = await Task.Run(() => this.DataService.AddNewSongs(stepmaniaDirLocation, smFiles, newPackName, this.ProgressNotifier));

                    this.DataModel.SongGroups.Add(newGroup);
                }
                catch (DuplicateSongPackException)
                {
                    this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Duplicate Pack", "The song pack you selected is already on the machine!", MessageIcon.Warning));
                }
                this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
            }
        }

        private async void OnRemoveSongPack(SongGroupModel songPack)
        {
            //TODO: Are you sure?
            var stepmaniaDirLocation = new DirectoryInfo(AppSettings.Get(Setting.StepmaniaInstallLocation));
           
            
            try
            {
                this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Removing Songs..."));

                await Task.Run(() => this.DataService.RemoveSongPack(stepmaniaDirLocation, songPack.Name, this.ProgressNotifier));

                this.DataModel.SongGroups.Remove(songPack);
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Song Pack Removed", $"The song pack {songPack.Name} is no longer on the machine."));
            }
            catch (SongPackNotFoundException)
            {
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Could Not Find Song Pack", $"Couldn't find a song pack named {songPack.Name}", MessageIcon.Warning));
            }
            catch (Exception e)
            {
                this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Error", $"An unexpected error occured!\n{e.Message}\n{e.StackTrace}", MessageIcon.Error));
            }

            this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
        }
    }
}