using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class TicketViewModel : ViewModelBase
    {
        #region Static stuff
        public const string TICKET_STATUS_NOT_STARTED = "Not Started";
        public const string TICKET_STATUS_IN_PROGRESS = "In Progress";
        public const string TICKET_STATUS_RESOLVED = "Resolved";
        public const string TICKET_STATUS_CLOSED = "Closed";
        public const string TICKET_STATUS_REJECTED = "Rejected";
        public class TicketPriority
        {
            public int Priority { get; private set; }
            public string Name { get; private set; }
            public TicketPriority(int priority, string name)
            {
                Name = name;
                Priority = priority;
            }
        }
        public static ObservableCollection<string> Statuses { get; private set; }
        public static ObservableCollection<TicketPriority> Priorities { get; private set; }

        static TicketViewModel()
        {
            Statuses = new ObservableCollection<string>();
            Statuses.Add(TICKET_STATUS_NOT_STARTED);
            Statuses.Add(TICKET_STATUS_IN_PROGRESS);
            Statuses.Add(TICKET_STATUS_RESOLVED);
            Statuses.Add(TICKET_STATUS_CLOSED);
            Statuses.Add(TICKET_STATUS_REJECTED);

            Priorities = new ObservableCollection<TicketPriority>();
            Priorities.Add(new TicketPriority(0, "None"));
            Priorities.Add(new TicketPriority(1, "Low"));
            Priorities.Add(new TicketPriority(2, "Normal"));
            Priorities.Add(new TicketPriority(3, "High"));
            Priorities.Add(new TicketPriority(4, "Urgent"));
        }
        #endregion

        public Ticket Model { get; private set; }
        public TicketTrackerViewModel TicketTrackerVm { get; private set; }
        public string Priority
        {
            get { return Priorities.Single(i => i.Priority == Model.Priority).Name; }
        }

        public TicketViewModel(Ticket model, TicketTrackerViewModel parent)
        {
            Model = model;
            TicketTrackerVm = parent;
        }
    }
}
