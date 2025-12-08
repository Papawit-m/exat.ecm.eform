using EXAT.ECM.EService.API.Services.Interfaces;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    /// <summary>
    /// No-operation encryption service for UAT environment
    /// Passes through data without encryption/decryption
    /// </summary>
    public class NoOpEncryptionService : IEncryptionService
    {
        private readonly ILogger<NoOpEncryptionService> _logger;

        public NoOpEncryptionService(ILogger<NoOpEncryptionService> logger)
        {
            _logger = logger;
            _logger.LogInformation("NoOp Encryption Service initialized (encryption disabled)");
        }

        public string Encrypt(string plainText)
        {
            _logger.LogDebug("NoOp Encrypt: Passing through without encryption");
            return plainText;
        }

        public string Decrypt(string encryptedText)
        {
            _logger.LogDebug("NoOp Decrypt: Passing through without decryption");
            return encryptedText;
        }
    }
}
