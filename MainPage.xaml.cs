using Aura.ViewModels;
using Aura.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aura
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // Получаем сервисы через DI контейнер
            var weatherService = MauiProgram.Services.GetService<IWeatherService>();
            var locationService = MauiProgram.Services.GetService<ILocationService>();
            var localStorage = MauiProgram.Services.GetService<ILocalStorageService>();

            BindingContext = new MainViewModel(weatherService, locationService, localStorage);

            // Скрываем тулбар
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void OnForecastClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // Анимация нажатия кнопки
                await button.ScaleTo(0.95, 100, Easing.CubicIn);
                await button.ScaleTo(1, 100, Easing.CubicOut);
            }

            try
            {
                var viewModel = BindingContext as MainViewModel;
                if (viewModel?.CurrentWeather != null && !string.IsNullOrEmpty(viewModel.CurrentWeather.City))
                {
                    var weatherService = MauiProgram.Services.GetService<IWeatherService>();
                    var localStorage = MauiProgram.Services.GetService<ILocalStorageService>();

                    var forecastViewModel = new ForecastViewModel(weatherService, localStorage);
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