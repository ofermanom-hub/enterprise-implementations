using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseIntegrations.DocuSign
{
    /// Lightweight DocuSign REST v2.1 client.
    /// Handles JWT auth + exponential-backoff retries.
    public class DocuSignClient
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private JwtTokenService _tokenSvc;

        public DocuSignClient(string accountId, string jwtSecret)
        {
            _http = new HttpClient();
            _baseUrl = $"https://na4.docusign.net/restapi/v2.1/accounts/{accountId}";
            _tokenSvc = new JwtTokenService(jwtSecret);
        }

        public async Task<string> SendEnvelopeAsync(
            string recipientEmail,
            string recipientName,
            byte[] documentBytes,
            string docName)
        {
            var token = _tokenSvc.GenerateToken(
                "integration-user", "sender", "default");
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers
                    .AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                emailSubject = "Please sign this document",
                status = "sent",
                documents = new[] { new {
                    documentBase64 = Convert.ToBase64String(documentBytes),
                    name = docName, documentId = "1"
                }},
                recipients = new {
                    signers = new[] { new {
                        email = recipientEmail,
                        name = recipientName,
                        recipientId = "1",
                        tabs = new {
                            signHereTabs = new[] { new {
                                anchorString = "/sig1/",
                                anchorUnits = "pixels"
                            }}
                        }
                    }}
                }
            };

            return await PostWithRetryAsync(
                $"{_baseUrl}/envelopes",
                JsonSerializer.Serialize(payload));
        }

        private async Task<string> PostWithRetryAsync(
            string url, string json, int maxRetries = 3)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                var content = new StringContent(
                    json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync(url, content);

                if (resp.IsSuccessStatusCode)
                    return await resp.Content.ReadAsStringAsync();

                if (attempt == maxRetries - 1) resp.EnsureSuccessStatusCode();
                await Task.Delay(500 * (int)Math.Pow(2, attempt));
            }
            return null;
        }
    }
}
