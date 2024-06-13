using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TradingBot.Domain.API.CoinSpotAPI.Request;

namespace TradingBot.Domain.API.CoinSpotAPI.DelegatingHandler;

public class AuthorisationDelegatingHandler(IOptions<CoinspotApiSettings> options, ILogger<AuthorisationDelegatingHandler> logger) : System.Net.Http.DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Authorising request");
        var nonce = DateTime.Now.Ticks;
        var msg = request.Content?.ReadAsStringAsync(cancellationToken).Result;
        var jsonMessage = msg?.Replace(BaseRequest.NoncePlaceholder, nonce.ToString()) ?? string.Empty;
        logger.LogInformation("Request message: {jsonMessage}", jsonMessage);
        request.Content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

        var hmac = new HMACSHA512(Encoding.ASCII.GetBytes(options.Value.Secret ?? ""));
        var byteArray = Encoding.ASCII.GetBytes(jsonMessage);

        using (var stream = new MemoryStream(byteArray)) {
            var hash = hmac.ComputeHash(stream).Aggregate("", (s, e) => s + $"{e:x2}", s => s);
            
            request.Headers.Add("sign", hash);
            request.Headers.Add("key", options.Value.ApiKey);
        }
        logger.LogInformation("Request authorised");
        return base.SendAsync(request, cancellationToken);
    }
}