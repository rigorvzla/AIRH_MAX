namespace Vosk_STT.Models
{
    public class MicrophoneInfo
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public int Channels { get; set; }
        public bool IsDefault { get; set; }

        public override string ToString()
        {
            var defaultMarker = IsDefault ? " (Predeterminado)" : "";
            return $"[{DeviceId}] {Name} - {Channels} canal(es){defaultMarker}";
        }
    }
}
