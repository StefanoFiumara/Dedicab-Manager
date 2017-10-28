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
            => this._mainWindowViewModel 
                    ?? (this._mainWindowViewModel = new MainWindowViewModel(this._eventAggregator, this._dataService, this._dataModel));
    }
}