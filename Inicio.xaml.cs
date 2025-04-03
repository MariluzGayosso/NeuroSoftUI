using System;
using System.Windows;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using System.Text.Json;
using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System.Windows.Controls;

namespace NeuroSoft
{
    public partial class Inicio : Window
    {

        public ChartValues<int> DatosAlertas { get; set; }
        private UserData CurrentUser { get; set; }

        public Inicio()
        {
            InitializeComponent();
            LoadUserData();
            InitializeCharts();
            LoadInitialData();
        }

        private void LoadUserData()
        {
            if (Application.Current.Properties.Contains("UserData"))
            {
                CurrentUser = Application.Current.Properties["UserData"] as UserData;
                

                // Actualizar UI con datos del usuario
                if (CurrentUser != null)
                {
                    txtNombreUsuario.Text = CurrentUser.nombre_completo;
                    txtCorreoUsuario.Text = CurrentUser.email;
                }
            }
            else
            {
                MessageBox.Show("No se encontraron datos de usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ReturnToLogin();
            }
        }

        private void InitializeCharts()
        {
            // Configuración inicial de gráficos
            DatosAlertas = new ChartValues<int> { 5, 10, 8, 15, 12 }; // Datos de ejemplo

            // Configuración adicional de gráficos según tu XAML
            // (Los gráficos ya están configurados en el XAML)
        }

        private async void LoadInitialData()
        {
            try
            {
                // Ejemplo: Cargar datos de alertas desde la API
                var response = await ApiHelper.GetAsync("alertas/ultimas/");

                if (response?.IsSuccessStatusCode == true)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var alertas = JsonSerializer.Deserialize<int[]>(content);

                    DatosAlertas.Clear();
                    DatosAlertas.AddRange(alertas);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Estás seguro que deseas cerrar sesión?", "Confirmar",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                await ApiHelper.Logout();
                ReturnToLogin();
            }
        }

        private void BtnSubirImagenes_Click(object sender, RoutedEventArgs e)
        {
            var ventanaSubir = new Subir();
            ventanaSubir.Show();
            this.Close();
        }

        private void BtnResultados_Click(object sender, RoutedEventArgs e)
        {
            //var resultadosWindow = new Resultados();
            //resultadosWindow.Show();
            this.Close();
        }

        private void BtnAlertas_Click(object sender, RoutedEventArgs e)
        {
            var alertasWindow = new Alertas();
            alertasWindow.Show();
            this.Close();
        }

        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            var registroWindow = new Registro();
            registroWindow.Show();
            this.Close();
        }


        private void ReturnToLogin()
        {
            var loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}