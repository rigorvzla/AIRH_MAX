using NAudio.CoreAudioApi;

namespace AIRH_MAX.ClassView
{
    public class AudioManager
    {
        private static readonly MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();
        private static readonly MMDevice _defaultPlaybackDevice = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        public static void VolumenPorcentual(float volumen)
        {
            if (volumen < 0 || volumen > 100)
                throw new ArgumentOutOfRangeException(nameof(volumen), "El volumen debe estar entre 0 y 100.");

            _defaultPlaybackDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volumen / 100f;
        }
    }
}
