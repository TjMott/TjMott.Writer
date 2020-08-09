using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class TicketTrackerViewModel : ViewModelBase
    {
        #region Static stuff
        private static BindingList<string> _statuses;
        public static BindingList<string> Statuses
        {
            get 
            { 
                if (_statuses == null)
                {
                    _statuses = new BindingList<string>();
                    _statuses.Add("All");
                    _statuses.Add("Open");

                    foreach (var s in TicketViewModel.Statuses)
                        _statuses.Add(s);
                }
                return _statuses;
            }
        }
        private static BindingList<TicketViewModel.TicketPriority> _priorities;
        public static BindingList<TicketViewModel.TicketPriority> Priorities
        {
            get 
            { 
                if (_priorities == null)
                {
                    _priorities = new BindingList<TicketViewModel.TicketPriority>();
                    _priorities.Add(new TicketViewModel.TicketPriority(-1, "All"));
                    foreach (var p in TicketViewModel.Priorities)
                        _priorities.Add(p);
                }
                return _priorities; 
            }
        }
        #endregion

        #region ICommands
        private ICommand _createTicketCommand;
        public ICommand CreateTicketCommand
        {
            get
            {
                if (_createTicketCommand == null)
                {
                    _createTicketCommand = new RelayCommand(param => CreateTicket());
                }
                return _createTicketCommand;
            }
        }
        #endregion

        #region Properties
        private string _statusFilter = "Open";
        public string StatusFilter
        {
            get { return _statusFilter; }
            set
            {
                if (value != _statusFilter)
                {
                    _statusFilter = value;
                    OnPropertyChanged("StatusFilter");
                    Load();
                }
            }
        }
        private TicketViewModel.TicketPriority _priorityFilter = Priorities.First();
        public TicketViewModel.TicketPriority PriorityFilter
        {
            get { return _priorityFilter; }
            set
            {
                if (value != _priorityFilter)
                {
                    _priorityFilter = value;
                    OnPropertyChanged("PriorityFilter");
                    Load();
                }
            }
        }
        private TicketViewModel _selectedTicket;
        public TicketViewModel SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                OnPropertyChanged("SelectedTicket");
            }
        }
        public UniverseViewModel UniverseVm { get; private set; }
        public BindingList<TicketViewModel> Tickets { get; private set; }

        #endregion

        public TicketTrackerViewModel(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Tickets = new BindingList<TicketViewModel>();
        }

        public void Load()
        {
            Tickets.Clear();

            var allTickets = Ticket.GetAllTickets(UniverseVm.Model.Connection).Where(i => i.UniverseId == UniverseVm.Model.id).Select(i => new TicketViewModel(i, this)).ToList();
            if (_statusFilter == "Open")
                allTickets = allTickets.Where(i => i.Model.Status != TicketViewModel.TICKET_STATUS_CLOSED && i.Model.Status != TicketViewModel.TICKET_STATUS_REJECTED).ToList();
            else if (_statusFilter != "All")
                allTickets = allTickets.Where(i => i.Model.Status == _statusFilter).ToList();

            if (_priorityFilter.Priority != -1)
                allTickets = allTickets.Where(i => i.Model.Priority == _priorityFilter.Priority).ToList();

            foreach (var ticket in allTickets)
            {
                Tickets.Add(ticket);
            }
        }

        public void CreateTicket()
        {
            Ticket ticket = new Ticket(UniverseVm.Model.Connection);
            ticket.UniverseId = UniverseVm.Model.id;
            ticket.Name = "New Ticket";
            ticket.Priority = 2;
            ticket.Status = "Not Started";
            ticket.DueDate = ""; // Not fully implemented.
            ticket.Create();

            // Create MarkdownDocument for ticket.
            MarkdownDocument doc = new MarkdownDocument(UniverseVm.Model.Connection);
            doc.Name = string.Format("Ticket {0}", ticket.id);
            doc.UniverseId = UniverseVm.Model.id;
            doc.MarkdownText = "New Ticket.";
            doc.PlainText = Markdig.Markdown.ToPlainText(doc.MarkdownText);
            doc.IsSpecial = true;
            doc.Create();
            ticket.MarkdownDocumentId = doc.id;
            ticket.Save();

            // Add to list.
            TicketViewModel vm = new TicketViewModel(ticket, this);
            Tickets.Add(vm);
        }
    }
}
