using System;
using Xunit; // Ha MSTest-et használsz, akkor 'using Microsoft.VisualStudio.TestTools.UnitTesting;' és [Fact] helyett [TestMethod]
using TicketSystem.BLL;
using TicketSystem.DAL;
using TicketSystem.Models;
using System.Linq;

namespace TicketSystem.Tests
{
    public class TicketServiceTests
    {
        // Segédfüggvény: Létrehoz egy "tiszta" service-t minden teszthez
        private TicketService CreateServiceWithMockData()
        {
            // 1. Létrehozzuk az üres memóriatárolókat
            ITicketRepository ticketRepo = new TicketRepository();
            IUserRepository userRepo = new UserRepository();

            // 2. Tesztadatok feltöltése (hogy legyen mivel dolgozni)
            userRepo.Add(new User("C001", "Teszt Elek", "elek@test.com", UserRole.Customer));
            userRepo.Add(new User("A101", "Admin Anna", "anna@test.com", UserRole.Agent));

            // 3. Visszaadjuk a Service-t, ami ezeket használja
            return new TicketService(ticketRepo, userRepo);
        }

        // --- 1. JEGY LÉTREHOZÁS TESZTEK ---

        [Fact] // Ez jelzi, hogy ez egy teszteset
        public void CreateTicket_ShouldCreateNewTicket_WhenInputIsValid()
        {
            // Arrange (Előkészítés)
            var service = CreateServiceWithMockData();

            // Act (Cselekvés)
            var ticket = service.CreateTicket("C001", "Hiba", "Nem megy a net", TicketCategory.Technical);

            // Assert (Ellenőrzés)
            Assert.NotNull(ticket); // Létrejött?
            Assert.Equal("Hiba", ticket.Title); // Jó a címe?
            Assert.Equal(TicketStatus.New, ticket.Status); // Új státuszú?
            Assert.NotNull(ticket.TicketId); // Kapott ID-t?
        }

        [Fact]
        public void CreateTicket_ShouldThrowException_WhenUserDoesNotExist()
        {
            var service = CreateServiceWithMockData();

            // Act & Assert: Hibát várunk, ha nem létező ID-val próbáljuk
            var ex = Assert.Throws<Exception>(() =>
                service.CreateTicket("NON_EXISTENT_USER", "Cím", "Leírás", TicketCategory.General));

            Assert.Equal("Nem létező felhasználó!", ex.Message);
        }

        [Fact]
        public void CreateTicket_ShouldThrowException_WhenTitleIsMissing()
        {
            var service = CreateServiceWithMockData();

            // Üres címmel hiba kell
            Assert.Throws<Exception>(() =>
                service.CreateTicket("C001", "", "Leírás", TicketCategory.General));
        }

        // --- 2. HOZZÁRENDELÉS (ASSIGN) TESZTEK ---

        [Fact]
        public void AssignTicket_ShouldAssignAgent_WhenDataIsCorrect()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            // Act: Hozzárendeljük az ügynököt
            service.AssignTicket(ticket.TicketId, "A101");

            // Assert
            var updatedTicket = service.GetTicketById(ticket.TicketId);
            Assert.Equal("A101", updatedTicket.AssignedAgentId);
            Assert.Equal(TicketStatus.InProgress, updatedTicket.Status); // Automata státuszváltás check
        }

        [Fact]
        public void AssignTicket_ShouldThrowException_WhenUserIsNotAgent()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            // Act & Assert: Ügyfélnek (C001) próbáljuk kiosztani a jegyet -> HIBA
            Assert.Throws<Exception>(() => service.AssignTicket(ticket.TicketId, "C001"));
        }

        // --- 3. ÜZENETKÜLDÉS ÉS STÁTUSZVÁLTÁS TESZTEK ---

        [Fact]
        public void AddMessage_ShouldAddMessage_ToTicket()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.AddMessage(ticket.TicketId, "C001", "Új infó");

            Assert.Single(ticket.Messages); // 1 üzenet van?
            Assert.Equal("Új infó", ticket.Messages[0].Text);
        }

        [Fact]
        public void AddMessage_ShouldAutoChangeStatus_WhenCustomerReplies()
        {
            // Scenario: Visszakérdezés állapotban az ügyfél ír -> Váltson Folyamatban-ra
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            // Kézzel beállítjuk "Waiting"-re
            service.ChangeStatus(ticket.TicketId, TicketStatus.WaitingForUser);

            // Act: Ügyfél ír
            service.AddMessage(ticket.TicketId, "C001", "Itt a válaszom");

            // Assert
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
        }

        // --- 4. ÁLLAPOTGÉP TESZTEK ---

        [Fact]
        public void ChangeStatus_ShouldUpdateStatus()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.ChangeStatus(ticket.TicketId, TicketStatus.Resolved);

            Assert.Equal(TicketStatus.Resolved, ticket.Status);
        }

        [Fact]
        public void ChangeStatus_ShouldThrowException_WhenReopeningClosedTicket()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            // Lezárjuk
            service.ChangeStatus(ticket.TicketId, TicketStatus.Closed);

            // Act & Assert: Próbáljuk visszanyitni
            var ex = Assert.Throws<Exception>(() =>
                service.ChangeStatus(ticket.TicketId, TicketStatus.InProgress));

            Assert.Contains("Lezárt jegy nem módosítható", ex.Message);
        }

        // --- 5. SZŰRÉS TESZTEK ---

        [Fact]
        public void GetTickets_ShouldFilterByStatus()
        {
            var service = CreateServiceWithMockData();
            // Létrehozunk 2 jegyet
            var t1 = service.CreateTicket("C001", "Jegy 1", "...", TicketCategory.General);
            var t2 = service.CreateTicket("C001", "Jegy 2", "...", TicketCategory.General);

            // Egyiket lezárjuk
            service.ChangeStatus(t1.TicketId, TicketStatus.Closed);

            // Act: Csak az Új jegyeket kérjük
            var result = service.GetTickets(statusFilter: TicketStatus.New);

            // Assert
            Assert.Single(result); // Csak 1 db legyen
            Assert.Equal("Jegy 2", result[0].Title);
        }

        [Fact]
        public void GetTickets_ShouldFilterByAgent()
        {
            var service = CreateServiceWithMockData();
            var t1 = service.CreateTicket("C001", "Jegy 1", "...", TicketCategory.General);

            service.AssignTicket(t1.TicketId, "A101");

            // Act: Kérjük az A101 jegyeit
            var result = service.GetTickets(agentIdFilter: "A101");

            Assert.Single(result);
        }
    }
}