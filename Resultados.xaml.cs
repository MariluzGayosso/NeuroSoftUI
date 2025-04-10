using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System.ComponentModel;

namespace NeuroSoft
{
    public partial class Resultados : Window
    {
        public ResultadoData Resultado { get; set; }
        private UserData CurrentUser { get; set; }
        public string rol { get; set; }

        public Resultados(ResultadoData resultado)
        {
            InitializeComponent();
            LoadUserData();
            Resultado = resultado ?? new ResultadoData
            {
                Probabilidades = new Dictionary<string, double>
                {
                    { "Poco probable", 0 },
                    { "Probable", 0 },
                    { "Muy probable", 0 }
                },
                DatosGrafica = new double[] { 0, 0, 0 },
                LabelsGrafica = new string[] { "Poco probable", "Probable", "Muy probable" }
            };
            DataContext = this;
            this.Loaded += async (sender, e) => await InitializeAsync();
        }

        private void LoadUserData()
        {
            if (Application.Current.Properties.Contains("UserData"))
            {
                CurrentUser = Application.Current.Properties["UserData"] as UserData;
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
        }

        private async Task InitializeAsync()
        {
            if (Resultado.Id > 0)
            {
                if (Resultado.EstadoAnalisis == "Procesando...")
                {
                    await ConsultarYActualizarResultado();
                }
                else
                {
                    UpdateUI();
                }
            }
        }

        private async Task ConsultarYActualizarResultado()
        {
            try
            {
                SetProcessingState(true, "Obteniendo resultados...");

                var consultaResponse = await ConsultarResultado(Resultado.Id);
                if (!consultaResponse.Success)
                {
                    ShowError(consultaResponse.Error ?? "Error al consultar el resultado");
                    return;
                }

                UpdateResultData(null, consultaResponse);
                UpdateUI();
            }
            catch (Exception ex)
            {
                ShowError($"Error inesperado: {ex.Message}");
                Debug.WriteLine($"Error: {ex}");
            }
            finally
            {
                SetProcessingState(false);
            }
        }

        private async Task<ApiResponse> ConsultarResultado(int estudioId)
        {
            var response = await ApiHelper.GetAsync($"resultados/consulta-resultado/?estudio_id={estudioId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return new ApiResponse { Success = false, Error = errorContent };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void UpdateResultData(ApiResponse procesarResponse, ApiResponse consultaResponse)
        {
            if (procesarResponse != null)
            {
                Resultado.ResultadoPrediccion = procesarResponse.Prediccion;
                Resultado.Precision = $"{procesarResponse.Precision:0.00}%";
                if (procesarResponse.Probabilidades != null)
                {
                    Resultado.Probabilidades = procesarResponse.Probabilidades;
                }
            }

            if (consultaResponse != null)
            {
                Resultado.ResultadoPrediccion = consultaResponse.Prediccion ?? Resultado.ResultadoPrediccion;
                Resultado.Precision = consultaResponse.Precision > 0 ? $"{consultaResponse.Precision:0.00}%" : Resultado.Precision;
                if (consultaResponse.Probabilidades != null)
                {
                    Resultado.Probabilidades = consultaResponse.Probabilidades;
                }
            }

            Resultado.EstadoAnalisis = "Finalizado";
        }

        private void UpdateUI()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CommandManager.InvalidateRequerySuggested();
            });
        }

        private void SetProcessingState(bool isProcessing, string message = "")
        {
            OverlayGrid.Visibility = isProcessing ? Visibility.Visible : Visibility.Collapsed;
            EstadoProgresoText.Text = message;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine(message);
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e) => NavigateTo<Inicio>();
        private void BtnSubirImagenes_Click(object sender, RoutedEventArgs e) => NavigateTo<Subir>();
        private void BtnAlertas_Click(object sender, RoutedEventArgs e) => NavigateTo<Alertas>();
        private void BtnRegistro_Click(object sender, RoutedEventArgs e) => NavigateTo<Registro>();

        private void NavigateTo<T>() where T : Window, new()
        {
            new T().Show();
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

                // Ajustar el ID restando 21 (por ejemplo, si el ID actual está adelantado en 22)
                int idCorregido = Resultado.Id - 22;

                // Verifica que el ID corregido sea válido (por ejemplo, no negativo)
                if (idCorregido < 1)
                {
                    MessageBox.Show("ID corregido inválido");
                    return;
                }

                // Realizar la llamada a la API con el ID corregido
                var response = await ApiHelper.GetAsync($"informes/generar-informe/{idCorregido}/");

                if (response?.IsSuccessStatusCode == true)
                {
                    // Si la respuesta es exitosa, guardar el PDF
                    await SavePdfReport(response);
                }
                else
                {
                    // Mostrar un mensaje de error si la llamada a la API falla
                    ShowApiError(response);
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que ocurra durante el proceso
                MessageBox.Show($"Error al descargar informe: {ex.Message}");
            }
        }

        private async Task SavePdfReport(HttpResponseMessage response)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Reporte_{SanitizeFileName(Resultado.NombrePaciente)}_{Resultado.FechaEstudio.Replace("/", "-")}.pdf"

            };

            if (saveDialog.ShowDialog() == true)
            {
                var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(saveDialog.FileName, pdfBytes);
                MessageBox.Show("Informe descargado correctamente");
            }
        }

        private string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input;
        }


        private async void ShowApiError(HttpResponseMessage response)
        {
            var error = response != null ? await response.Content.ReadAsStringAsync() : "Error desconocido";
            MessageBox.Show($"Error al descargar informe: {error}");
        }
    }

    public class ResultadoData : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string NombrePaciente { get; set; }
        public string FechaEstudio { get; set; }
        public string FechaReporte { get; set; }
        public string DatosChecklist { get; set; }
        public string EstadoAnalisis { get; set; }
        public string ResultadoPrediccion { get; set; }
        public string Precision { get; set; }

        private Dictionary<string, double> probabilidades;
        public Dictionary<string, double> Probabilidades
        {
            get { return probabilidades; }
            set
            {
                probabilidades = value;
                OnPropertyChanged(nameof(Probabilidades));
                ActualizarDatosGrafica();
            }
        }

        private double[] datosGrafica;
        public double[] DatosGrafica
        {
            get { return datosGrafica; }
            set
            {
                datosGrafica = value;
                OnPropertyChanged(nameof(DatosGrafica));
            }
        }

        private string[] labelsGrafica;
        public string[] LabelsGrafica
        {
            get { return labelsGrafica; }
            set
            {
                labelsGrafica = value;
                OnPropertyChanged(nameof(LabelsGrafica));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ActualizarDatosGrafica()
        {
            if (Probabilidades != null)
            {
                DatosGrafica = Probabilidades.Values.ToArray();
                LabelsGrafica = Probabilidades.Keys.ToArray();
            }
        }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public int ResultadoId { get; set; }
        public string Prediccion { get; set; }
        public double Precision { get; set; }
        public string ImagenUrl { get; set; }
        public Dictionary<string, double> Probabilidades { get; set; }
        public string Error { get; set; }
    }
}