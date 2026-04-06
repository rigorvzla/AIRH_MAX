using System.ComponentModel;

namespace AIRH_MAX.ViewModels
{
    public class Notificaciones : INotifyPropertyChanged
    {
        private string mensajeBox;
        private string mensajeBoxMute;
        private string LogBox;
        private string PizarraBox;
        private bool IABox;
        private string UserVoiceBox;

        public event PropertyChangedEventHandler PropertyChanged;

        public string MensajeBox
        {
            get { return mensajeBox; }
            set
            {
                mensajeBox = value;
                OnPropertyChanged("MensajeBox");
            }
        }

        public string MensajeBoxMute
        {
            get { return mensajeBoxMute; }
            set
            {
                mensajeBoxMute = value;
                OnPropertyChanged("MensajeBoxMute");
            }
        }

        public string Log
        {
            get { return LogBox; }
            set
            {
                LogBox = value;
                OnPropertyChanged("Log");
            }
        }

        public string Pizarra
        {
            get { return PizarraBox; }
            set
            {
                PizarraBox = value;
                OnPropertyChanged("Pizarra");
            }
        }

        public bool IA
        {
            get { return IABox; }
            set
            {
                IABox = value;
                OnPropertyChanged("IA");
            }
        }

        public string UserVoice
        {
            get { return UserVoiceBox; }
            set
            {
                UserVoiceBox = value;
                OnPropertyChanged("UserVoice");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
