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
        // Ez a lista helyettesíti most az adatbázist
        private List<Ticket> _tickets;

        public TicketRepository()
        {
            _tickets = new List<Ticket>();
        }

        private readonly object _lock = new object();

        public void Add(Ticket ticket)
        {
            lock (_lock) // Csak egy szál léphet be ide egyszerre
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
            // Megkeresi az elsőt, akinek az ID-ja egyezik, vagy null-t ad vissza
            return _tickets.FirstOrDefault(t => t.TicketId == id);
        }

        public void Update(Ticket ticket)
        {
            // Memóriában lévő lista esetén, ha módosítod az objektumot, 
            // az a listában is módosul, így ide nem feltétlen kell kód, 
            // de adatbázisnál itt lenne az UPDATE parancs.
        }

        public int GetNextId()
        {
            lock (_lock) // Itt is zárni kell
            {
                return _tickets.Count + 1;
            }
        }
    }
}
