using System.Windows;

namespace NeuroSoft
{
    public partial class Inicio : Window
    {
        public Inicio()
        {
            InitializeComponent();

            // Iniciar datos de las gráficas
            DatosAlertas = new LiveCharts.ChartValues<int> { 5, 10, 8 };  // Ejemplo de valores para las alertas
            // Configurar los datos de la gráfica de alertas
            IniciarGraficaAlertas();
        }

        // Propiedad de datos para la gráfica de alertas
        public LiveCharts.ChartValues<int> DatosAlertas { get; set; }

        // Método para inicializar la gráfica de alertas
        private void IniciarGraficaAlertas()
        {
            // Configuración de la gráfica (si se necesita más detalle)
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

        // Evento de clic para el botón "Subir Imágenes"
        private void BtnSubirImagenes_Click(object sender, RoutedEventArgs e)
        {
            // Crear una nueva instancia de la ventana Subir
            Subir ventanaSubir = new Subir();

            // Mostrar la ventana de subir imágenes
            ventanaSubir.Show();

            // Cerrar la ventana de inicio (si lo deseas)
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

    }
}
