using Aura.ViewModels;
using Aura.Services;

namespace Aura
{
    public partial class MainPage : ContentPage
    {
        private readonly IWeatherService _weatherService;
        private readonly ILocationService _locationService;

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            // Создаем сервисы
            _weatherService = new WeatherService();
            _locationService = new LocationService();

            BindingContext = new MainViewModel(_weatherService, _locationService);

            // Скрываем тулбар
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnForecastClicked(object sender, EventArgs e)
        {
            try
            {
                var viewModel = BindingContext as MainViewModel;
                if (viewModel?.CurrentWeather != null && !string.IsNullOrEmpty(viewModel.CurrentWeather.City))
                {
                    // Используем тот же сервис погоды
                    var forecastViewModel = new ForecastViewModel(_weatherService);
                    await forecastViewModel.LoadForecastAsync(viewModel.CurrentWeather.City);

                    var forecastPage = new ForecastPage();
                    forecastPage.BindingContext = forecastViewModel;

                    await Navigation.PushAsync(forecastPage);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось получить данные о городе", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть прогноз: {ex.Message}", "OK");
            }
        }
    }
}