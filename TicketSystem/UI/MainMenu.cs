using System;
using System.Collections.Generic;
using System.Linq;
using TicketSystem.BLL;
using TicketSystem.Models;
using TicketSystem.DAL;

namespace TicketSystem.UI
{
    public class MainMenu
    {
        private readonly TicketService _ticketService;
        private readonly StatisticsService _statsService;
        private readonly IUserRepository _userRepo;
        private User? _currentUser;

        public MainMenu(TicketService ticketService, StatisticsService statsService, IUserRepository userRepo)
        {
            _ticketService = ticketService;
            _statsService = statsService;
            _userRepo = userRepo;
        }

        public void Show()
        {
            while (true)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("=== TICKET RENDSZER BEJELENTKEZÉS ===");
                Console.WriteLine("(Kilépéshez hagyd üresen és nyomj Entert)");
                Console.Write("Kérem a User ID-t (pl. C001, A101, A102): ");
                string userId = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userId)) break;

                _currentUser = _userRepo.GetById(userId);

                if (_currentUser == null)
                {
                    Console.WriteLine("Hibás azonosító! Enter...");
                    Console.ReadLine();
                    continue;
                }

                if (_currentUser.Role == UserRole.Agent) AgentMenuLoop();
                else CustomerMenuLoop();
            }
        }

        private void AgentMenuLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== MUNKATÁRS MENÜ ({_currentUser.Name}) ===");
                Console.WriteLine("1. Jegyek listázása (Szűrés)");
                Console.WriteLine("2. Jegy keresése ID alapján");
                Console.WriteLine("3. Statisztikák");
                Console.WriteLine("0. Kijelentkezés");
                Console.Write("Választás: ");

                switch (Console.ReadLine())
                {
                    case "1": FilterTicketsScreen(); break; // ÚJ szűrő menü
                    case "2": FindTicketScreen(); break;
                    case "3": ShowStatisticsScreen(); break;
                    case "0": return;
                }
            }
        }

        private void CustomerMenuLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"=== ÜGYFÉL MENÜ ({_currentUser.Name}) ===");
                Console.WriteLine("1. Saját jegyeim (Időrendben)");
                Console.WriteLine("2. Saját jegyeim (Státusz szerint)");
                Console.WriteLine("3. Új jegy létrehozása");
                Console.WriteLine("0. Kijelentkezés");
                Console.Write("Választás: ");

                switch (Console.ReadLine())
                {
                    case "1": ListTicketsScreen(false, sortByDate: true); break;
                    case "2": ListTicketsScreen(false, sortByDate: false); break; // Alapból nem, de lehetne status filtert kérni
                    case "3": CreateTicketScreen(); break;
                    case "0": return;
                }
            }
        }

        // ÚJ: Szűrő menü Agenteknek
        private void FilterTicketsScreen()
        {
            Console.Clear();
            Console.WriteLine("--- SZŰRÉSI LEHETŐSÉGEK ---");
            Console.WriteLine("1. Összes jegy");
            Console.WriteLine("2. Csak a sajátjaim (Assigned to Me)");
            Console.WriteLine("3. Csak 'Új' státuszúak");
            Console.WriteLine("4. Csak 'Technical' kategória");
            Console.Write("Választás: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "2": ListTicketsScreen(true, assignedToMe: true); break;
                case "3": ListTicketsScreen(true, statusFilter: TicketStatus.New); break;
                case "4": ListTicketsScreen(true, catFilter: TicketCategory.Technical); break;
                default: ListTicketsScreen(true); break; // Összes
            }
        }

        private void ListTicketsScreen(bool isAgent, bool assignedToMe = false, TicketStatus? statusFilter = null, TicketCategory? catFilter = null, bool sortByDate = false)
        {
            int page = 0;
            int pageSize = 10;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== JEGYEK LISTÁJA ===");

                string agentFilter = assignedToMe ? _currentUser.Id : null;
                string customerFilter = isAgent ? null : _currentUser.Id;

                // Meghívjuk a BLL bővített metódusát
                var tickets = _ticketService.GetTickets(agentFilter, statusFilter, catFilter, customerFilter, sortByDate);

                var pageItems = tickets.Skip(page * pageSize).Take(pageSize).ToList();

                if (pageItems.Count == 0 && page > 0) { page--; continue; }

                foreach (var t in pageItems)
                {
                    TicketView.PrintListItem(t, _currentUser.Id);
                }

                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Oldal: {page + 1} | [N] Köv. | [P] Előző | [ID] Megnyitás | [BACK] Vissza");
                Console.Write("Választás: ");
                string input = Console.ReadLine()?.ToUpper();

                if (input == "BACK") return;
                if (input == "N") page++;
                if (input == "P" && page > 0) page--;
                if (input != null && input.StartsWith("T")) OpenTicketDetails(input);
            }
        }

        private void OpenTicketDetails(string ticketId)
        {
            var ticket = _ticketService.GetTicketById(ticketId);
            if (ticket == null) { Console.WriteLine("Nincs ilyen jegy! Enter..."); Console.ReadLine(); return; }

            bool isAgent = _currentUser.Role == UserRole.Agent;

            while (true)
            {
                // Frissítjük az objektumot (hátha változott)
                ticket = _ticketService.GetTicketById(ticketId);

                TicketView.PrintDetails(ticket, isAgent);
                if (isAgent) TicketView.PrintHistory(ticket.History); // Csak agent látja a naplót
                TicketView.PrintMessages(ticket.Messages, isAgent);

                Console.WriteLine("MŰVELETEK:");
                Console.WriteLine("1. Üzenet írása");

                if (isAgent)
                {
                    Console.WriteLine("2. Státusz módosítása");
                    Console.WriteLine("3. Átadás másnak / Átvétel");
                    Console.WriteLine("4. Belső megjegyzés írása (Internal Note)");
                }
                Console.WriteLine("0. Vissza");
                Console.Write("Választás: ");
                string choice = Console.ReadLine();

                try
                {
                    if (choice == "0") return;
                    if (choice == "1")
                    {
                        Console.Write("Üzenet: ");
                        _ticketService.AddMessage(ticket.TicketId, _currentUser.Id, Console.ReadLine(), false);
                    }
                    if (choice == "4" && isAgent)
                    {
                        Console.Write("BELSŐ megjegyzés: ");
                        _ticketService.AddMessage(ticket.TicketId, _currentUser.Id, Console.ReadLine(), true);
                    }
                    if (choice == "2" && isAgent)
                    {
                        Console.WriteLine("Új státusz (0:New, 1:InProgress, 2:Waiting, 3:Resolved, 4:Closed):");
                        if (int.TryParse(Console.ReadLine(), out int s))
                            _ticketService.ChangeStatus(ticket.TicketId, (TicketStatus)s, _currentUser.Id);
                    }
                    if (choice == "3" && isAgent)
                    {
                        Console.Write("Kinek adjuk át? (pl. A101, A102): ");
                        string targetAgent = Console.ReadLine();
                        _ticketService.AssignTicket(ticket.TicketId, targetAgent, _currentUser.Id);
                        Console.WriteLine("Átadva!");
                        System.Threading.Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"HIBA: {ex.Message}");
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }
        }

        private void CreateTicketScreen()
        {
            Console.Clear();
            Console.WriteLine("=== ÚJ JEGY ===");
            Console.Write("Cím: "); string title = Console.ReadLine();
            Console.Write("Leírás: "); string desc = Console.ReadLine();
            Console.WriteLine("Kategória (0:General, 1:Technical, 2:Billing, 3:Password): ");
            int catInt = int.Parse(Console.ReadLine() ?? "0");

            try
            {
                var t = _ticketService.CreateTicket(_currentUser.Id, title, desc, (TicketCategory)catInt);
                Console.WriteLine($"Siker! ID: {t.TicketId}. Enter...");
                Console.ReadLine();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.ReadLine(); }
        }

        private void FindTicketScreen()
        {
            Console.Write("Ticket ID: ");
            string id = Console.ReadLine();
            if (!string.IsNullOrEmpty(id)) OpenTicketDetails(id);
        }

        private void ShowStatisticsScreen()
        {
            Console.Clear();
            Console.WriteLine(_statsService.GenerateReport());
            Console.WriteLine("\nEnter...");
            Console.ReadLine();
        }
    }
}