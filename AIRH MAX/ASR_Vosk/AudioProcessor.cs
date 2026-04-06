using System.Diagnostics;

namespace Vosk_STT
{
    public class AudioProcessor : IDisposable
    {
        private readonly int _sampleRate = 16000;
        private readonly int _bitsPerSample = 16;
        private readonly int _channels = 1;
        private float[] _noiseProfile;
        private bool _noiseProfileCaptured = false;
        private readonly object _processingLock = new object();

        public AudioProcessor()
        {
            Debug.WriteLine("🎧 Procesador de audio inicializado - Supresión de ruido activa");
        }

        public byte[] ProcessAudio(byte[] audioData, int bytesRecorded)
        {
            lock (_processingLock)
            {
                try
                {
                    // Si el audio es muy corto, devolver sin procesar
                    if (bytesRecorded < 100)
                        return audioData;

                    // Convertir byte[] a float[]
                    float[] audioFloat = ConvertByteToFloat(audioData, bytesRecorded);

                    // Aplicar filtro paso alto para eliminar ruido de baja frecuencia
                    audioFloat = ApplyHighPassFilter(audioFloat, 100); // 100 Hz cutoff

                    // Aplicar compuerta de noise gate
                    audioFloat = ApplyNoiseGate(audioFloat, 0.015f); // Threshold del 1.5%

                    // Aplicar reducción de ruido espectral simple
                    audioFloat = ApplySpectralNoiseReduction(audioFloat);

                    // Convertir de vuelta a byte[]
                    return ConvertFloatToByte(audioFloat);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ Error procesando audio: {ex.Message}");
                    return audioData; // Devolver original si hay error
                }
            }
        }

        private float[] ConvertByteToFloat(byte[] audioData, int bytesRecorded)
        {
            int sampleCount = bytesRecorded / 2; // 16-bit = 2 bytes por sample
            float[] floatData = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)((audioData[i * 2 + 1] << 8) | audioData[i * 2]);
                floatData[i] = sample / 32768.0f; // Normalizar a [-1, 1]
            }

            return floatData;
        }

        private byte[] ConvertFloatToByte(float[] floatData)
        {
            byte[] byteData = new byte[floatData.Length * 2];

            for (int i = 0; i < floatData.Length; i++)
            {
                // Asegurar que esté en el rango [-1, 1]
                float sample = Math.Clamp(floatData[i], -1.0f, 1.0f);
                short intSample = (short)(sample * 32767);

                byteData[i * 2] = (byte)(intSample & 0xFF);
                byteData[i * 2 + 1] = (byte)((intSample >> 8) & 0xFF);
            }

            return byteData;
        }

        private float[] ApplyHighPassFilter(float[] audioData, float cutoffFreq)
        {
            // Filtro paso alto simple (RC high-pass)
            float[] filtered = new float[audioData.Length];
            if (audioData.Length == 0) return filtered;

            float dt = 1.0f / _sampleRate;
            float rc = 1.0f / (2.0f * (float)Math.PI * cutoffFreq);
            float alpha = rc / (rc + dt);

            float prevInput = audioData[0];
            float prevOutput = audioData[0];

            for (int i = 0; i < audioData.Length; i++)
            {
                filtered[i] = alpha * (prevOutput + audioData[i] - prevInput);
                prevInput = audioData[i];
                prevOutput = filtered[i];
            }

            return filtered;
        }

        private float[] ApplyNoiseGate(float[] audioData, float threshold)
        {
            float[] gated = new float[audioData.Length];

            // Calcular energía RMS del frame
            float rms = CalculateRMS(audioData);

            // Aplicar noise gate suave (no corte abrupto)
            float gain = 1.0f;
            if (rms < threshold)
            {
                // Reducir ganancia progresivamente
                gain = (rms / threshold) * 0.3f; // Reducir a 30% como máximo
            }

            for (int i = 0; i < audioData.Length; i++)
            {
                gated[i] = audioData[i] * gain;
            }

            return gated;
        }

        private float[] ApplySpectralNoiseReduction(float[] audioData)
        {
            // Reducción de ruido espectral simple basada en umbralización
            int frameSize = 256; // Tamaño más pequeño para mejor tiempo real
            int hopSize = 128;

            if (audioData.Length < frameSize)
                return audioData;

            float[] processed = new float[audioData.Length];
            int processedSamples = 0;

            while (processedSamples + frameSize <= audioData.Length)
            {
                // Extraer frame
                float[] frame = new float[frameSize];
                Array.Copy(audioData, processedSamples, frame, 0, frameSize);

                // Aplicar ventana de Hann para reducir artifacts
                ApplyHannWindow(frame);

                // Reducción espectral simple (atenuación de bins de baja energía)
                for (int i = 0; i < frameSize; i++)
                {
                    float magnitude = Math.Abs(frame[i]);
                    // Atenuar componentes de muy baja energía (probable ruido)
                    if (magnitude < 0.02f)
                    {
                        frame[i] *= 0.2f; // Reducir a 20%
                    }
                    // Atenuar ligeramente componentes de energía media
                    else if (magnitude < 0.08f)
                    {
                        frame[i] *= 0.6f; // Reducir a 60%
                    }
                }

                // Copiar frame procesado al resultado
                Array.Copy(frame, 0, processed, processedSamples, frameSize);
                processedSamples += hopSize;
            }

            // Rellenar cualquier muestra restante
            if (processedSamples < audioData.Length)
            {
                Array.Copy(audioData, processedSamples, processed, processedSamples,
                           audioData.Length - processedSamples);
            }

            return processed;
        }

        private void ApplyHannWindow(float[] frame)
        {
            for (int i = 0; i < frame.Length; i++)
            {
                float window = 0.5f * (1 - (float)Math.Cos(2 * Math.PI * i / (frame.Length - 1)));
                frame[i] *= window;
            }
        }

        private float CalculateRMS(float[] audioData)
        {
            if (audioData.Length == 0) return 0;

            double sum = 0;
            foreach (float sample in audioData)
            {
                sum += sample * sample;
            }
            return (float)Math.Sqrt(sum / audioData.Length);
        }

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}
