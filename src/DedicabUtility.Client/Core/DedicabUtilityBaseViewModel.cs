using DedicabUtility.Client.Services;
using FanoMvvm.Core;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Core
{
    public class DedicabUtilityBaseViewModel : BaseViewModel
    {
        public DedicabDataProvider DataProvider { get; }

        public DedicabUtilityBaseViewModel(IEventAggregator eventAggregator, DedicabDataProvider dataProvider) : base(eventAggregator)
        {
            this.DataProvider = dataProvider;
        }
    }
}