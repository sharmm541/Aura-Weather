using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Aura.Models;
using Aura.Services;

namespace Aura.ViewModels
{
    public class ForecastViewModel : INotifyPropertyChanged
    {
        private readonly IWeatherService _weatherService;
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

        public ForecastViewModel(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task LoadForecastAsync(string city)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                City = city;

                var forecastData = await _weatherService.GetForecastAsync(city);

                Forecast.Clear();

                // Добавляем проверку на null
                if (forecastData?.List != null)
                {
                    foreach (var item in forecastData.List)
                    {
                        Forecast.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine($"Ошибка загрузки прогноза: {ex.Message}");

                // Можно показать сообщение пользователю
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    "Не удалось загрузить прогноз погоды", "OK");
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