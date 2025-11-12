using System.Text.Json;

namespace Aura.Services
{
    public interface ILocalStorageService
    {
        Task SaveWeatherDataAsync(string key, object data);
        Task<T> GetWeatherDataAsync<T>(string key);
        Task<bool> ContainsKeyAsync(string key);
        Task RemoveAsync(string key);
    }

    public class LocalStorageService : ILocalStorageService
    {
        public async Task SaveWeatherDataAsync(string key, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                await SecureStorage.SetAsync(key, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения данных: {ex.Message}");
            }
        }

        public async Task<T> GetWeatherDataAsync<T>(string key)
        {
            try
            {
                var json = await SecureStorage.GetAsync(key);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
            }

            return default(T);
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            var data = await SecureStorage.GetAsync(key);
            return !string.IsNullOrEmpty(data);
        }

        public Task RemoveAsync(string key)
        {
            SecureStorage.Remove(key);
            return Task.CompletedTask;
        }
    }
}