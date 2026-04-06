namespace Vosk_STT
{
    public class AdvancedNoiseReducer : IDisposable
    {
        private readonly int _sampleRate;
        private float[] _noiseFloor;
        private int _framesProcessed = 0;
        private readonly Queue<float[]> _noiseSamples = new Queue<float[]>();

        public AdvancedNoiseReducer(int sampleRate = 16000) => _sampleRate = sampleRate;

        public float[] ApplyAdaptiveNoiseReduction(float[] audioData)
        {
            if (audioData.Length == 0) return audioData;
            float[] processed = (float[])audioData.Clone();
            processed = ApplyAdaptiveNoiseGate(processed);
            processed = ApplySpectralNoiseReduction(processed);
            _framesProcessed++;
            return processed;
        }

        public float[] ApplyAggressiveNoiseReduction(float[] audioData)
        {
            if (audioData.Length == 0) return audioData;
            float[] processed = (float[])audioData.Clone();
            float rms = CalculateRMS(audioData);
            if (rms < 0.01f)
                for (int i = 0; i < processed.Length; i++)
                    processed[i] *= 0.05f;
            return processed;
        }

        public float[] ApplyDCBiasRemoval(float[] audioData)
        {
            if (audioData.Length == 0) return audioData;
            float dcBias = audioData.Average();
            float[] corrected = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
                corrected[i] = audioData[i] - dcBias;
            return corrected;
        }

        public float[] ApplyHighPassFilter(float[] audioData, float cutoffFreq)
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

        public float[] ApplySoftClipping(float[] audioData)
        {
            float[] clipped = new float[audioData.Length];
            float threshold = 0.9f;
            for (int i = 0; i < audioData.Length; i++)
            {
                float sample = audioData[i];
                if (sample > threshold)
                    sample = threshold + (sample - threshold) / (1 + Math.Abs(sample - threshold));
                else if (sample < -threshold)
                    sample = -threshold + (sample + threshold) / (1 + Math.Abs(sample + threshold));
                clipped[i] = sample;
            }
            return clipped;
        }

        private float[] ApplyAdaptiveNoiseGate(float[] audioData)
        {
            float rms = CalculateRMS(audioData);
            float[] gated = new float[audioData.Length];
            float threshold = 0.015f;

            if (_framesProcessed < 30) UpdateNoiseFloor(audioData, rms);
            if (_noiseFloor != null && _framesProcessed > 30)
                threshold = Math.Max(0.015f, CalculateRMS(_noiseFloor) * 1.8f);

            float gain = rms >= threshold * 2.0f ? 1.0f :
                        rms <= threshold ? 0.1f :
                        0.1f + (0.9f * ((rms - threshold) / threshold));

            for (int i = 0; i < audioData.Length; i++)
                gated[i] = audioData[i] * gain;
            return gated;
        }

        private float[] ApplySpectralNoiseReduction(float[] audioData)
        {
            int frameSize = 512, hopSize = 256;
            if (audioData.Length < frameSize) return audioData;

            float[] processed = new float[audioData.Length];
            int processedSamples = 0;

            while (processedSamples + frameSize <= audioData.Length)
            {
                float[] frame = new float[frameSize];
                Array.Copy(audioData, processedSamples, frame, 0, frameSize);

                // Aplicar ventana Hann
                for (int i = 0; i < frameSize; i++)
                {
                    float window = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (frameSize - 1)));
                    frame[i] *= window;
                }

                // Reducción espectral
                for (int i = 0; i < frameSize; i++)
                {
                    float magnitude = Math.Abs(frame[i]);
                    if (magnitude < 0.01f) frame[i] *= 0.1f;
                    else if (magnitude < 0.05f) frame[i] *= 0.4f;
                }

                Array.Copy(frame, 0, processed, processedSamples, frameSize);
                processedSamples += hopSize;
            }

            if (processedSamples < audioData.Length)
                Array.Copy(audioData, processedSamples, processed, processedSamples, audioData.Length - processedSamples);

            return processed;
        }

        private void UpdateNoiseFloor(float[] audioData, float currentRms)
        {
            if (currentRms < 0.02f)
            {
                _noiseSamples.Enqueue((float[])audioData.Clone());
                if (_noiseSamples.Count > 8) _noiseSamples.Dequeue();
                if (_noiseSamples.Count >= 4)
                {
                    int length = audioData.Length;
                    _noiseFloor = new float[length];
                    foreach (var sample in _noiseSamples)
                        for (int i = 0; i < length; i++)
                            _noiseFloor[i] += sample[i] / _noiseSamples.Count;
                }
            }
        }

        private float CalculateRMS(float[] audioData)
        {
            if (audioData.Length == 0) return 0;
            double sum = 0;
            foreach (float sample in audioData) sum += sample * sample;
            return (float)Math.Sqrt(sum / audioData.Length);
        }

        public void CalibrateNoiseFloor()
        {
            _noiseSamples.Clear();
            _framesProcessed = 0;
            _noiseFloor = null;
        }

        public void Dispose()
        {
            _noiseSamples.Clear();
            _noiseFloor = null;
        }
    }
}
