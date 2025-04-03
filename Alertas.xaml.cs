using System;
using System.Windows;
using Microsoft.Win32; // Para el diálogo de selección de archivo
using NeuroSoft.Models;

namespace NeuroSoft
{
    /// <summary>
    /// Lógica de interacción para Alertas.xaml
    /// </summary>
    public partial class Alertas : Window
    {
        private UserData CurrentUser { get; set; }
        public Alertas()
        {
            InitializeComponent();
            LoadUserData();
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
            //Resultados resultadosWindow = new Resultados();
            //resultadosWindow.Show();
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
