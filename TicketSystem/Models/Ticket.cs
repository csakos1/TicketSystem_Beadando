using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class Ticket
    {
        public string TicketId { get; set; }        // Egyedi azonosító, pl. "T001"
        public string CustomerId { get; set; }      // Kié a jegy?
        public string AssignedAgentId { get; set; } // Ki dolgozik rajta? (lehet null)

        public string Title { get; set; }           // Rövid cím
        public string Description { get; set; }     // Hosszú leírás

        public TicketCategory Category { get; set; } // Enum
        public TicketStatus Status { get; set; }     // Enum (Állapot)

        public DateTime CreatedAt { get; set; }      // Létrehozás ideje

        // Ez a lista tárolja a beszélgetést (Kompozíció)
        public List<Message> Messages { get; set; } = new List<Message>();

        // Konstruktor új jegy létrehozásához
        public Ticket(string id, string customerId, string title, string description, TicketCategory category)
        {
            TicketId = id;
            CustomerId = customerId;
            Title = title;
            Description = description;
            Category = category;

            // Alapértelmezett értékek beállítása
            Status = TicketStatus.New;
            CreatedAt = DateTime.Now;
            AssignedAgentId = null; // Még senki nem vette fel
        }

        // Üres konstruktor JSON-höz
        public Ticket() { }
    }
}
