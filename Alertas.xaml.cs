using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32; // Para el diálogo de selección de archivo
using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeuroSoft
{
    /// <summary>
    /// Lógica de interacción para Alertas.xaml
    /// </summary>
    public partial class Alertas : Window
    {
        private UserData CurrentUser { get; set; }
        public string rol { get; set; }
        private List<Alerta> listaAlertas = new List<Alerta>();  // Lista para almacenar las alertas

        public Alertas()
        {
            InitializeComponent();
            LoadUserData();
            LoadAlertas();
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
                    // Aquí cambiamos la visibilidad del botón según el rol
                    if (CurrentUser.rol == "Admin")
                    {
                        BtnRegistro.Visibility = Visibility.Visible;  // Mostrar el botón si es Admin
                    }
                    else
                    {
                        BtnRegistro.Visibility = Visibility.Collapsed;  // Ocultar el botón si no es Admin
                    }
                }
            }
            else
            {
                MessageBox.Show("No se encontraron datos de usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void LoadAlertas()
        {
            using HttpClient client = new HttpClient();

            try
            {
                // 1. Obtener resultados
                var responseResultados = await client.GetAsync("http://localhost:8000/api/resultados/resultados/");
                if (!responseResultados.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error al obtener resultados: {responseResultados.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var jsonResultados = await responseResultados.Content.ReadAsStringAsync();
                var resultados = JsonSerializer.Deserialize<List<ResultadoAlertaApi>>(jsonResultados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 2. Obtener pacientes
                var responsePacientes = await client.GetAsync("http://localhost:8000/api/pacientes/");
                if (!responsePacientes.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Error al obtener pacientes: {responsePacientes.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var jsonPacientes = await responsePacientes.Content.ReadAsStringAsync();
                var pacientes = JsonSerializer.Deserialize<List<PacienteApi>>(jsonPacientes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (resultados == null || pacientes == null)
                {
                    MessageBox.Show("No se pudieron cargar los datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3. Combinar datos
                listaAlertas.Clear();  // Limpiar la lista antes de cargar nuevas alertas
                foreach (var resultado in resultados)
                {
                    var paciente = pacientes.FirstOrDefault(p => p.id == resultado.paciente);
                    if (paciente == null) continue;

                    string nombreCompleto = $"{paciente.nombre} {paciente.apellido_paterno} {paciente.apellido_materno}";

                    string color = resultado.prediccion_ia switch
                    {
                        "Muy probable" => "#EE752F", // Naranja
                        "Probable" => "#F9AF10", // Amarillo
                        "Poco probable" => "#58A55C", // Verde
                        _ => "#FFFFFF" // Blanco por defecto
                    };

                    listaAlertas.Add(new Alerta
                    {
                        FechaReporte = resultado.fecha_resultado,
                        IDArchivo = resultado.id.ToString(), // <-- Aquí se iguala el id
                        NombrePaciente = nombreCompleto,
                        Prediccion = resultado.prediccion_ia,
                        ColorPrediccion = color
                    });
                }

                // 4. Asignar al DataGrid
                DataGridAlertas.ItemsSource = listaAlertas;  // Esto asigna la lista de alertas al DataGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar alertas: {ex.Message}");
            }
        }

        // Método de filtrado
        private void FiltrarAlertas()
        {
            var nombrePacienteFiltro = txtNombrePaciente.Text.ToLower();
            var idArchivoFiltro = txtIDArchivo.Text.ToLower();
            var fechaReporteFiltro = dpFechaReporte.SelectedDate;
            var prediccionFiltro = cmbPrediccionIA.SelectedItem as ComboBoxItem;

            // Filtrar la lista de alertas
            var alertasFiltradas = listaAlertas.Where(a =>
                (string.IsNullOrEmpty(nombrePacienteFiltro) || a.NombrePaciente.ToLower().Contains(nombrePacienteFiltro)) &&
                (string.IsNullOrEmpty(idArchivoFiltro) || a.IDArchivo.Contains(idArchivoFiltro)) &&
                (!fechaReporteFiltro.HasValue ||
                 (DateTime.TryParse(a.FechaReporte, out DateTime fechaReporte) && fechaReporte.Date == fechaReporteFiltro.Value.Date)) &&
                (prediccionFiltro == null || a.Prediccion == prediccionFiltro.Content.ToString())
            ).ToList();

            // Actualizar el DataGrid
            DataGridAlertas.ItemsSource = alertasFiltradas;
        }

        // Evento para el botón de búsqueda
        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            FiltrarAlertas();

            // Limpiar los campos después de la búsqueda
            txtNombrePaciente.Clear();
            txtIDArchivo.Clear();
            dpFechaReporte.SelectedDate = null;
            cmbPrediccionIA.SelectedItem = null;
        }

        private async void BtnEliminarAlerta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.DataContext is Alerta alerta)
                {
                    // Confirmación de eliminación
                    var confirm = MessageBox.Show($"¿Deseas eliminar la alerta para el paciente {alerta.NombrePaciente}?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm == MessageBoxResult.Yes)
                    {
                        // Realizar la solicitud DELETE a la API de resultados
                        var response = await ApiHelper.DeleteAsync($"resultados/resultados/{alerta.IDArchivo}/");

                        // Verificar si la solicitud fue exitosa
                        if (response.IsSuccessStatusCode)
                        {
                            // Si la respuesta es exitosa, elimina la alerta de la lista local
                            listaAlertas.Remove(alerta);

                            // Actualiza el DataGrid
                            DataGridAlertas.ItemsSource = null;
                            DataGridAlertas.ItemsSource = listaAlertas;

                            // Mostrar mensaje de éxito
                            MessageBox.Show("Alerta eliminada.");
                        }
                        else
                        {
                            // Si la respuesta no es exitosa, mostrar error
                             string errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Error al eliminar la alerta: {response.ReasonPhrase}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Evento para el botón "Inicio" en el menú lateral
        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            // Crear una nueva instancia de la ventana Inicio
            Inicio inicioWindow = new Inicio();
            inicioWindow.Show();

            // Cerrar la ventana actual (Resultados)
            this.Close();
        }

        // Evento para el botón "Subir Imágenes" en el menú lateral
        private void BtnSubirImagenes_Click(object sender, RoutedEventArgs e)
        {
            // Crear una nueva instancia de la ventana Subir
            Subir subirWindow = new Subir();
            subirWindow.Show();

            // Cerrar la ventana actual (Resultados)
            this.Close();
        }

        // Evento para el botón "Resultados" en el menú lateral
        private void BtnResultados_Click(object sender, RoutedEventArgs e)
        {
            new Resultados(null).Show();
            this.Close();
        }

        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            var registroWindow = new Registro();
            registroWindow.Show();
            this.Close();
        }

        // Evento de clic para el botón "Salir"
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show("¿Estás seguro de que quieres salir?", "Salir", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (resultado == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
