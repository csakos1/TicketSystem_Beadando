using System;
using Xunit;
using TicketSystem.BLL;
using TicketSystem.DAL;
using TicketSystem.Models;
using System.Linq;

namespace TicketSystem.Tests
{
    public class TicketServiceTests
    {
        private TicketService CreateServiceWithMockData()
        {
            ITicketRepository ticketRepo = new TicketRepository();
            IUserRepository userRepo = new UserRepository();

            userRepo.Add(new User("C001", "Teszt Elek", "elek@test.com", UserRole.Customer));
            userRepo.Add(new User("A101", "Admin Anna", "anna@test.com", UserRole.Agent));

            return new TicketService(ticketRepo, userRepo);
        }

        [Fact]
        public void CreateTicket_ShouldCreateNewTicket_WhenInputIsValid()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Hiba", "Nem megy a net", TicketCategory.Technical);

            Assert.NotNull(ticket);
            Assert.Equal("Hiba", ticket.Title);
            Assert.Equal(TicketStatus.New, ticket.Status);
        }

        [Fact]
        public void CreateTicket_ShouldThrowException_WhenUserDoesNotExist()
        {
            var service = CreateServiceWithMockData();
            Assert.Throws<Exception>(() =>
                service.CreateTicket("NON_EXISTENT_USER", "Cím", "Leírás", TicketCategory.General));
        }

        [Fact]
        public void CreateTicket_ShouldThrowException_WhenTitleIsMissing()
        {
            var service = CreateServiceWithMockData();
            Assert.Throws<Exception>(() =>
                service.CreateTicket("C001", "", "Leírás", TicketCategory.General));
        }

        [Fact]
        public void AssignTicket_ShouldAssignAgent_WhenDataIsCorrect()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.AssignTicket(ticket.TicketId, "A101", "A101");

            var updatedTicket = service.GetTicketById(ticket.TicketId);
            Assert.NotNull(updatedTicket);
            Assert.Equal("A101", updatedTicket.AssignedAgentId);
            Assert.Equal(TicketStatus.InProgress, updatedTicket.Status);
        }

        [Fact]
        public void AssignTicket_ShouldThrowException_WhenUserIsNotAgent()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            Assert.Throws<Exception>(() => service.AssignTicket(ticket.TicketId, "C001", "A101"));
        }

        [Fact]
        public void AddMessage_ShouldAddMessage_ToTicket()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.AddMessage(ticket.TicketId, "C001", "Új infó", false);

            Assert.Single(ticket.Messages);
            Assert.Equal("Új infó", ticket.Messages[0].Text);
        }

        [Fact]
        public void AddMessage_ShouldAutoChangeStatus_WhenCustomerReplies()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.ChangeStatus(ticket.TicketId, TicketStatus.WaitingForUser, "A101");

            service.AddMessage(ticket.TicketId, "C001", "Itt a válaszom", false);

            Assert.Equal(TicketStatus.InProgress, ticket.Status);
        }

        [Fact]
        public void ChangeStatus_ShouldUpdateStatus()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.ChangeStatus(ticket.TicketId, TicketStatus.Resolved, "A101");

            Assert.Equal(TicketStatus.Resolved, ticket.Status);
        }

        [Fact]
        public void ChangeStatus_ShouldThrowException_WhenReopeningClosedTicket()
        {
            var service = CreateServiceWithMockData();
            var ticket = service.CreateTicket("C001", "Teszt", "...", TicketCategory.General);

            service.ChangeStatus(ticket.TicketId, TicketStatus.Closed, "A101");

            Assert.Throws<Exception>(() =>
                service.ChangeStatus(ticket.TicketId, TicketStatus.InProgress, "A101"));
        }

        [Fact]
        public void GetTickets_ShouldFilterByStatus()
        {
            var service = CreateServiceWithMockData();
            var t1 = service.CreateTicket("C001", "Jegy 1", "...", TicketCategory.General);
            var t2 = service.CreateTicket("C001", "Jegy 2", "...", TicketCategory.General);

            service.ChangeStatus(t1.TicketId, TicketStatus.Closed, "A101");

            var result = service.GetTickets(statusFilter: TicketStatus.New);

            Assert.Single(result);
            Assert.Equal("Jegy 2", result[0].Title);
        }

        [Fact]
        public void GetTickets_ShouldFilterByAgent()
        {
            var service = CreateServiceWithMockData();
            var t1 = service.CreateTicket("C001", "Jegy 1", "...", TicketCategory.General);

            service.AssignTicket(t1.TicketId, "A101", "A101");

            var result = service.GetTickets(assignedToFilter: "A101");

            Assert.Single(result);
        }
    }
}