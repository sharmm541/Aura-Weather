using System.Globalization;

namespace Aura.Converters
{
    public class WeatherIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string weatherType)
            {
                return weatherType.ToLower() switch
                {
                    "clear" => "☀️",
                    "clouds" => "☁️",
                    "rain" => "🌧️",
                    "drizzle" => "🌦️",
                    "thunderstorm" => "⛈️",
                    "snow" => "❄️",
                    "mist" or "fog" or "haze" => "🌫️",
                    _ => "🌈"
                };
            }
            return "🌈";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}