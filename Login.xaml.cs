using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace NeuroSoft
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            DatabaseHelper.CrearBaseDeDatos(); // Verificar que la BD exista al iniciar
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor, ingrese usuario y contraseña.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ValidarUsuario(usuario, password))
            {
                MessageBox.Show("Bienvenido a NeuroSoft", "Acceso concedido", MessageBoxButton.OK, MessageBoxImage.Information);
                Inicio inicio = new Inicio();
                inicio.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarUsuario(string usuario, string password)
        {
            using (var conn = new MySqlConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Usuarios WHERE Usuario=@usuario AND Password=SHA2(@password, 256)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@usuario", usuario);
                    cmd.Parameters.AddWithValue("@password", password);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private void TxtUsuario_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsuario.Text == "Ingresa tu nombre de usuario")
            {
                txtUsuario.Text = "";
                txtUsuario.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        private void TxtUsuario_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                txtUsuario.Text = "Ingresa tu nombre de usuario";
                txtUsuario.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);
            }
        }
    }

    public static class DatabaseHelper
    {
        public static string ConnectionString = "Server=127.0.0.1;Database=NeuroSoftDB;Uid=root;Pwd=;SslMode=none;";

        public static void CrearBaseDeDatos()
        {
            try
            {
                // Conexión sin base de datos para crearla
                using (var conn = new MySqlConnection("Server=127.0.0.1;Uid=root;Pwd=;SslMode=none;"))
                {
                    conn.Open();
                    string createDatabaseQuery = "CREATE DATABASE IF NOT EXISTS NeuroSoftDB;";
                    using (var cmd = new MySqlCommand(createDatabaseQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Ahora conectamos con la base de datos
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        CREATE TABLE IF NOT EXISTS Usuarios (
                            ID INT AUTO_INCREMENT PRIMARY KEY,
                            Usuario VARCHAR(50) NOT NULL UNIQUE,
                            Password VARCHAR(64) NOT NULL
                        );

                        INSERT IGNORE INTO Usuarios (Usuario, Password) VALUES ('admin', SHA2('1234', 256));
                    ";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Base de datos configurada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al configurar la base de datos:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
