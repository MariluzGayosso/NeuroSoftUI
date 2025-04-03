using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using LiveCharts;
using LiveCharts.Wpf;
using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System.Text.Json;
using Microsoft.Win32;
using System.Collections.Generic;

namespace NeuroSoft
{
    public partial class Resultados : Window
    {

        public ResultadoData Resultado { get; set; }
        private UserData CurrentUser { get; set; }

        public Resultados(ResultadoData resultado)
        {
            InitializeComponent();
            LoadUserData();
            Resultado = resultado ?? new ResultadoData();

            // Inicializar propiedades necesarias
            if (Resultado.Probabilidades == null)
            {
                Resultado.Probabilidades = new Dictionary<string, double>
                {
                    { "Poco probable", 0 },
                    { "Probable", 0 },
                    { "Muy probable", 0 }
                };
            }

            DataContext = this;

            // Esperar a que la ventana esté completamente cargada
            this.Loaded += (sender, e) =>
            {
                CargarImagen();
                ConfigurarGrafico();

                if (Resultado.Id > 0 && Resultado.EstadoAnalisis == "Procesando...")
                {
                    _ = ProcesarImagenYMostrarResultados();
                }
            };
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

        private async Task ProcesarImagenYMostrarResultados()
        {
            try
            {
                Debug.WriteLine("Iniciando procesamiento de imagen...");

                if (OverlayGrid != null) OverlayGrid.Visibility = Visibility.Visible;
                if (EstadoProgresoText != null) EstadoProgresoText.Text = "Procesando imagen...";

                var requestData = new { estudio_id = Resultado.Id };
                Debug.WriteLine($"Enviando solicitud para estudio ID: {Resultado.Id}");

                var response = await ApiHelper.PostAsync("resultados/procesar-imagen/", requestData);

                if (response == null)
                {
                    Debug.WriteLine("Error: La respuesta de la API es nula");
                    MessageBox.Show("No se recibió respuesta del servidor");
                    return;
                }

                Debug.WriteLine($"Respuesta recibida. Status: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Contenido de la respuesta: {content}");

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse == null)
                {
                    Debug.WriteLine("Error: No se pudo deserializar la respuesta");
                    MessageBox.Show("Error al interpretar la respuesta del servidor");
                    return;
                }

                if (!apiResponse.Success)
                {
                    Debug.WriteLine($"Error del servidor: {apiResponse.Error}");
                    MessageBox.Show(apiResponse.Error ?? "Error desconocido al procesar la imagen");
                    return;
                }

                Debug.WriteLine("Procesamiento exitoso. Actualizando UI...");

                // Asegurarnos que las probabilidades están inicializadas
                Resultado.Probabilidades ??= new Dictionary<string, double>
        {
            { "Poco probable", 0 },
            { "Probable", 0 },
            { "Muy probable", 0 }
        };

                // Actualizar datos
                Resultado.ResultadoPrediccion = apiResponse.Prediccion ?? "Desconocido";
                Resultado.TipoACV = $"{apiResponse.Precision}% de precisión";
                Resultado.ImagenBase64 = apiResponse.ImagenBase64;
                Resultado.ResumenAnalisis = apiResponse.Resumen ?? "Sin resumen disponible";
                Resultado.EstadoAnalisis = "Finalizado";

                // Actualizar probabilidades si existen
                if (apiResponse.Probabilidades != null)
                {
                    foreach (var kvp in apiResponse.Probabilidades)
                    {
                        if (Resultado.Probabilidades.ContainsKey(kvp.Key))
                        {
                            Resultado.Probabilidades[kvp.Key] = kvp.Value;
                        }
                    }
                    Debug.WriteLine($"Probabilidades actualizadas: {string.Join(", ", Resultado.Probabilidades)}");
                }

                // Forzar actualización de la UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ConfigurarGrafico();
                    CargarImagen();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error inesperado: {ex}");
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
            finally
            {
                Debug.WriteLine("Finalizando procesamiento...");
                if (OverlayGrid != null) OverlayGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfigurarGrafico()
        {
            try
            {
                if (GraficoResultados == null)
                {
                    Debug.WriteLine("Error: GraficoResultados es null");
                    return;
                }

                // Verificar datos
                if (Resultado?.DatosGrafica == null)
                {
                    Debug.WriteLine("Error: DatosGrafica es null");
                    return;
                }

                Debug.WriteLine($"Configurando gráfico con datos: {string.Join(", ", Resultado.DatosGrafica)}");

                // Crear nueva serie
                var series = new ColumnSeries
                {
                    Title = "Predicción IA",
                    Values = new ChartValues<double>(Resultado.DatosGrafica),
                    Fill = System.Windows.Media.Brushes.DodgerBlue,
                    DataLabels = true
                };

                // Configurar ejes
                var axisX = new Axis
                {
                    Title = "Probabilidad",
                    Labels = new[] { "Poco probable", "Probable", "Muy probable" },
                    Separator = new LiveCharts.Wpf.Separator { Step = 1 }
                };

                var axisY = new Axis
                {
                    Title = "Porcentaje (%)",
                    LabelFormatter = value => value.ToString("N0"),
                    MinValue = 0,
                    MaxValue = 100
                };

                // Asignar al gráfico
                GraficoResultados.Series = new SeriesCollection { series };
                GraficoResultados.AxisX.Clear();
                GraficoResultados.AxisX.Add(axisX);
                GraficoResultados.AxisY.Clear();
                GraficoResultados.AxisY.Add(axisY);
                GraficoResultados.LegendLocation = LegendLocation.Top;

                // Forzar actualización
                GraficoResultados.Update(true);
                Debug.WriteLine("Gráfico configurado exitosamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al configurar gráfico: {ex}");
            }
        }

        private void CargarImagen()
        {
            if (!string.IsNullOrEmpty(Resultado.ImagenBase64) && ImagenResultado != null)
            {
                try
                {
                    var bytes = Convert.FromBase64String(Resultado.ImagenBase64);
                    using (var ms = new MemoryStream(bytes))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        ImagenResultado.Source = image;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al cargar imagen: {ex.Message}");
                }
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
            new Subir().Show();
            this.Close();
        }

        private void BtnAlertas_Click(object sender, RoutedEventArgs e)
        {
            new Alertas().Show();
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

        private async void BtnDescargarInforme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Resultado.Id == 0)
                {
                    MessageBox.Show("No hay resultados para descargar");
                    return;
                }

                var response = await ApiHelper.GetAsync($"resultados/generar-informe/{Resultado.Id}");

                if (response?.IsSuccessStatusCode == true)
                {
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "PDF files (*.pdf)|*.pdf",
                        FileName = $"Reporte_{Resultado.Id}_{DateTime.Now:yyyyMMdd}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                        MessageBox.Show("Informe descargado correctamente");
                    }
                }
                else
                {
                    var error = response != null ? await response.Content.ReadAsStringAsync() : "Error desconocido";
                    MessageBox.Show($"Error al descargar informe: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar informe: {ex.Message}");
            }
        }
    }

    
}