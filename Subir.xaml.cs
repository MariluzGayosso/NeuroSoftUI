using System;
using System.Windows;
using Microsoft.Win32; // Para el diálogo de selección de archivo

namespace NeuroSoft
{
    public partial class Subir : Window
    {
        public Subir()
        {
            InitializeComponent();
        }

        // Eventos del menú lateral
  
        // Evento para el botón "Inicio" en el menú lateral
        private void BtnInicio_Click(object sender, RoutedEventArgs e)
        {
            // Crear una nueva instancia de la ventana Inicio
            Inicio inicioWindow = new Inicio();
            inicioWindow.Show();

            // Cerrar la ventana actual (Subir)
            this.Close();
        }

        // Evento para el botón "Resultados" en el menú lateral
        private void BtnResultados_Click(object sender, RoutedEventArgs e)
        {
            Resultados resultadosWindow = new Resultados();
            resultadosWindow.Show();
            this.Close();
        }

        // Evento para el botón "Alertas" en el menú lateral
        private void BtnAlertas_Click(object sender, RoutedEventArgs e)
        {
            Alertas alertasWindow = new Alertas();
            alertasWindow.Show();
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

        // ============================
        // Eventos de funcionalidad de imágenes
        // ============================

        // Evento para seleccionar una imagen (por ejemplo, para el botón "Subir Imagenes" de la sección de imágenes)
        private void btnSubirImagen_Click(object sender, RoutedEventArgs e)
        {
            // Crear un diálogo de selección de archivo
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen",
                Filter = "Archivos de imagen (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };

            // Mostrar el diálogo y verificar si el usuario seleccionó un archivo
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                // Aquí podrías manejar la imagen seleccionada (por ejemplo, cargarla en un Image control)
                MessageBox.Show($"Imagen seleccionada: {filePath}");
            }
        }

        // Evento para cerrar la ventana de Subir (por ejemplo, un botón "Salir" dentro de la sección de imágenes)
        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
