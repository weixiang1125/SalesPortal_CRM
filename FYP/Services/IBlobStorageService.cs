using Azure.Storage.Blobs;

namespace CRM_API.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _containerName = "chatmedia";

        public BlobStorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AzureBlob");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(fileName);

            // ✅ Set the correct MIME type
            var headers = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, headers); // apply headers here

            return blobClient.Uri.ToString();
        }

    }
}
