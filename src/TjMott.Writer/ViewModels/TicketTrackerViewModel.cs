using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TjMott.Writer.Models.SQLiteClasses;

namespace TjMott.Writer.ViewModels
{
    public class TicketTrackerViewModel : ViewModelBase
    {
        #region Static stuff
        private static ObservableCollection<string> _statuses;
        public static ObservableCollection<string> Statuses
        {
            get
            {
                if (_statuses == null)
                {
                    _statuses = new ObservableCollection<string>();
                    _statuses.Add("All");
                    _statuses.Add("Open");

                    foreach (var s in TicketViewModel.Statuses)
                        _statuses.Add(s);
                }
                return _statuses;
            }
        }
        private static ObservableCollection<TicketViewModel.TicketPriority> _priorities;
        public static ObservableCollection<TicketViewModel.TicketPriority> Priorities
        {
            get
            {
                if (_priorities == null)
                {
                    _priorities = new ObservableCollection<TicketViewModel.TicketPriority>();
                    _priorities.Add(new TicketViewModel.TicketPriority(-1, "All"));
                    foreach (var p in TicketViewModel.Priorities)
                        _priorities.Add(p);
                }
                return _priorities;
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
        public ObservableCollection<TicketViewModel> Tickets { get; private set; }

        #endregion

        public TicketTrackerViewModel(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Tickets = new ObservableCollection<TicketViewModel>();
        }

        public void Load()
        {
            LoadAsync().Wait();
        }

        public async Task LoadAsync()
        {
            Tickets.Clear();

            var allTickets = (await Ticket.LoadAll(UniverseVm.Model.Connection)).Where(i => i.UniverseId == UniverseVm.Model.id).Select(i => new TicketViewModel(i, this)).ToList();
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

        public async Task CreateTicket()
        {
            Ticket ticket = new Ticket(UniverseVm.Model.Connection);
            ticket.UniverseId = UniverseVm.Model.id;
            ticket.Name = "New Ticket";
            ticket.Priority = 2;
            ticket.Status = "Not Started";
            ticket.DueDate = ""; // Not fully implemented.
            await ticket.CreateAsync().ConfigureAwait(false);

            // Create MarkdownDocument for ticket.
            /*MarkdownDocument doc = MarkdownDocumentViewModel.CreateDocForItem(ticket, ticket.UniverseId, true, string.Format("Ticket {0}", ticket.id));
            doc.MarkdownText = "New Ticket.";
            doc.PlainText = Markdig.Markdown.ToPlainText(doc.MarkdownText);
            doc.Save();*/

            // Add to list.
            TicketViewModel vm = new TicketViewModel(ticket, this);
            Tickets.Add(vm);

            // Show edit dialog.
            //vm.Edit();
        }
    }
}
