using EXAT.ECM.EService.API.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    /// <summary>
    /// AES encryption service for EXAT API communication
    /// Uses AES-256-CBC with PKCS7 padding
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly ILogger<AesEncryptionService> _logger;

        public AesEncryptionService(string key, string iv, ILogger<AesEncryptionService> logger)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "AES key cannot be null or empty");

            if (string.IsNullOrEmpty(iv))
                throw new ArgumentNullException(nameof(iv), "AES IV cannot be null or empty");

            // Convert string key/iv to bytes
            _key = Encoding.UTF8.GetBytes(key);
            _iv = Encoding.UTF8.GetBytes(iv);
            _logger = logger;

            // Validate key and IV lengths
            if (_key.Length != 32)
                throw new ArgumentException($"AES key must be 32 bytes (256 bits). Provided: {_key.Length} bytes", nameof(key));

            if (_iv.Length != 16)
                throw new ArgumentException($"AES IV must be 16 bytes (128 bits). Provided: {_iv.Length} bytes", nameof(iv));

            _logger.LogInformation("AES Encryption Service initialized with key length: {KeyLength} bits, IV length: {IvLength} bits",
                _key.Length * 8, _iv.Length * 8);
        }

        /// <summary>
        /// Encrypt plain text using AES-256-CBC
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                _logger.LogWarning("Attempted to encrypt null or empty string");
                return plainText;
            }

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        var encrypted = msEncrypt.ToArray();
                        var base64 = Convert.ToBase64String(encrypted);

                        _logger.LogDebug("Encrypted text length: {Length} bytes, Base64 length: {Base64Length} chars",
                            encrypted.Length, base64.Length);

                        return base64;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw new CryptographicException("Failed to encrypt data", ex);
            }
        }

        /// <summary>
        /// Decrypt encrypted text using AES-256-CBC
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
            {
                _logger.LogWarning("Attempted to decrypt null or empty string");
                return encryptedText;
            }

            try
            {
                _logger.LogInformation("🔓 Starting decryption...");
                _logger.LogDebug("Encrypted text length: {Length} chars", encryptedText.Length);
                _logger.LogDebug("Encrypted text (first 100 chars): {Text}",
                    encryptedText.Length > 100 ? encryptedText.Substring(0, 100) + "..." : encryptedText);

                var buffer = Convert.FromBase64String(encryptedText);
                _logger.LogDebug("Decoded Base64 to {Length} bytes", buffer.Length);

                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    _logger.LogDebug("AES configured: Mode={Mode}, Padding={Padding}, KeySize={KeySize}, BlockSize={BlockSize}",
                        aes.Mode, aes.Padding, aes.KeySize, aes.BlockSize);

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(buffer))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        var decrypted = srDecrypt.ReadToEnd();

                        _logger.LogInformation("✅ Decryption successful! Decrypted text length: {Length} chars", decrypted.Length);
                        _logger.LogDebug("Decrypted text (first 200 chars): {Text}",
                            decrypted.Length > 200 ? decrypted.Substring(0, 200) + "..." : decrypted);

                        return decrypted;
                    }
                }
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "❌ Invalid Base64 string format. Length: {Length}, Text: {Text}",
                    encryptedText.Length,
                    encryptedText.Length > 50 ? encryptedText.Substring(0, 50) + "..." : encryptedText);
                throw new CryptographicException("Invalid encrypted data format", ex);
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "❌ Cryptographic error during decryption. This usually means wrong key/IV or corrupted data.");
                _logger.LogError("Key length: {KeyLength} bytes, IV length: {IvLength} bytes", _key.Length, _iv.Length);
                throw new CryptographicException("Failed to decrypt data. Invalid key/IV or corrupted data.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error during decryption: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                throw new CryptographicException("Failed to decrypt data", ex);
            }
        }
    }
}
