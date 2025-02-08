using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NewPoint.ErrorLocalizationAPI.Clients
{
    public class RedisClient : IRedisClient
    {
        private readonly IDatabase _database;
        private readonly ConnectionMultiplexer _redis;

        public RedisClient(string redisConnection = "redis:6379")
        {
            _redis = ConnectionMultiplexer.Connect(redisConnection);
            _database = _redis.GetDatabase();
        }

        /// <summary>
        /// Checks if an error localization exists in Redis.
        /// </summary>
        public async Task<bool> ErrorLocalizationExists(string errorCode, string language)
        {
            try
            {
                return await _database.KeyExistsAsync(GetErrorLocalizationKey(errorCode, language));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisClient] Error checking localization existence: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Retrieves the localized error message from Redis by error code and language.
        /// </summary>
        public async Task<string> GetErrorLocalization(string errorCode, string language)
        {
            try
            {
                return await _database.StringGetAsync(GetErrorLocalizationKey(errorCode, language));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisClient] Error retrieving localization: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Stores an error localization in Redis with an optional expiration time.
        /// </summary>
        public async Task SaveErrorLocalization(string errorCode, string language, string localizedMessage, TimeSpan? expiry = null)
        {
            try
            {
                expiry ??= TimeSpan.FromDays(7);
                await _database.StringSetAsync(GetErrorLocalizationKey(errorCode, language), localizedMessage, expiry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisClient] Error saving localization: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a localized error message from Redis.
        /// </summary>
        public async Task DeleteErrorLocalization(string errorCode, string language)
        {
            try
            {
                await _database.KeyDeleteAsync(GetErrorLocalizationKey(errorCode, language));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisClient] Error deleting localization: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all localizations for a specific error code (for all languages).
        /// </summary>
        public async Task<Dictionary<string, string>> GetAllLocalizationsForError(string errorCode)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                var keys = server.Keys(pattern: $"error:{errorCode}:*");

                var localizations = new Dictionary<string, string>();

                foreach (var key in keys)
                {
                    string value = await _database.StringGetAsync(key);
                    string language = key.ToString().Split(':')[2]; // Extracting language part
                    localizations[language] = value;
                }

                return localizations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisClient] Error retrieving all localizations: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates Redis key for an error localization.
        /// </summary>
        private static string GetErrorLocalizationKey(string errorCode, string language) => $"error:{errorCode}:{language}";
    }

    public interface IRedisClient
    {
        Task<bool> ErrorLocalizationExists(string errorCode, string language);
        Task<string> GetErrorLocalization(string errorCode, string language);
        Task SaveErrorLocalization(string errorCode, string language, string localizedMessage, TimeSpan? expiry = null);
        Task DeleteErrorLocalization(string errorCode, string language);
        Task<Dictionary<string, string>> GetAllLocalizationsForError(string errorCode);
    }
}
