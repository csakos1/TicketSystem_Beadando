using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public enum TicketStatus
    {
        New,
        InProgress,
        WaitingForUser,
        Resolved,
        Closed
    }

    public enum TicketCategory
    {
        General,
        Technical,
        Billing,
        PasswordReset
    }

    public enum UserRole
    {
        Customer,
        Agent
    }
}
