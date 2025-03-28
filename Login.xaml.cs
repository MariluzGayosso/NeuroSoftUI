using System;
using System.Windows;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NeuroSoft.Helpers;
using NeuroSoft.Models;

namespace NeuroSoft
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            CheckExistingSession();
        }

        private void CheckExistingSession()
        {
            if (Application.Current.Properties.Contains("JwtToken"))
            {
                var inicio = new Inicio();
                inicio.Show();
                this.Close();
            }
        }

        private async void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Password;

            // Validación básica
            if (string.IsNullOrEmpty(usuario) || usuario == "Ingresa tu nombre de usuario")
            {
                MessageBox.Show("Por favor ingrese un nombre de usuario válido", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor ingrese su contraseña", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var loginData = new
                {
                    username = usuario,
                    password = password
                };

                string json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Usar ApiHelper en lugar de HttpClient directo
                var response = await ApiHelper.PostAsync("usuarios/login/", loginData);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent);

                    // Guardar tokens y datos de usuario
                    Application.Current.Properties["JwtToken"] = loginResponse.access;
                    Application.Current.Properties["RefreshToken"] = loginResponse.refresh;
                    Application.Current.Properties["UserData"] = loginResponse.user;

                    // Mostrar ventana principal
                    var inicio = new Inicio();
                    inicio.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtUsuario_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsuario.Text == "Ingresa tu nombre de usuario")
            {
                txtUsuario.Text = "";
                txtUsuario.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TxtUsuario_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                txtUsuario.Text = "Ingresa tu nombre de usuario";
                txtUsuario.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}