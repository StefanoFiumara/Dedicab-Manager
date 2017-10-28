using DedicabUtility.Client.Services;
using FanoMvvm.Core;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Core
{
    public class DedicabUtilityBaseViewModel : BaseViewModel
    {
        private DedicabDataModel _dataModel;
        public DedicabDataService DataService { get; }

        public DedicabDataModel DataModel
        {
            get { return this._dataModel; }
            set
            {
                this._dataModel = value;
                this.OnPropertyChanged();
            }
        }

        public DedicabUtilityBaseViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) : base(eventAggregator)
        {
            this.DataService = dataService;
            this.DataModel = dataModel;
        }
    }
}