using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly string _phoneNumberId;

    public WhatsAppService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _accessToken = config["WhatsApp:AccessToken"];
        _phoneNumberId = config["WhatsApp:PhoneNumberId"];
    }

    public async Task<bool> SendTextMessage(string toPhoneNumber, string messageText)
    {
        var requestUri = $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneNumber,
            type = "text",
            text = new { body = messageText }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _httpClient.PostAsync(requestUri, content);
        return response.IsSuccessStatusCode;
    }
}
