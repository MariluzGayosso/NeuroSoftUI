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

    public class Medico
    {
        public string Username { get; set; }
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
    }


    public class UsuarioData
    {
        internal string Username;

        public int id { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string rol { get; set; }
        public bool es_activo { get; set; }

        // Propiedad auxiliar para mostrar el nombre completo en el DataGrid
        public string NombreCompleto => $"{first_name} {last_name}";

        // Propiedad auxiliar para mostrar el correo en el DataGrid
        public string Correo => email;
        public string TelefonoUser => telefono;
        public string UserNameUser => username;
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
        public string NombrePaciente { get; set; }
        public string FechaEstudio { get; set; }
        public string FechaReporte { get; set; }
        public string DatosChecklist { get; set; }
        public string EstadoAnalisis { get; set; }
        public string ResultadoPrediccion { get; set; } // "Muy probable", "Probable", etc.
        public string Precision { get; set; } // "99.99%"
        public string ImagenUrl { get; set; }
        public Dictionary<string, double> Probabilidades { get; set; }
    }

    public class Alerta
    {
        public string FechaReporte { get; set; }
        public string IDArchivo { get; set; }
        public string NombrePaciente { get; set; }
        public string Prediccion { get; set; }
        public string ColorPrediccion { get; set; } // Para mostrar el color del estado
    }

    public class ResultadoAlertaApi
    {
        public int id { get; set; }
        public string fecha_resultado { get; set; }
        public string prediccion_ia { get; set; }
        public int paciente { get; set; }
    }

    public class PacienteApi
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string apellido_paterno { get; set; }
        public string apellido_materno { get; set; }
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

    public class PredictionResponse
    {
        public string prediccion { get; set; }
        public double precision { get; set; }
        public string imagen_bytes { get; set; }
        public List<double> probabilidades { get; set; }
        public string resumen { get; set; }
        public string estado { get; set; }
    }
}