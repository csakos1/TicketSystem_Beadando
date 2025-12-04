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
                if (OperatingSystem.IsWindows()) Console.SetCursorPosition(0, 0);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("=== TICKET RENDSZER BEJELENTKEZÉS ===");
                Console.WriteLine("(Kilépéshez hagyd üresen és nyomj Entert)");
                Console.Write("Kérem a User ID-t (pl. C001, A101, A102): ");
                string? userId = Console.ReadLine();

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
                Console.WriteLine($"=== MUNKATÁRS MENÜ ({_currentUser!.Name}) ===");
                Console.WriteLine("1. Jegyek listázása és Szűrés");
                Console.WriteLine("2. Jegy keresése ID alapján");
                Console.WriteLine("3. Statisztikák");
                Console.WriteLine("0. Kijelentkezés");
                Console.Write("Választás: ");

                switch (Console.ReadLine())
                {
                    case "1": FilterTicketsScreen(); break;
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
                Console.WriteLine($"=== ÜGYFÉL MENÜ ({_currentUser!.Name}) ===");
                Console.WriteLine("1. Saját jegyeim (Időrendben)");
                Console.WriteLine("2. Saját jegyeim (Státusz szerint)");
                Console.WriteLine("3. Új jegy létrehozása");
                Console.WriteLine("0. Kijelentkezés");
                Console.Write("Választás: ");

                switch (Console.ReadLine())
                {
                    case "1": ListTicketsScreen(false, sortByDate: true); break;
                    case "2": ListTicketsScreen(false, sortByDate: false); break;
                    case "3": CreateTicketScreen(); break;
                    case "0": return;
                }
            }
        }

        private void FilterTicketsScreen()
        {
            Console.Clear();
            Console.WriteLine("--- SZŰRÉSI LEHETŐSÉGEK ---");
            Console.WriteLine("1. Összes jegy");
            Console.WriteLine("2. Csak a sajátjaim (Assigned to Me)");
            Console.WriteLine("3. Szűrés STÁTUSZ alapján");
            Console.WriteLine("4. Szűrés KATEGÓRIA alapján");
            Console.WriteLine("5. Szűrés LÉTREHOZÁS DÁTUMA alapján");
            Console.WriteLine("0. Vissza");
            Console.Write("Választás: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ListTicketsScreen(true);
                    break;

                case "2":
                    // Itt javítottam: közvetlenül az ID-t adjuk át filterként
                    ListTicketsScreen(true, assignedToFilter: _currentUser!.Id);
                    break;

                case "3":
                    var status = PickEnum<TicketStatus>("Válassz státuszt:");
                    if (status.HasValue) ListTicketsScreen(true, statusFilter: status.Value);
                    break;

                case "4":
                    var cat = PickEnum<TicketCategory>("Válassz kategóriát:");
                    if (cat.HasValue) ListTicketsScreen(true, catFilter: cat.Value);
                    break;

                case "5":
                    Console.Write("\nAdd meg a dátumot (ÉÉÉÉ-HH-NN): ");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                    {
                        ListTicketsScreen(true, dateFilter: date);
                    }
                    else
                    {
                        Console.WriteLine("Hibás dátum! Enter...");
                        Console.ReadLine();
                    }
                    break;

                case "0": return;
            }
        }

        // JAVÍTVA: assignedToMe (bool) helyett assignedToFilter (string?)
        private void ListTicketsScreen(
            bool isAgent,
            string? assignedToFilter = null,
            TicketStatus? statusFilter = null,
            TicketCategory? catFilter = null,
            DateTime? dateFilter = null,
            bool sortByDate = false)
        {
            int page = 0;
            int pageSize = 10;

            while (true)
            {
                Console.Clear();
                Console.Write("=== JEGYEK LISTÁJA");
                if (statusFilter != null) Console.Write($" (Státusz: {statusFilter})");
                if (catFilter != null) Console.Write($" (Kat: {catFilter})");
                if (assignedToFilter != null) Console.Write($" (Felelős: {assignedToFilter})");
                if (dateFilter != null) Console.Write($" (Dátum: {dateFilter:yyyy-MM-dd})");
                Console.WriteLine(" ===");

                string? customerFilter = isAgent ? null : _currentUser!.Id;

                var tickets = _ticketService.GetTickets(assignedToFilter, statusFilter, catFilter, customerFilter, dateFilter, sortByDate);

                var pageItems = tickets.Skip(page * pageSize).Take(pageSize).ToList();

                if (pageItems.Count == 0 && page > 0) { page--; continue; }
                if (pageItems.Count == 0 && page == 0) Console.WriteLine("   Nincs megjeleníthető jegy.");

                foreach (var t in pageItems)
                {
                    TicketView.PrintListItem(t, _currentUser!.Id);
                }

                Console.WriteLine(new string('-', 60));
                Console.WriteLine($"Oldal: {page + 1} | [N] Köv. | [P] Előző | [ID] Megnyitás | [BACK] Vissza");
                Console.Write("Parancs: ");
                string? input = Console.ReadLine()?.ToUpper();

                if (input == "BACK") return;
                if (input == "N") page++;
                if (input == "P" && page > 0) page--;
                if (!string.IsNullOrEmpty(input) && input.StartsWith("T")) OpenTicketDetails(input);
            }
        }

        private void OpenTicketDetails(string ticketId)
        {
            var ticket = _ticketService.GetTicketById(ticketId);
            if (ticket == null) { Console.WriteLine("Nincs ilyen jegy! Enter..."); Console.ReadLine(); return; }

            bool isAgent = _currentUser!.Role == UserRole.Agent;

            while (true)
            {
                ticket = _ticketService.GetTicketById(ticketId);
                if (ticket == null) return; // Ha közben törölték volna

                TicketView.PrintDetails(ticket, isAgent);
                if (isAgent) TicketView.PrintHistory(ticket.History);
                TicketView.PrintMessages(ticket.Messages, isAgent);

                Console.WriteLine("MŰVELETEK:");
                Console.WriteLine("1. Üzenet írása");

                if (isAgent)
                {
                    Console.WriteLine("2. Státusz módosítása");
                    Console.WriteLine("3. Átadás másnak / Átvétel");
                    Console.WriteLine("4. Belső megjegyzés írása");
                }
                Console.WriteLine("0. Vissza");
                Console.Write("Választás: ");
                string? choice = Console.ReadLine();

                try
                {
                    if (choice == "0") return;
                    if (choice == "1")
                    {
                        Console.Write("Üzenet: ");
                        string msg = Console.ReadLine() ?? ""; // Null check javítva
                        _ticketService.AddMessage(ticket.TicketId, _currentUser.Id, msg, false);
                    }
                    if (choice == "4" && isAgent)
                    {
                        Console.Write("BELSŐ megjegyzés: ");
                        string msg = Console.ReadLine() ?? ""; // Null check javítva
                        _ticketService.AddMessage(ticket.TicketId, _currentUser.Id, msg, true);
                    }
                    if (choice == "2" && isAgent)
                    {
                        var newStatus = PickEnum<TicketStatus>("Új státusz:");
                        if (newStatus.HasValue)
                            _ticketService.ChangeStatus(ticket.TicketId, newStatus.Value, _currentUser.Id);
                    }
                    if (choice == "3" && isAgent)
                    {
                        Console.Write("Kinek adjuk át? (pl. A101, A102): ");
                        string targetAgent = Console.ReadLine() ?? ""; // Null check javítva
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
            Console.Write("Cím: ");
            string title = Console.ReadLine() ?? ""; // Null check
            Console.Write("Leírás: ");
            string desc = Console.ReadLine() ?? "";  // Null check

            var cat = PickEnum<TicketCategory>("Válassz kategóriát:");

            if (cat.HasValue)
            {
                try
                {
                    var t = _ticketService.CreateTicket(_currentUser!.Id, title, desc, cat.Value);
                    Console.WriteLine($"Siker! ID: {t.TicketId}. Enter...");
                    Console.ReadLine();
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); Console.ReadLine(); }
            }
        }

        private void FindTicketScreen()
        {
            Console.Write("Ticket ID: ");
            string? id = Console.ReadLine();
            if (!string.IsNullOrEmpty(id)) OpenTicketDetails(id);
        }

        private void ShowStatisticsScreen()
        {
            Console.Clear();
            Console.WriteLine(_statsService.GenerateReport());
            Console.WriteLine("\nEnter...");
            Console.ReadLine();
        }

        private T? PickEnum<T>(string prompt) where T : struct, Enum
        {
            Console.Clear();
            Console.WriteLine(prompt);
            var values = Enum.GetValues<T>();
            int i = 1;
            foreach (var val in values)
            {
                Console.WriteLine($"{i}. {val}");
                i++;
            }
            Console.Write("Választás száma: ");
            if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= values.Length)
            {
                return (T)values.GetValue(selection - 1)!;
            }
            Console.WriteLine("Érvénytelen választás!");
            Console.ReadLine();
            return null;
        }
    }
}