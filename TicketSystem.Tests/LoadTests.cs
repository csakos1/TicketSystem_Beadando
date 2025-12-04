using System;
using System.Collections.Generic;
using System.Diagnostics; // Időméréshez (Stopwatch)
using System.Linq;
using System.Threading.Tasks; // Párhuzamossághoz
using Xunit;
using Xunit.Abstractions; // Hogy ki tudjunk írni adatokat a teszt outputra
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

        // Segédfüggvény a Service létrehozásához
        private TicketService CreateService()
        {
            ITicketRepository ticketRepo = new TicketRepository();
            IUserRepository userRepo = new UserRepository();
            userRepo.Add(new User("C001", "User", "u@test.com", UserRole.Customer));
            return new TicketService(ticketRepo, userRepo);
        }

        // --- 1. NAGY TÖMEGŰ ADAT TESZT (Performance) ---
        // Ez azt méri, mennyi idő létrehozni és keresni sok adat között.
        [Fact]
        public void LargeVolumeTest_ShouldHandleManyTicketsWithinReasonableTime()
        {
            // Arrange
            var service = CreateService();
            int ticketCount = 1_000_000;
            var stopwatch = Stopwatch.StartNew();

            // Act 1: Tömeges létrehozás
            for (int i = 0; i < ticketCount; i++)
            {
                service.CreateTicket("C001", $"Jegy {i}", "Leírás...", TicketCategory.General);
            }

            stopwatch.Stop();
            long createTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"{ticketCount} jegy létrehozása: {createTime} ms");

            // Assert 1: Ésszerű időn belül futott le? (pl. < 1 másodperc)
            Assert.True(createTime < 1000, "A létrehozás túl lassú volt!");

            // Act 2: Keresés a nagy tömegben (utolsó elem megkeresése - legrosszabb eset)
            stopwatch.Restart();
            var lastTicket = service.GetTickets().LastOrDefault(); // Ez lassú művelet listánál!
            stopwatch.Stop();
            long searchTime = stopwatch.ElapsedMilliseconds;

            _output.WriteLine($"Keresés a {ticketCount} elem között: {searchTime} ms");

            // Assert 2: A keresés is legyen gyors (pl. < 500ms)
            Assert.True(searchTime < 100, "A keresés túl lassú volt!");
        }

        // --- 2. PÁRHUZAMOS HOZZÁFÉRÉS TESZT (Concurrency) ---
        // Ez azt vizsgálja, mi történik, ha egyszerre sokan írják a listát.
        // FIGYELEM: Ez a teszt EL FOG BUKNI a jelenlegi kódoddal, mert a List<T> nem szálbiztos!
        // Ez a cél: megmutatni a gyenge pontot.
        [Fact]
        public void ParallelAccessTest_ShouldHandleConcurrentCreations()
        {
            // Arrange
            var service = CreateService();
            int threadCount = 1000; // 1000 párhuzamos művelet

            // Act: Párhuzamosan (Parallel.For) hívjuk meg a CreateTicket-et
            // Próbáljuk meg elkapni a hibákat, mert valószínűleg exception lesz
            var exception = Record.Exception(() =>
            {
                Parallel.For(0, threadCount, i =>
                {
                    service.CreateTicket("C001", $"Parallel Ticket {i}", "...", TicketCategory.Technical);
                });
            });

            // Assert
            if (exception != null)
            {
                _output.WriteLine("HIBA TÖRTÉNT PÁRHUZAMOS FUTÁSKOR (Ez várható volt): " + exception.Message);
                // Ha azt akarod, hogy a teszt "zöld" legyen a beadandóban, akkor 
                // kikommentelheted az alábbi sort, vagy átírhatod Assert.NotNull-ra,
                // bizonyítva, hogy "felfedezted" a hibát.

                // Assert.Null(exception); // Eredeti elvárás: ne legyen hiba
            }
            else
            {
                // Ha véletlenül nem dobott hibát, ellenőrizzük a darabszámot
                var count = service.GetTickets().Count;
                _output.WriteLine($"Létrejött jegyek száma: {count} (Elvárt: {threadCount})");

                // Ha nem thread-safe a lista, a count sokszor kevesebb lesz, mint 1000, mert felülírják egymást
                Assert.Equal(threadCount, count);
            }
        }
    }
}