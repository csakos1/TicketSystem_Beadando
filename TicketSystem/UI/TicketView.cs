using System;
using System.Collections.Generic;
using System.Linq;
using TicketSystem.Models;

namespace TicketSystem.UI
{
    public static class TicketView
    {
        public static void PrintListItem(Ticket t, string currentUserId)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"[{t.TicketId}] ");

            // Ha hozzám van rendelve, jelöljük meg!
            if (t.AssignedAgentId == currentUserId)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write(" [SAJÁT] ");
                Console.ResetColor();
            }

            string title = t.Title.Length > 25 ? t.Title.Substring(0, 22) + "..." : t.Title.PadRight(25);
            Console.Write($" {title} ");

            switch (t.Status)
            {
                case TicketStatus.New: Console.ForegroundColor = ConsoleColor.Green; break;
                case TicketStatus.Closed: Console.ForegroundColor = ConsoleColor.DarkGray; break;
                case TicketStatus.InProgress: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case TicketStatus.WaitingForUser: Console.ForegroundColor = ConsoleColor.Magenta; break;
                case TicketStatus.Resolved: Console.ForegroundColor = ConsoleColor.Blue; break;
            }
            Console.Write($"{t.Status.ToString().PadRight(14)}");
            Console.ResetColor();

            Console.WriteLine($" | {t.Category} | {t.CreatedAt:MM.dd HH:mm}");
        }

        public static void PrintDetails(Ticket ticket, bool isAgent)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"--- JEGY ADATLAP: {ticket.TicketId} ---");
            Console.ResetColor();
            Console.WriteLine($"Cím: {ticket.Title} | Kat: {ticket.Category} | Létrehozva: {ticket.CreatedAt:yyyy-MM-dd HH:mm}");
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

        public static void PrintMessages(List<Message> messages, bool isAgent)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("--- ÜZENETEK ---");
            Console.ResetColor();

            if (messages.Count == 0) Console.WriteLine("(Nincs üzenet)");

            foreach (var msg in messages.TakeLast(6))
            {
                // Ha belső üzenet és nem agent nézi, akkor kihagyjuk
                if (msg.IsInternal && !isAgent) continue;

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"[{msg.Timestamp:HH:mm}] ");

                if (msg.IsInternal)
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[BELSŐ] ");
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{msg.Sender}: ");
                Console.ResetColor();
                Console.WriteLine(msg.Text);
            }
            Console.WriteLine("------------------------------------------------------------");
        }

        public static void PrintHistory(List<StatusChangeLog> history)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("NAPLÓ (utolsó 3 esemény):");
            foreach (var log in history.TakeLast(3))
            {
                Console.WriteLine($" > {log.Timestamp:HH:mm} [{log.ModifierName}]: {log.OldStatus} -> {log.NewStatus}");
            }
            Console.WriteLine("------------------------------------------------------------");
            Console.ResetColor();
        }
    }
}