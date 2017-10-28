using System;
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
using FanoMvvm.Commands;
using FanoMvvm.Core;
using FanoMvvm.Events;
using FanoMvvm.Utility;
using Ookii.Dialogs.Wpf;

namespace DedicabUtility.Client
{
    public class MainWindowViewModel : DedicabUtilityBaseViewModel
    {
        public ICommand SongOverviewNavigationCommand { get; set; }
        public ICommand TournamentManagementNavigationCommand { get; set; }
        public ICommand OpenInstallLocationCommand { get; set; }
        public ICommand BrowseForInstallLocationCommand { get; set; }
        public ICommand ClosePopupCommand { get; set; }

        private BaseViewModel _selectedViewModel;
        public BaseViewModel SelectedViewModel
        {
            get => this._selectedViewModel;
            set
            {
                if (this._selectedViewModel == value) return;
                this._selectedViewModel = value;
                this.Model.LogoVisibility = this._selectedViewModel == null ? Visibility.Visible : Visibility.Collapsed;
                this.OnPropertyChanged();
            }
        }

        private SongOverviewViewModel SongOverview { get; set; }
        private TournamentSetViewModel TournamentSet { get; set; }

        private MainWindowModel _model;
        public MainWindowModel Model
        {
            get => this._model;
            set
            {
                if (this._model == value) return;
                this._model = value;
                this.OnPropertyChanged();
            }
        }

        public MainWindowViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, dataService, dataModel)
        {
            this.Initialize();
        }

        private void Initialize()
        {
            this.Model = new MainWindowModel();

            this.InitializeCommands();

            this.EventAggregator.Subscribe<PopupEvent, PopupEventArgs>(e =>
            {
                this.Model.ErrorPopupModel.Title = e.Title;
                this.Model.ErrorPopupModel.Message = e.Message;
                this.Model.ErrorPopupModel.MessageIcon = e.MessageIcon;
                this.Model.ErrorPopupModel.Visibility = Visibility.Visible;
            });

            this.EventAggregator.Subscribe<SetIsBusyEvent, IsBusyEventArgs>(args =>
            {
                this.IsBusy = args.BusyState;
                this.BusyText = args.BusyText;
            });

            this.EventAggregator.Subscribe<UpdateSongDataEvent>(this.OnUpdateSongData);
            this.SongOverview = new SongOverviewViewModel(this.EventAggregator, this.DataService, this.DataModel);
            this.TournamentSet = new TournamentSetViewModel(this.EventAggregator, this.DataService, this.DataModel);

            this.VerifyStepmaniaInstallLocation();
        }

        private void InitializeCommands()
        {
            this.SongOverviewNavigationCommand = new RelayCommand(() =>
                {
                    this.SelectedViewModel = this.SongOverview;
                    this.EventAggregator.Publish<SongOverviewNavigationEvent>();
                },
                () => string.IsNullOrEmpty(this.Model.StepmaniaInstallLocation) == false);

            this.TournamentManagementNavigationCommand = new RelayCommand(() =>
                {
                    this.SelectedViewModel = this.TournamentSet;
                    this.EventAggregator.Publish<TournamentManagerNavigationEvent>();
                },
                () => string.IsNullOrEmpty(this.Model.StepmaniaInstallLocation) == false);

            this.ClosePopupCommand = new RelayCommand(() => this.Model.ErrorPopupModel.Visibility = Visibility.Hidden);

            this.OpenInstallLocationCommand = new RelayCommand(this.OpenInstallLocation);
            this.BrowseForInstallLocationCommand = new RelayCommand(BrowseForInstallLocation);
        }

        private void VerifyStepmaniaInstallLocation()
        {
            var installLocation = AppSettings.Get(Setting.StepmaniaInstallLocation);

            if (installLocation == null)
            {
                this.ShowPopup("Stepmania Install Location is not set!",
                    "You must select the location of your Stepmania installation before using the program.", MessageIcon.Warning);
            }
            else
            {
                this.Model.StepmaniaInstallLocation = installLocation;
                this.EventAggregator.Publish<UpdateSongDataEvent>();
            }
        }

        private async void OnUpdateSongData()
        {
            this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, "Loading Songs..."));

            var stepmaniaDirLocation = new DirectoryInfo(AppSettings.Get(Setting.StepmaniaInstallLocation));

            var progress = new Progress<string>(i => this.BusyText = $"Loading Songs... \n{i}");

             var songGroups = await Task.Run(() => this.DataService.GetUpdatedSongData(stepmaniaDirLocation, progress));

            //have to use dumb copy constructor since we can't bind to objects created in separate thread.
            this.DataModel.SongGroups = new ObservableCollection<SongGroupModel>(songGroups.Select(g => new SongGroupModel(g)));

            this.EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(false));
        }

        private void OpenInstallLocation()
        {
            try
            {
                using (new MouseOverride(Cursors.Wait))
                {
                    Process.Start(this.Model.StepmaniaInstallLocation);
                }
            }
            catch (Exception ex)
            {
                this.ShowPopup("Could not open Stepmania install location", $"Error: {ex.Message}", MessageIcon.Error);
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
                this.Model.StepmaniaInstallLocation = selectedDirectory.FullName;
                AppSettings.Set(Setting.StepmaniaInstallLocation, this.Model.StepmaniaInstallLocation);
                this.EventAggregator.Publish<UpdateSongDataEvent>();
            }
            else
            {
                this.ShowPopup("Invalid Location", "The selected location is not a Stepmania directory", MessageIcon.Error);
            }
        }

        private void ShowPopup(string title, string message, MessageIcon icon = MessageIcon.Success)
        {
            var popupEventArgs = new PopupEventArgs(title, message, icon);
            this.EventAggregator.Publish<PopupEvent, PopupEventArgs>(popupEventArgs);
        }
    }
}