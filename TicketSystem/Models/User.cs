using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystem.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public User(string id, string name, string email, UserRole role)
        {
            Id = id;
            Name = name;
            Email = email;
            Role = role;
        }

        public User() { }
    }
}
