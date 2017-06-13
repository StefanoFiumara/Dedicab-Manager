using DedicabUtility.Client.Services;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Core
{
    public class CompositionRoot
    {
        private readonly DedicabDataProvider _dataProvider = new DedicabDataProvider();
        private readonly IEventAggregator _eventAggregator = new EventAggregator();

        private MainWindowViewModel _mainWindowViewModel;
        public MainWindowViewModel MainWindowViewModel 
            => this._mainWindowViewModel 
                    ?? (this._mainWindowViewModel = new MainWindowViewModel(this._eventAggregator, this._dataProvider));
    }
}