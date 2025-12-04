using System;
using System.Linq;
using System.Text;
using TicketSystem.DAL;
using TicketSystem.Models;

namespace TicketSystem.BLL
{
    public class StatisticsService
    {
        private readonly ITicketRepository _ticketRepo;

        public StatisticsService(ITicketRepository ticketRepo)
        {
            _ticketRepo = ticketRepo;
        }

        public string GenerateReport()
        {
            var tickets = _ticketRepo.GetAll();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("=== RENDSZER STATISZTIKA ===");
            sb.AppendLine($"Összes jegy száma: {tickets.Count}");
            sb.AppendLine("---------------------------");

            sb.AppendLine("Eloszlás státusz szerint:");
            var statusGroups = tickets.GroupBy(t => t.Status);
            foreach (var group in statusGroups)
                sb.AppendLine($" - {group.Key}: {group.Count()} db");

            sb.AppendLine("\nEloszlás kategória szerint:");
            var catGroups = tickets.GroupBy(t => t.Category);
            foreach (var group in catGroups)
                sb.AppendLine($" - {group.Key}: {group.Count()} db");

            // MEGOLDÁSI IDŐ SZÁMÍTÁSA
            var resolvedOrClosed = tickets
                .Where(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.ResolvedAt != null)
                .ToList();

            if (resolvedOrClosed.Count > 0)
            {
                double avgMinutes = resolvedOrClosed
                    .Average(t => (t.ResolvedAt.Value - t.CreatedAt).TotalMinutes);
                sb.AppendLine($"\nÁtlagos megoldási idő: {avgMinutes:F1} perc");
            }
            else
            {
                sb.AppendLine("\nÁtlagos megoldási idő: Nincs elég adat (vagy nincs megoldott jegy)");
            }

            return sb.ToString();
        }
    }
}