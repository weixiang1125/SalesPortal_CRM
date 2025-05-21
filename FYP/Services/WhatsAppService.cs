using SharedLibrary.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class WhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly string _phoneNumberId;
    private readonly string _baseUrl;
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
        // Try get dynamic ngrok URL
        try
        {
            var tunnels = JsonDocument.Parse(new HttpClient().GetStringAsync("http://127.0.0.1:4040/api/tunnels").Result);
            _baseUrl = tunnels.RootElement
                .GetProperty("tunnels")
                .EnumerateArray()
                .FirstOrDefault(x => x.GetProperty("proto").GetString() == "https")
                .GetProperty("public_url")
                .GetString();
            Console.WriteLine("🔗 Ngrok BaseUrl from API: " + _baseUrl);
        }
        catch
        {
            _baseUrl = config["WhatsApp:BaseUrl"]; // fallback
            Console.WriteLine("⚠️ Using fallback BaseUrl from appsettings.json: " + _baseUrl);
        }
    }

    public async Task<bool> SendMessageAsync(ChatMessage message)
    {
        string toPhoneNumber = NormalizePhone(message.ContactPhone);
        string type = message.MessageType?.ToLower();
        var requestUri = $"https://graph.facebook.com/v18.0/{_phoneNumberId}/messages";
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        if (type != "text")
        {
            string fullLink = $"{_baseUrl.TrimEnd('/')}{message.MessageText}";
            Console.WriteLine($"📸 Final WhatsApp media link: {fullLink}");
        }
        object payload = type switch
        {
            "text" => new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "text",
                text = new { body = message.MessageText }
            },
            "image" => new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "image",
                image = new { link = $"{_baseUrl.TrimEnd('/')}{message.MessageText}" }
            },
            "video" => new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "video",
                video = new { link = $"{_baseUrl.TrimEnd('/')}{message.MessageText}" }
            },
            "audio" => new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "audio",
                audio = new { link = $"{_baseUrl.TrimEnd('/')}{message.MessageText}" }
            },
            "document" => new
            {
                messaging_product = "whatsapp",
                to = toPhoneNumber,
                type = "document",
                document = new { link = $"{_baseUrl.TrimEnd('/')}{message.MessageText}" }
            },
            _ => null
        };

        if (payload == null)
        {
            Console.WriteLine("❌ Unsupported message type: " + type);
            return false;
        }

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(requestUri, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"📤 WhatsApp SendMessageAsync ({type}): {response.StatusCode}, Body: {responseBody}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error sending WhatsApp message: " + ex.Message);
            return false;
        }
    }
}

