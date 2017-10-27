using DedicabUtility.Client.Core;
using FanoMvvm.Events;

namespace DedicabUtility.Client.Events
{
    public abstract class SongOverviewNavigationEvent : BaseEvent { }

    public abstract class TournamentManagerNavigationEvent : BaseEvent { }

    public abstract class PopupEvent : BaseEvent<PopupEventArgs> { }

    public abstract class SetIsBusyEvent : BaseEvent<IsBusyEventArgs> { }

    public abstract class UpdateSongDataEvent : BaseEvent { }

    public class IsBusyEventArgs
    {
        public IsBusyEventArgs(bool busyState, string busyText = "")
        {
            this.BusyText = busyText;
            this.BusyState = busyState;
        }

        public string BusyText { get; set; }
        public bool BusyState { get; set; }
    }

    public class PopupEventArgs
    {
        public string Title { get; }
        public string Message { get;  }
        public MessageIcon MessageIcon { get; }

        public PopupEventArgs(string title, string message, MessageIcon icon = MessageIcon.Success)
        {
            this.Title = title;
            this.Message = message;
            this.MessageIcon = icon;
        }
    }
}