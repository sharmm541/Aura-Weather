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
        private CurrentWeather _currentWeather;
        private bool _isBusy;
        private string _statusMessage;
        private string _city = "Москва";

        public CurrentWeather CurrentWeather
        {
            get => _currentWeather;
            set
            {
                _currentWeather = value;
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
                _city = value;
                OnPropertyChanged();
                // Автоматически обновляем погоду при изменении города
                if (!string.IsNullOrEmpty(value) && value.Length > 2)
                {
                    _ = LoadWeatherAsync();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand UseLocationCommand { get; }

        public MainViewModel(IWeatherService weatherService, ILocationService locationService)
        {
            _weatherService = weatherService;
            _locationService = locationService;
            RefreshCommand = new Command(async () => await LoadWeatherAsync());
            UseLocationCommand = new Command(async () => await UseLocationAsync());

            // Загружаем погоду при запуске
            _ = LoadWeatherAsync();
        }

        private async Task LoadWeatherAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Загрузка погоды...";

                CurrentWeather = await _weatherService.GetCurrentWeatherAsync(City);
                StatusMessage = "Данные обновлены";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UseLocationAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Получение локации...";

                // Используем сервис локации
                var location = await _locationService.GetCurrentLocationAsync();

                if (location != null)
                {
                    StatusMessage = "Получение погоды по локации...";
                    CurrentWeather = await _weatherService.GetWeatherByLocationAsync(location.Latitude, location.Longitude);
                    City = CurrentWeather.City;
                    StatusMessage = "Данные обновлены по геолокации";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка геолокации: {ex.Message}";

                // Показываем более подробное сообщение для пользователя
                await Application.Current.MainPage.DisplayAlert("Ошибка локации",
                    ex.Message + "\n\nПроверьте:\n• Разрешение на доступ к локации\n• Включена ли геолокация на устройстве", "OK");
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