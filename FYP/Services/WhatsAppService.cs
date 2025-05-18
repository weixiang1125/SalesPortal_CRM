using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly string _phoneNumberId;
    private string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return phone;
        phone = phone.Trim().Replace(" ", "");
        return phone.StartsWith("+") ? phone : "+" + phone;
    }

    public WhatsAppService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _accessToken = config["WhatsApp:AccessToken"];
        _phoneNumberId = config["WhatsApp:PhoneNumberId"];
    }

    public async Task<bool> SendTextMessage(string toPhoneNumber, string messageText)
    {
        toPhoneNumber = NormalizePhone(toPhoneNumber);

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

        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📤 WhatsApp SendTextMessage: {response.StatusCode}, Body: {responseBody}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error sending WhatsApp message: " + ex.Message);
            return false;
        }
    }
}
