using AIRH_MAX.ClassView;
using System.Windows;

namespace AIRH_MAX.Popups
{
    public partial class WizardStart : Window
    {
        int pag;

        public WizardStart()
        {
            InitializeComponent();
            wiz.IsEnabled = true;
        }

        private void Wizard_Finish(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            Engrane.Wizard.Add(txtNombreUsuario.Text);
            Engrane.Wizard.Add(txtNombreIA.Text);
            Engrane.Wizard.Add(txtDespedida.Text);
        }

        private void Wizard_Cancel(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Wizard_Next(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            if (pag == 0)
            {
                pag++;
            }
            else if (pag == 1)
            {
                if (string.IsNullOrEmpty(txtNombreUsuario.Text) || string.IsNullOrEmpty(txtNombreIA.Text) || string.IsNullOrEmpty(txtDespedida.Text))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("Debes asignar los valores requeridos.", "AV-AIRH MAX", MessageBoxButton.OK, MessageBoxImage.Warning);
                    e.Cancel = true;
                }
            }
        }

        private void Wizard_Previous(object sender, Xceed.Wpf.Toolkit.Core.CancelRoutedEventArgs e)
        {
            pag--;
        }
    }
}
