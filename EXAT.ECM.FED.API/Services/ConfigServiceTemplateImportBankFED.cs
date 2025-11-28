using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using EXAT.ECM.FED.API.Models.IMPORT;
using EXAT.ECM.FED.API.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace EXAT.ECM.FED.API.Services
{
    /// <summary>
    /// ดึง config ของ Template การนำเข้า Fleet/Bank FED พร้อม Caching
    /// </summary>
    public class ConfigServiceTemplateImportBankFED : IConfigService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfigServiceTemplateImportBankFED> _logger;
        private readonly string _connectionString;

        // ปรับชื่อ cache key ให้เป็น constant
        private const string CacheKeyPrefix = "TemplateConfig_";

        public ConfigServiceTemplateImportBankFED(
            IConfiguration configuration,
            IMemoryCache cache,
            ILogger<ConfigServiceTemplateImportBankFED> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // รองรับทั้ง ConnectionStrings และ Environment Variable สำรอง
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
        }

        /// <summary>
        /// ดึงข้อมูลการตั้งค่าของ Template ตามชื่อที่ระบุ (มี Cache)
        /// </summary>
        public async Task<IReadOnlyDictionary<string, TemplateFieldConfig>> GetTemplateConfigAsync(
            string templateName)
            => await GetTemplateConfigAsync(templateName, CancellationToken.None);

        // overload รองรับ cancellation (optional)
        public async Task<IReadOnlyDictionary<string, TemplateFieldConfig>> GetTemplateConfigAsync(
            string templateName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                return new ReadOnlyDictionary<string, TemplateFieldConfig>(new Dictionary<string, TemplateFieldConfig>());

            // ทำให้ cache key คงที่ (trim + upper)
            var normalizedName = templateName.Trim();
            var cacheKey = $"{CacheKeyPrefix}{normalizedName}";

            // ป้องกันการ query ซ้ำด้วย GetOrCreateAsync
            var result = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromHours(1));
                entry.SetAbsoluteExpiration(TimeSpan.FromHours(6));

                try
                {
                    var dict = await QueryTemplateConfigAsync(normalizedName, cancellationToken);
                    // เก็บ ReadOnly เพื่อป้องกันการแก้ไขภายหลัง
                    return (IReadOnlyDictionary<string, TemplateFieldConfig>)
                        new ReadOnlyDictionary<string, TemplateFieldConfig>(dict);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load template config for {TemplateName}", normalizedName);
                    // กัน cache ก้อน error: ปล่อยให้ throw จะไม่ cache ค่า error
                    throw;
                }
            });

            return result ?? new ReadOnlyDictionary<string, TemplateFieldConfig>(new Dictionary<string, TemplateFieldConfig>());
        }

        /// <summary>
        /// ล้าง cache ของ template ที่ระบุ
        /// </summary>
        public Task ClearCacheForTemplateAsync(string templateName)
        {
            if (!string.IsNullOrWhiteSpace(templateName))
            {
                var cacheKey = $"{CacheKeyPrefix}{templateName.Trim()}";
                _cache.Remove(cacheKey);
                _logger.LogDebug("Cleared cache for template {TemplateName}", templateName);
            }
            return Task.CompletedTask;
        }

        // ===================== PRIVATE =====================

        private async Task<Dictionary<string, TemplateFieldConfig>> QueryTemplateConfigAsync(
            string templateName,
            CancellationToken cancellationToken)
        {
            var sql = @"
SELECT FIELD_NAME,
       SOURCE_COLUMN_NAME,
       SOURCE_COLUMN_INDEX,
       IS_REQUIRED
FROM   EFM_FED.FLEET_CARD_IMPORT_CONFIGS
WHERE  TEMPLATE_NAME = :p_TemplateName";

            var result = new Dictionary<string, TemplateFieldConfig>(StringComparer.OrdinalIgnoreCase);

            // >>> เปลี่ยนมาใช้ OracleConnection ตรง ๆ ตามที่ร้องขอ <<<
            using (var connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (var command = new OracleCommand(sql, connection))
                {
                    command.BindByName = true;
                    // เผื่อ template name ยาว กำหนด size สัก 200 (ปรับตาม schema จริง)
                    var p = new OracleParameter("p_TemplateName", OracleDbType.Varchar2, 200, templateName, ParameterDirection.Input);
                    command.Parameters.Add(p);

                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken).ConfigureAwait(false))
                    {
                        var ordFieldName = reader.GetOrdinal("FIELD_NAME");
                        var ordSourceColName = reader.GetOrdinal("SOURCE_COLUMN_NAME");
                        var ordSourceColIndex = reader.GetOrdinal("SOURCE_COLUMN_INDEX");
                        var ordIsRequired = reader.GetOrdinal("IS_REQUIRED");

                        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            if (reader.IsDBNull(ordFieldName)) continue;

                            var fieldName = reader.GetString(ordFieldName);

                            string? sourceColName = reader.IsDBNull(ordSourceColName)
                                ? null
                                : reader.GetString(ordSourceColName);

                            int? sourceColIndex = reader.IsDBNull(ordSourceColIndex)
                                ? (int?)null
                                : reader.GetInt32(ordSourceColIndex);

                            // IS_REQUIRED = 1 => true (กันกรณี type เป็น number/decimal)
                            bool isRequired;
                            if (!reader.IsDBNull(ordIsRequired))
                            {
                                try
                                {
                                    // บาง schema เก็บเป็น NUMBER(1) / INT
                                    isRequired = Convert.ToInt32(reader.GetValue(ordIsRequired)) == 1;
                                }
                                catch
                                {
                                    isRequired = false;
                                }
                            }
                            else
                            {
                                isRequired = false;
                            }

                            var cfg = new TemplateFieldConfig
                            {
                                FieldName = fieldName,
                                SourceColumnName = sourceColName,
                                SourceColumnIndex = sourceColIndex,
                                IsRequired = isRequired
                            };

                            // ใช้ key = FIELD_NAME (case-insensitive ตาม Dictionary)
                            result[fieldName] = cfg;
                        }
                    }
                }
            }

            return result;
        }
    }
}
