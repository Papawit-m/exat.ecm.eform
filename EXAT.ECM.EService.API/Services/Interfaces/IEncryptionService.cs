namespace EXAT.ECM.EService.API.Services.Interfaces
{
    /// <summary>
    /// Service for encrypting and decrypting data using AES algorithm
    /// Used for EXAT Production API communication
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypt plain text using AES-256-CBC
        /// </summary>
        /// <param name="plainText">Plain text to encrypt</param>
        /// <returns>Base64 encoded encrypted string</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypt encrypted text using AES-256-CBC
        /// </summary>
        /// <param name="encryptedText">Base64 encoded encrypted string</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string encryptedText);
    }
}
