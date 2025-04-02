using System;
using System.Collections.Generic;

namespace NeuroSoft.Models
{
    public class LoginResponse
    {
        public string refresh { get; set; }
        public string access { get; set; }
        public UserData user { get; set; }
    }

    public class UserData
    {
        public int id { get; set; }
        public string username { get; set; }
        public string nombre_completo { get; set; }
        public string email { get; set; }
        public string rol { get; set; }
    }

    public class PacienteData
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string apellido_paterno { get; set; }
        public string apellido_materno { get; set; }
        public string curp { get; set; }
        public string nss { get; set; }
    }

    public class EstudioData
    {
        internal int id;

        public int paciente_id { get; set; }
        public string fecha_estudio { get; set; } 
        public string sede { get; set; }
        public string prioridad { get; set; } 
        public int checklist_causas { get; set; }
        public int checklist_emergencia { get; set; }
        public string ruta_archivo { get; set; }
    }

    public class ChecklistCausasData
    {
        public int id { get; set; }
        public int paciente_id { get; set; } // Asegúrate que coincide con el backend
        public bool traumatismo_cabeza { get; set; } // minúsculas y guiones bajos
        public bool presion_alta { get; set; }
        public bool colesterol_alto { get; set; }
        public bool diabetes { get; set; }
        public bool antecedentes_familiares { get; set; }
        public bool enfermedad_cardiovascular { get; set; }
    }

    public class ChecklistEmergenciaData
    {
        public int id { get; set; }
        public int paciente_id { get; set; }
        public bool dificultad_hablar { get; set; }
        public bool entumecimiento_cara { get; set; }
        public bool entumecimiento_brazo { get; set; }
        public bool problemas_ver { get; set; }
        public bool dolor_cabeza { get; set; }
        public bool problemas_caminar { get; set; }
    }

    public class ResultadoData
    {
        public int Id { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public string FechaEstudio { get; set; } = string.Empty;
        public string FechaReporte { get; set; } = string.Empty;
        public string EstadoAnalisis { get; set; } = "Procesando...";
        public string ResultadoPrediccion { get; set; } = string.Empty;
        public string TipoACV { get; set; } = string.Empty;
        public string DatosChecklist { get; set; } = string.Empty;
        public string ImagenBase64 { get; set; } = string.Empty;
        public string ResumenAnalisis { get; set; } = string.Empty;

        private Dictionary<string, double> _probabilidades = new Dictionary<string, double>
    {
        { "Poco probable", 0 },
        { "Probable", 0 },
        { "Muy probable", 0 }
    };

        public Dictionary<string, double> Probabilidades
        {
            get => _probabilidades;
            set
            {
                _probabilidades = value ?? new Dictionary<string, double>
            {
                { "Poco probable", 0 },
                { "Probable", 0 },
                { "Muy probable", 0 }
            };
            }
        }

        public double[] DatosGrafica => new[]
        {
        Probabilidades["Poco probable"],
        Probabilidades["Probable"],
        Probabilidades["Muy probable"]
    };

        public string[] LabelsGrafica => new[] { "Poco probable", "Probable", "Muy probable" };
        public Func<double, string> LabelFormatter => value => value.ToString("N0");
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public int ResultadoId { get; set; }
        public string Prediccion { get; set; }
        public double Precision { get; set; }
        public string ImagenBase64 { get; set; }
        public string Resumen { get; set; }
        public Dictionary<string, double> Probabilidades { get; set; }
        public string Error { get; set; }
    }
}