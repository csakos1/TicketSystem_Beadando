using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSystem.Models;

namespace TicketSystem.DAL
{
    public class TicketRepository : ITicketRepository
    {
        private List<Ticket> _tickets;

        public TicketRepository()
        {
            _tickets = new List<Ticket>();
        }

        private readonly object _lock = new object();

        public void Add(Ticket ticket)
        {
            lock (_lock)
            {
                _tickets.Add(ticket);
            }
        }

        public List<Ticket> GetAll()
        {
            return _tickets;
        }

        public Ticket GetById(string id)
        {
            return _tickets.FirstOrDefault(t => t.TicketId == id);
        }

        public void Update(Ticket ticket)
        {

        }

        public int GetNextId()
        {
            lock (_lock)
            {
                return _tickets.Count + 1;
            }
        }
    }
}
