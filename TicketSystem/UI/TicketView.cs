using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSystem.Models;


namespace TicketSystem.UI
{
    public static class TicketView
    {
        // Listázásnál egy sorba írunk mindent
        public static void PrintListItem(Ticket t)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{t.TicketId}] ");
            Console.ResetColor();

            // Cím hossza limitálva, hogy kiférjen
            string title = t.Title.Length > 30 ? t.Title.Substring(0, 27) + "..." : t.Title.PadRight(30);
            Console.Write($"{title} ");

            // Státusz színezése
            switch (t.Status)
            {
                case TicketStatus.New: Console.ForegroundColor = ConsoleColor.Green; break;
                case TicketStatus.Closed: Console.ForegroundColor = ConsoleColor.DarkGray; break;
                case TicketStatus.InProgress: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case TicketStatus.WaitingForUser: Console.ForegroundColor = ConsoleColor.Magenta; break;
            }
            Console.Write($"{t.Status.ToString().PadRight(12)}"); // Fix szélesség
            Console.ResetColor();

            Console.WriteLine($" | {t.Category}");
        }

        // Részletes nézet - TÖMÖRÍTVE
        public static void PrintDetails(Ticket ticket)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"--- JEGY ADATLAP: {ticket.TicketId} ---");
            Console.ResetColor();

            // Egy sorba több adatot teszünk
            Console.WriteLine($"Cím: {ticket.Title} | Kat: {ticket.Category} | Létrehozva: {ticket.CreatedAt:yyyy-MM-dd}");

            Console.Write("Státusz: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{ticket.Status}   ");
            Console.ResetColor();
            Console.WriteLine($"Felelős: {(ticket.AssignedAgentId ?? "-")}");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("------------------------------------------------------------");
            Console.ResetColor();

            Console.WriteLine("LEÍRÁS:");
            Console.WriteLine(ticket.Description);
            Console.WriteLine();
        }

        // Üzenetek - CSAK AZ UTOLSÓ 5 DB és EGY SORBAN
        public static void PrintMessages(List<Message> messages)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("--- UTOLSÓ ÜZENETEK ---");
            Console.ResetColor();

            if (messages.Count == 0)
            {
                Console.WriteLine("(Nincs üzenet)");
            }
            else
            {
                // Csak az utolsó 5 üzenetet mutatjuk, hogy ne kelljen görgetni
                var lastMessages = messages.TakeLast(5).ToList();

                foreach (var msg in lastMessages)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"[{msg.Timestamp:HH:mm}] ");

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{msg.Sender}: ");

                    Console.ResetColor();
                    Console.WriteLine(msg.Text);
                }

                if (messages.Count > 5)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"... (további {messages.Count - 5} régebbi üzenet elrejtve)");
                    Console.ResetColor();
                }
            }
            Console.WriteLine("------------------------------------------------------------");
        }
    }
}
