using AIRH_MAX.ClassView;
using System.IO;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace AIRH_MAX.ControlUsuario
{
    public partial class Control_MenuVideos : UserControl
    {
        List<string> Videos = new List<string>();

        public Control_MenuVideos()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Opacity = Engrane.File_Config().Opacidad;
            Formato();
            txtVideoCount.Text = Videos.Count.ToString() + " videos";
        }

        private void Formato()
        {
            DirectoryInfo di = new DirectoryInfo(Engrane.File_Config().Dir_Videos);
            var format = ClassView.List.FormatList.videoExtensions;

            foreach (var item in format)
            {
                var files = di.GetFiles("*" + item, SearchOption.AllDirectories);
                foreach (var fi in files)
                {
                    string datos = $@"Archivo: {System.IO.Path.GetFileNameWithoutExtension(fi.Name)}
Ruta: {fi.FullName}";
                    var ViewModel = new ClassView.ViewModel.Chat_Item();
                    ViewModel.Nombre_IA = Engrane.File_Config().Asistente;
                    ViewModel.Tiempo = DateTime.Now.ToShortTimeString();
                    ViewModel.Mensaje = datos;
                    ViewModel.ImageSource = "/PNG/AIRH.png";
                    TextBoxVisual.Items.Add(ViewModel);
                    Videos.Add(fi.FullName);
                }
            }
        }

        private void DgVideos_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TextBoxVisual.SelectedIndex != -1 && File.Exists(Videos[TextBoxVisual.SelectedIndex]))
            {
                Engrane.EXE(Videos[TextBoxVisual.SelectedIndex]);
            }
        }
    }
}
