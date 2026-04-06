using Microsoft.Data.Sqlite;
using System.Data;

namespace AIRH_MAX.ClassView.DB_Manager
{
    public static class DatabaseManager
    {
        private static string ConnectionString => $"Data Source={Environment.CurrentDirectory}\\AIRH.db;";

        // Método común para obtener el nombre de la tabla desde el enum
        private static string GetTableName(Tablas.Tipo tabla)
        {
            return tabla.ToString();
        }

        // Método común para ejecutar consultas sin retorno
        private static void ExecuteNonQuery(string query, Action<SqliteCommand> parameterSetup = null)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            using (var command = new SqliteCommand(query, connection))
            {
                connection.Open();
                parameterSetup?.Invoke(command);
                command.ExecuteNonQuery();
            }
        }

        public static void InsertarComando(Tablas.Tipo tabla, string cmd, string acc, string resp)
        {
            string tableName = GetTableName(tabla);
            string query = $"INSERT INTO {tableName} (Comando, Accion, Respuesta) VALUES (@cmd, @acc, @resp)";

            ExecuteNonQuery(query, command =>
            {
                command.Parameters.AddWithValue("@cmd", cmd);
                command.Parameters.AddWithValue("@acc", acc);
                command.Parameters.AddWithValue("@resp", resp);
            });
        }

        public static void Actualizar(Tablas.Tipo tabla, string cmd, string acc, string resp, string id)
        {
            string tableName = GetTableName(tabla);
            string query = $"UPDATE {tableName} SET Comando=@cmd, Accion=@acc, Respuesta=@resp WHERE Id=@id";

            ExecuteNonQuery(query, command =>
            {
                command.Parameters.AddWithValue("@cmd", cmd);
                command.Parameters.AddWithValue("@acc", acc);
                command.Parameters.AddWithValue("@resp", resp);
                command.Parameters.AddWithValue("@id", id);
            });
        }

        public static void Eliminar(Tablas.Tipo tabla, string id = null)
        {
            string tableName = GetTableName(tabla);
            string query = string.IsNullOrEmpty(id)
                ? $"DELETE FROM {tableName}"
                : $"DELETE FROM {tableName} WHERE Id = @id";

            ExecuteNonQuery(query, command =>
            {
                if (!string.IsNullOrEmpty(id))
                {
                    command.Parameters.AddWithValue("@id", id);
                }
            });
        }

        public static DataTable ObtenerTabla(Tablas.Tipo tabla)
        {
            string tableName = GetTableName(tabla);
            string query = $"SELECT * FROM {tableName}";

            using (var connection = new SqliteConnection(ConnectionString))
            using (var command = new SqliteCommand(query, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    return dataTable;
                }
            }
        }

        private static string[] ConsultarElemento(Tablas.Tipo tabla, string comando)
        {
            string tableName = GetTableName(tabla);
            string query = $"SELECT * FROM {tableName} WHERE Comando=@comando";

            using (var connection = new SqliteConnection(ConnectionString))
            using (var command = new SqliteCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@comando", comando);

                using (var reader = command.ExecuteReader())
                {
                    string columna1 = string.Empty;
                    string columna2 = string.Empty;
                    string columna3 = string.Empty;

                    if (reader.Read())
                    {
                        columna1 = reader.GetString(1);
                        columna2 = reader.GetString(2);
                        columna3 = reader["Respuesta"].ToString();
                    }

                    return new string[] { columna1, columna2, columna3 };
                }
            }
        }

        // Versión alternativa que devuelve un diccionario o objeto
        public static Dictionary<string, string> ConsultarElementoDiccionario(Tablas.Tipo tabla, string comando)
        {
            string tableName = GetTableName(tabla);
            string query = $"SELECT * FROM {tableName} WHERE Comando=@comando";

            using (var connection = new SqliteConnection(ConnectionString))
            using (var command = new SqliteCommand(query, connection))
            {
                connection.Open();
                command.Parameters.AddWithValue("@comando", comando);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Dictionary<string, string>
                        {
                            ["Comando"] = reader.GetString(1),
                            ["Accion"] = reader.GetString(2),
                            ["Respuesta"] = reader["Respuesta"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public static string EjecutarComandoPersonal(string comando)
        {
            var partes = comando.Split('_', 2);

            if (partes.Length != 2)
            {
                return string.Empty;
            }

            // Intentar convertir el prefijo a enum
            if (!Enum.TryParse<Tablas.Tipo>(partes[0], out Tablas.Tipo tabla))
            {
                return string.Empty;
            }

            try
            {
                var segmento = ConsultarElemento(tabla, partes[1]);
                Engrane.EXE(segmento[1]); // Usa el índice 1 para Accion
                return segmento[2]; // Retorna Respuesta
            }
            catch (Exception a)
            {
                Views.MainWindow.NotificacionEvent.Log = a.Message;
                return string.Empty;
            }
        }
    }
}