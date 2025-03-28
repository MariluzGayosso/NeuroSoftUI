using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NeuroSoft.Helpers
{
    public static class ApiHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "http://localhost:8000/api/";

        static ApiHelper()
        {
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                if (Application.Current.Properties.Contains("JwtToken"))
                {
                    var token = Application.Current.Properties["JwtToken"].ToString();
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                return await _httpClient.PostAsync(endpoint, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la solicitud POST: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            try
            {
                if (Application.Current.Properties.Contains("JwtToken"))
                {
                    var token = Application.Current.Properties["JwtToken"].ToString();
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }

                return await _httpClient.GetAsync(endpoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la solicitud GET: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static async Task<bool> Logout()
        {
            try
            {
                var response = await PostAsync("usuarios/logout/", new { });

                if (response?.IsSuccessStatusCode == true)
                {
                    Application.Current.Properties.Remove("JwtToken");
                    Application.Current.Properties.Remove("RefreshToken");
                    Application.Current.Properties.Remove("UserData");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}