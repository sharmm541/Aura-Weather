using Microsoft.Maui.ApplicationModel;

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
                // Проверяем и запрашиваем разрешение
                var hasPermission = await CheckAndRequestPermissionAsync();
                if (!hasPermission)
                {
                    throw new Exception("Разрешение на доступ к локации не предоставлено");
                }

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                return location ?? throw new Exception("Не удалось получить локацию");
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                throw new Exception("Геолокация не поддерживается на этом устройстве", fnsEx);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                throw new Exception("Геолокация отключена на устройстве", fneEx);
            }
            catch (PermissionException pEx)
            {
                throw new Exception("Разрешение на доступ к локации не предоставлено", pEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения локации: {ex.Message}", ex);
            }
        }

        public async Task<bool> CheckAndRequestPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status == PermissionStatus.Granted)
                    return true;

                if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    // На iOS нельзя запрашивать повторно, если пользователь отказал
                    throw new Exception("Разрешение на локацию отклонено. Пожалуйста, включите его в настройках устройства.");
                }

                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка проверки разрешений: {ex.Message}", ex);
            }
        }
    }
}