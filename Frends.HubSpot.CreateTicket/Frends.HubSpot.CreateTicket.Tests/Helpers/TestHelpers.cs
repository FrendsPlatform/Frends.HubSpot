using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Frends.HubSpot.CreateTicket.Tests.Helpers
{
    public static class TestHelpers
    {
        public static async Task<bool> DeleteTestTicket(
            string ticketId,
            string apiKey,
            string baseUrl,
            CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            await client.DeleteAsync($"{baseUrl.TrimEnd('/')}/crm/v3/objects/tickets/{ticketId}", cancellationToken);
            var response = await client.DeleteAsync($"{baseUrl.TrimEnd('/')}/crm/v3/objects/tickets/{ticketId}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return false;

            response.EnsureSuccessStatusCode();
            return false;
        }

        public static async Task<string> GetTestTicket(
            string ticketId,
            string apiKey,
            string baseUrl,
            CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var response = await client.GetAsync(
                $"{baseUrl.TrimEnd('/')}/crm/v3/objects/tickets/{ticketId}" +
                "?properties=subject,content,hs_ticket_priority,hs_pipeline,hs_pipeline_stage,hs_ticket_category,hubspot_owner_id",
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public static async Task<string> GetTestTicketAssociations(
            string ticketId,
            string toObjectType,
            string apiKey,
            string baseUrl,
            CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var response = await client.GetAsync(
                $"{baseUrl.TrimEnd('/')}/crm/v4/objects/tickets/{ticketId}/associations/{toObjectType}",
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
