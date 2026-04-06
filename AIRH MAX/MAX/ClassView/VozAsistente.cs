using System.Speech.Synthesis;

namespace MAX.ClassView
{
    internal class VozAsistente
    {
        public static async void SpeakTalk(string texto, bool async = false)
        {
            if (async == false)
            {
                if (Engrane.File_Config().VozIA)
                {
                    Engrane._piper.Speak(texto);
                }
                else
                {
                    if (Engrane.AIRH_Voz.State.ToString() == "Ready")
                    {
                        Engrane.AIRH_Voz.SelectVoice(Engrane.File_Config().Voz);
                        Engrane.AIRH_Voz.Speak(texto);
                    }
                }
            }
            else
            {
                if (Engrane.File_Config().VozIA)
                {
                    Engrane._piper.Speak(texto);
                }
                else
                {
                    await SpeakTalkAsyncWindows(texto);
                }
            }
        }
        public static void SpeakTest(string texto, InstalledVoice voice = null)
        {
            if (voice is not null)
            {
                if (Engrane.AIRH_Voz.State.ToString() == "Ready")
                {
                    Engrane.AIRH_Voz.SelectVoice(voice.VoiceInfo.Name);
                    Engrane.AIRH_Voz.SpeakAsync(texto);
                }
            }
            else
            {
                Engrane._piper.Speak(texto);
            }
        }

        private static Task SpeakTalkAsyncWindows(string texto)
        {
            var completionSource = new TaskCompletionSource<bool>();

            EventHandler<SpeakCompletedEventArgs> completedHandler = null;

            completedHandler = (s, e) =>
            {
                Engrane.AIRH_Voz.SpeakCompleted -= completedHandler;

                if (e.Error != null)
                    completionSource.SetException(e.Error);
                else if (e.Cancelled)
                    completionSource.SetCanceled();
                else
                    completionSource.SetResult(true);
            };

            Engrane.AIRH_Voz.SpeakCompleted += completedHandler;

            try
            {
                if (Engrane.AIRH_Voz.State.ToString() == "Ready")
                {
                    Engrane.AIRH_Voz.SelectVoice(Engrane.File_Config().Voz);
                    Engrane.AIRH_Voz.SpeakAsync(texto);
                }
            }
            catch
            {
                Engrane.AIRH_Voz.SpeakCompleted -= completedHandler;
                throw;
            }

            return completionSource.Task;
        }
    }
}