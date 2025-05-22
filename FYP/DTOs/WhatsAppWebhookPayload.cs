namespace CRM_API.DTOs
{
    public class WhatsAppWebhookPayload
    {
        public string Object { get; set; }
        public List<Entry> Entry { get; set; }
    }

    public class Entry
    {
        public string Id { get; set; }
        public List<Change> Changes { get; set; }
    }

    public class Change
    {
        public Value Value { get; set; }
        public string Field { get; set; }
    }

    public class Value
    {
        public string Messaging_Product { get; set; }
        public Metadata Metadata { get; set; }
        public List<WhatsAppContact> Contacts { get; set; }

        public List<Message> Messages { get; set; }
    }

    public class Metadata
    {
        public string Display_Phone_Number { get; set; }
        public string Phone_Number_Id { get; set; }
    }

    public class WhatsAppContact
    {
        public Profile Profile { get; set; }
        public string Wa_Id { get; set; }
    }


    public class Profile
    {
        public string Name { get; set; }
    }

    public class Message
    {
        public string From { get; set; }
        public string Id { get; set; }
        public string Timestamp { get; set; }
        public string Type { get; set; }

        public MessageText Text { get; set; }


        public MediaContent Image { get; set; }
        public MediaContent Video { get; set; }
        public MediaContent Audio { get; set; }
        public MediaContent Document { get; set; }
    }

    public class MediaContent
    {
        public string Id { get; set; }
        public string MimeType { get; set; }
        public string Sha256 { get; set; }
        public string Filename { get; set; }
    }


    public class MessageText
    {
        public string Body { get; set; }
    }
}
