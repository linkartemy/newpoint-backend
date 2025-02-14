using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewPoint.ErrorLocalizationAPI.Clients;

namespace NewPoint.ErrorLocalizationAPI.Services
{
    public class LocalizationLoader
    {
        private readonly IRedisClient _redisClient;
        private readonly ILogger<LocalizationLoader> _logger;
        private readonly string _localizationPath = "localizations";

        public LocalizationLoader(IRedisClient redisClient, ILogger<LocalizationLoader> logger)
        {
            _redisClient = redisClient;
            _logger = logger;
        }

        /// <summary>
        /// Loads all localization JSON files and stores them in Redis.
        /// </summary>
        public async Task LoadAndStoreLocalizations()
        {
            try
            {
                if (!Directory.Exists(_localizationPath))
                {
                    _logger.LogWarning($"Localization directory {_localizationPath} not found.");
                    return;
                }

                var files = Directory.GetFiles(_localizationPath, "errors.*.json", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string language = Path.GetFileNameWithoutExtension(file).Split('.')[1];
                    _logger.LogInformation($"Processing localization file: {file} (Language: {language})");

                    var jsonString = await File.ReadAllTextAsync(file);
                    var localizations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                    if (localizations != null)
                    {
                        foreach (var entry in localizations)
                        {
                            await _redisClient.SaveErrorLocalization(entry.Key, language, entry.Value);
                        }
                        _logger.LogInformation($"Loaded {localizations.Count} localizations for '{language}' into Redis.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading localizations: {ex.Message}");
            }
        }
    }
}
