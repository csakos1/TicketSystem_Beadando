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
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Console.SetWindowSize(100, 30);
                    Console.SetBufferSize(100, 300);
                }
                catch (Exception) { }
            }
            Console.CursorVisible = true;

            ITicketRepository ticketRepo = new TicketRepository();
            IUserRepository userRepo = new UserRepository();

            string jsonPath = "data.json";

            Console.WriteLine("Rendszer indítása...");
            JsonDataLoader.LoadData(jsonPath, userRepo, ticketRepo);
            System.Threading.Thread.Sleep(500);

            TicketService ticketService = new TicketService(ticketRepo, userRepo);
            StatisticsService statsService = new StatisticsService(ticketRepo);

            MainMenu menu = new MainMenu(ticketService, statsService, userRepo);
            menu.Show();
        }
    }
}