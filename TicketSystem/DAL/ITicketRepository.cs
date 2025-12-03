using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSystem.Models;

namespace TicketSystem.DAL
{
    public interface ITicketRepository
    {
        void Add(Ticket ticket);                // Hozzáadás
        List<Ticket> GetAll();                  // Összes lekérése
        Ticket GetById(string id);              // Egy konkrét jegy lekérése
        void Update(Ticket ticket);             // Frissítés (bár memóriában ez referencia miatt automatikus, de szép kiírni)
        int GetNextId();                        // Segéd: következő ID generálásához
    }
}
