using NAudio.Wave;

namespace Vosk_STT.Models
{
    public class MicrophoneStats
    {
        public bool IsRecording { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public WaveFormat WaveFormat { get; set; }

        public override string ToString()
        {
            if (WaveFormat == null)
            {
                return $"Estado: {(IsRecording ? "Grabando" : "Detenido")} | " +
                       $"Dispositivo: {DeviceName} | " +
                       $"Formato: No disponible";
            }

            return $"Estado: {(IsRecording ? "Grabando" : "Detenido")} | " +
                   $"Dispositivo: {DeviceName} | " +
                   $"Formato: {WaveFormat.SampleRate}Hz, {WaveFormat.BitsPerSample}bit, {WaveFormat.Channels} canal(es)";
        }

        // Método adicional para obtener información formateada
        public string GetFormatInfo()
        {
            if (WaveFormat == null) return "Formato no disponible";

            return $"{WaveFormat.SampleRate}Hz, {WaveFormat.BitsPerSample}bit, {WaveFormat.Channels} canal(es)";
        }

        // Método para verificar compatibilidad con Vosk
        public bool IsVoskCompatible()
        {
            if (WaveFormat == null) return false;

            return WaveFormat.SampleRate == 16000 &&
                   WaveFormat.BitsPerSample == 16 &&
                   WaveFormat.Channels == 1;
        }

        // Método para obtener recomendaciones de configuración
        public string GetConfigurationRecommendation()
        {
            if (WaveFormat == null) return "No se puede determinar la configuración";

            var recommendations = new List<string>();

            if (WaveFormat.SampleRate != 16000)
                recommendations.Add($"Cambiar sample rate a 16000Hz (actual: {WaveFormat.SampleRate}Hz)");

            if (WaveFormat.BitsPerSample != 16)
                recommendations.Add($"Cambiar bits a 16 (actual: {WaveFormat.BitsPerSample}bit)");

            if (WaveFormat.Channels != 1)
                recommendations.Add($"Cambiar a mono (actual: {WaveFormat.Channels} canales)");

            return recommendations.Count > 0 ?
                string.Join("; ", recommendations) :
                "Configuración óptima para Vosk";
        }
    }
}
