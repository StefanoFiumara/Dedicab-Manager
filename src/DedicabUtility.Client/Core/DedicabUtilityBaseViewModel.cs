using System;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Services;
using Fano.Events.Core;
using Fano.Mvvm.Core;

namespace DedicabUtility.Client.Core
{
    public class DedicabUtilityBaseViewModel : BaseViewModel
    {
        private DedicabDataModel _dataModel;
        public DedicabDataService DataService { get; }

        protected Progress<string> ProgressNotifier { get; }

        public DedicabDataModel DataModel
        {
            get { return _dataModel; }
            set
            {
                _dataModel = value;
                OnPropertyChanged();
            }
        }

        public DedicabUtilityBaseViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) : base(eventAggregator)
        {
            DataService = dataService;
            DataModel = dataModel;

            ProgressNotifier = new Progress<string>(i =>
            {
                EventAggregator.Publish<SetIsBusyEvent, IsBusyEventArgs>(new IsBusyEventArgs(true, $"Please Wait...\n{i}"));
            });
        }

        protected void ShowPopup(string title, string message, MessageIcon icon)
        {
            EventAggregator.Publish<PopupEvent, PopupEventArgs>(new PopupEventArgs(title, message, icon));
        }
    }
}