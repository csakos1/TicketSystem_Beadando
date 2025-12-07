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

            // 1. Státusz szerinti eloszlás
            sb.AppendLine("Eloszlás státusz szerint:");
            var statusGroups = tickets.GroupBy(t => t.Status);
            foreach (var group in statusGroups)
                sb.AppendLine($" - {group.Key}: {group.Count()} db");

            // 2. Kategória szerinti eloszlás
            sb.AppendLine("\nEloszlás kategória szerint:");
            var catGroups = tickets.GroupBy(t => t.Category);
            foreach (var group in catGroups)
                sb.AppendLine($" - {group.Key}: {group.Count()} db");

            // 3. Megoldási arány
            int closedCount = tickets.Count(t => t.Status == TicketStatus.Closed || t.Status == TicketStatus.Resolved);
            double ratio = tickets.Count > 0 ? (double)closedCount / tickets.Count * 100 : 0;
            sb.AppendLine($"\nMegoldott/Lezárt arány: {ratio:F1}%");

            // 4. Átlagos megoldási idő
            var resolvedOrClosed = tickets
                .Where(t => (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed) && t.ResolvedAt != null)
                .ToList();

            if (resolvedOrClosed.Count > 0)
            {
                double avgMinutes = resolvedOrClosed
                    .Average(t => (t.ResolvedAt.Value - t.CreatedAt).TotalMinutes);
                sb.AppendLine($"Átlagos megoldási idő: {avgMinutes:F1} perc");
            }
            else
            {
                sb.AppendLine("Átlagos megoldási idő: Nincs elég adat (vagy nincs megoldott jegy)");
            }

            return sb.ToString();
        }
    }
}