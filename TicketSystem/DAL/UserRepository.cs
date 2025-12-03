using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSystem.Models;

namespace TicketSystem.DAL
{
    public class UserRepository : IUserRepository
    {
        private List<User> _users;

        public UserRepository()
        {
            _users = new List<User>();
        }

        public void Add(User user)
        {
            _users.Add(user);
        }

        public User GetById(string id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public List<User> GetAll()
        {
            return _users;
        }
    }
}
