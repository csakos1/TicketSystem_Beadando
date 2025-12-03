using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSystem.Models;

namespace TicketSystem.DAL
{
    public interface IUserRepository
    {
        void Add(User user);
        User GetById(string id);
        List<User> GetAll();
    }
}
