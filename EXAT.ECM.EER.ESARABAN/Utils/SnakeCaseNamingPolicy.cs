using System.Text.Json;

namespace EXAT.ECM.EER.ESARABAN.Utils
{
    /// <summary>
    /// JSON naming policy for snake_case conversion
    /// Converts PascalCase and camelCase to snake_case
    /// Example: "BookId" -> "book_id", "UserAd" -> "user_ad"
    /// </summary>
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // eSaraban API uses different naming conventions:
            // - Fields with underscores (user_ad, book_owner): Keep as-is (already snake_case)
            // - CamelCase fields (bookFile, bookAttach): Convert to LOWERCASE ONLY (no underscore)
            // This matches the Postman Collection format

            // If already has underscores, assume it's snake_case - keep as-is
            if (name.Contains('_'))
                return name.ToLower();

            // For camelCase/PascalCase without underscores: convert to lowercase only
            return name.ToLower();
        }
    }
}
