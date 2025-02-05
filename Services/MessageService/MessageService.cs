using DowntimeTracker.Data;
using DowntimeTracker.Services.MessageService;
using Newtonsoft.Json.Linq;
using System.Net;
using static DowntimeTracker.Services.AccessService;

namespace DiscrepancyReport.Services.MessageService
{
    public class MessageServices
    {
        private IConfiguration _config;
        private readonly TCZNT5000 _context;

        public MessageServices(IConfiguration config, TCZNT5000 context)
        {
            _config = config;
            _context = context;
        }

        public Task<HttpWebResponse> SendMessage(MessageModel message)
        {
            return Task.Run(() =>
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://websvc.tcz.flextronics.com/Notification/SEND");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    dynamic jsonObject = new JObject();
                    jsonObject.To = message.To;
                    jsonObject.Cc = message.Cc;
                    jsonObject.Subject = message.Subject;
                    jsonObject.Text = message.Body;

                    streamWriter.Write(jsonObject);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                return (HttpWebResponse)httpWebRequest.GetResponse();
            });
        }

        // access review email message
        public MessageModel AccessReview(List<UserAccessDto>? aUsers, List<UserAccessDto>? nUsers)
        {
            string[] emails_list = new string[] { "henryk.janikowski@flex.com" };
            // email variables
            string email_disclaimer = "Niniejsza wiadomość e-mail została wygenerowana automatycznie." +
                " Prosimy nie odpowiadać na nią, ponieważ ten adres e-mail jest wykorzystywany tylko do wysyłania, a nie odbierania wiadomości e-mail.";

            // Email body context
            string body_context = $"Proszę o przegląd poniższych użytkowników w celu weryfiacji dostępów do aplikacji";

            // Check if aUsers is not null and has items
            if (aUsers != null && aUsers.Any())
            {
                body_context += $"<br><br><strong>Administratorzy</strong><br><br>";
                body_context += "<table style='border-collapse: collapse; width: 100%;'>";
                body_context += "<tr style='border: 1px solid black;'>" +
                    "<th style='border: 1px solid black; padding: 8px;'>Imię i Nazwisko (Login)</th>" +
                    "<th style='border: 1px solid black; padding: 8px;'>Poziom Dostępu</th>" +
                    "<th style='border: 1px solid black; padding: 8px;'>Ilość Dni od Ostatniego Logowania</th>" +
                    "</tr>";
                foreach (var user in aUsers)
                {
                    body_context += $"<tr style='border: 1px solid black;'><td style='border: 1px solid black; padding: 8px;'>{user.NameSurname} ({user.UserAd})</td>" +
                        $"<td style='border: 1px solid black; padding: 8px;'>{user.AccessLevel}</td>" +
                        $"<td style='border: 1px solid black; padding: 8px;'>{user.DaysSinceLastLogin}</td>" +
                        $"</tr>";
                }
            }

            body_context += "</table><br><br>";

            // Second table
            if (nUsers != null && nUsers.Any())
            {
                body_context += $"<strong>Normalni użytkownicy:</strong><br><br>";
                body_context += "<table style='border-collapse: collapse; width: 100%;'>";
                body_context += "<tr style='border: 1px solid black;'>" +
                    "<th style='border: 1px solid black; padding: 8px;'>Imię i Nazwisko (Login)</th>" +
                    "<th style='border: 1px solid black; padding: 8px;'>Poziom Dostępu</th>" +
                    "<th style='border: 1px solid black; padding: 8px;'>Ilość Dni od Ostatniego Logowania</th>" +
                    "</tr>";
                foreach (var user in nUsers)
                {
                    body_context += $"<tr style='border: 1px solid black;'><td style='border: 1px solid black; padding: 8px;'>{user.NameSurname} ({user.UserAd})</td>" +
                        $"<td style='border: 1px solid black; padding: 8px;'>{user.AccessLevel}</td>" +
                        $"<td style='border: 1px solid black; padding: 8px;'>{user.DaysSinceLastLogin}</td>" +
                        $"</tr>";
                }
            }

            body_context += "</table><br>";
            body_context += email_disclaimer;

            return new MessageModel
            {
                To = string.Join(";", emails_list),
                Cc = string.Join(";", "karoljacek.sliwka@flex.com"),
                Subject = $"Downtime Tracker - Kwartalny przegląd dostępów.",
                Body = body_context // Ensure body_context is also initialized
            };

        }
    }
}
