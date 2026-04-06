namespace Vosk_STT
{
    public class VoiceActivityDetector
    {
        public event Action<float> OnVoiceLevel; // Valor entre 0 y 1
        public event Action<float> OnRMSLevel;   // Valor RMS crudo
        public event Action<bool> OnVoiceActive; // Estado de actividad

        // NUEVO: Propiedades para configuración de visualización
        private float _voiceLevelSmoothing = 0.3f;
        private float _currentSmoothedLevel = 0f;
        private float _peakLevel = 0f;
        private DateTime _lastPeakTime = DateTime.MinValue;

        private readonly int _sampleRate;
        private bool _isVoiceDetected;
        private int _voiceFrameCount, _silenceFrameCount;

        // NUEVOS UMBRALES AJUSTABLES
        private float _closeVoiceThreshold = 0.02f;    // Voz cercana clara
        private float _farSoundThreshold = 0.005f;     // Sonidos lejanos (se atenúan)
        private float _minVolumeThreshold = 0.001f;    // Mínimo absoluto (silencia casi por completo)

        // Para detección más inteligente
        private float _spectralLowThreshold = 300f;    // Frecuencia baja = sonido lejano
        private float _spectralVoiceThreshold = 800f;  // Frecuencia de voz típica

        public VoiceActivityDetector(int sampleRate = 16000) => _sampleRate = sampleRate;

        public bool HasVoiceActivity(float[] audioData)
        {
            float rms = CalculateRMS(audioData);
            float spectralCentroid = CalculateSpectralCentroid(audioData);
            float zeroCrossingRate = CalculateZeroCrossingRate(audioData);

            // DETERMINAR TIPO DE SONIDO SEGÚN UMBRALES
            SoundType currentSoundType = ClassifySound(rms, spectralCentroid, zeroCrossingRate);

            bool isCloseVoice = currentSoundType == SoundType.CloseVoice;
            bool isFarSound = currentSoundType == SoundType.FarSound;
            bool isWeakVoice = currentSoundType == SoundType.WeakVoice;

            // Lógica de detección mejorada
            if (isCloseVoice || isWeakVoice)
            {
                _voiceFrameCount++;
                _silenceFrameCount = 0;

                // Confirmar con múltiples frames para evitar falsos positivos
                if (_voiceFrameCount >= 3 && !_isVoiceDetected)
                {
                    _isVoiceDetected = true;
                }
            }
            else if (isFarSound)
            {
                // Sonido lejano: reducir conteo de voz pero no resetear completamente
                _silenceFrameCount++;
                if (_voiceFrameCount > 0) _voiceFrameCount--;
            }
            else // Silencio o ruido muy bajo
            {
                _silenceFrameCount++;
                if (_voiceFrameCount > 0) _voiceFrameCount--;

                // Requerir más silencio para considerar que la voz terminó
                if (_silenceFrameCount > 15 && _isVoiceDetected)
                {
                    _isVoiceDetected = false;
                }
            }

            float normalizedLevel = CalculateNormalizedVoiceLevel(rms, spectralCentroid, currentSoundType);

            // Emitir eventos de nivel
            OnRMSLevel?.Invoke(rms);
            OnVoiceLevel?.Invoke(normalizedLevel);
            OnVoiceActive?.Invoke(_isVoiceDetected || isWeakVoice);

            return _isVoiceDetected || isWeakVoice;
        }

        private float CalculateNormalizedVoiceLevel(float rms, float spectralCentroid, SoundType soundType)
        {
            // Nivel base basado en RMS (ajustado)
            float baseLevel = Math.Min(rms * 10f, 1.0f); // RMS típico para voz: 0.1 = nivel 1.0

            // Ajustar según tipo de sonido
            float multiplier = 1.0f;
            switch (soundType)
            {
                case SoundType.CloseVoice:
                    multiplier = 1.2f; // Aumentar nivel para voz cercana
                    break;
                case SoundType.WeakVoice:
                    multiplier = 0.8f; // Reducir para voz débil
                    break;
                case SoundType.FarSound:
                    multiplier = 0.4f; // Reducir significativamente
                    break;
                case SoundType.BackgroundNoise:
                    multiplier = 0.2f; // Muy reducido
                    break;
                case SoundType.Silence:
                    multiplier = 0.0f; // Casi cero
                    break;
            }

            float rawLevel = baseLevel * multiplier;

            // Suavizar el nivel para evitar saltos bruscos en la UI
            _currentSmoothedLevel = _currentSmoothedLevel * (1 - _voiceLevelSmoothing) +
                                   rawLevel * _voiceLevelSmoothing;

            // Seguimiento de picos
            if (_currentSmoothedLevel > _peakLevel)
            {
                _peakLevel = _currentSmoothedLevel;
                _lastPeakTime = DateTime.Now;
            }
            else if ((DateTime.Now - _lastPeakTime).TotalSeconds > 0.5f)
            {
                // Decaimiento del pico después de 0.5 segundos
                _peakLevel *= 0.95f;
            }

            return Math.Clamp(_currentSmoothedLevel, 0f, 1f);
        }

        // NUEVO: Método para obtener información de niveles
        public VoiceLevelInfo GetVoiceLevelInfo()
        {
            return new VoiceLevelInfo
            {
                CurrentLevel = _currentSmoothedLevel,
                PeakLevel = _peakLevel,
                IsVoiceActive = _isVoiceDetected,
                SmoothingFactor = _voiceLevelSmoothing
            };
        }

        // NUEVO: Configurar suavizado
        public void SetSmoothing(float smoothing)
        {
            _voiceLevelSmoothing = Math.Clamp(smoothing, 0.1f, 0.9f);
        }

        // ... (resto del código existente)

        // NUEVA CLASE para información de niveles
        public class VoiceLevelInfo
        {
            public float CurrentLevel { get; set; }      // 0-1
            public float PeakLevel { get; set; }         // 0-1
            public bool IsVoiceActive { get; set; }
            public float SmoothingFactor { get; set; }

            public override string ToString()
            {
                return $"Level: {CurrentLevel:P0} | Peak: {PeakLevel:P0} | Active: {IsVoiceActive}";
            }
        }

        // NUEVO MÉTODO: Clasificar tipo de sonido
        private SoundType ClassifySound(float rms, float spectralCentroid, float zcr)
        {
            // Voz cercana y clara (alta energía, frecuencias vocales)
            if (rms >= _closeVoiceThreshold &&
                spectralCentroid >= _spectralVoiceThreshold &&
                spectralCentroid <= 2200f &&
                zcr > 0.08f && zcr < 0.35f)
            {
                return SoundType.CloseVoice;
            }

            // Voz débil o lejana (energía media, podría ser voz)
            if (rms >= _farSoundThreshold && rms < _closeVoiceThreshold &&
                spectralCentroid > 400f)
            {
                return SoundType.WeakVoice;
            }

            // Sonidos lejanos o graves (baja energía, baja frecuencia)
            if (rms >= _farSoundThreshold &&
                spectralCentroid < _spectralLowThreshold)
            {
                return SoundType.FarSound;
            }

            // Ruido de fondo (muy baja energía)
            if (rms >= _minVolumeThreshold && rms < _farSoundThreshold)
            {
                return SoundType.BackgroundNoise;
            }

            return SoundType.Silence;
        }

        // NUEVOS MÉTODOS PARA AJUSTAR UMBRALES
        public void SetCloseVoiceThreshold(float threshold)
        {
            _closeVoiceThreshold = Math.Max(0.005f, Math.Min(0.1f, threshold));
        }

        public void SetFarSoundThreshold(float threshold)
        {
            _farSoundThreshold = Math.Max(0.001f, Math.Min(0.02f, threshold));
        }

        public void SetMinVolumeThreshold(float threshold)
        {
            _minVolumeThreshold = Math.Max(0.0001f, Math.Min(0.005f, threshold));
        }

        public void SetSpectralThresholds(float lowThreshold, float voiceThreshold)
        {
            _spectralLowThreshold = lowThreshold;
            _spectralVoiceThreshold = voiceThreshold;
        }

        // Métodos existentes (mantener igual)
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

        private float CalculateRMS(float[] audioData)
        {
            if (audioData.Length == 0) return 0;
            double sum = 0;
            foreach (float sample in audioData) sum += sample * sample;
            return (float)Math.Sqrt(sum / audioData.Length);
        }

        public void Reset()
        {
            _isVoiceDetected = false;
            _voiceFrameCount = 0;
            _silenceFrameCount = 0;
        }

        // ENUM para tipos de sonido
        private enum SoundType
        {
            Silence,
            BackgroundNoise,
            FarSound,
            WeakVoice,
            CloseVoice
        }
    }
}