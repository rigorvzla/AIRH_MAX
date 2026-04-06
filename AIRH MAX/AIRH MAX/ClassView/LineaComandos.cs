using AIRH_MAX.Views;
using System.Windows;

namespace AIRH_MAX.ClassView
{
    internal class LineaComandos
    {
        public static void Scandisk()
        {
            var result = Xceed.Wpf.Toolkit.MessageBox.Show(
                "Se escaneará uno a uno cada disco de almacenamiento, para ser reparado.\n¿Continuar?",
                "AV-AIRH MAX",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;
            ScanDisk scan = new(Engrane.File_Config().Asistente);
            scan.ShowDialog();
        }

        public static void IPReset()
        {
            var result = Xceed.Wpf.Toolkit.MessageBox.Show(
                "Se reiniciara y repararan los adaptadores de red.\n¿Continuar?",
                "AV-AIRH MAX",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            IPClean ip = new(Engrane.File_Config().Asistente);
            ip.ShowDialog();      
        }
    }
}
