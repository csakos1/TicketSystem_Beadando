using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class StatusChangeLog
    {
        public DateTime Timestamp { get; set; }
        public string ModifierName { get; set; } = string.Empty;
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;

        public StatusChangeLog() { }

        public StatusChangeLog(string modifierName, string oldStatus, string newStatus)
        {
            Timestamp = DateTime.Now;
            ModifierName = modifierName;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}
