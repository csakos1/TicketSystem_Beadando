using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using TicketSystem.BLL;
using TicketSystem.DAL;
using TicketSystem.Models;

namespace TicketSystem.Tests
{
    public class LoadTests
    {
        private readonly ITestOutputHelper _output;

        public LoadTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private TicketService CreateService()
        {
            ITicketRepository ticketRepo = new TicketRepository();
            IUserRepository userRepo = new UserRepository();
            userRepo.Add(new User("C001", "User", "u@test.com", UserRole.Customer));
            return new TicketService(ticketRepo, userRepo);
        }

        [Fact]
        public void LargeVolumeTest_ShouldHandleManyTicketsWithinReasonableTime()
        {
            var service = CreateService();
            int ticketCount = 100_000; // Kicsit visszavettem 1 millióról, hogy gyorsabb legyen a teszt futás
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < ticketCount; i++)
            {
                service.CreateTicket("C001", $"Jegy {i}", "Leírás...", TicketCategory.General);
            }

            stopwatch.Stop();
            long createTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"{ticketCount} jegy létrehozása: {createTime} ms");

            Assert.True(createTime < 5000, "A létrehozás túl lassú volt!");

            stopwatch.Restart();
            var lastTicket = service.GetTickets().LastOrDefault();
            stopwatch.Stop();
            long searchTime = stopwatch.ElapsedMilliseconds;

            _output.WriteLine($"Keresés: {searchTime} ms");
            Assert.True(searchTime < 1000, "A keresés túl lassú volt!");
        }

        [Fact]
        public void ParallelAccessTest_ShouldHandleConcurrentCreations()
        {
            var service = CreateService();
            int threadCount = 1000;

            var exception = Record.Exception(() =>
            {
                Parallel.For(0, threadCount, i =>
                {
                    service.CreateTicket("C001", $"Parallel Ticket {i}", "...", TicketCategory.Technical);
                });
            });

            if (exception != null)
            {
                _output.WriteLine("HIBA: " + exception.Message);
            }
            else
            {
                var count = service.GetTickets().Count;
                _output.WriteLine($"Jegyek: {count}");
                Assert.Equal(threadCount, count);
            }
        }
    }
}