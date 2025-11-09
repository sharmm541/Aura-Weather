using System.Text.Json.Serialization;

namespace Aura.Models
{
    public class CurrentWeather
    {
        [JsonPropertyName("name")]
        public string City { get; set; }

        [JsonPropertyName("main")]
        public MainData Main { get; set; }

        [JsonPropertyName("weather")]
        public Weather[] Weather { get; set; }

        [JsonPropertyName("wind")]
        public WindData Wind { get; set; }

        public string Description => Weather?.Length > 0 ? Weather[0].Description : "Неизвестно";
        public string Icon => Weather?.Length > 0 ? Weather[0].Icon : "01d";
        public double Temperature => Main?.Temp ?? 0;
        public int Humidity => Main?.Humidity ?? 0;
        public double Pressure => Main?.Pressure ?? 0;
        public double WindSpeed => Wind?.Speed ?? 0;
        public string WindDirection => GetWindDirection(Wind?.Deg ?? 0);

        private string GetWindDirection(double degrees)
        {
            if (degrees >= 337.5 || degrees < 22.5) return "С";
            if (degrees >= 22.5 && degrees < 67.5) return "СВ";
            if (degrees >= 67.5 && degrees < 112.5) return "В";
            if (degrees >= 112.5 && degrees < 157.5) return "ЮВ";
            if (degrees >= 157.5 && degrees < 202.5) return "Ю";
            if (degrees >= 202.5 && degrees < 247.5) return "ЮЗ";
            if (degrees >= 247.5 && degrees < 292.5) return "З";
            return "СЗ";
        }
    }

    public class ForecastData
    {
        [JsonPropertyName("list")]
        public List<ForecastItem> List { get; set; }

        [JsonPropertyName("city")]
        public CityInfo City { get; set; }
    }

    public class ForecastItem
    {
        [JsonPropertyName("dt")]
        public long DateTimeStamp { get; set; }

        [JsonPropertyName("main")]
        public MainData Main { get; set; }

        [JsonPropertyName("weather")]
        public Weather[] Weather { get; set; }

        [JsonPropertyName("wind")]
        public WindData Wind { get; set; }

        public DateTime Date => DateTimeOffset.FromUnixTimeSeconds(DateTimeStamp).DateTime;
        public string DayString => Date.ToString("dddd");
        public string DateString => Date.ToString("dd MMMM yyyy");
        public string Description => Weather?.Length > 0 ? Weather[0].Description : "Неизвестно";
        public string WeatherType => Weather?.Length > 0 ? Weather[0].Main : "Clear";
        public double Temperature => Main?.Temp ?? 0;
        public int Humidity => Main?.Humidity ?? 0;
        public double WindSpeed => Wind?.Speed ?? 0;
    }

    public class MainData
    {
        [JsonPropertyName("temp")]
        public double Temp { get; set; }

        [JsonPropertyName("humidity")]
        public int Humidity { get; set; }

        [JsonPropertyName("pressure")]
        public double Pressure { get; set; }
    }

    public class Weather
    {
        [JsonPropertyName("main")]
        public string Main { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }
    }

    public class WindData
    {
        [JsonPropertyName("speed")]
        public double Speed { get; set; }

        [JsonPropertyName("deg")]
        public double Deg { get; set; }
    }

    public class CityInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}