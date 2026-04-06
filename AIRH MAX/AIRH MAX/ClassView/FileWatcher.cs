using AIRH_MAX.ClassView.ViewModel;
using AIRH_MAX.Properties;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace AIRH_MAX.ClassView
{
    internal class FileWatcher
    {
        static FileSystemWatcher fs;

        private static void FileSearcher(string ruta, bool creado, bool cambiado, bool renombrado, bool eliminado, bool SubdirectorioWatcher = false, string filtro = "*.*")
        {
            if (!Directory.Exists(ruta))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Directorio no encontrado: " + ruta, "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            fs = new FileSystemWatcher(ruta);
            fs.EnableRaisingEvents = true;
            fs.IncludeSubdirectories = SubdirectorioWatcher;
            fs.Filter = filtro;

            if (creado.Equals(true))
            {
                fs.Created += new FileSystemEventHandler(FileCheked);
            }
            if (cambiado.Equals(true))
            {
                fs.Changed += new FileSystemEventHandler(FileChange);
            }
            if (eliminado.Equals(true))
            {
                fs.Deleted += new FileSystemEventHandler(FileDeleted);
            }
            if (renombrado.Equals(true))
            {
                fs.Renamed += new RenamedEventHandler(fs_Renamed);
            }

            void fs_Renamed(object sender, RenamedEventArgs e)
            {
                Engrane.ni.ShowBalloonTip(10000, "Se ha renombrado un archivo en:", Path.GetDirectoryName(e.FullPath), ToolTipIcon.None);
                return;
            }

            void FileCheked(object sender, FileSystemEventArgs e)
            {
                Engrane.ni.ShowBalloonTip(10000, "Se ha creado un archivo en:", Path.GetDirectoryName(e.FullPath), ToolTipIcon.None);
                return;
            }

            void FileChange(object sender, FileSystemEventArgs e)
            {
                Engrane.ni.ShowBalloonTip(10000, "Se ha modificado un archivo en:", Path.GetDirectoryName(e.FullPath), ToolTipIcon.None);
            }

            void FileDeleted(object sender, FileSystemEventArgs e)
            {
                Engrane.ni.ShowBalloonTip(10000, "Se ha eliminado un archivo en:", Path.GetDirectoryName(e.FullPath), ToolTipIcon.None);
                return;
            }
        }

        public static void MonitorFile()
        {
            foreach (var item in Settings.Default.MonitorFile)
            {
                if (!item.Equals(string.Empty))
                {
                    FileWatch f = JsonConvert.DeserializeObject<FileWatch>(item);
                    FileSearcher(f.Ruta, f.Creado, f.Creado, f.Renombrado, f.Eliminado);
                }

            }
        }
    }
}
