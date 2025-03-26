using System;
using System.Windows;
using Microsoft.Win32; // Para el diálogo de selección de archivo

namespace NeuroSoft
{
    public partial class Resultados : Window
    {
        public Resultados()
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

        // Evento para el botón "Alertas" en el menú lateral
        private void BtnAlertas_Click(object sender, RoutedEventArgs e)
        {
            // Crear una nueva instancia de la ventana Alertas
            Alertas alertasWindow = new Alertas();
            alertasWindow.Show();

            // Cerrar la ventana actual (Resultados)
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
