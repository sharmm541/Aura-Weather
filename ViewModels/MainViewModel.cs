using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Aura.Models;
using Aura.Services;
using Microsoft.Maui.ApplicationModel;

namespace Aura.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IWeatherService _weatherService;
        private readonly ILocationService _locationService;
        private readonly ILocalStorageService _localStorage;
        private CurrentWeather _currentWeather;
        private bool _isBusy;
        private string _statusMessage;
        private string _city = "Москва";
        private DateTime _lastUpdateTime;

        public CurrentWeather CurrentWeather
        {
            get => _currentWeather;
            set
            {
                _currentWeather = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ForecastItem> HourlyForecast { get; } = new();

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (_city != value)
                {
                    _city = value;
                    OnPropertyChanged();
                    // Сохраняем выбранный город
                    _ = _localStorage.SaveWeatherDataAsync("last_city", value);
                    // Автоматически обновляем погоду при изменении города
                    if (!string.IsNullOrEmpty(value) && value.Length > 2)
                    {
                        _ = LoadWeatherAsync();
                    }
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand UseLocationCommand { get; }

        public MainViewModel(IWeatherService weatherService, ILocationService locationService, ILocalStorageService localStorage)
        {
            _weatherService = weatherService;
            _locationService = locationService;
            _localStorage = localStorage;

            RefreshCommand = new Command(async () => await LoadWeatherAsync(true));
            UseLocationCommand = new Command(async () => await UseLocationAsync());

            // Загружаем последний город и погоду при запуске
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Загружаем последний сохраненный город
                var lastCity = await _localStorage.GetWeatherDataAsync<string>("last_city");
                if (!string.IsNullOrEmpty(lastCity))
                {
                    City = lastCity;
                }
                else
                {
                    // Загружаем погоду для города по умолчанию
                    await LoadWeatherAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации: {ex.Message}");
                // Загружаем погоду для города по умолчанию при ошибке
                await LoadWeatherAsync();
            }
        }

        private async Task LoadWeatherAsync(bool forceRefresh = false)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // Проверяем кэш (5 минут)
                if (!forceRefresh && await ShouldUseCachedWeatherAsync())
                {
                    var cachedWeather = await _localStorage.GetWeatherDataAsync<CurrentWeather>($"weather_{City}");
                    var cachedForecast = await _localStorage.GetWeatherDataAsync<ForecastData>($"forecast_{City}");

                    if (cachedWeather != null)
                    {
                        CurrentWeather = cachedWeather;
                        UpdateHourlyForecast(cachedForecast);
                        StatusMessage = "Данные из кэша";
                        return;
                    }
                }

                StatusMessage = "Загрузка погоды...";

                // Загружаем текущую погоду и прогноз
                var weather = await _weatherService.GetCurrentWeatherAsync(City);
                var forecast = await _weatherService.GetForecastAsync(City);

                if (weather != null && forecast != null)
                {
                    CurrentWeather = weather;
                    UpdateHourlyForecast(forecast);
                    _lastUpdateTime = DateTime.Now;

                    // Сохраняем в кэш
                    await _localStorage.SaveWeatherDataAsync($"weather_{City}", weather);
                    await _localStorage.SaveWeatherDataAsync($"forecast_{City}", forecast);
                    await _localStorage.SaveWeatherDataAsync("last_update", _lastUpdateTime);

                    StatusMessage = "Данные обновлены";
                }
            }
            catch (Exception ex)
            {
                // Пробуем загрузить из кэша при ошибке
                var cachedWeather = await _localStorage.GetWeatherDataAsync<CurrentWeather>($"weather_{City}");
                var cachedForecast = await _localStorage.GetWeatherDataAsync<ForecastData>($"forecast_{City}");

                if (cachedWeather != null)
                {
                    CurrentWeather = cachedWeather;
                    UpdateHourlyForecast(cachedForecast);
                    StatusMessage = "Данные из кэша (ошибка сети)";
                }
                else
                {
                    StatusMessage = $"Ошибка: {ex.Message}";
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateHourlyForecast(ForecastData forecastData)
        {
            HourlyForecast.Clear();

            if (forecastData?.List != null)
            {
                var now = DateTime.Now;

                // Почасовой прогноз на ближайшие 24 часа (берем каждый 3-й интервал)
                var hourlyItems = forecastData.List
                    .Where(x => x.Date >= now && x.Date <= now.AddHours(24))
                    .Where((x, index) => index % 3 == 0) // Берем каждый 3-й элемент (примерно каждые 6 часов)
                    .Take(8) // Берем до 8 интервалов
                    .ToList();

                // Если данных мало, берем все доступные
                if (hourlyItems.Count < 4)
                {
                    hourlyItems = forecastData.List
                        .Where(x => x.Date >= now && x.Date <= now.AddHours(24))
                        .Take(8)
                        .ToList();
                }

                foreach (var item in hourlyItems)
                {
                    HourlyForecast.Add(item);
                }
            }
        }

        private async Task<bool> ShouldUseCachedWeatherAsync()
        {
            try
            {
                var lastUpdate = await _localStorage.GetWeatherDataAsync<DateTime>("last_update");
                return lastUpdate != default && (DateTime.Now - lastUpdate).TotalMinutes < 5;
            }
            catch
            {
                return false;
            }
        }

        private async Task UseLocationAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Получение локации...";

                var location = await _locationService.GetCurrentLocationAsync();

                if (location != null)
                {
                    StatusMessage = "Получение погоды по локации...";

                    // Загружаем текущую погоду и прогноз
                    var weather = await _weatherService.GetWeatherByLocationAsync(location.Latitude, location.Longitude);
                    var forecast = await _weatherService.GetForecastByLocationAsync(location.Latitude, location.Longitude);

                    if (weather != null && forecast != null)
                    {
                        // ФИКС БАГА: Сохраняем город до обновления данных
                        var newCity = weather.City;
                        CurrentWeather = weather;
                        UpdateHourlyForecast(forecast);
                        City = newCity; // Это вызовет автоматическое сохранение города

                        // Сохраняем в кэш
                        await _localStorage.SaveWeatherDataAsync($"weather_{newCity}", weather);
                        await _localStorage.SaveWeatherDataAsync($"forecast_{newCity}", forecast);
                        _lastUpdateTime = DateTime.Now;
                        await _localStorage.SaveWeatherDataAsync("last_update", _lastUpdateTime);

                        StatusMessage = "Данные обновлены по геолокации";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка геолокации: {ex.Message}";

                // Показываем alert только если приложение запущено
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка локации",
                        ex.Message + "\n\nПроверьте:\n• Разрешение на доступ к локации\n• Включена ли геолокация на устройстве", "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}