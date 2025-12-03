using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public enum TicketStatus
    {
        New,            // Új
        InProgress,     // Folyamatban
        WaitingForUser, // Visszakérdezés az ügyféltől
        Resolved,       // Megoldva
        Closed          // Lezárva
    }

    // A jegy kategóriája
    public enum TicketCategory
    {
        General,        // Általános
        Technical,      // Technikai hiba
        Billing,        // Számlázás
        PasswordReset   // Jelszóhelyreállítás
    }

    // Felhasználói szerepkörök (hogy tudjuk, ki az Ügyfél és ki az Agent)
    public enum UserRole
    {
        Customer, // Ügyfél
        Agent     // Ügyfélszolgálatos
    }
}
