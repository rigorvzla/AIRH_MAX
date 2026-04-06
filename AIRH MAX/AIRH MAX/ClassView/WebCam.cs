using FlashCap;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace AIRH_MAX.ClassView
{
    internal class WebCam
    {
        public static string Name = TakeWebCamName();

        public static async void TomarFoto(Image image)
        {
            try
            {
                string path = $"WebCam_{Directory.GetFiles(Engrane.File_Config().Dir_Pantalla_Capturas).Count()}";
                string ruta = await TakeOneShotToFileAsync(path, default);
                image.Source = new BitmapImage(new Uri(Path.GetFullPath(ruta)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Marshal.GetHRForException(ex);
            }
        }

        public static async Task<string> TomarFotoIntruso()
        {
            string ruta = string.Empty;

            try
            {
                string path = $"Intruso";
                string rut = await TakeOneShotToFileAsync(path, default);
                ruta = rut;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Marshal.GetHRForException(ex);
                ruta = ex.Message;
            }
            return ruta;
        }

        private static string TakeWebCamName()
        {
            string web = string.Empty;

            try
            {
                var devices = new CaptureDevices();
                var descriptor = devices.EnumerateDescriptors().
                    FirstOrDefault();

                if (descriptor == null)
                {
                    return web;
                }

                web = descriptor.Name;
            }
            catch (Exception)
            {
            }


            return web;
        }

        private static async Task<string> TakeOneShotToFileAsync(string fileName, CancellationToken ct)
        {
            ///////////////////////////////////////////////////////////////
            // Initialize and detection capture devices.

            // Step 1: Enumerate capture devices:
            var devices = new CaptureDevices();
            var descriptor = devices.EnumerateDescriptors().
                // You could filter by device type and characteristics.
                //Where(d => d.DeviceType == DeviceTypes.DirectShow).  // Only DirectShow device.
                FirstOrDefault();
            if (descriptor == null)
            {
                Console.WriteLine($"Could not detect any capture interfaces.");
                return string.Empty;
            }

#if false
        // Step 2-1: Request video characteristics strictly:
        // Will raise exception when parameters are not accepted.
        var characteristics = new VideoCharacteristics(
            PixelFormats.JPEG, 1920, 1080, 60);
#else
            // Step 2-2: Or, you could choice from device descriptor:
            var characteristics0 = descriptor.Characteristics.
                //Where(c => c.PixelFormat == PixelFormats.JPEG).  // Only MJPEG characteristics.
                FirstOrDefault(c => c.PixelFormat != PixelFormats.Unknown);
            if (characteristics0 == null)
            {
                Console.WriteLine($"Could not select primary characteristics.");
                return string.Empty;
            }
#endif

            Console.WriteLine($"Selected capture device: {descriptor}, {characteristics0}");

            ///////////////////////////////////////////////////////////////
            // Start capture and get one image.

#if true
            // Step 3: New interface: Simple take one shot.
            var image = await descriptor.TakeOneShotAsync(
                characteristics0, ct);

            Console.WriteLine($"Captured {image.Length} bytes.");
#else
        // Equivalent implementation

        // Step 3-1: Open the capture device with specific characteristics:
        var tcs = new TaskCompletionSource<byte[]>();
        using var captureDevice = await descriptor0.OpenAsync(
            characteristics0,
            bufferScope =>
            {
                ////////////////////////////////////////////////
                // Pixel buffer has arrived.

                // Step 3-2: Copy image data binary:
                var image = bufferScope.Buffer.CopyImage();

                Console.WriteLine($"Captured {image.Length} bytes.");

                // Step 3-3: Relay to outside continuation.
                tcs.TrySetResult(image);

                // If you output to each files from continuous image data,
                // it would be easier to output directly to file here.
                // In that case, use:
                // * `isScattering` argument to true.
                // * `maxQueuingFrames` argument.
                // * `bufferScope.ReleaseNow()` method.
                // and be careful not to cause frame dropping.
            },
            ct);

        // Step 4: Start capturing:
        await captureDevice.StartAsync(ct);

        Console.WriteLine($"Device opened.");

        // Step 5: Waiting to continue:
        var image = await tcs.Task;

        // Step 6: Stop capturing:
        await captureDevice.StopAsync(ct);

        Console.WriteLine($"Device stopped.");
#endif

            ///////////////////////////////////////////////////////////////
            // Save image data to file.
            // Step 7: Construct storing file name:
            var extension = characteristics0.PixelFormat switch
            {
                PixelFormats.JPEG => ".jpg",
                PixelFormats.PNG => ".png",   // (Very rare device, I dont know)
                _ => ".bmp",
            };
            var path = Engrane.File_Config().Dir_Pantalla_Capturas + $"\\{fileName}{extension}";

            // Step 8: Write to the file:
            using var fs = new FileStream(
                path,
                FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,
                65536, true);
            await fs.WriteAsync(image, 0, image.Length, ct);
            await fs.FlushAsync(ct);

            Console.WriteLine($"The image wrote to file {path}.");

            return path;
        }
    }
}
