using System.Diagnostics;

namespace Vosk_STT
{
    public class ProfessionalAudioProcessor : IDisposable
    {
        private readonly int _sampleRate = 16000;
        private readonly VoiceActivityDetector _vad;
        private readonly AdvancedNoiseReducer _noiseReducer;
        private readonly VoiceOptimizedEQ _eq;
        private readonly BreathAndPopRemover _breathRemover;
        private bool _enabled = true;

        // NUEVAS CONFIGURACIONES DE GANANCIA
        private float _closeVoiceGain = 1.0f;      // Ganancia normal para voz cercana
        private float _weakVoiceGain = 0.7f;       // Ganancia reducida para voz débil
        private float _farSoundGain = 0.3f;        // Ganancia baja para sonidos lejanos
        private float _noiseGain = 0.1f;           // Ganancia mínima para ruido

        // Para seguimiento de estado
        private SoundCategory _lastSoundCategory = SoundCategory.Silence;

        // NUEVO: Eventos para UI
        public event Action<float> OnVoiceLevelChanged;      // Nivel 0-1
        public event Action<float> OnRMSLevelChanged;        // RMS crudo
        public event Action<bool> OnVoiceActivityChanged;    // Activo/inactivo
        public event Action<string> OnVoiceLevelInfo;        // Información textual

        public ProfessionalAudioProcessor(int sampleRate = 16000)
        {
            _sampleRate = sampleRate;
            _vad = new VoiceActivityDetector(sampleRate);
            _noiseReducer = new AdvancedNoiseReducer(sampleRate);
            _eq = new VoiceOptimizedEQ(sampleRate);
            _breathRemover = new BreathAndPopRemover(sampleRate);

            // CONECTAR EVENTOS DEL VAD
            _vad.OnVoiceLevel += (level) =>
            {
                OnVoiceLevelChanged?.Invoke(level);
                OnVoiceLevelInfo?.Invoke($"Nivel de voz: {level:P0}");
            };

            _vad.OnRMSLevel += (rms) =>
            {
                OnRMSLevelChanged?.Invoke(rms);
            };

            _vad.OnVoiceActive += (isActive) =>
            {
                OnVoiceActivityChanged?.Invoke(isActive);
                OnVoiceLevelInfo?.Invoke(isActive ? "🎤 Voz activa" : "🤫 Voz inactiva");
            };

            // APLICAR CONFIGURACIÓN POR DEFECTO
            SetCloseVoicePriority();

            
        }

        // NUEVO: Método para obtener información de niveles
        public VoiceActivityDetector.VoiceLevelInfo GetVoiceLevelInfo()
        {
            return _vad.GetVoiceLevelInfo();
        }

        // NUEVO: Configurar sensibilidad del medidor
        public void SetVoiceMeterSensitivity(float sensitivity)
        {
            // Ajustar umbrales según sensibilidad (0-1)
            float adjustedCloseThreshold = 0.02f - (sensitivity * 0.015f);
            float adjustedFarThreshold = 0.005f - (sensitivity * 0.004f);

            _vad.SetCloseVoiceThreshold(Math.Max(0.001f, adjustedCloseThreshold));
            _vad.SetFarSoundThreshold(Math.Max(0.0005f, adjustedFarThreshold));

            Debug.WriteLine($"🎚️ Sensibilidad del medidor ajustada a: {sensitivity:P0}");
        }

        // NUEVO: Configurar suavizado del medidor
        public void SetVoiceMeterSmoothing(float smoothing)
        {
            _vad.SetSmoothing(smoothing);
            Debug.WriteLine($"🌀 Suavizado del medidor ajustado a: {smoothing:F2}");
        }

        public byte[] ProcessAudio(byte[] audioData, int bytesRecorded)
        {
            if (!_enabled || bytesRecorded < 64)
                return audioData;

            try
            {
                float[] audioFloat = ConvertByteToFloat(audioData, bytesRecorded);

                // 1. Procesamiento básico (siempre aplicado)
                audioFloat = _noiseReducer.ApplyDCBiasRemoval(audioFloat);
                audioFloat = _noiseReducer.ApplyHighPassFilter(audioFloat, 80f);

                // 2. ¡ESTA ES LA LÍNEA CRÍTICA QUE FALTA!
                // Debemos llamar a HasVoiceActivity para que se disparen los eventos
                bool hasVoice = _vad.HasVoiceActivity(audioFloat);

                // 3. Analizar tipo de sonido (opcional, pero mantengámoslo)
                float rms = CalculateRMS(audioFloat);
                float spectralCentroid = CalculateSpectralCentroid(audioFloat);
                float zcr = CalculateZeroCrossingRate(audioFloat);

                SoundCategory category = DetermineSoundCategory(rms, spectralCentroid, zcr);

                // 4. Aplicar procesamiento según categoría
                audioFloat = ProcessBasedOnCategory(audioFloat, category, rms);

                // 5. Convertir de vuelta
                return ConvertFloatToByte(audioFloat);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Error procesando audio: {ex.Message}");
                return audioData;
            }
        }

        // NUEVO MÉTODO: Determinar categoría de sonido
        private SoundCategory DetermineSoundCategory(float rms, float spectralCentroid, float zcr)
        {
            // Voz cercana y clara
            if (rms >= 0.02f && spectralCentroid >= 800f && spectralCentroid <= 2200f)
                return SoundCategory.CloseVoice;

            // Voz débil o a distancia media
            if (rms >= 0.01f && rms < 0.02f && spectralCentroid >= 500f)
                return SoundCategory.WeakVoice;

            // Sonidos lejanos (baja frecuencia, baja energía)
            if (rms >= 0.005f && rms < 0.01f && spectralCentroid < 300f)
                return SoundCategory.FarSound;

            // Ruido de fondo
            if (rms >= 0.001f && rms < 0.005f)
                return SoundCategory.BackgroundNoise;

            // Silencio
            return SoundCategory.Silence;
        }

        // NUEVO MÉTODO: Procesar según categoría
        private float[] ProcessBasedOnCategory(float[] audioData, SoundCategory category, float rms)
        {
            float gain = 1.0f;
            float[] processed = audioData;

            switch (category)
            {
                case SoundCategory.CloseVoice:
                    // Procesamiento completo para voz cercana
                    gain = _closeVoiceGain;
                    processed = _noiseReducer.ApplyAdaptiveNoiseReduction(processed);
                    processed = _breathRemover.RemoveBreathAndPops(processed);
                    processed = _eq.ApplyVoiceEQ(processed);
                    processed = ApplyVoiceEnhancement(processed);
                    break;

                case SoundCategory.WeakVoice:
                    // Procesamiento reducido para voz débil
                    gain = _weakVoiceGain;
                    processed = _noiseReducer.ApplyAdaptiveNoiseReduction(processed);
                    processed = ApplyMidBoost(processed, 2000f, 3f); // Aumentar claridad
                    break;

                case SoundCategory.FarSound:
                    // Atenuar sonidos lejanos
                    gain = _farSoundGain;
                    processed = ApplyLowPassFilter(processed, 2000f); // Cortar frecuencias altas
                    processed = ApplySoftAttenuation(processed, 0.5f); // Atenuación suave
                    break;

                case SoundCategory.BackgroundNoise:
                    // Reducir mucho el ruido
                    gain = _noiseGain;
                    processed = ApplyAggressiveNoiseReduction(processed);
                    break;

                case SoundCategory.Silence:
                    // Silencio - mantener muy bajo
                    gain = 0.05f;
                    break;
            }

            // Aplicar ganancia
            if (gain != 1.0f)
            {
                for (int i = 0; i < processed.Length; i++)
                {
                    processed[i] *= gain;
                }
            }

            // Soft clipping final para evitar distorsión
            return _noiseReducer.ApplySoftClipping(processed);
        }

        // NUEVOS MÉTODOS DE PROCESAMIENTO ESPECÍFICO
        private float[] ApplyVoiceEnhancement(float[] audioData)
        {
            // Realce específico para voz
            var presenceBoost = BiQuadFilter.PeakFilter(_sampleRate, 1800f, 1.2f, 4f);
            var clarityBoost = BiQuadFilter.PeakFilter(_sampleRate, 3000f, 2.0f, 2f);

            float[] enhanced = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                float sample = audioData[i];
                sample = presenceBoost.Transform(sample);
                sample = clarityBoost.Transform(sample);
                enhanced[i] = sample;
            }
            return enhanced;
        }

        private float[] ApplyMidBoost(float[] audioData, float freq, float gainDb)
        {
            var midBoost = BiQuadFilter.PeakFilter(_sampleRate, freq, 1.5f, gainDb);
            float[] boosted = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                boosted[i] = midBoost.Transform(audioData[i]);
            }
            return boosted;
        }

        private float[] ApplyLowPassFilter(float[] audioData, float cutoffFreq)
        {
            var lowPass = BiQuadFilter.LowPassFilter(_sampleRate, cutoffFreq, 0.7f);
            float[] filtered = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                filtered[i] = lowPass.Transform(audioData[i]);
            }
            return filtered;
        }

        private float[] ApplySoftAttenuation(float[] audioData, float attenuationFactor)
        {
            // Atenuación no lineal (más suave para sonidos más bajos)
            float[] attenuated = new float[audioData.Length];
            for (int i = 0; i < audioData.Length; i++)
            {
                float sample = audioData[i];
                float absSample = Math.Abs(sample);

                // Fórmula de atenuación suave
                float attenuation = 1.0f - (1.0f - attenuationFactor) * absSample;
                attenuated[i] = sample * attenuation;
            }
            return attenuated;
        }

        private float[] ApplyAggressiveNoiseReduction(float[] audioData)
        {
            float[] reduced = (float[])audioData.Clone();
            float noiseFloor = 0.008f;

            for (int i = 0; i < reduced.Length; i++)
            {
                if (Math.Abs(reduced[i]) < noiseFloor)
                {
                    reduced[i] *= 0.1f; // Reducir al 10%
                }
            }
            return reduced;
        }

        // MÉTODOS DE CONFIGURACIÓN
        public void SetGains(float closeVoice = 1.0f, float weakVoice = 0.7f,
                           float farSound = 0.3f, float noise = 0.1f)
        {
            _closeVoiceGain = Math.Clamp(closeVoice, 0.5f, 2.0f);
            _weakVoiceGain = Math.Clamp(weakVoice, 0.3f, 1.5f);
            _farSoundGain = Math.Clamp(farSound, 0.1f, 0.8f);
            _noiseGain = Math.Clamp(noise, 0.01f, 0.3f);
        }

        public void SetThresholds(float closeVoice = 0.02f, float farSound = 0.005f)
        {
            _vad.SetCloseVoiceThreshold(closeVoice);
            _vad.SetFarSoundThreshold(farSound);
        }

        private void SetDefaultThresholds()
        {
            // Valores por defecto optimizados
            _vad.SetCloseVoiceThreshold(0.02f);
            _vad.SetFarSoundThreshold(0.005f);
            _vad.SetMinVolumeThreshold(0.001f);
            _vad.SetSpectralThresholds(300f, 800f);
        }

        // MÉTODOS DE CÁLCULO AUXILIARES
        private float CalculateRMS(float[] audioData)
        {
            if (audioData.Length == 0) return 0;
            double sum = 0;
            foreach (float sample in audioData) sum += sample * sample;
            return (float)Math.Sqrt(sum / audioData.Length);
        }

        private float CalculateSpectralCentroid(float[] audioData)
        {
            float magnitudeSum = 0f, weightedSum = 0f;
            for (int i = 0; i < audioData.Length; i++)
            {
                float freq = (float)i * _sampleRate / (2f * audioData.Length);
                float magnitude = Math.Abs(audioData[i]);
                weightedSum += freq * magnitude;
                magnitudeSum += magnitude;
            }
            return magnitudeSum > 0 ? weightedSum / magnitudeSum : 0f;
        }

        private float CalculateZeroCrossingRate(float[] audioData)
        {
            if (audioData.Length < 2) return 0f;
            int crossings = 0;
            for (int i = 1; i < audioData.Length; i++)
                if (audioData[i] * audioData[i - 1] < 0) crossings++;
            return (float)crossings / (audioData.Length - 1);
        }

        // MÉTODOS DE CONVERSIÓN (mantener igual)
        private float[] ConvertByteToFloat(byte[] audioData, int bytesRecorded)
        {
            int sampleCount = bytesRecorded / 2;
            float[] floatData = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)((audioData[i * 2 + 1] << 8) | audioData[i * 2]);
                floatData[i] = sample / 32768.0f;
            }
            return floatData;
        }

        private byte[] ConvertFloatToByte(float[] floatData)
        {
            byte[] byteData = new byte[floatData.Length * 2];
            for (int i = 0; i < floatData.Length; i++)
            {
                float sample = Math.Clamp(floatData[i], -1.0f, 1.0f);
                short intSample = (short)(sample * 32767);
                byteData[i * 2] = (byte)(intSample & 0xFF);
                byteData[i * 2 + 1] = (byte)((intSample >> 8) & 0xFF);
            }
            return byteData;
        }

        public void EnableProcessing(bool enable) => _enabled = enable;

        public void CalibrateNoiseFloor() => _noiseReducer.CalibrateNoiseFloor();

        public void Dispose() => _noiseReducer?.Dispose();

        // ENUM para categorías de sonido
        private enum SoundCategory
        {
            Silence,
            BackgroundNoise,
            FarSound,
            WeakVoice,
            CloseVoice
        }

        public void SetCloseVoicePriority()
        {
            SetGains(1.0f, 0.5f, 0.2f, 0.05f);
            SetThresholds(0.015f, 0.003f);
            SetSpectralThresholds(250f, 600f); // Umbrales espectrales más estrictos
            Debug.WriteLine("✅ Procesador configurado: Voz cercana prioritaria");
            Debug.WriteLine("   - Umbral voz cercana: 0.015");
            Debug.WriteLine("   - Umbral sonidos lejanos: 0.003");
            Debug.WriteLine("   - Ganancia sonidos lejanos: 20% (reducción del 80%)");
            Debug.WriteLine("🎙️ Procesador profesional de audio con medidor de voz");
        }

        public void SetBalancedMode()
        {
            SetGains(1.0f, 0.7f, 0.4f, 0.1f);
            SetThresholds(0.02f, 0.005f);
            SetSpectralThresholds(300f, 800f);
            Debug.WriteLine("✅ Procesador configurado: Modo balanceado");
        }

        public void SetSensitiveMode()
        {
            SetGains(1.0f, 0.8f, 0.6f, 0.3f);
            SetThresholds(0.01f, 0.002f);
            SetSpectralThresholds(200f, 500f);
            Debug.WriteLine("✅ Procesador configurado: Modo sensible");
        }

        // Método auxiliar para configurar umbrales espectrales
        private void SetSpectralThresholds(float lowThreshold, float voiceThreshold)
        {
            _vad.SetSpectralThresholds(lowThreshold, voiceThreshold);
        }
    }
}