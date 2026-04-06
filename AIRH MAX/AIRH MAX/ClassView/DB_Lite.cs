using AIRH_MAX.Models;
using Microsoft.Data.Sqlite;
using System.Data;
using System.IO;
using System.IO.Ports;
using static AIRH_MAX.ClassView.Temporizador;
using static AIRH_MAX.ClassView.ViewModel.Libreta;

namespace AIRH_MAX.ClassView
{
    internal class DB_Lite
    {

        public static void InsertarComando(string tabla, string cmd, string acc, string resp)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO {tabla} (Comando, Accion, Respuesta) VALUES ('{cmd}', '{acc}', '{resp}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarGamer(string cmd, string tecla, string tiempo)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Gamer (Letra, Comando, Tiempo) VALUES ('{cmd}', '{tecla}', '{tiempo}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarGamerAuto(string letra, string tiempo)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Autokey (Letra, Tiempo_Accion) VALUES ('{letra}', '{tiempo}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarArduino(string cmd, string acc, string resp, string Puerto, string BaudRate)
        {
            string cmdSpace = cmd.Replace(" ", "_");
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Arduino (Puerto, BaudRate, Comando, Accion, Respuesta) VALUES ('{Puerto}', '{BaudRate}', '{cmdSpace}', '{acc}', '{resp}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarContacto(string nombre, string apellido, string telefono)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Contactos (Nombre, Apellido, Telefono) VALUES ('{nombre}', '{apellido}', '{telefono}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarAgenda(string Evento, string Recordar, string Fecha, string Hora, string Ruta, string Accion)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Agenda (Evento, Recordar, Fecha, Hora, Ruta, Accion) VALUES ('{Evento}', '{Recordar}', '{Fecha}', '{Hora}', '{Ruta}', '{Accion}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarMultimedia(string nombre, string ruta, string tipo)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Multimedia (Nombre, Ruta, Tipo) VALUES ('{nombre}', '{ruta}', '{tipo}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarDiscord(string canal, string webhook)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"INSERT INTO Discord (Canal, Webhook) VALUES ('canal_{canal}', '{webhook}')";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void InsertarTemporizador(string recordatorio, string tiempo, string unidad)
        {
            DateTime CalcularFechaVencimiento(string unidad)
            {
                return unidad.ToLower() switch
                {
                    "segundos" => DateTime.Now.AddSeconds(Convert.ToInt64(tiempo)),
                    "minutos" => DateTime.Now.AddMinutes(Convert.ToInt64(tiempo)),
                    "horas" => DateTime.Now.AddHours(Convert.ToInt64(tiempo)),
                    "dias" => DateTime.Now.AddDays(Convert.ToInt64(tiempo)),
                    _ => throw new ArgumentException($"Unidad de tiempo no válida: {unidad}")
                };
            }

            DateTime dueDate = CalcularFechaVencimiento(unidad);

            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string consulta = "INSERT INTO Temporizador (Recordatorio, Unidad) VALUES (@Recordatorio, @Unidad)";
                    using (SqliteCommand command = new SqliteCommand(consulta, connection))
                    {
                        command.Parameters.AddWithValue("@Recordatorio", recordatorio);
                        command.Parameters.AddWithValue("@Unidad", dueDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al insertar el recordatorio: {ex.Message}");
            }
        }

        public static void Actualizar(string tabla, string cmd, string acc, string resp, string id)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"UPDATE {tabla} SET Comando='{cmd}', Accion='{acc}', Respuesta='{resp}' WHERE Id={id};";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void ActualizarGamer(string cmd, int id, string tiempo)
        {
            string T;
            if (tiempo == string.Empty)
            {
                T = "0";
            }
            else
            {
                T = tiempo;
            }

            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"UPDATE Gamer SET Comando='{cmd}', Tiempo='{T}' WHERE Id={id};";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void ActualizarGamerAuto(string letra, int id, string tiempo)
        {
            string T;
            if (tiempo == string.Empty)
            {
                T = "0";
            }
            else
            {
                T = tiempo;
            }

            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"UPDATE Autokey SET Letra='{letra}', Tiempo_Accion='{T}' WHERE Id={id};";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void ActualizarArduino(string tabla, string cmd, string acc, string resp, string id, string Puerto, string BaudRate)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string consulta = $"UPDATE {tabla} SET Puerto='{Puerto}, BaudRate={BaudRate}, Comando='{cmd}', Accion='{acc}', Respuesta='{resp}' WHERE Id={id};";
            SqliteCommand command = new SqliteCommand(consulta, connection);
            command.ExecuteNonQuery();
            connection.Close();
        }

        public static void Eliminar(string tabla, string id = null)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string consulta = string.IsNullOrEmpty(id)
                    ? $"DELETE FROM {tabla}"
                    : $"DELETE FROM {tabla} WHERE Id = @id";

                using (var command = new SqliteCommand(consulta, connection))
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        command.Parameters.AddWithValue("@id", id);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        public static int GetIdFromTable(string tabla, string letra)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT Id FROM {tabla} WHERE Letra = @letra;";
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@letra", letra);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0);
                        }
                    }
                }
            }

            return 0;
        }

        public static string ConsultarContactos(string datos)
        {
            string Nombre = string.Empty;
            string Apellido = string.Empty;

            char[] separadores = { '_', ' '};
            var partes = datos.Split(separadores, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 3)
            {
                Nombre = partes[1].ToLower();
                Apellido = partes[2].ToLower();
            }
            else
            {
                Nombre = partes[0].ToLower();
                Apellido = partes[1].ToLower();
            }

                try
                {
                    string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();

                        string sql = "SELECT Telefono FROM Contactos WHERE Nombre=@Nombre AND Apellido=@Apellido";
                        using (var command = new SqliteCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Nombre", Nombre);
                            command.Parameters.AddWithValue("@Apellido", Apellido);

                            using (var reader = command.ExecuteReader())
                            {
                                return reader.Read() ? reader.GetString(0) : "NaN";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al consultar contactos: {ex.Message}");
                    return "NaN";
                }
        }

        public static string ConsultarDiscord(string datos)
        {
            string canal = datos.Replace("discord_", "");

            try
            {
                string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Webhook FROM Discord WHERE Canal = @Canal";

                    using (var command = new SqliteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Canal", canal);
                        var result = command.ExecuteScalar(); 
                        return result?.ToString() ?? "NaN";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar contactos: {ex.Message}");
                return "NaN";
            }
        }

        public static bool ConsultaGamer(string letra, string tabla)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string sql = $"SELECT Id FROM {tabla} WHERE Letra = @letra;";
                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@letra", letra);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static string[] ConsultaComandoGamer(string comando)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = $"SELECT Letra, Comando, Tiempo FROM Gamer WHERE Comando='{comando}'";
            SqliteCommand command = new SqliteCommand(sql, connection);
            SqliteDataReader reader = command.ExecuteReader();

            string Comando = string.Empty;
            string Letra = string.Empty;
            string Tiempo = string.Empty;

            while (reader.Read())
            {
                Letra = reader.GetString(0);
                Comando = reader.GetString(1);
                Tiempo = reader.GetString(2);
            }

            string[] arr = { Letra, Comando, Tiempo };
            return arr;
        }

        public static DataTable ObtenerTabla(string tabla)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand command = new SqliteCommand($"SELECT * FROM {tabla}", connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        return dataTable;
                    }
                }
            }
        }

        private static string[] ConsultarElemento(string tabla, string comando)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            string sql = $"SELECT * FROM {tabla} WHERE Comando='{comando}'";
            SqliteCommand command = new SqliteCommand(sql, connection);
            SqliteDataReader reader = command.ExecuteReader();

            string columna1 = string.Empty;
            string columna2 = string.Empty;
            string columna3 = string.Empty;

            while (reader.Read())
            {
                columna1 = reader.GetString(1);
                columna2 = reader.GetString(2);
                columna3 = reader["Respuesta"].ToString();
            }

            string[] Columnas = { columna1, columna2, columna3 };
            connection.Close();

            return Columnas;
        }

        public static string EjecutarComandoPersonal(string comando)
        {
            var tablas = new HashSet<string> { "App", "Arduino", "Carpetas", "Social", "Textos", "Web" };
            var partes = comando.Split('_', 2); 

            if (partes.Length != 2 || !tablas.Contains(partes[0]))
            {
                return string.Empty;
            }

            try
            {
                var segmento = ConsultarElemento(partes[0], partes[1]);
                Engrane.EXE(segmento[1]);
                return segmento[2];
            }
            catch (Exception a)
            {
                Views.MainWindow.NotificacionEvent.Log = a.Message;
            }

            return string.Empty;
        }

        public static Dictionary<string, int> GamerAutokey()
        {
            var dic = new Dictionary<string, int>();
            DataTable datos = ObtenerTabla("Autokey");

            foreach (DataRow item in datos.Rows)
            {
                dic.Add(item.ItemArray[1].ToString(), Convert.ToInt32(item.ItemArray[2]));
            }
            return dic;
        }

        public static void ExportTableToCSV(string tableName, string outputPath)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            // Conexión a la base de datos SQLite
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                // Consulta para leer los datos de la tabla
                string query = "SELECT * FROM " + tableName;

                // Adaptador para leer los datos
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        // Crear un DataTable para almacenar los datos
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        // Exportar los datos a un archivo CSV
                        using (StreamWriter writer = new StreamWriter(outputPath))
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                foreach (DataColumn column in dataTable.Columns)
                                {
                                    writer.Write(row[column].ToString());
                                    if (column != dataTable.Columns[dataTable.Columns.Count - 1])
                                    {
                                        writer.Write(",");
                                    }
                                }
                                writer.WriteLine();
                            }
                        }
                    }
                }
            }
        }

        public static void ImportCSVToTable(string tableName, string csvPath)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            // Conexión a la base de datos SQLite
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Leer los datos del archivo CSV y guardarlos en un DataTable
                DataTable dataTable = CsvToDataTable(csvPath);

                // Insertar los datos en la tabla
                using (SqliteCommand command = new SqliteCommand("INSERT INTO " + tableName + " (Id, Letra, Comando) VALUES (@Id, @Letra, @Comando)", connection))
                {
                    command.Parameters.AddWithValue("@Id", DbType.String);
                    command.Parameters.AddWithValue("@Letra", DbType.String);
                    command.Parameters.AddWithValue("@Comando", DbType.String);

                    foreach (DataRow row in dataTable.Rows)
                    {
                        command.Parameters["@Id"].Value = row["Id"];
                        command.Parameters["@Letra"].Value = row["Letra"];
                        command.Parameters["@Comando"].Value = row["Comando"];
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static DataTable CsvToDataTable(string csvPath)
        {
            DataTable dataTable = new DataTable();

            // Leer las líneas del archivo CSV
            string[] lines = System.IO.File.ReadAllLines(csvPath);

            if (lines.Length > 0)
            {
                // Separar la primera línea (nombres de las columnas)
                string[] headers = lines[0].Split(',');

                // Agregar las columnas a la tabla
                foreach (string header in headers)
                {
                    dataTable.Columns.Add(header.Trim());
                }

                // Agregar las filas a la tabla
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] fields = lines[i].Split(',');
                    DataRow row = dataTable.NewRow();

                    for (int j = 0; j < headers.Length; j++)
                    {
                        row[j] = fields[j].Trim();
                    }

                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        public static List<string> ObtenerRegistros(string tabla, string campo, bool usarNombreApellido = false)
        {
            var resultados = new List<string>();
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                if (usarNombreApellido)
                {
                    string sql = "SELECT Nombre, Apellido FROM Contactos";
                    using (var command = new SqliteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nombre = reader.GetString(0);
                            string apellido = reader.GetString(1);
                            resultados.Add($"{tabla}_{nombre}_{apellido}".ToLower());
                        }
                    }
                }
                else
                {
                    string sql = $"SELECT {campo} FROM {tabla}";
                    using (var command = new SqliteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        int indiceCampo = reader.GetOrdinal(campo);
                        while (reader.Read())
                        {
                            string valor = reader.GetString(indiceCampo);
                            resultados.Add($"{tabla}_{valor}".ToLower());
                        }
                    }
                }
            }

            return resultados;
        }
        public static List<Reminder> ObtenerRecordatorios()
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            List<Reminder> reminders = new List<Reminder>();

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string consulta = "SELECT Id, Recordatorio, Unidad FROM Temporizador";
                    using (SqliteCommand command = new SqliteCommand(consulta, connection))
                    {
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reminders.Add(new Reminder
                                {
                                    Id = Convert.ToString(reader["Id"]),
                                    Message = reader["Recordatorio"].ToString(),
                                    DueDate = DateTime.Parse(reader["Unidad"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Views.MainWindow.NotificacionEvent.Log = $"Error al obtener los recordatorios: {ex.Message}";
            }

            return reminders;
        }
        public static List<Evento> ConsultarAgenda()
        {
            var eventos = new List<Evento>();
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Agenda";
                    using (var command = new SqliteCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                eventos.Add(new Evento
                                {
                                    Id = reader.GetInt32(0),
                                    EventoNombre = reader.GetString(1),
                                    Recordar = reader.GetString(2),
                                    Fecha = reader.GetString(3),
                                    Hora = reader.GetString(4),
                                    Ruta = reader.GetString(5),
                                    Accion = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Views.MainWindow.NotificacionEvent.Log = $"Error al consultar la agenda: {ex.Message}";
            }

            return eventos;
        }

        public static string EjecutarMultimedia(string nombre, string tipo)
        {
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Corregido: usar nombres de parámetros consistentes
                string consulta = "SELECT Ruta FROM Multimedia WHERE Nombre LIKE @nombre AND Tipo = @tipo";

                using (var command = new SqliteCommand(consulta, connection))
                {
                    // Corregido: nombres de parámetros que coinciden con la consulta
                    command.Parameters.AddWithValue("@nombre", $"%{nombre.Replace("multimedia_", "")}%");
                    command.Parameters.AddWithValue("@tipo", tipo);

                    var result = command.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        private static ComandoArduino ConsultarComandoArduino(string comandoBuscado)
        {
            string comandoLimpio = comandoBuscado.Split("_", 2)[1];
            string connectionString = $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";
            var comando = new ComandoArduino();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string sql = $"SELECT puerto, baudrate, comando, accion, respuesta FROM Arduino WHERE comando = @comando";

                using (var cmd = new SqliteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@comando", comandoLimpio);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            comando.Puerto = reader["puerto"].ToString();
                            comando.Baudrate = reader["baudrate"].ToString();
                            comando.Comando = reader["comando"].ToString();
                            comando.Accion = reader["accion"].ToString();
                            comando.Respuesta = reader["respuesta"].ToString();
                            return comando;
                        }
                    }
                }
            }

            return comando;
        }

        public static string EjecutarAccionArduino(string comando)
        {
            if (!comando.Split("_", 2)[0].Equals("arduino"))
            {
                return string.Empty;
            }

            try
            {
                var config = ConsultarComandoArduino(comando);
                if (config.Comando == null)
                {
                    return string.Empty;
                }

                using (var serialPort = new SerialPort())
                {
                    serialPort.PortName = config.Puerto;
                    serialPort.BaudRate = Convert.ToInt16(config.Baudrate);
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Handshake = Handshake.None;
                    serialPort.ReadTimeout = 500;
                    serialPort.WriteTimeout = 500;

                    try
                    {
                        serialPort.Open();
                        serialPort.DiscardOutBuffer();
                        serialPort.WriteLine(config.Accion);
                        serialPort.Close();

                        return config.Respuesta ?? "Comando enviado al Arduino.";
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return config.Respuesta ?? "El puerto está en uso por otra aplicación.";
                    }
                    catch (IOException)
                    {
                        return config.Respuesta ?? "Error de entrada/salida con el puerto serial.";
                    }
                    catch (Exception ex) when (ex is SystemException || ex is InvalidOperationException)
                    {
                        return config.Respuesta ?? "Error al comunicarse con el Arduino.";
                    }
                }
            }
            catch
            {
                return "No se encuentra un Arduino conectado o hay un error en la configuración.";
            }
        }
    }
}