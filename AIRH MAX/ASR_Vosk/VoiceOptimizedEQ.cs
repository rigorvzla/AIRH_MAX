namespace Vosk_STT
{
    public class VoiceOptimizedEQ
    {
        private readonly BiQuadFilter _lowShelf, _highShelf, _presencePeak;
        public VoiceOptimizedEQ(int sampleRate = 16000)
        {
            _lowShelf = BiQuadFilter.LowShelfFilter(sampleRate, 120f, 0.8f, -10f);
            _highShelf = BiQuadFilter.HighShelfFilter(sampleRate, 3500f, 0.8f, 4f);
            _presencePeak = BiQuadFilter.PeakFilter(sampleRate, 1800f, 1.2f, 3f);
        }

        public float[] ApplyVoiceEQ(float[] audioData)
        {
            float[] processed = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                float sample = audioData[i];
                sample = _lowShelf.Transform(sample);
                sample = _presencePeak.Transform(sample);
                sample = _highShelf.Transform(sample);
                processed[i] = sample;
            }
            return processed;
        }
    }
}
