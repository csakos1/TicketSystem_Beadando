using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using TicketSystem.Models;

namespace TicketSystem.DAL
{
    public static class JsonDataLoader
    {
        private class DataWrapper
        {
            public List<User>? Users { get; set; }
            public List<Ticket>? Tickets { get; set; }
        }

        public static void LoadData(string filePath, IUserRepository userRepo, ITicketRepository ticketRepo)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Figyelem: Az adatfájl nem található: " + filePath);
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var data = JsonSerializer.Deserialize<DataWrapper>(jsonString, options);

                if (data != null)
                {
                    if (data.Users != null)
                    {
                        foreach (var user in data.Users) userRepo.Add(user);
                    }

                    if (data.Tickets != null)
                    {
                        foreach (var ticket in data.Tickets) ticketRepo.Add(ticket);
                    }

                    Console.WriteLine("Adatok sikeresen betöltve.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hiba a JSON betöltésekor: " + ex.Message);
            }
        }
    }
}