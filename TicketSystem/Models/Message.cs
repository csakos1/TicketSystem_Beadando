using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class Message
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsInternal { get; set; }

        public Message(string sender, string text, bool isInternal = false)
        {
            Sender = sender;
            Text = text;
            Timestamp = DateTime.Now;
            IsInternal = isInternal;
        }

        public Message() { }
    }
}
