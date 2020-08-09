using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Dialogs;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
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
        public static BindingList<string> Statuses { get; private set; }
        public static BindingList<TicketPriority> Priorities { get; private set; }

        static TicketViewModel()
        {
            Statuses = new BindingList<string>();
            Statuses.Add(TICKET_STATUS_NOT_STARTED);
            Statuses.Add(TICKET_STATUS_IN_PROGRESS);
            Statuses.Add(TICKET_STATUS_RESOLVED);
            Statuses.Add(TICKET_STATUS_CLOSED);
            Statuses.Add(TICKET_STATUS_REJECTED);

            Priorities = new BindingList<TicketPriority>();
            Priorities.Add(new TicketPriority(0, "None"));
            Priorities.Add(new TicketPriority(1, "Low"));
            Priorities.Add(new TicketPriority(2, "Normal"));
            Priorities.Add(new TicketPriority(3, "High"));
            Priorities.Add(new TicketPriority(4, "Urgent"));
        }
        #endregion

        #region ICommands
        private ICommand _editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    _editCommand = new RelayCommand(param => Edit());
                }
                return _editCommand;
            }
        }
        #endregion

        public Ticket Model { get; private set; }
        public TicketTrackerViewModel TicketTrackerVm { get; private set; }
        public string Priority
        {
            get { return Priorities.Single(i => i.Priority == Model.Priority).Name; }
        }

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

        public void Edit()
        {
            EditTicketDialog dialog = new EditTicketDialog(DialogOwner, this);
            dialog.ShowDialog();
        }
    }
}
