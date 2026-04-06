using AIRH_MAX.ClassView;
using System.Windows;
using System.Windows.Input;
using Rectangle = System.Drawing.Rectangle;

namespace AIRH_MAX.Popups
{
    public partial class ClickerPosition : Window
    {
        public ClickerPosition()
        {
            InitializeComponent();
            InitializeWindowSize();
        }

        private void InitializeWindowSize()
        {
            Rectangle desktopBounds = Screen.PrimaryScreen.Bounds;
            this.Width = desktopBounds.Width;
            this.Height = desktopBounds.Height;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Engrane.PosX = Convert.ToInt32(e.GetPosition(this).X);
            Engrane.PosY = Convert.ToInt32(e.GetPosition(this).Y);
            Close();
        }
    }
}
