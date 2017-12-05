using DedicabUtility.Client.Services;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Core
{
    public class CompositionRoot
    {
        private readonly DedicabDataService _dataService = new DedicabDataService();
        private readonly DedicabDataModel _dataModel = new DedicabDataModel();
        private readonly IEventAggregator _eventAggregator = new EventAggregator();

        private MainWindowViewModel _mainWindowViewModel;
        public MainWindowViewModel MainWindowViewModel 
            => _mainWindowViewModel 
                    ?? (_mainWindowViewModel = new MainWindowViewModel(_eventAggregator, _dataService, _dataModel));
    }
}