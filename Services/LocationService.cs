using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace Aura.Services
{
    public interface ILocationService
    {
        Task<Location> GetCurrentLocationAsync();
        Task<bool> CheckAndRequestPermissionAsync();
    }

    public class LocationService : ILocationService
    {
        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                // Проверяем интернет соединение
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    throw new Exception("Отсутствует интернет-соединение. Проверьте подключение к сети.");
                }

                // Проверяем и запрашиваем разрешение
                var hasPermission = await CheckAndRequestPermissionAsync();
                if (!hasPermission)
                {
                    throw new Exception("Разрешение на доступ к геолокации не предоставлено. Пожалуйста, предоставьте разрешение в настройках приложения.");
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    throw new Exception("Не удалось получить данные о местоположении. Попробуйте позже.");
                }

                return location;
            }
            catch (FeatureNotSupportedException)
            {
                throw new Exception("Геолокация не поддерживается на этом устройстве.");
            }
            catch (FeatureNotEnabledException)
            {
                throw new Exception("Геолокация отключена на устройстве. Включите её в настройках.");
            }
            catch (PermissionException)
            {
                throw new Exception("Разрешение на доступ к геолокации не предоставлено. Проверьте настройки приложения.");
            }
            catch (Exception ex)
            {
                // Общая ошибка
                if (ex.Message.Contains("internet", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Ошибка соединения. Проверьте подключение к интернету.");
                }

                throw new Exception($"Ошибка геолокации: {ex.Message}");
            }
        }

        public async Task<bool> CheckAndRequestPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                    return true;

                if (status == PermissionStatus.Denied)
                {
                    // На iOS нельзя запрашивать повторно, если пользователь отказал
                    if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        throw new Exception("Разрешение на геолокацию отклонено. Включите его в настройках устройства.");
                    }

                    // На Android можно запросить снова
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                else
                {
                    // Первый запрос
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }

                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка проверки разрешений: {ex.Message}");
            }
        }
    }
}