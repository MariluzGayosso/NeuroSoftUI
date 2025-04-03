using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using NeuroSoft.Helpers;
using NeuroSoft.Models;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;

namespace NeuroSoft
{
    public partial class Subir : Window
    {
        public string _rutaArchivoSeleccionado;
        private UserData CurrentUser { get; set; }
        public string PrioridadSeleccionada { get; set; }

        public Subir()
        {
            InitializeComponent();
            dpFechaEstudio.SelectedDate = DateTime.Today;
            LoadUserData();

            txtPrioridad.SelectedIndex = 1;
            PrioridadSeleccionada = "Media";
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtPrioridad.SelectedItem != null)
            {
                PrioridadSeleccionada = (txtPrioridad.SelectedItem as ComboBoxItem)?.Content.ToString();
                Debug.WriteLine($"Prioridad seleccionada: {PrioridadSeleccionada}");
            }
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
                }
            }
            else
            {
                MessageBox.Show("No se encontraron datos de usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnSubirArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_rutaArchivoSeleccionado))
                {
                    MessageBox.Show("Seleccione un archivo médico");
                    return;
                }

                if (string.IsNullOrEmpty(txtNombre.Text) ||
                    string.IsNullOrEmpty(txtApellidoPaterno.Text) ||
                    string.IsNullOrEmpty(txtCURP.Text))
                {
                    MessageBox.Show("Complete todos los campos obligatorios");
                    return;
                }

                // 1. Crear paciente
                var pacienteData = new
                {
                    nombre = txtNombre.Text,
                    apellido_paterno = txtApellidoPaterno.Text,
                    apellido_materno = txtApellidoMaterno.Text,
                    curp = txtCURP.Text,
                    nss = txtNSS.Text
                };

                var pacienteResponse = await ApiHelper.PostAsync("pacientes/", pacienteData);
                if (!pacienteResponse.IsSuccessStatusCode)
                {
                    var error = await pacienteResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al crear paciente: {error}");
                    return;
                }

                var pacienteContent = await pacienteResponse.Content.ReadAsStringAsync();
                var pacienteResult = JsonSerializer.Deserialize<PacienteData>(
                    pacienteContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                int pacienteId = pacienteResult.id;

                // 2. Crear checklists
                var causasData = new
                {
                    paciente_id = pacienteId,
                    traumatismo_cabeza = chkTraumatismoCabeza.IsChecked ?? false,
                    presion_alta = chkPresionAlta.IsChecked ?? false,
                    colesterol_alto = chkColesterolAlto.IsChecked ?? false,
                    diabetes = chkDiabetes.IsChecked ?? false,
                    antecedentes_familiares = chkAntecedentesFamiliares.IsChecked ?? false,
                    enfermedad_cardiovascular = chkEnfermedadCardiovascular.IsChecked ?? false
                };

                var emergenciaData = new
                {
                    paciente_id = pacienteId,
                    dificultad_hablar = chkDificultadHablar.IsChecked ?? false,
                    entumecimiento_cara = chkEntumecimientoCara.IsChecked ?? false,
                    entumecimiento_brazo = chkEntumecimientoBrazo.IsChecked ?? false,
                    problemas_ver = chkProblemasVer.IsChecked ?? false,
                    dolor_cabeza = chkDolorCabeza.IsChecked ?? false,
                    problemas_caminar = chkProblemasCaminar.IsChecked ?? false
                };

                var causasResponse = await ApiHelper.PostAsync("checklists/causas/", causasData);
                if (!causasResponse.IsSuccessStatusCode)
                {
                    var error = await causasResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al crear checklist de causas: {error}");
                    return;
                }

                var emergenciaResponse = await ApiHelper.PostAsync("checklists/emergencia/", emergenciaData);
                if (!emergenciaResponse.IsSuccessStatusCode)
                {
                    var error = await emergenciaResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al crear checklist de emergencia: {error}");
                    return;
                }

                // 3. Crear estudio
                using var causasDoc = JsonDocument.Parse(await causasResponse.Content.ReadAsStringAsync());
                var causasId = causasDoc.RootElement.GetProperty("id").GetInt32();

                using var emergenciaDoc = JsonDocument.Parse(await emergenciaResponse.Content.ReadAsStringAsync());
                var emergenciaId = emergenciaDoc.RootElement.GetProperty("id").GetInt32();

                var estudioData = new
                {
                    paciente_id = pacienteId,
                    fecha_estudio = dpFechaEstudio.SelectedDate?.ToString("yyyy-MM-dd"),
                    ruta_archivo = _rutaArchivoSeleccionado,
                    checklist_causas = causasId,
                    checklist_emergencia = emergenciaId,
                    sede = txtSedeEstudio.Text,
                    prioridad = PrioridadSeleccionada
                };

                var estudioResponse = await ApiHelper.PostAsync("estudios/estudios/", estudioData);
                if (!estudioResponse.IsSuccessStatusCode)
                {
                    var error = await estudioResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al crear estudio: {error}");
                    return;
                }

                var estudioContent = await estudioResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"Estudio creado: {estudioContent}");

                var estudioResult = JsonSerializer.Deserialize<EstudioData>(
                    estudioContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 4. Procesar la imagen con el ID del estudio
                var procesarResponse = await ProcesarImagen(estudioResult.id);
                if (!procesarResponse.Success)
                {
                    MessageBox.Show($"Error al procesar imagen: {procesarResponse.Error}");
                    return;
                }

                // 5. Mostrar ventana de resultados
                var resultado = new ResultadoData
                {
                    Id = estudioResult.id,
                    NombrePaciente = $"{txtNombre.Text} {txtApellidoPaterno.Text}",
                    FechaEstudio = dpFechaEstudio.SelectedDate?.ToString("dd/MM/yyyy"),
                    FechaReporte = DateTime.Now.ToString("dd/MM/yyyy"),
                    DatosChecklist = FormatChecklistData(causasData, emergenciaData),
                    EstadoAnalisis = "Procesando...",
                    ResultadoPrediccion = procesarResponse.Prediccion,
                    Precision = $"{procesarResponse.Precision:0.00}%",
                    Probabilidades = procesarResponse.Probabilidades ?? new Dictionary<string, double>
                    {
                        { "Poco probable", 0 },
                        { "Probable", 0 },
                        { "Muy probable", 0 }
                    }
                };

                var ventanaResultados = new Resultados(resultado);
                ventanaResultados.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
                Debug.WriteLine($"Error: {ex}");
            }
        }

        private async Task<ApiResponse> ProcesarImagen(int estudioId)
        {
            try
            {
                if (estudioId <= 0)
                {
                    return new ApiResponse { Success = false, Error = "ID de estudio no válido" };
                }

                var requestData = new Dictionary<string, object>
                {
                    { "estudio_id", estudioId }
                };

                Debug.WriteLine($"Enviando solicitud para procesar imagen. Estudio ID: {estudioId}");

                var response = await ApiHelper.PostAsync("resultados/procesar-imagen/", requestData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"Error al procesar imagen. Código: {response.StatusCode}. Mensaje: {errorContent}");
                    return new ApiResponse { Success = false, Error = errorContent };
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Respuesta de procesar-imagen: {responseContent}");

                return JsonSerializer.Deserialize<ApiResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al procesar imagen: {ex}");
                return new ApiResponse { Success = false, Error = ex.Message };
            }
        }

        private string FormatChecklistData(dynamic causas, dynamic emergencia)
        {
            return $"● Causas:\n" +
                   $"  - Traumatismo: {BoolToCheck(causas.traumatismo_cabeza)}\n" +
                   $"  - Presión alta: {BoolToCheck(causas.presion_alta)}\n" +
                   $"  - Colesterol alto: {BoolToCheck(causas.colesterol_alto)}\n" +
                   $"  - Diabetes: {BoolToCheck(causas.diabetes)}\n" +
                   $"  - Antecedentes familiares: {BoolToCheck(causas.antecedentes_familiares)}\n" +
                   $"  - Enfermedad cardiovascular: {BoolToCheck(causas.enfermedad_cardiovascular)}\n" +
                   $"● Síntomas:\n" +
                   $"  - Dificultad al hablar: {BoolToCheck(emergencia.dificultad_hablar)}\n" +
                   $"  - Entumecimiento cara: {BoolToCheck(emergencia.entumecimiento_cara)}\n" +
                   $"  - Entumecimiento brazo: {BoolToCheck(emergencia.entumecimiento_brazo)}\n" +
                   $"  - Problemas de visión: {BoolToCheck(emergencia.problemas_ver)}\n" +
                   $"  - Dolor de cabeza: {BoolToCheck(emergencia.dolor_cabeza)}\n" +
                   $"  - Problemas al caminar: {BoolToCheck(emergencia.problemas_caminar)}";
        }

        private string BoolToCheck(bool valor) => valor ? "✅" : "❌";

        private void BtnSeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos médicos (*.dcm;*.jpg;*.png;*.tif)|*.dcm;*.jpg;*.png;*.tif|Todos los archivos (*.*)|*.*",
                Multiselect = false,
                Title = "Seleccionar imagen médica"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _rutaArchivoSeleccionado = openFileDialog.FileName;
                txtArchivoSeleccionado.Text = Path.GetFileName(_rutaArchivoSeleccionado);
            }
        }

        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            new Inicio().Show();
            this.Close();
        }

        private void BtnResultados_Click(object sender, RoutedEventArgs e)
        {
            new Resultados(null).Show();
            this.Close();
        }

        private void BtnAlertas_Click(object sender, RoutedEventArgs e)
        {
            new Alertas().Show();
            this.Close();
        }

        private void BtnRegistro_Click(object sender, RoutedEventArgs e)
        {
            new Registro().Show();
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
    }

    public class EstudioData
    {
        public int id { get; set; }
        // Otras propiedades que devuelve el API
    }

    public class PacienteData
    {
        public int id { get; set; }
        // Otras propiedades que devuelve el API
    }
}