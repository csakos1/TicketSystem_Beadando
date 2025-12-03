using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using TicketSystem.Models;

namespace TicketSystem.DAL
{
    public static class JsonDataLoader
    {
        // Ez az osztály csak az adatok struktúráját tükrözi a JSON-ben
        private class DataWrapper
        {
            public List<User> Users { get; set; }
            public List<Ticket> Tickets { get; set; }
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
                // Fájl beolvasása szövegként
                string jsonString = File.ReadAllText(filePath);

                // Konfiguráció, hogy kezelje az Enumokat szövegként is
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                // Deszerializálás (JSON -> Objektum)
                var data = JsonSerializer.Deserialize<DataWrapper>(jsonString, options);

                if (data != null)
                {
                    // Feltöltjük a repókat
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
