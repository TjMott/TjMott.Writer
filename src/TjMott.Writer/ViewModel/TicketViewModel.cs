using System;
using System.ComponentModel;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class TicketViewModel : ViewModelBase
    {
        #region Static stuff
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
        public static BindingList<string> Statuses { get; private set; }
        public static BindingList<TicketPriority> Priorities { get; private set; }

        static TicketViewModel()
        {
            Statuses = new BindingList<string>();
            Statuses.Add("Not Started");
            Statuses.Add("In Progress");
            Statuses.Add("Resolved");
            Statuses.Add("Closed");
            Statuses.Add("Rejected");

            Priorities = new BindingList<TicketPriority>();
            Priorities.Add(new TicketPriority(0, "None"));
            Priorities.Add(new TicketPriority(1, "Low"));
            Priorities.Add(new TicketPriority(2, "Normal"));
            Priorities.Add(new TicketPriority(3, "High"));
            Priorities.Add(new TicketPriority(4, "Urgent"));
        }
        #endregion

        public Ticket Model { get; private set; }
        public TicketTrackerViewModel TicketTrackerVm { get; private set; }

        private MarkdownDocumentViewModel _markdownDoc;
        public MarkdownDocumentViewModel MarkdownDocument
        {
            get
            {
                if (_markdownDoc == null)
                {
                    MarkdownDocument doc = new MarkdownDocument(Model.Connection);
                    doc.id = Model.MarkdownDocumentId.Value;
                    doc.Load();
                    _markdownDoc = new MarkdownDocumentViewModel(doc, TicketTrackerVm.UniverseVm);
                }
                return _markdownDoc;
            }
        }

        public TicketViewModel(Ticket model, TicketTrackerViewModel parent)
        {
            Model = model;
            TicketTrackerVm = parent;
        }
    }
}
