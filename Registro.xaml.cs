using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuroSoft
{
    /// <summary>
    /// Lógica de interacción para Registro.xaml
    /// </summary>
    public partial class Registro : Window
    {
        private UserData CurrentUser { get; set; }
        public string rol { get; set; }
        public string telefono { get; set; }
        public string username { get; set; }

        // Lista para simular la base de datos
        private List<UsuarioData> medicosRegistrados = new List<UsuarioData>();

        public Registro()
        {
            InitializeComponent();
            LoadUserData();
            LoadMedicos(); // Carga la lista de médicos al iniciar la ventana

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
            }
        }

        private async void LoadMedicos()
        {
            try
            {
                var response = await ApiHelper.GetAsync("usuarios/");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var todosLosUsuarios = JsonSerializer.Deserialize<List<UsuarioData>>(
                        content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var medicos = todosLosUsuarios?
                        .Where(u => u.rol?.Equals("Médico", StringComparison.OrdinalIgnoreCase) == true)
                        .ToList();

                    medicosRegistrados = medicos ?? new List<UsuarioData>();  // Guardamos la lista de médicos
                    dgMedicos.ItemsSource = medicosRegistrados;  // Asignamos al DataGrid
                }
                else
                {
                    MessageBox.Show("Error al cargar la lista de médicos");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar médicos: {ex.Message}");
            }
        }


        private async void BtnRegistrarMedico_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones de campos vacíos
                if (string.IsNullOrEmpty(txtNombre.Text) ||
                    string.IsNullOrEmpty(txtApellidos.Text) ||
                    string.IsNullOrEmpty(txtCorreo.Text) ||
                    string.IsNullOrEmpty(txtTelefono.Text) ||
                    string.IsNullOrEmpty(txtUsername.Text) ||
                    string.IsNullOrEmpty(txtPassword.Password) ||
                    string.IsNullOrEmpty(txtConfirmPassword.Password))
                {
                    MessageBox.Show("Complete todos los campos obligatorios");
                    return;
                }

                // Verificar que las contraseñas coincidan
                if (txtPassword.Password != txtConfirmPassword.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden.");
                    return;
                }

                // Verificación de duplicados
                var medicos = (List<UsuarioData>)dgMedicos.ItemsSource;
                if (medicos.Any(m => m.Username == txtUsername.Text))
                {
                    MessageBox.Show("El Username ya está registrado.");
                    return;
                }

                if (medicos.Any(m => m.email.Equals(txtCorreo.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Este correo ya está registrado.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (medicos.Any(m => m.telefono == txtTelefono.Text))
                {
                    MessageBox.Show("Este número de teléfono ya está registrado.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Crear objeto JSON para el médico
                var usuarioData = new
                {
                    first_name = txtNombre.Text,
                    last_name = txtApellidos.Text,
                    email = txtCorreo.Text,
                    telefono = txtTelefono.Text,
                    username = txtUsername.Text,
                    password = txtPassword.Password,
                    password2 = txtConfirmPassword.Password, // Django normalmente requiere confirmar la contraseña
                    rol = "Médico",
                    es_activo = true
                };

                // Hacer la solicitud POST a la API
                var usuarioResponse = await ApiHelper.PostAsync("usuarios/", usuarioData);

                // Verificar si la solicitud fue exitosa
                if (!usuarioResponse.IsSuccessStatusCode)
                {
                    var error = await usuarioResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al dar de alta al Médico:\n{error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Médico registrado correctamente");

                // Refrescar la lista de médicos en el DataGrid
                LoadMedicos();

                // Limpiar los campos del formulario
                txtNombre.Clear();
                txtApellidos.Clear();
                txtCorreo.Clear();
                txtTelefono.Clear();
                txtUsername.Clear();
                txtPassword.Clear();
                txtConfirmPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEliminarMedico_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is UsuarioData medico)
                {
                    // Confirmación de eliminación
                    var confirm = MessageBox.Show($"¿Deseas eliminar al médico {medico.NombreCompleto}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.Yes)
                    {
                        // Realizar la solicitud DELETE a la API
                        var response = await ApiHelper.DeleteAsync($"usuarios/{medico.id}/");

                        // Verificar si la solicitud fue exitosa
                        if (response.IsSuccessStatusCode)
                        {
                            // Si la respuesta es exitosa, elimina el médico de la lista local
                            var medicosRegistrados = (List<UsuarioData>)dgMedicos.ItemsSource;
                            medicosRegistrados.Remove(medico);

                            // Actualiza el DataGrid
                            dgMedicos.ItemsSource = null;
                            dgMedicos.ItemsSource = medicosRegistrados;

                            // Mostrar mensaje de éxito
                            MessageBox.Show("Médico eliminado.");
                        }
                        else
                        {
                            // Si la respuesta no es exitosa, mostrar error
                            MessageBox.Show($"Error al eliminar al médico: {response.ReasonPhrase}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Métodos del menú lateral
        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            new Inicio().Show();
            this.Close();
        }

        private void BtnSubirImagenes_Click(object sender, RoutedEventArgs e)
        {
            var ventanaSubir = new Subir();
            ventanaSubir.Show();
            this.Close();
        }

        private void BtnResultados_Click(object sender, RoutedEventArgs e)
        {
            new Resultados(null).Show();
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

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Estás seguro de que quieres salir?", "Salir",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void txtNombre_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
