using Microsoft.Win32;
using System.Diagnostics;
using System.Management;

namespace InfoSystem_v2.Helpers
{
    internal class SystemHelper
    {
        public static string[] DirectX()
        {
            var psi = new ProcessStartInfo();
            if (nint.Size == 4 && Environment.Is64BitOperatingSystem)
            {
                psi.FileName = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    "sysnative\\dxdiag.exe");
            }
            else
            {
                psi.FileName = Path.Combine(
                    Environment.SystemDirectory,
                    "dxdiag.exe");
            }
            string path = Path.GetTempFileName();
            try
            {
                psi.Arguments = "/t " + path;
                using (var prc = Process.Start(psi))
                {
                    prc.WaitForExit();
                    if (prc.ExitCode != 0)
                    {
                        throw new Exception("DXDIAG failed with exit code " + prc.ExitCode.ToString());
                    }
                }
                return File.ReadAllLines(path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        public static string DirectX_Value(string device)
        {
            string value = string.Empty;

            string[] dx = DirectX();
            foreach (var item in dx)
            {
                value = item.Split(':')[0].Trim();

                if (value.Equals(device))
                {
                    value = item.Split(':')[1].Trim();
                    break;
                }
            }

            return value;
        }

        private static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 528040)
                return "4.8 o superior";
            if (releaseKey >= 461808)
                return "4.7.2";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";

            return "No NetFramework 4.5 o version antigua detectada";
        }

        public static string NetFramework()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                }
                else
                {
                    return ".NET Framework Version 4.5 o version antigua detectada.";
                }
            }
        }

        public static string DotNet()
        {
            var process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "--list-sdks";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }

        public static string CaptionSO()
        {
            string OS = string.Empty;
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementObject os in searcher2.Get())
            {
                OS = os["Caption"].ToString();
            }

            return OS;
        }

        public static string ArquitectureSO()
        {
            string OS = string.Empty;
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");

            foreach (ManagementObject os in searcher2.Get())
            {
                OS = os["OSArchitecture"].ToString();
            }

            return OS;
        }

        public static string ModeloWindows()
        {
            RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\OEMInformation");
            string Model = (string)rkey.GetValue("Model");

            try
            {
                if (Model.Contains("DR Lite"))
                {
                    Model = (string)rkey.GetValue("Model");
                }
                else
                {
                    Model = "Windows original o desconocido";
                }
            }
            catch (Exception)
            {
                Model = "Windows original o desconocido";
            }
            return Model;
        }

        public static string CompilacionSO()
        {
            string OS = string.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT BuildNumber FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                OS += os["BuildNumber"];
            }
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
            OS += "." + registryKey.GetValue("UBR").ToString();

            return OS;
        }

        public static string VersionWindows()
        {
            RegistryKey rkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
            string Model = (string)rkey.GetValue("DisplayVersion");

            try
            {
                Model = (string)rkey.GetValue("DisplayVersion");

            }
            catch (Exception)
            {
                Model = "null";
            }
            return Model;
        }
    }
}
