namespace Vosk_STT
{
    public class BreathAndPopRemover
    {
        private readonly int _sampleRate;
        private float[] _lastFrame = new float[0];

        public BreathAndPopRemover(int sampleRate = 16000) => _sampleRate = sampleRate;

        public float[] RemoveBreathAndPops(float[] audioData)
        {
            if (audioData.Length == 0) return audioData;

            // Detectar respiraciones
            float highFreqEnergy = CalculateHighFrequencyEnergy(audioData);
            float totalEnergy = CalculateRMS(audioData);

            if (highFreqEnergy > totalEnergy * 2.5f && totalEnergy < 0.008f)
            {
                float[] attenuated = new float[audioData.Length];
                for (int i = 0; i < audioData.Length; i++)
                    attenuated[i] = audioData[i] * 0.4f;
                return attenuated;
            }

            // Aplicar filtro de mediana para pops
            float[] processed = ApplyMedianFilter(audioData, 3);

            // Crossfade entre frames
            if (_lastFrame.Length > 0)
                processed = ApplyCrossfade(_lastFrame, processed, 8);

            _lastFrame = (float[])audioData.Clone();
            return processed;
        }

        private float CalculateHighFrequencyEnergy(float[] audioData)
        {
            var highPass = ApplySimpleHighPass(audioData, 800f);
            return CalculateRMS(highPass);
        }

        private float[] ApplySimpleHighPass(float[] audioData, float cutoffFreq)
        {
            if (audioData.Length < 2) return audioData;
            float[] filtered = new float[audioData.Length];
            float dt = 1.0f / _sampleRate;
            float rc = 1.0f / (2.0f * (float)Math.PI * cutoffFreq);
            float alpha = rc / (rc + dt);
            filtered[0] = audioData[0];
            for (int i = 1; i < audioData.Length; i++)
                filtered[i] = alpha * (filtered[i - 1] + audioData[i] - audioData[i - 1]);
            return filtered;
        }

        private float[] ApplyMedianFilter(float[] data, int windowSize)
        {
            float[] result = new float[data.Length];
            int halfWindow = windowSize / 2;
            for (int i = 0; i < data.Length; i++)
            {
                var window = new List<float>();
                for (int j = Math.Max(0, i - halfWindow); j <= Math.Min(data.Length - 1, i + halfWindow); j++)
                    window.Add(data[j]);
                window.Sort();
                result[i] = window[window.Count / 2];
            }
            return result;
        }

        private float[] ApplyCrossfade(float[] previous, float[] current, int fadeSamples)
        {
            float[] result = (float[])current.Clone();
            for (int i = 0; i < Math.Min(fadeSamples, previous.Length); i++)
            {
                float fadeFactor = (float)i / fadeSamples;
                int prevIndex = previous.Length - fadeSamples + i;
                result[i] = (previous[prevIndex] * (1 - fadeFactor)) + (current[i] * fadeFactor);
            }
            return result;
        }

        private float CalculateRMS(float[] audioData)
        {
            if (audioData.Length == 0) return 0;
            double sum = 0;
            foreach (float sample in audioData) sum += sample * sample;
            return (float)Math.Sqrt(sum / audioData.Length);
        }
    }
}
