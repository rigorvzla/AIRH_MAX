using System.Drawing.Imaging;
using System.IO;

namespace AIRH_MAX.ClassView
{
    internal class Screenshots
    {
        public static string Screen_Primary()
        {
            var screenRect = Screen.PrimaryScreen.Bounds;
            using var screenshot = new Bitmap(screenRect.Width, screenRect.Height);
            using var g = Graphics.FromImage(screenshot);
            g.CopyFromScreen(screenRect.Location, Point.Empty, screenRect.Size);

            string destinoCaptura = Engrane.File_Config().Dir_Pantalla_Capturas;
            Directory.CreateDirectory(destinoCaptura); 

            string captura = Path.Combine(destinoCaptura, $"Captura_{Directory.GetFiles(destinoCaptura).Length}.png");
            screenshot.Save(captura, ImageFormat.Png);
            return Path.GetFileName(captura);
        }

        public static string[] Screen_All()
        {
            string path = Engrane.File_Config().Dir_Pantalla_Capturas;
            Directory.CreateDirectory(path); 

            return Screen.AllScreens.Select((screen, index) =>
            {
                Rectangle screenRect = screen.Bounds;
                using var screenshot = new Bitmap(screenRect.Width, screenRect.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(screenRect.Location, Point.Empty, screenRect.Size);
                }

                string fileName = $"Monitor_{index + 1}_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.png";
                string filePath = Path.Combine(path, fileName);

                screenshot.Save(filePath, ImageFormat.Png);
                return filePath;
            }).ToArray();
        }
    }
}
