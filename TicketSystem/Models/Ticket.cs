using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class Ticket
    {
        public string TicketId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string? AssignedAgentId { get; set; } // Ez lehet null (? jel)

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public TicketCategory Category { get; set; }
        public TicketStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public List<Message> Messages { get; set; } = new List<Message>();
        public List<StatusChangeLog> History { get; set; } = new List<StatusChangeLog>();

        public Ticket(string id, string customerId, string title, string description, TicketCategory category)
        {
            TicketId = id;
            CustomerId = customerId;
            Title = title;
            Description = description;
            Category = category;

            Status = TicketStatus.New;
            CreatedAt = DateTime.Now;
            AssignedAgentId = null;
        }

        public Ticket() { }
    }
}
