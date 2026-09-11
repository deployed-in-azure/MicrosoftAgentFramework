namespace _15_SelfHostedAgent_AgentId_OnBehalfOfFlow
{
    public class MsalLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var isTokenEndpoint = request.RequestUri != null && request.RequestUri.AbsolutePath.EndsWith("/oauth2/v2.0/token");

            if (isTokenEndpoint && request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                var decodedBody = System.Web.HttpUtility.UrlDecode(requestBody);
                var formattedParams = string.Join("\n", decodedBody.Split('&'));

                Console.WriteLine("\n=== OBO TOKEN EXCHANGE REQUEST ===");
                Console.WriteLine($"URL: {request.RequestUri}");
                Console.WriteLine("BODY:");
                Console.WriteLine(formattedParams);
                Console.WriteLine("==================================\n");
            }
            else
            {
                Console.WriteLine($"[HttpTrace] {request.Method} {request.RequestUri}");
            }

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpTrace] Request to {request.RequestUri} threw: {ex}");
                throw;
            }

            if (!isTokenEndpoint)
            {
                Console.WriteLine($"[HttpTrace] {request.Method} {request.RequestUri} -> {(int)response.StatusCode}");
            }

            if (isTokenEndpoint && response.IsSuccessStatusCode && response.Content != null)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var bearerToken = System.Text.Json.JsonDocument.Parse(responseBody).RootElement.GetProperty("access_token").GetString();

                Console.WriteLine("\n=== OBO TOKEN EXCHANGE RESPONSE ===");
                Console.WriteLine($"URL: {request.RequestUri}");
                Console.WriteLine("BEARER TOKEN:");
                Console.WriteLine(bearerToken);
                Console.WriteLine("====================================\n");
            }
            else if (isTokenEndpoint && !response.IsSuccessStatusCode && response.Content != null)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine("\n=== OBO TOKEN EXCHANGE FAILED ===");
                Console.WriteLine($"URL: {request.RequestUri}");
                Console.WriteLine($"STATUS: {(int)response.StatusCode}");
                Console.WriteLine($"BODY: {errorBody}");
                Console.WriteLine("==================================\n");
            }

            return response;
        }
    }
}
