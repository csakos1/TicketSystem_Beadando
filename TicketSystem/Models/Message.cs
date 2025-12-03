using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class Message
    {
        public string Sender { get; set; }      // Ki írta? (pl. "customer" vagy a név)
        public string Text { get; set; }        // Az üzenet szövege
        public DateTime Timestamp { get; set; } // Mikor írta?

        public Message(string sender, string text)
        {
            Sender = sender;
            Text = text;
            Timestamp = DateTime.Now; // Létrehozáskor az aktuális időt kapja
        }

        // Üres konstruktor JSON-höz
        public Message() { }
    }
}
