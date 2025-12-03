using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class User
    {
        // Ezek a Property-k (Adattagok)
        public string Id { get; set; }      // pl. "C001"
        public string Name { get; set; }    // pl. "Kiss Péter"
        public string Email { get; set; }   // pl. "peter@example.com"
        public UserRole Role { get; set; }  // Enum használata

        // Konstruktor (hogy könnyű legyen létrehozni)
        public User(string id, string name, string email, UserRole role)
        {
            Id = id;
            Name = name;
            Email = email;
            Role = role;
        }

        // Üres konstruktor a későbbi JSON betöltéshez (néha kell)
        public User() { }
    }
}
