using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSystem.Models;
using TicketSystem.DAL;

namespace TicketSystem.BLL
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepo;
        private readonly IUserRepository _userRepo;

        // Dependency Injection (kézi): megkapja a tárolókat
        public TicketService(ITicketRepository ticketRepo, IUserRepository userRepo)
        {
            _ticketRepo = ticketRepo;
            _userRepo = userRepo;
        }

        // Új jegy létrehozása
        public Ticket CreateTicket(string userId, string title, string description, TicketCategory category)
        {
            // Validáció: Létezik-e a user?
            var user = _userRepo.GetById(userId);
            if (user == null) throw new Exception("Nem létező felhasználó!");
            if (string.IsNullOrWhiteSpace(title)) throw new Exception("A cím nem lehet üres!");

            // ID generálás (egyszerűsítve)
            string newId = "T" + _ticketRepo.GetNextId().ToString("D3"); // pl. T005

            var ticket = new Ticket(newId, userId, title, description, category);
            _ticketRepo.Add(ticket);

            return ticket;
        }

        // Jegyek listázása (szűrőkkel)
        public List<Ticket> GetTickets(string agentIdFilter = null, TicketStatus? statusFilter = null)
        {
            var tickets = _ticketRepo.GetAll();

            // Szűrés, ha van megadva paraméter
            if (agentIdFilter != null)
            {
                tickets = tickets.Where(t => t.AssignedAgentId == agentIdFilter).ToList();
            }
            if (statusFilter != null)
            {
                tickets = tickets.Where(t => t.Status == statusFilter).ToList();
            }

            return tickets;
        }

        public Ticket GetTicketById(string id)
        {
            return _ticketRepo.GetById(id);
        }

        // Üzenet hozzáadása (és automata státuszváltás logika)
        public void AddMessage(string ticketId, string userId, string text)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null) throw new Exception("A jegy nem található.");

            var user = _userRepo.GetById(userId);
            if (user == null) throw new Exception("Ismeretlen felhasználó.");

            // Üzenet rögzítése
            ticket.Messages.Add(new Message(user.Name, text));

            // LOGIKA: Ha az Ügyfél válaszol és "Visszakérdezés" volt, akkor legyen "Folyamatban"
            if (user.Role == UserRole.Customer && ticket.Status == TicketStatus.WaitingForUser)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            _ticketRepo.Update(ticket);
        }

        // Jegy hozzárendelése munkatárshoz
        public void AssignTicket(string ticketId, string agentId)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            var agent = _userRepo.GetById(agentId);

            if (ticket == null) throw new Exception("Hibás Ticket ID");
            if (agent == null || agent.Role != UserRole.Agent) throw new Exception("Hibás Agent ID");

            ticket.AssignedAgentId = agentId;

            // Ha még Új volt, állítsuk Folyamatban-ra, mert valaki felvette
            if (ticket.Status == TicketStatus.New)
            {
                ticket.Status = TicketStatus.InProgress;
            }

            _ticketRepo.Update(ticket);
        }

        // Státuszváltás (Állapotgép logika)
        public void ChangeStatus(string ticketId, TicketStatus newStatus)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null) throw new Exception("Jegy nem található");

            // Szabály: Lezárt jegyet nem lehet újranyitni (kivéve ha admin, de most egyszerűsítünk)
            if (ticket.Status == TicketStatus.Closed && newStatus != TicketStatus.Closed)
            {
                throw new Exception("Lezárt jegy nem módosítható!");
            }

            ticket.Status = newStatus;
            _ticketRepo.Update(ticket);
        }
    }
}
