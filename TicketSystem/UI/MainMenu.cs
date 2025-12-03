using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private User _currentUser;

        public MainMenu(TicketService ticketService, StatisticsService statsService, IUserRepository userRepo)
        {
            _ticketService = ticketService;
            _statsService = statsService;
            _userRepo = userRepo;
        }

        // --- FŐ BELÉPÉSI PONT ---
        public void Show()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("=== TICKET RENDSZER BEJELENTKEZÉS ===");
                Console.WriteLine("(Kilépéshez hagyd üresen és nyomj Entert)");
                Console.Write("Kérem a User ID-t (pl. C001 vagy A101): ");
                string userId = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userId)) break;

                _currentUser = _userRepo.GetById(userId);

                if (_currentUser == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Hibás azonosító! Nyomj Entert...");
                    Console.ResetColor();
                    Console.ReadLine();
                    continue;
                }

                Console.WriteLine($"Üdvözöllek, {_currentUser.Name} ({_currentUser.Role})!");
                System.Threading.Thread.Sleep(1000);

                if (_currentUser.Role == UserRole.Agent)
                {
                    AgentMenuLoop();
                }
                else
                {
                    CustomerMenuLoop();
                }
            }
        }

        // --- MUNKATÁRS MENÜ ---
        private void AgentMenuLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"=== MUNKATÁRS MENÜ ({_currentUser.Name}) ===");
                Console.ResetColor();
                Console.WriteLine("1. Összes nyitott jegy listázása");
                Console.WriteLine("2. Jegy keresése ID alapján");
                Console.WriteLine("3. Statisztikák");
                Console.WriteLine("0. Kijelentkezés");
                Console.Write("Választás: ");

                switch (Console.ReadLine())
                {
                    case "1": ListTicketsScreen(isAgent: true); break;
                    case "2": FindTicketScreen(); break;
                    case "3": ShowStatisticsScreen(); break;
                    case "0": return;
                }
            }
        }

        // --- ÜGYFÉL MENÜ ---
        private void CustomerMenuLoop()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"=== ÜGYFÉL MENÜ ({_currentUser.Name}) ===");
                Console.ResetColor();
                Console.WriteLine("1. Saját jegyeim listázása");
                Console.WriteLine("2. Új jegy létrehozása");
                Console.WriteLine("0. Kijelentkezés");
                Console.Write("Választás: ");

                switch (Console.ReadLine())
                {
                    case "1": ListTicketsScreen(isAgent: false); break;
                    case "2": CreateTicketScreen(); break;
                    case "0": return;
                }
            }
        }

        // --- LISTÁZÁS (Itt használjuk a TicketView-t!) ---
        private void ListTicketsScreen(bool isAgent)
        {
            int page = 0;
            int pageSize = 10; // 10 jegy kényelmesen elfér a képernyőn az új kompakt nézettel

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== JEGYEK LISTÁJA ===");
                Console.WriteLine($"{"[ID]",-7} {"CÍM",-31} {"STÁTUSZ",-12} | KATEGÓRIA"); // Fejléc
                Console.WriteLine(new string('-', 60));

                List<Ticket> tickets;
                if (isAgent)
                {
                    tickets = _ticketService.GetTickets();
                }
                else
                {
                    tickets = _ticketService.GetTickets().Where(t => t.CustomerId == _currentUser.Id).ToList();
                }

                var pageItems = tickets.Skip(page * pageSize).Take(pageSize).ToList();

                if (pageItems.Count == 0 && page > 0)
                {
                    page--;
                    continue;
                }

                foreach (var t in pageItems)
                {
                    TicketView.PrintListItem(t);
                }

                // Üres sorok kitöltése, hogy a menü mindig ugyanott legyen (opcionális, de szép)
                int emptyLines = pageSize - pageItems.Count;
                for (int i = 0; i < emptyLines; i++) Console.WriteLine();

                Console.WriteLine(new string('-', 60));
                // Itt a menü, és közvetlenül utána jön majd a kurzor
                Console.WriteLine($"Oldal: {page + 1} | [N] Köv. | [P] Előző | [ID] Megnyitás | [BACK] Vissza");
                Console.Write("Választás: "); // Itt villog majd a kurzor

                string input = Console.ReadLine()?.ToUpper();

                if (input == "BACK") return;
                if (input == "N") page++;
                if (input == "P" && page > 0) page--;

                if (input != null && input.StartsWith("T"))
                {
                    OpenTicketDetails(input);
                }
            }
        }

        // --- JEGY RÉSZLETEK (Itt is TicketView-t használunk!) ---
        private void OpenTicketDetails(string ticketId)
        {
            var ticket = _ticketService.GetTicketById(ticketId);
            if (ticket == null)
            {
                Console.WriteLine("Nincs ilyen jegy! Enter...");
                Console.ReadLine();
                return;
            }

            while (true)
            {
                // 1. Megjelenítés kiszervezve a TicketView osztályba
                TicketView.PrintDetails(ticket);
                TicketView.PrintMessages(ticket.Messages);

                // 2. Menü kirajzolása
                Console.WriteLine("MŰVELETEK:");
                Console.WriteLine("1. Üzenet írása");

                if (_currentUser.Role == UserRole.Agent)
                {
                    Console.WriteLine("2. Státusz módosítása");
                    Console.WriteLine("3. Jegy átvétele (Assign)");
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
                        string msg = Console.ReadLine();
                        _ticketService.AddMessage(ticket.TicketId, _currentUser.Id, msg);
                        Console.WriteLine("Üzenet elküldve!");
                        System.Threading.Thread.Sleep(500); // Kis szünet, hogy lássa a kiírást
                    }

                    if (choice == "2" && _currentUser.Role == UserRole.Agent)
                    {
                        Console.WriteLine("Új státusz (0:New, 1:InProgress, 2:Waiting, 3:Resolved, 4:Closed):");
                        if (int.TryParse(Console.ReadLine(), out int statusInt))
                        {
                            _ticketService.ChangeStatus(ticket.TicketId, (TicketStatus)statusInt);
                            Console.WriteLine("Státusz módosítva!");
                            System.Threading.Thread.Sleep(500);
                        }
                    }

                    if (choice == "3" && _currentUser.Role == UserRole.Agent)
                    {
                        _ticketService.AssignTicket(ticket.TicketId, _currentUser.Id);
                        Console.WriteLine("Jegy hozzád rendelve!");
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

        // --- ÚJ JEGY LÉTREHOZÁSA ---
        private void CreateTicketScreen()
        {
            Console.Clear();
            Console.WriteLine("=== ÚJ JEGY LÉTREHOZÁSA ===");
            Console.Write("Cím: ");
            string title = Console.ReadLine();
            Console.Write("Leírás: ");
            string desc = Console.ReadLine();

            Console.WriteLine("Kategória (0:General, 1:Technical, 2:Billing, 3:Password): ");
            int catInt = int.Parse(Console.ReadLine() ?? "0");

            try
            {
                var t = _ticketService.CreateTicket(_currentUser.Id, title, desc, (TicketCategory)catInt);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Jegy sikeresen létrehozva! ID: {t.TicketId}");
                Console.ResetColor();
                Console.WriteLine("Nyomj Entert...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Hiba: {ex.Message}");
                Console.ResetColor();
                Console.ReadLine();
            }
        }

        // --- JEGY KERESÉSE ---
        private void FindTicketScreen()
        {
            Console.Write("Add meg a Ticket ID-t (pl. T001): ");
            string id = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(id))
            {
                OpenTicketDetails(id);
            }
        }

        // --- STATISZTIKA ---
        private void ShowStatisticsScreen()
        {
            Console.Clear();
            // A StatisticsService generálja a szöveget, mi csak kiírjuk
            Console.WriteLine(_statsService.GenerateReport());
            Console.WriteLine("\nNyomj Entert a visszalépéshez...");
            Console.ReadLine();
        }
    }
}
