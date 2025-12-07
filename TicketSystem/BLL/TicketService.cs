using System;
using System.Collections.Generic;
using System.Linq;
using TicketSystem.Models;
using TicketSystem.DAL;

namespace TicketSystem.BLL
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepo;
        private readonly IUserRepository _userRepo;

        public TicketService(ITicketRepository ticketRepo, IUserRepository userRepo)
        {
            _ticketRepo = ticketRepo;
            _userRepo = userRepo;
        }

        public Ticket CreateTicket(string userId, string title, string description, TicketCategory category)
        {
            var user = _userRepo.GetById(userId);
            if (user == null) throw new Exception("Nem létező felhasználó!");
            if (string.IsNullOrWhiteSpace(title)) throw new Exception("A cím nem lehet üres!");

            string newId = "T" + _ticketRepo.GetNextId().ToString("D3");
            var ticket = new Ticket(newId, userId, title, description, category);

            ticket.History.Add(new StatusChangeLog(user.Name, "-", "New"));

            _ticketRepo.Add(ticket);
            return ticket;
        }

        public List<Ticket> GetTickets(
            string? assignedToFilter = null,
            TicketStatus? statusFilter = null,
            TicketCategory? categoryFilter = null,
            string? customerIdFilter = null,
            DateTime? dateFilter = null,
            bool sortByDateDesc = false)
        {
            CheckAndAutoCloseTickets();

            var tickets = _ticketRepo.GetAll().AsQueryable();

            if (assignedToFilter != null)
                tickets = tickets.Where(t => t.AssignedAgentId == assignedToFilter);

            if (statusFilter != null)
                tickets = tickets.Where(t => t.Status == statusFilter);

            if (categoryFilter != null)
                tickets = tickets.Where(t => t.Category == categoryFilter);

            if (customerIdFilter != null)
                tickets = tickets.Where(t => t.CustomerId == customerIdFilter);

            if (dateFilter != null)
                tickets = tickets.Where(t => t.CreatedAt.Date == dateFilter.Value.Date);

            if (sortByDateDesc)
                tickets = tickets.OrderByDescending(t => t.CreatedAt);
            else
                tickets = tickets.OrderBy(t => t.CreatedAt);

            return tickets.ToList();
        }

        public Ticket? GetTicketById(string id)
        {
            return _ticketRepo.GetById(id);
        }

        public void AddMessage(string ticketId, string userId, string text, bool isInternal)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null) throw new Exception("A jegy nem található.");

            var user = _userRepo.GetById(userId);
            if (user == null) throw new Exception("Ismeretlen felhasználó.");

            ticket.Messages.Add(new Message(user.Name, text, isInternal));

            if (!isInternal)
            {
                if (user.Role == UserRole.Agent &&
                    ticket.Status != TicketStatus.Closed &&
                    ticket.Status != TicketStatus.Resolved)
                {
                    ChangeStatusLogic(ticket, TicketStatus.WaitingForUser, user.Name);
                }

                if (user.Role == UserRole.Customer &&
                   (ticket.Status == TicketStatus.WaitingForUser || ticket.Status == TicketStatus.New))
                {
                    ChangeStatusLogic(ticket, TicketStatus.InProgress, user.Name);
                }
            }

            _ticketRepo.Update(ticket);
        }

        public void AssignTicket(string ticketId, string agentId, string modifierUserId)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null) throw new Exception("Hibás Ticket ID");

            var agent = _userRepo.GetById(agentId);
            if (agent == null || agent.Role != UserRole.Agent) throw new Exception("Hibás Agent ID (Cél)");

            var modifier = _userRepo.GetById(modifierUserId);
            if (modifier == null) throw new Exception("Hibás módosító User ID");

            string oldAgent = ticket.AssignedAgentId ?? "Nincs";
            ticket.AssignedAgentId = agentId;

            ticket.History.Add(new StatusChangeLog(modifier.Name, $"Assign: {oldAgent}", $"Assign: {agent.Name}"));

            if (ticket.Status == TicketStatus.New)
            {
                ChangeStatusLogic(ticket, TicketStatus.InProgress, modifier.Name);
            }

            _ticketRepo.Update(ticket);
        }

        public void ChangeStatus(string ticketId, TicketStatus newStatus, string modifierUserId)
        {
            var ticket = _ticketRepo.GetById(ticketId);
            if (ticket == null) throw new Exception("Jegy nem található");

            var user = _userRepo.GetById(modifierUserId);
            if (user == null) throw new Exception("Hibás módosító User ID");

            ChangeStatusLogic(ticket, newStatus, user.Name);
            _ticketRepo.Update(ticket);
        }

        private void ChangeStatusLogic(Ticket ticket, TicketStatus newStatus, string modifierName)
        {
            if (ticket.Status == newStatus) return;

            if (ticket.Status == TicketStatus.Closed && newStatus != TicketStatus.Closed)
                throw new Exception("Lezárt jegy nem módosítható!");

            ticket.History.Add(new StatusChangeLog(modifierName, ticket.Status.ToString(), newStatus.ToString()));

            ticket.Status = newStatus;

            if (newStatus == TicketStatus.Resolved)
            {
                ticket.ResolvedAt = DateTime.Now;
            }
            else
            {
                ticket.ResolvedAt = null;
            }
        }

        private void CheckAndAutoCloseTickets()
        {
            var resolvedTickets = _ticketRepo.GetAll()
                .Where(t => t.Status == TicketStatus.Resolved && t.ResolvedAt != null).ToList();

            foreach (var t in resolvedTickets)
            {
                if (t.ResolvedAt.HasValue && (DateTime.Now - t.ResolvedAt.Value).TotalMinutes >= 1)
                {
                    t.History.Add(new StatusChangeLog("RENDSZER", "Resolved", "Closed"));
                    t.Status = TicketStatus.Closed;
                    _ticketRepo.Update(t);
                }
            }
        }
    }
}