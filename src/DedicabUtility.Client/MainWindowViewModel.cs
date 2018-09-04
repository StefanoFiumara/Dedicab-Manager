using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DedicabUtility.Client.Core;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Models;
using DedicabUtility.Client.Modules.SongOverview;
using DedicabUtility.Client.Modules.TournamentSet;
using DedicabUtility.Client.Services;
using Fano.Events.Core;
using Fano.Logging.Core;
using Fano.Mvvm.Commands;
using Fano.Mvvm.Core;
using Fano.Mvvm.Utility;
using Ookii.Dialogs.Wpf;

namespace DedicabUtility.Client
{
    public class MainWindowViewModel : DedicabUtilityBaseViewModel
    {
        public ICommand OpenInstallLocationCommand { get; set; }
        public ICommand BrowseForInstallLocationCommand { get; set; }
        public ICommand ClosePopupCommand { get; set; }
        
        public SongOverviewViewModel SongOverview { get; set; }
        public TournamentSetViewModel TournamentSet { get; set; }

        private MainWindowModel _model;
        public MainWindowModel Model
        {
            get => _model;
            set
            {
                if (_model == value) return;
                _model = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(IEventAggregator eventAggregator, ILogger log, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, log, dataService, dataModel)
        {
            Initialize();
        }

        private void Initialize()
        {
            Model = new MainWindowModel();

            InitializeCommands();

            EventAggregator.Subscribe<PopupEvent, PopupEventArgs>(e =>
            {
                Model.ErrorPopupModel.Title = e.Title;
                Model.ErrorPopupModel.Message = e.Message;
                Model.ErrorPopupModel.MessageIcon = e.MessageIcon;
                Model.ErrorPopupModel.Visibility = Visibility.Visible;
            });

            EventAggregator.Subscribe<SetIsBusyEvent, IsBusyEventArgs>(args =>
            {
                IsBusy = args.BusyState;
                BusyText = args.BusyText;
            });

            EventAggregator.Subscribe<UpdateSongDataEvent>(OnUpdateSongData);
            SongOverview = new SongOverviewViewModel(EventAggregator, Log, DataService, DataModel);
            TournamentSet = new TournamentSetViewModel(EventAggregator, Log, DataService, DataModel);

            VerifyStepmaniaInstallLocation();
        }

        private void InitializeCommands()
        {
            ClosePopupCommand = new RelayCommand(() => Model.ErrorPopupModel.Visibility = Visibility.Hidden);

            OpenInstallLocationCommand = new RelayCommand(OpenInstallLocation);
            BrowseForInstallLocationCommand = new RelayCommand(BrowseForInstallLocation);
        }

        private void VerifyStepmaniaInstallLocation()
        {
            var installLocation = AppSettings.Get(Setting.StepmaniaInstallLocation);

            if (installLocation == null)
            {
                ShowPopup("Stepmania Install Location is not set!",
                    "You must select the location of your Stepmania installation before using the program.", MessageIcon.Warning);
            }
            else if (Directory.Exists(installLocation) == false)
            {
                ShowPopup("Stepmania Install Location could not be found!",
                    "You must select the location of your Stepmania installation before using the program.", MessageIcon.Warning);
            }
            else
            {
                Model.StepmaniaInstallLocation = installLocation;
                EventAggregator.Publish<UpdateSongDataEvent>();
            }
        }

        private async void OnUpdateSongData()
        {
            EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Loading Songs..."));

            var stepmaniaDirLocation = AppSettings.Get(Setting.StepmaniaInstallLocation);
            
             var songGroups = await Task.Run(() => DataService.ScanSongData(stepmaniaDirLocation, ProgressNotifier));
            
            var groupModels = new List<SongGroupModel>();

            foreach (var songGroup in songGroups)
            {
                var songDataModels = songGroup.Select(g => new SongDataModel(g.SmFile))
                                              .OrderBy(sm => sm.SongName);

                var groupModel = new SongGroupModel(songGroup.Key, songDataModels);
                groupModels.Add(groupModel);
            }

            DataModel.SongGroups = new ObservableCollection<SongGroupModel>(groupModels.OrderBy(g => g.Name) );

            EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
        }

        private void OpenInstallLocation()
        {
            try
            {
                using (new MouseOverride(Cursors.Wait))
                {
                    Process.Start(Model.StepmaniaInstallLocation);
                }
            }
            catch (Exception ex)
            {
                ShowPopup("Could not open Stepmania install location", $"Error: {ex.Message}", MessageIcon.Error);
            }
        }

        private void BrowseForInstallLocation()
        {
            var folderDialog = new VistaFolderBrowserDialog();

            var dialogResult = folderDialog.ShowDialog();
            if (dialogResult != true) return;

            var selectedDirectory = new DirectoryInfo(folderDialog.SelectedPath);

            if (selectedDirectory.EnumerateDirectories("Songs").Any())
            {
                Model.StepmaniaInstallLocation = selectedDirectory.FullName;
                AppSettings.Set(Setting.StepmaniaInstallLocation, Model.StepmaniaInstallLocation);
                EventAggregator.Publish<UpdateSongDataEvent>();
            }
            else
            {
                ShowPopup("Invalid Location", "The selected location is not a Stepmania directory", MessageIcon.Error);
            }
        }
    }
}