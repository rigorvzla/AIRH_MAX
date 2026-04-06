namespace Vosk_STT
{
    // ============================================================================
    // FILTROS BIQUAD (necesarios para EQ y compresión)
    // ============================================================================
    public class BiQuadFilter
    {
        private float _a0, _a1, _a2, _b1, _b2;
        private float _prevInput1, _prevInput2, _prevOutput1, _prevOutput2;

        public static BiQuadFilter LowPassFilter(float sampleRate, float cutoffFreq, float q = 1.0f)
        {
            var filter = new BiQuadFilter();
            filter.SetLowPass(sampleRate, cutoffFreq, q);
            return filter;
        }

        public static BiQuadFilter HighPassFilter(float sampleRate, float cutoffFreq, float q = 1.0f)
        {
            var filter = new BiQuadFilter();
            filter.SetHighPass(sampleRate, cutoffFreq, q);
            return filter;
        }

        public static BiQuadFilter LowShelfFilter(float sampleRate, float cutoffFreq, float q, float gainDb)
        {
            var filter = new BiQuadFilter();
            filter.SetLowShelf(sampleRate, cutoffFreq, q, gainDb);
            return filter;
        }

        public static BiQuadFilter HighShelfFilter(float sampleRate, float cutoffFreq, float q, float gainDb)
        {
            var filter = new BiQuadFilter();
            filter.SetHighShelf(sampleRate, cutoffFreq, q, gainDb);
            return filter;
        }

        public static BiQuadFilter PeakFilter(float sampleRate, float centerFreq, float q, float gainDb)
        {
            var filter = new BiQuadFilter();
            filter.SetPeak(sampleRate, centerFreq, q, gainDb);
            return filter;
        }

        public float Transform(float input)
        {
            float output = _a0 * input + _a1 * _prevInput1 + _a2 * _prevInput2 - _b1 * _prevOutput1 - _b2 * _prevOutput2;
            _prevInput2 = _prevInput1;
            _prevInput1 = input;
            _prevOutput2 = _prevOutput1;
            _prevOutput1 = output;
            return output;
        }

        private void SetLowPass(float sampleRate, float cutoffFreq, float q)
        {
            float w0 = 2 * (float)Math.PI * cutoffFreq / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2 * q);
            float cosw0 = (float)Math.Cos(w0);
            _a0 = (1 - cosw0) / 2;
            _a1 = 1 - cosw0;
            _a2 = (1 - cosw0) / 2;
            _b1 = -2 * cosw0;
            _b2 = 1 - alpha;
            Normalize();
        }

        private void SetHighPass(float sampleRate, float cutoffFreq, float q)
        {
            float w0 = 2 * (float)Math.PI * cutoffFreq / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2 * q);
            float cosw0 = (float)Math.Cos(w0);
            _a0 = (1 + cosw0) / 2;
            _a1 = -(1 + cosw0);
            _a2 = (1 + cosw0) / 2;
            _b1 = -2 * cosw0;
            _b2 = 1 - alpha;
            Normalize();
        }

        private void SetLowShelf(float sampleRate, float cutoffFreq, float q, float gainDb)
        {
            float a = (float)Math.Pow(10, gainDb / 40);
            float w0 = 2 * (float)Math.PI * cutoffFreq / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2 * q);
            float cosw0 = (float)Math.Cos(w0);
            _a0 = a * ((a + 1) - (a - 1) * cosw0 + 2 * (float)Math.Sqrt(a) * alpha);
            _a1 = 2 * a * ((a - 1) - (a + 1) * cosw0);
            _a2 = a * ((a + 1) - (a - 1) * cosw0 - 2 * (float)Math.Sqrt(a) * alpha);
            _b1 = -2 * ((a - 1) + (a + 1) * cosw0);
            _b2 = (a + 1) - (a - 1) * cosw0 - 2 * (float)Math.Sqrt(a) * alpha;
            Normalize();
        }

        private void SetHighShelf(float sampleRate, float cutoffFreq, float q, float gainDb)
        {
            float a = (float)Math.Pow(10, gainDb / 40);
            float w0 = 2 * (float)Math.PI * cutoffFreq / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2 * q);
            float cosw0 = (float)Math.Cos(w0);
            _a0 = a * ((a + 1) + (a - 1) * cosw0 + 2 * (float)Math.Sqrt(a) * alpha);
            _a1 = -2 * a * ((a - 1) + (a + 1) * cosw0);
            _a2 = a * ((a + 1) + (a - 1) * cosw0 - 2 * (float)Math.Sqrt(a) * alpha);
            _b1 = 2 * ((a - 1) - (a + 1) * cosw0);
            _b2 = (a + 1) - (a - 1) * cosw0 - 2 * (float)Math.Sqrt(a) * alpha;
            Normalize();
        }

        private void SetPeak(float sampleRate, float centerFreq, float q, float gainDb)
        {
            float a = (float)Math.Pow(10, gainDb / 40);
            float w0 = 2 * (float)Math.PI * centerFreq / sampleRate;
            float alpha = (float)Math.Sin(w0) / (2 * q);
            _a0 = 1 + alpha * a;
            _a1 = -2 * (float)Math.Cos(w0);
            _a2 = 1 - alpha * a;
            _b1 = -2 * (float)Math.Cos(w0);
            _b2 = 1 - alpha / a;
            Normalize();
        }

        private void Normalize()
        {
            float norm = 1.0f / _a0;
            _a0 *= norm;
            _a1 *= norm;
            _a2 *= norm;
            _b1 *= norm;
            _b2 *= norm;
        }
    }
}
