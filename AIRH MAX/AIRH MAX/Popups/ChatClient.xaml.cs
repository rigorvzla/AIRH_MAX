using AIRH_MAX.ViewModels;
using System.Windows;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace AIRH_MAX.Popups
{
    public partial class ChatClient : Window
    {
        private ChatClientViewModel _viewModel;

        public ChatClient(string userName = null)
        {
            InitializeComponent();

            // Crear ViewModel y asignar como DataContext
            _viewModel = new ChatClientViewModel(this, userName);
            DataContext = _viewModel;

            // Manejar el evento Closing
            this.Closing += ChatClient_Closing;
        }

        private void ChatClient_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Asegurar limpieza al cerrar
            _viewModel.Cleanup();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.HandleKeyDown(Key.Enter);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _viewModel.ScrollToBottom(MessageScrollViewer);
                }), System.Windows.Threading.DispatcherPriority.Background);
                e.Handled = true;
            }

        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _viewModel.ScrollToBottom(MessageScrollViewer);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.Cleanup();
            base.OnClosing(e);
        }

        private void ChatWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}