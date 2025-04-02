using System;
using System.Diagnostics;
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
        private const string BaseUrl = "http://localhost:8000/api/"; // Ajusta según tu configuración
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };

        static ApiHelper()
        {
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Configurar timeout (opcional)
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Configura el token JWT para las solicitudes futuras
        /// </summary>
        public static void SetAuthToken()
        {
            if (Application.Current.Properties.Contains("JwtToken"))
            {
                var token = Application.Current.Properties["JwtToken"].ToString();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Limpia el token JWT de las solicitudes
        /// </summary>
        public static void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        /// <summary>
        /// Realiza una solicitud POST autenticada
        /// </summary>
        public static async Task<HttpResponseMessage> PostAsync(string endpoint, object data)
        {
            try
            {
                SetAuthToken();

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Log para depuración
                Debug.WriteLine($"POST a: {BaseUrl}{endpoint}");
                Debug.WriteLine($"Datos: {json}");

                var response = await _httpClient.PostAsync(endpoint, content);

                // Log de respuesta
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Respuesta: {response.StatusCode}\n{responseContent}");

                return response;
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Error HTTP: {httpEx.Message}");
                MessageBox.Show($"Error de conexión: {httpEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("La solicitud tardó demasiado. Por favor intente nuevamente.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex}");
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Realiza una solicitud GET autenticada
        /// </summary>
        public static async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            try
            {
                SetAuthToken();

                Debug.WriteLine($"GET a: {BaseUrl}{endpoint}");

                var response = await _httpClient.GetAsync(endpoint);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Respuesta: {response.StatusCode}\n{responseContent}");

                return response;
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"Error HTTP: {httpEx.Message}");
                MessageBox.Show($"Error de conexión: {httpEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show("La solicitud tardó demasiado. Por favor intente nuevamente.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex}");
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Realiza una solicitud PUT autenticada
        /// </summary>
        public static async Task<HttpResponseMessage> PutAsync(string endpoint, object data)
        {
            try
            {
                SetAuthToken();

                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Debug.WriteLine($"PUT a: {BaseUrl}{endpoint}");
                Debug.WriteLine($"Datos: {json}");

                var response = await _httpClient.PutAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Respuesta: {response.StatusCode}\n{responseContent}");

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en PUT: {ex}");
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Realiza una solicitud DELETE autenticada
        /// </summary>
        public static async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            try
            {
                SetAuthToken();

                Debug.WriteLine($"DELETE a: {BaseUrl}{endpoint}");

                var response = await _httpClient.DeleteAsync(endpoint);

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Respuesta: {response.StatusCode}\n{responseContent}");

                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en DELETE: {ex}");
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Cierra la sesión y limpia los tokens
        /// </summary>
        public static async Task<bool> Logout()
        {
            try
            {
                var response = await PostAsync("auth/logout/", new { });

                if (response?.IsSuccessStatusCode == true)
                {
                    Application.Current.Properties.Remove("JwtToken");
                    Application.Current.Properties.Remove("RefreshToken");
                    Application.Current.Properties.Remove("UserData");
                    ClearAuthToken();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en logout: {ex}");
                MessageBox.Show($"Error al cerrar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Intenta refrescar el token JWT usando el refresh token
        /// </summary>
        public static async Task<bool> RefreshToken()
        {
            if (!Application.Current.Properties.Contains("RefreshToken"))
                return false;

            try
            {
                var refreshToken = Application.Current.Properties["RefreshToken"].ToString();
                var response = await _httpClient.PostAsync("auth/token/refresh/",
                    new StringContent(JsonSerializer.Serialize(new { refresh = refreshToken }),
                    Encoding.UTF8,
                    "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenRefreshResponse>(content);

                    Application.Current.Properties["JwtToken"] = tokenResponse.Access;
                    SetAuthToken();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al refrescar token: {ex}");
            }

            return false;
        }

        private class TokenRefreshResponse
        {
            public string Access { get; set; }
        }
    }
}