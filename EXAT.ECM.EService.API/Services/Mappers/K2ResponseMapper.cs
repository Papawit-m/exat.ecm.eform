using EXAT.ECM.EService.API.Model.Responses;
using System.Net;

namespace EXAT.ECM.EService.API.Services.Mappers
{
    public class K2ResponseMapper
    {
        /// <summary>
        /// Map AccessTokenResponse to K2 format
        /// </summary>
        public static K2ApiResponse<K2AccessTokenResponse> MapToK2AccessToken(
            ApiResponse<AccessTokenResponse> response)
        {
            if (!response.Success || response.Data == null)
            {
                return new K2ApiResponse<K2AccessTokenResponse>
                {
                    Success = false,
                    Message = response.Message ?? "Failed to get access token",
                    ErrorCode = response.ErrorCode ?? "ERROR",
                    ErrorDetail = response.ErrorCode ?? "Failed to get access token"
                };
            }

            var expiresAt = DateTime.Now
                .AddSeconds(ParseExpiredIn(response.Data.ExpiredIn ?? "3600"))
                .ToString("yyyy-MM-dd HH:mm:ss");

            // Format ExpiredIn to yyyy-MM-dd HH:mm:ss if it's a DateTime string
            var formattedExpiredIn = FormatExpiredIn(response.Data.ExpiredIn ?? "3600");

            // Calculate expiresDuration (how long the token is valid for)
            var expiresDuration = CalculateExpiresDuration(response.Data.ExpiredIn ?? "3600");

            return new K2ApiResponse<K2AccessTokenResponse>
            {
                Success = true,
                Message = response.Message ?? "Access token retrieved successfully",
                Data = new K2AccessTokenResponse
                {
                    AccessToken = response.Data.AccessToken ?? string.Empty,
                    ExpiredIn = formattedExpiredIn,
                    ExpiresAt = expiresAt,
                    ExpiresDuration = expiresDuration
                }
            };
        }

        /// <summary>
        /// Map LoginResponse to K2 format
        /// </summary>
        public static K2ApiResponse<K2LoginResponseData> MapToK2Login(
            LoginResponse response)
        {
            if (!response.Success || response.LoginData == null)
            {
                return new K2ApiResponse<K2LoginResponseData>
                {
                    Success = false,
                    Message = response.Message ?? "Login failed",
                    ErrorCode = "LOGIN_FAILED",
                    ErrorDetail = response.Message ?? "Login failed"
                };
            }

            // Use actual ExpiredIn from token response, or default to 3600 if not provided
            var expiredIn = response.ExpiredIn ?? "3600";
            var expiresAt = DateTime.Now
                .AddSeconds(ParseExpiredIn(expiredIn))
                .ToString("yyyy-MM-dd HH:mm:ss");
            var expiresDuration = CalculateExpiresDuration(expiredIn);

            return new K2ApiResponse<K2LoginResponseData>
            {
                Success = true,
                Message = response.Message ?? "Login successful",
                Data = new K2LoginResponseData
                {
                    AccessToken = response.AccessToken ?? string.Empty,
                    ExpiresAt = expiresAt,
                    ExpiresDuration = expiresDuration,
                    LoginData = MapToK2LoginData(response.LoginData),
                    MemberByEmail = response.MemberByEmail != null ? MapToK2MemberData(response.MemberByEmail) : null,
                    MemberByCustomerId = response.MemberByCustomerId != null ? MapToK2MemberData(response.MemberByCustomerId) : null
                }
            };
        }

        /// <summary>
        /// Map LoginData to K2 format
        /// </summary>
        public static K2LoginData MapToK2LoginData(LoginData? loginData)
        {
            if (loginData == null)
            {
                return new K2LoginData();
            }

            return new K2LoginData
            {
                MemberId = loginData.MemberId ?? string.Empty,
                CustomerId = loginData.CustomerId ?? string.Empty,
                Email = loginData.Email ?? string.Empty,
                UserType = loginData.UserType ?? string.Empty,
                AccountType = loginData.AccountType ?? string.Empty,
                Title = loginData.Title ?? string.Empty,
                FirstName = loginData.FirstName ?? string.Empty,
                LastName = loginData.LastName ?? string.Empty,
                FullName = $"{loginData.FirstName} {loginData.LastName}".Trim(),
                CreatedAt = loginData.CreatedAt ?? string.Empty,
                CreatedBy = loginData.CreatedBy ?? string.Empty,
                Active = loginData.Active ?? string.Empty,
                IsConsentLatest = loginData.IsConsentLatest
            };
        }

        /// <summary>
        /// Map Member to K2 format
        /// </summary>
        public static K2ApiResponse<K2MemberResponseData> MapToK2Member(
            ApiResponse<Member> response)
        {
            if (!response.Success || response.Data == null)
            {
                return new K2ApiResponse<K2MemberResponseData>
                {
                    Success = false,
                    Message = response.Message ?? "Failed to get member data",
                    ErrorCode = response.ErrorCode ?? "ERROR",
                    ErrorDetail = response.ErrorCode ?? "Failed to get member data"
                };
            }

            return new K2ApiResponse<K2MemberResponseData>
            {
                Success = true,
                Message = response.Message ?? "Member data retrieved successfully",
                Data = MapToK2MemberData(response.Data)
            };
        }

        /// <summary>
        /// Map Member to K2MemberResponseData
        /// </summary>
        public static K2MemberResponseData MapToK2MemberData(Member member)
        {
            // Build address list from contact and tax addresses
            var addresses = new List<K2Address>();
            if (member.ContactAddress != null)
            {
                var contactAddr = MapToK2Address(member.ContactAddress, "Contact");
                if (contactAddr != null) addresses.Add(contactAddr);
            }
            if (member.TaxAddress != null)
            {
                var taxAddr = MapToK2Address(member.TaxAddress, "Tax");
                if (taxAddr != null) addresses.Add(taxAddr);
            }

            return new K2MemberResponseData
            {
                MemberId = member.MemberId ?? string.Empty,
                CustomerId = member.CustomerId ?? string.Empty,
                Email = member.Email ?? string.Empty,
                UserType = member.UserType ?? string.Empty,
                AccountType = member.AccountType ?? string.Empty,
                Title = member.Title ?? string.Empty,
                FirstName = member.FirstName ?? string.Empty,
                LastName = member.LastName ?? string.Empty,
                FullName = $"{member.FirstName} {member.LastName}".Trim(),
                PhoneNo = member.PhoneNo ?? string.Empty,
                DateOfBirth = member.DateOfBirth ?? string.Empty,
                Active = member.Active ?? string.Empty,
                IsConsentLatest = member.IsConsentLatest,
                ContactAddress = MapToK2Address(member.ContactAddress, "Contact"),
                TaxAddress = MapToK2Address(member.TaxAddress, "Tax"),
                Addresses = addresses,
                CreatedAt = string.Empty, // Not available in EXAT Member model
                CreatedBy = string.Empty, // Not available in EXAT Member model
                UpdatedAt = string.Empty, // Not available in EXAT Member model
                UpdatedBy = string.Empty  // Not available in EXAT Member model
            };
        }

        /// <summary>
        /// Map Address to K2 format
        /// </summary>
        private static K2Address? MapToK2Address(Address? address, string addressType)
        {
            if (address == null)
            {
                return null;
            }

            var fullAddress = BuildFullAddress(address);

            return new K2Address
            {
                HouseNo = address.HouseNo ?? string.Empty,
                VillageNo = address.VillageNo ?? string.Empty,
                VillageName = address.VillageName ?? string.Empty,
                Road = address.Road ?? string.Empty,
                Lane = address.Lane ?? string.Empty,
                SubDistrict = address.SubDistrict ?? string.Empty,
                District = address.District ?? string.Empty,
                Province = address.Province ?? string.Empty,
                PostalCode = address.PostalCode ?? string.Empty,
                Country = "Thailand", // Default country (not in EXAT Address model)
                AddressType = addressType,
                FullAddress = fullAddress
            };
        }

        /// <summary>
        /// Build full address string from address parts (Thai format)
        /// </summary>
        private static string BuildFullAddress(Address address)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(address.HouseNo))
                parts.Add($"เลขที่ {address.HouseNo}");

            if (!string.IsNullOrWhiteSpace(address.VillageNo))
                parts.Add($"หมู่ {address.VillageNo}");

            if (!string.IsNullOrWhiteSpace(address.VillageName))
                parts.Add(address.VillageName);

            if (!string.IsNullOrWhiteSpace(address.Lane))
                parts.Add($"ซอย {address.Lane}");

            if (!string.IsNullOrWhiteSpace(address.Road))
                parts.Add($"ถนน {address.Road}");

            if (!string.IsNullOrWhiteSpace(address.SubDistrict))
                parts.Add($"ตำบล/แขวง {address.SubDistrict}");

            if (!string.IsNullOrWhiteSpace(address.District))
                parts.Add($"อำเภอ/เขต {address.District}");

            if (!string.IsNullOrWhiteSpace(address.Province))
                parts.Add($"จังหวัด {address.Province}");

            if (!string.IsNullOrWhiteSpace(address.PostalCode))
                parts.Add(address.PostalCode);

            return string.Join(" ", parts);
        }

        /// <summary>
        /// Parse ExpiredIn string to seconds
        /// Handles both numeric seconds and ISO DateTime string formats
        /// </summary>
        private static double ParseExpiredIn(string expiredIn)
        {
            // Try parsing as numeric seconds first (backward compatibility)
            if (double.TryParse(expiredIn, out double seconds))
            {
                return seconds;
            }

            // Try parsing as ISO DateTime string (EXAT format: "2025-11-05T00:42:12+07:00")
            if (DateTime.TryParse(expiredIn, out DateTime expireTime))
            {
                var secondsUntilExpire = (expireTime - DateTime.Now).TotalSeconds;
                // Return seconds until expiry, or default to 1 hour if already expired
                return secondsUntilExpire > 0 ? secondsUntilExpire : 3600;
            }

            // Default fallback: 1 hour
            return 3600;
        }

        /// <summary>
        /// Format ExpiredIn to yyyy-MM-dd HH:mm:ss
        /// Handles both numeric seconds and ISO DateTime string formats
        /// </summary>
        private static string FormatExpiredIn(string expiredIn)
        {
            // Try parsing as ISO DateTime string first
            if (DateTime.TryParse(expiredIn, out DateTime expireTime))
            {
                return expireTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Try parsing as numeric seconds
            if (double.TryParse(expiredIn, out double seconds))
            {
                return DateTime.Now.AddSeconds(seconds).ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Default: return original value
            return expiredIn;
        }

        /// <summary>
        /// Calculate token duration in human-readable format
        /// Handles both numeric seconds and ISO DateTime string formats
        /// Returns format: "X hours Y minutes" or "Y minutes Z seconds"
        /// </summary>
        private static string CalculateExpiresDuration(string expiredIn)
        {
            double totalSeconds = 0;

            // Try parsing as numeric seconds first
            if (double.TryParse(expiredIn, out double seconds))
            {
                totalSeconds = seconds;
            }
            // Try parsing as ISO DateTime string
            else if (DateTime.TryParse(expiredIn, out DateTime expireTime))
            {
                totalSeconds = (expireTime - DateTime.Now).TotalSeconds;
                if (totalSeconds < 0) totalSeconds = 3600; // Default if expired
            }
            else
            {
                totalSeconds = 3600; // Default 1 hour
            }

            // Convert to human-readable format
            var timeSpan = TimeSpan.FromSeconds(totalSeconds);

            if (timeSpan.TotalHours >= 1)
            {
                var hours = (int)timeSpan.TotalHours;
                var minutes = timeSpan.Minutes;
                return minutes > 0
                    ? $"{hours} hours {minutes} minutes"
                    : $"{hours} hours";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                var minutes = (int)timeSpan.TotalMinutes;
                var secs = timeSpan.Seconds;
                return secs > 0
                    ? $"{minutes} minutes {secs} seconds"
                    : $"{minutes} minutes";
            }
            else
            {
                return $"{(int)timeSpan.TotalSeconds} seconds";
            }
        }

        /// <summary>
        /// Create K2 error response
        /// </summary>
        public static K2ErrorResponse CreateErrorResponse(
            string message,
            string errorCode = "ERROR",
            string? errorDetail = null)
        {
            return new K2ErrorResponse
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode,
                ErrorDetail = errorDetail ?? message,
                Timestamp = DateTime.UtcNow.ToString("o")
            };
        }
    }
}
