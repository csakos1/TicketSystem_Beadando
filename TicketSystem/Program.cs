using System;
using System.IO;
using TicketSystem.BLL;
using TicketSystem.DAL;
using TicketSystem.UI;

namespace TicketSystem.App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // --- UI FIXÁLÁS ---
            // Ez a rész oldja meg a csúszkálást!
            // Csak Windowson működik garantáltan (Visual Studio alatt ez az alap)
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Console.SetWindowSize(100, 30); // 100 karakter széles, 30 sor magas
                    Console.SetBufferSize(100, 300); // A görgethető terület magassága (elég nagy a listához)
                }
                catch (Exception)
                {
                    // Ha véletlenül nem engedi a rendszer (pl. túl nagy a betűméret), nem omlunk össze
                }
            }
            Console.CursorVisible = true;
            // ------------------

            // 1. Réteg: Adathozzáférés (DAL)
            ITicketRepository ticketRepo = new TicketRepository();
            IUserRepository userRepo = new UserRepository();

            string jsonPath = "data.json";

            // Kis trükk: kiírjuk, hogy töltünk, majd törlünk, hogy tiszta legyen a képernyő
            Console.WriteLine("Rendszer indítása...");
            JsonDataLoader.LoadData(jsonPath, userRepo, ticketRepo);
            System.Threading.Thread.Sleep(500); // Fél mp szünet, hogy látszódjon

            // 2. Réteg: Üzleti Logika (BLL)
            TicketService ticketService = new TicketService(ticketRepo, userRepo);
            StatisticsService statsService = new StatisticsService(ticketRepo);

            // 3. Réteg: UI indítása
            MainMenu menu = new MainMenu(ticketService, statsService, userRepo);
            menu.Show();
        }
    }
}