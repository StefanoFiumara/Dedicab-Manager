using DedicabUtility.Client.Core;
using DedicabUtility.Client.Events;
using DedicabUtility.Client.Services;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Modules.TournamentSet
{
    public class TournamentSetViewModel : DedicabUtilityBaseViewModel
    {
        public TournamentSetViewModel(IEventAggregator eventAggregator, DedicabDataService dataService, DedicabDataModel dataModel) 
            : base(eventAggregator, dataService, dataModel)
        {
            Initialize();
        }
        private void Initialize()
        {
            EventAggregator.Subscribe<TournamentManagerNavigationEvent>(OnTournamentSetNavigation);
            
        }

        private void OnTournamentSetNavigation()
        {
            
        }
    }
}