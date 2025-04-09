using System;
using System.Windows;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Wpf;
using System.Text.Json;
using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Windows.Media;


namespace NeuroSoft
{
    public partial class Inicio : Window
    {

        public ChartValues<int> DatosAlertas { get; set; }
        private UserData CurrentUser { get; set; }
        public string rol { get; set; }

        private List<Alerta> listaAlertas = new List<Alerta>();

        // Asegúrate de que esta variable no esté declarada dentro de otro bloque de ejecución activo.
        private DateTime fechaLimite;

        public Inicio()
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
                this.Close();
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
                        IDArchivo = resultado.id.ToString(),
                        NombrePaciente = nombreCompleto,
                        Prediccion = resultado.prediccion_ia,
                        ColorPrediccion = color
                    });
                }

                // 4. Asignar al DataGrid
                DataGridAlertas.ItemsSource = listaAlertas;  // Esto asigna la lista de alertas al DataGrid

                // Llamar a InitializeCharts solo después de cargar las alertas
                InitializeCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar alertas: {ex.Message}");
            }
        }

        private void InitializeCharts()
        {
            // Filtrar y convertir fechas de forma segura
            var agrupadasPorFecha = listaAlertas
                .Select(alerta =>
                {
                    bool exito = DateTime.TryParse(alerta.FechaReporte, out DateTime fecha);
                    return new { Alerta = alerta, Fecha = exito ? fecha.Date : (DateTime?)null };
                })
                .Where(x => x.Fecha.HasValue)
                .GroupBy(x => x.Fecha.Value)
                .OrderBy(grupo => grupo.Key)
                .ToList();

            // Filtrar solo la última semana
            DateTime fechaLimite = DateTime.Now.AddDays(-7);
            agrupadasPorFecha = agrupadasPorFecha.Where(grupo => grupo.Key >= fechaLimite).ToList();

            // Inicializar colecciones
            DatosAlertas = new ChartValues<int>();
            List<string> etiquetasFechas = new List<string>();

            foreach (var grupo in agrupadasPorFecha)
            {
                DatosAlertas.Add(grupo.Count());
                etiquetasFechas.Add(grupo.Key.ToString("dd MMM"));
            }

            // Asignar datos al gráfico
            AlertasChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Alertas",
                    Values = DatosAlertas,
                    Fill = new SolidColorBrush(Color.FromRgb(255, 87, 51)) // mismo color que XAML
                }
            };

            AlertasChart.AxisX.Clear();
            AlertasChart.AxisX.Add(new Axis
            {
                Title = "Fecha",
                Labels = etiquetasFechas
            });

            AlertasChart.AxisY.Clear();
            AlertasChart.AxisY.Add(new Axis
            {
                Title = "Cantidad",
                LabelFormatter = value => value.ToString("N0")
            });
        }

        private async void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Estás seguro que deseas cerrar sesión?", "Confirmar",
                                            MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                await ApiHelper.Logout();
                this.Close();
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
    }
}