using System.Diagnostics;

namespace AIRH_MAX.ClassView
{
    internal class PlayerSpy
    {
        public static void Player(string input)
        {
            Process proceso = new Process();
            proceso.StartInfo.FileName = Environment.CurrentDirectory + "\\x64\\ffplay.exe";
            proceso.StartInfo.Arguments = $@"-i ""{input}"" -nodisp -autoexit";
            proceso.StartInfo.UseShellExecute = false;
            proceso.StartInfo.CreateNoWindow = true;
            proceso.Start();
            proceso.WaitForExit();
        }
    }
}
