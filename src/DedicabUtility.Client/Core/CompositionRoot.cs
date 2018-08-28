using DedicabUtility.Client.Services;
using Fano.Events.Core;
using Fano.Logging.Core;

namespace DedicabUtility.Client.Core
{
    public class CompositionRoot
    {
        private readonly DedicabDataModel _dataModel;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _log;
        private readonly DedicabDataService _dataService;
        private MainWindowViewModel _mainWindowViewModel;

        public CompositionRoot()
        {
            _eventAggregator = new EventAggregator();
            _log = new FileLogger("DedicabUtility.log");
            _dataService = new DedicabDataService(_log);
            _dataModel = new DedicabDataModel();
        }

        
        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                if (_mainWindowViewModel == null)
                {
                    _mainWindowViewModel = new MainWindowViewModel(_eventAggregator, _log, _dataService, _dataModel);
                }

                return _mainWindowViewModel;
            }
        }
    }
}