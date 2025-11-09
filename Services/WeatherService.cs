using System.Text.Json;
using Aura.Models;

namespace Aura.Services
{
    public interface IWeatherService
    {
        Task<CurrentWeather> GetCurrentWeatherAsync(string city);
        Task<ForecastData> GetForecastAsync(string city);
        Task<CurrentWeather> GetWeatherByLocationAsync(double lat, double lon);
        Task<ForecastData> GetForecastByLocationAsync(double lat, double lon);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private const string API_KEY = "d9c07ba7f386cd4b7802eb4b9f33b524";
        private const string BASE_URL = "https://api.openweathermap.org/data/2.5";

        public WeatherService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<CurrentWeather> GetCurrentWeatherAsync(string city)
        {
            try
            {
                var url = $"{BASE_URL}/weather?q={Uri.EscapeDataString(city)}&appid={API_KEY}&units=metric&lang=ru";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var weather = JsonSerializer.Deserialize<CurrentWeather>(json);
                    return weather;
                }

                throw new Exception($"Ошибка API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения погоды: {ex.Message}");
            }
        }

        public async Task<ForecastData> GetForecastAsync(string city)
        {
            try
            {
                var url = $"{BASE_URL}/forecast?q={Uri.EscapeDataString(city)}&appid={API_KEY}&units=metric&lang=ru";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var forecast = JsonSerializer.Deserialize<ForecastData>(json);

                    // Фильтруем для получения одного прогноза на день
                    if (forecast?.List != null)
                    {
                        forecast.List = forecast.List
                            .Where(x => x.Date.Hour == 12) // Берем прогноз на полдень каждого дня
                            .Take(5) // Берем 5 дней
                            .ToList();
                    }

                    return forecast ?? new ForecastData { List = new List<ForecastItem>() };
                }

                throw new Exception($"Ошибка API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения прогноза: {ex.Message}");
            }
        }

        public async Task<CurrentWeather> GetWeatherByLocationAsync(double lat, double lon)
        {
            try
            {
                var url = $"{BASE_URL}/weather?lat={lat}&lon={lon}&appid={API_KEY}&units=metric&lang=ru";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var weather = JsonSerializer.Deserialize<CurrentWeather>(json);
                    return weather;
                }

                throw new Exception($"Ошибка API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения погоды по локации: {ex.Message}");
            }
        }

        public async Task<ForecastData> GetForecastByLocationAsync(double lat, double lon)
        {
            try
            {
                var url = $"{BASE_URL}/forecast?lat={lat}&lon={lon}&appid={API_KEY}&units=metric&lang=ru";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var forecast = JsonSerializer.Deserialize<ForecastData>(json);

                    if (forecast?.List != null)
                    {
                        forecast.List = forecast.List
                            .Where(x => x.Date.Hour == 12)
                            .Take(5)
                            .ToList();
                    }

                    return forecast;
                }

                throw new Exception($"Ошибка API: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения прогноза по локации: {ex.Message}");
            }
        }
    }
}