using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using TjMott.Writer.Model.SQLiteClasses;

namespace TjMott.Writer.ViewModel
{
    public class TicketTrackerViewModel : ViewModelBase
    {
        #region Static stuff
        public static BindingList<string> Statuses
        {
            get { return TicketViewModel.Statuses; }
        }
        public static BindingList<TicketViewModel.TicketPriority> Priorities
        {
            get { return TicketViewModel.Priorities; }
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

        public TicketTrackerViewModel(UniverseViewModel universe)
        {
            UniverseVm = universe;
            Tickets = new BindingList<TicketViewModel>();
        }

        public void Load()
        {
            Tickets.Clear();

            var allTickets = Ticket.GetAllTickets(UniverseVm.Model.Connection).Where(i => i.UniverseId == UniverseVm.Model.id).Select(i => new TicketViewModel(i, this)).ToList();
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
            doc.Create();
            ticket.MarkdownDocumentId = doc.id;
            ticket.Save();

            // Add to list.
            TicketViewModel vm = new TicketViewModel(ticket, this);
            Tickets.Add(vm);
        }
    }
}
