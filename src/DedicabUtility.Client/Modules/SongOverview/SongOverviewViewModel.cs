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
            get => _model;
            set {
                if (Equals(value, _model)) return;
                _model = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddSongsCommand { get; set; }
        public ICommand RemoveSongPackCommand { get; set; }

        public SongOverviewViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, dataService, dataModel)
        {
            Initialize();
        }

        private void Initialize()
        {
            Model = new SongOverviewModel();
            AddSongsCommand = new RelayCommand(OnAddSongs);
            RemoveSongPackCommand = new RelayCommand<SongGroupModel>(OnRemoveSongPack, s => s != null);
        }

        private string PromptFolderBrowser()
        {
            var dialog = new VistaFolderBrowserDialog();

            var dialogResult = dialog.ShowDialog();

            return dialogResult == true ? dialog.SelectedPath : string.Empty;
        }

        private async void OnAddSongs()
        {
            var selectedDirectory = PromptFolderBrowser();
            if (string.IsNullOrEmpty(selectedDirectory)) return;

            var smFiles = Directory.EnumerateFiles(selectedDirectory, "*.sm", SearchOption.AllDirectories).ToList();

            if (Directory.EnumerateDirectories(selectedDirectory).Any() == false)
            {
                ShowPopup("Not A Song Group?", "There were no subfolders found in the selected folder.\nAre you sure you selected a song group?", MessageIcon.Warning);
                return;
            }
            if (!smFiles.Any())
            {
                ShowPopup("No Songs Found", "There were no songs found in the selected folder.", MessageIcon.Warning);
                return;
            }

            EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Loading Songs..."));

            string newPackName = selectedDirectory.Split(Path.DirectorySeparatorChar).Last();

            var stepmaniaDirLocation = AppSettings.Get(Setting.StepmaniaInstallLocation);
               
            try
            {
                var newGroup = await Task.Run(() => DataService.AddNewSongs(stepmaniaDirLocation, smFiles, newPackName, ProgressNotifier));
                DataModel.SongGroups.Add(newGroup);
            }
            catch (DuplicateSongPackException)
            {
                EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Duplicate Pack", "The song pack you selected is already on the machine!", MessageIcon.Warning));
            }

            EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
        }

        private async void OnRemoveSongPack(SongGroupModel songPack)
        {
            //TODO: Are you sure?
            var stepmaniaDirLocation = AppSettings.Get(Setting.StepmaniaInstallLocation);
            
            try
            {
                EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Removing Songs..."));

                await Task.Run(() => DataService.RemoveSongPack(stepmaniaDirLocation, songPack.Name, ProgressNotifier));

                DataModel.SongGroups.Remove(songPack);
                EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Song Pack Removed", $"The song pack {songPack.Name} is no longer on the machine."));
            }
            catch (SongPackNotFoundException)
            {
                EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Could Not Find Song Pack", $"Couldn't find a song pack named {songPack.Name}", MessageIcon.Warning));
            }
            catch (Exception e)
            {
                EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs("Error", $"An unexpected error occured!\n{e.Message}\n{e.StackTrace}", MessageIcon.Error));
            }

            EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
        }
    }
}