using AIRH_MAX.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace AIRH_MAX.ClassView
{
    class Support
    {          
        /// <summary> <c></c> Devuelve la ruta del archivo seleccionado</summary>  
        /// <param name="Directorio_Inicial">Selecciona el directorio inicial de la busqueda</param>
        /// <param name="Titulo">Escriba el titulo del selector de archivo</param>
        /// <param name="DefaultExt">Seleccione las extenciones a mostrar Ej.: "txt files (*.txt)|*.txt|All files (*.*)|*.*" (Opcional)</param>
        /// <param name="Ext">Establece la cantidad de extenciones seleccionadas</param>
        public static string BuscarArchivo(string Directorio_Inicial = null, string DefaultExt = "All files(*.*)|*.*", int Ext = 1)
        {
            OpenFileDialog ofd = new()
            {
                InitialDirectory = Directorio_Inicial,
                Title = "AV-AIRH MAX",
                Filter = DefaultExt,
                FilterIndex = Ext,
                Multiselect = false
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
               return ofd.FileName;
            }
            return string.Empty;
        }

        public static string BuscarCarpeta()
        {
            FolderBrowserDialog fbd = new()
            {
                RootFolder = Environment.SpecialFolder.MyComputer,
                Description = "AV-AIRH MAX",
                ShowNewFolderButton = true
            };


            if (fbd.ShowDialog() == DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            return string.Empty;
        }

        /// <summary> <c></c> Deja escribir solo numeros y borrar, evento PreviewKeyDown </summary>  
        public static void SoloNumeros(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int key = (int)e.Key;

            e.Handled = !(key >= 34 && key <= 43 ||
                          key >= 74 && key <= 83 ||
            key == 2);
        }

        public static string NormalizeFileSize(long fileSize)
        {
            string[] units = ["B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];
            double size = fileSize;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {units[unit]}";
        }

        public static void TempFilesConvert(string contenido)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "\\Temp\\"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\Temp\\");
                File.WriteAllText(Environment.CurrentDirectory + "\\Temp\\MaxMass.txt", contenido);
            }
        }

        public static string ValidateIconName(string iconName)
        {
            Regex illegalInFileName = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);
            string TituloLimpio = illegalInFileName.Replace(iconName, "");
            return TituloLimpio;
        }

        public static List<string> FormatDevicesList(List<NetworkDevice> devices)
        {
            var result = new List<string>();

            if (devices == null || devices.Count == 0)
            {
                result.Add("No se encontraron dispositivos");
                return result;
            }

            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];

                // UNA SOLA LÍNEA con toda la información del dispositivo
                string deviceLine = $"Device {i + 1}\nIP: {device.ip}\nMAC: {device.mac}\nType: {device.type}\nVendor: {device.vendor}";
                result.Add(deviceLine);
            }

            return result;
        }
    }
}
