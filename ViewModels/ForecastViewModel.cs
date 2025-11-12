using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Aura.Models;
using Aura.Services;

namespace Aura.ViewModels
{
    public class ForecastViewModel : INotifyPropertyChanged
    {
        private readonly IWeatherService _weatherService;
        private readonly ILocalStorageService _localStorage;
        private bool _isBusy;
        private string _city;

        public ObservableCollection<ForecastItem> Forecast { get; } = new();
        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public ForecastViewModel(IWeatherService weatherService, ILocalStorageService localStorage)
        {
            _weatherService = weatherService;
            _localStorage = localStorage;
        }

        public async Task LoadForecastAsync(string city)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                City = city;

                // Пробуем загрузить из кэша
                var cachedForecast = await _localStorage.GetWeatherDataAsync<ForecastData>($"forecast_{city}");
                if (cachedForecast?.List != null && cachedForecast.List.Any())
                {
                    UpdateForecast(cachedForecast);
                }

                // Загружаем свежие данные
                var forecastData = await _weatherService.GetForecastAsync(city);

                if (forecastData?.List != null)
                {
                    UpdateForecast(forecastData);

                    // Сохраняем в кэш
                    await _localStorage.SaveWeatherDataAsync($"forecast_{city}", forecastData);
                    await _localStorage.SaveWeatherDataAsync($"forecast_last_update_{city}", DateTime.Now);
                }
                else if (cachedForecast == null)
                {
                    throw new Exception("Не удалось загрузить прогноз");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки прогноза: {ex.Message}");

                // Пробуем показать кэшированные данные
                var cachedForecast = await _localStorage.GetWeatherDataAsync<ForecastData>($"forecast_{city}");
                if (cachedForecast?.List != null && cachedForecast.List.Any())
                {
                    UpdateForecast(cachedForecast);

                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Информация",
                            "Показаны кэшированные данные (ошибка сети)", "OK");
                    }
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Ошибка",
                            "Не удалось загрузить прогноз погоды. Проверьте подключение к интернету.", "OK");
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateForecast(ForecastData forecastData)
        {
            Forecast.Clear();

            if (forecastData?.List != null)
            {
                foreach (var item in forecastData.List)
                {
                    Forecast.Add(item);
                }
            }
        }

        // Метод для проверки актуальности кэша
        public async Task<bool> IsForecastCacheValidAsync(string city)
        {
            try
            {
                var lastUpdate = await _localStorage.GetWeatherDataAsync<DateTime>($"forecast_last_update_{city}");
                return lastUpdate != default && (DateTime.Now - lastUpdate).TotalHours < 3; // 3 часа
            }
            catch
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}