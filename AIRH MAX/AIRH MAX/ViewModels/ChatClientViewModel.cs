using AIRH_MAX.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace AIRH_MAX.ViewModels
{
    public class ChatClientViewModel : INotifyPropertyChanged
    {
        #region Constantes
        private const string API_BASE_URL = "https://servermax.onrender.com";
        private const string API_KEY = "2025";
        #endregion

        #region Propiedades y Campos

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private DispatcherTimer _heartbeatTimer;
        private DispatcherTimer _reconnectTimer;
        private string _userName;
        private int _userCount = 0;
        private string _connectionStatus = "Desconectado";
        private SolidColorBrush _statusColor = new SolidColorBrush(Colors.Red);
        private string _currentMessage;
        private bool _isLoading;
        private string _loadingMessage;
        private bool _isConnected;
        private int _reconnectAttempts = 0;
        private const int MAX_RECONNECT_ATTEMPTS = 5;
        private bool _isClosing = false; // Nueva bandera para saber si estamos cerrando

        public ObservableCollection<ChatMessage> Messages { get; set; }
        public ObservableCollection<ChatUser> ConnectedUsers { get; set; }

        // Propiedades con notificación de cambios
        public string CurrentMessage
        {
            get => _currentMessage;
            set
            {
                _currentMessage = value;
                OnPropertyChanged(nameof(CurrentMessage));
                OnPropertyChanged(nameof(CanSend));
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public int UserCount
        {
            get => _userCount;
            set
            {
                _userCount = value;
                OnPropertyChanged(nameof(UserCount));
            }
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set
            {
                _statusColor = value;
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string LoadingMessage
        {
            get => _isLoading ? _loadingMessage : "";
            set
            {
                _loadingMessage = value;
                OnPropertyChanged(nameof(LoadingMessage));
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
                OnPropertyChanged(nameof(CanSend));
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();

                if (value)
                {
                    _reconnectAttempts = 0;
                }
            }
        }

        public bool CanSend => IsConnected && !string.IsNullOrWhiteSpace(CurrentMessage);

        // Comandos
        public ICommand SendCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand MinimizeCommand { get; }

        // Referencia a la ventana
        private Window _window;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public ChatClientViewModel(Window window, string userName = null)
        {
            _window = window;
            _userName = userName ?? $"Usuario_{new Random().Next(1000, 9999)}";
            _isClosing = false;

            // Inicializar colecciones
            Messages = new ObservableCollection<ChatMessage>();
            ConnectedUsers = new ObservableCollection<ChatUser>();

            // Configurar comandos
            SendCommand = new RelayCommand(ExecuteSend, CanExecuteSend);
            CloseCommand = new RelayCommand(ExecuteClose);
            MinimizeCommand = new RelayCommand(ExecuteMinimize);

            // Inicializar timers
            _heartbeatTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(25) };
            _heartbeatTimer.Tick += HeartbeatTimer_Tick;

            _reconnectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _reconnectTimer.Tick += ReconnectTimer_Tick;

            // Mensaje de bienvenida local
            //Messages.Add(ChatMessage.SystemMessage($"🟢 Iniciando sesión como '{_userName}'..."));

            // Iniciar conexión
            _ = InitializeChatAsync();
        }

        #endregion

        #region Métodos Públicos

        public void HandleKeyDown(Key key)
        {
            if (key == Key.Enter && CanSend)
            {
                ExecuteSend(null);
            }
        }

        public void Cleanup()
        {
            _isClosing = true; // Marcar que estamos cerrando
            _heartbeatTimer?.Stop();
            _reconnectTimer?.Stop();

            if (_webSocket != null)
            {
                _cancellationTokenSource?.Cancel();

                if (_webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var closeTask = _webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Cliente cerrando",
                            CancellationToken.None);

                        // Esperar máximo 2 segundos
                        closeTask.Wait(TimeSpan.FromSeconds(2));
                    }
                    catch { }
                }

                _webSocket.Dispose();
                _webSocket = null;
            }

            // Limpiar colecciones
            ConnectedUsers.Clear();
            UserCount = 0;
        }

        public void ScrollToBottom(ScrollViewer scrollViewer)
        {
            scrollViewer?.Dispatcher.BeginInvoke(() =>
            {
                scrollViewer.ScrollToBottom();
            });
        }

        #endregion

        #region Métodos de Conexión

        private async Task InitializeChatAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "Conectando al servidor de chat...";
                ConnectionStatus = "Conectando...";
                StatusColor = new SolidColorBrush(Colors.Orange);

                // Construir URL del WebSocket
                var wsUrl = $"{API_BASE_URL.Replace("https://", "wss://").Replace("http://", "ws://")}/ws/{Uri.EscapeDataString(_userName)}";

                // Mensaje de conexión (opcional, puedes quitarlo si quieres menos logs)
                // Messages.Add(ChatMessage.SystemMessage($"🔌 Conectando..."));

                // Conectar al WebSocket
                await ConnectWebSocketAsync(wsUrl);

                //// Opcional: Obtener configuración de la API
                //_ = Task.Run(async () =>
                //{
                //    try
                //    {
                //        var config = await GetApiConfigAsync();
                //        await Application.Current.Dispatcher.InvokeAsync(() =>
                //        {
                //            // Solo mostrar si hay novedades no vacías
                //            if (!string.IsNullOrEmpty(config.Version))
                //            {
                //                Messages.Add(ChatMessage.SystemMessage($"📱 Versión: {config.Version}"));
                //            }
                //            if (!string.IsNullOrEmpty(config.Novedad) && config.Novedad != "Sin novedades")
                //            {
                //                Messages.Add(ChatMessage.SystemMessage($"📢 {config.Novedad}"));
                //            }
                //        });
                //    }
                //    catch (Exception ex)
                //    {
                //        // Silenciar errores de configuración - no son críticos
                //        Console.WriteLine($"Error obteniendo config: {ex.Message}");
                //    }
                //});
            }
            catch (Exception ex)
            {
                if (!_isClosing) // Solo mostrar errores si no estamos cerrando
                {
                    ConnectionStatus = "Error de conexión";
                    StatusColor = new SolidColorBrush(Colors.Red);
                    Messages.Add(ChatMessage.SystemMessage($"❌ Error al conectar: {ex.Message}", true));

                    _reconnectAttempts++;
                    if (_reconnectAttempts <= MAX_RECONNECT_ATTEMPTS)
                    {
                        Messages.Add(ChatMessage.SystemMessage($"🔄 Reintentando en 5 segundos... (Intento {_reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS})"));
                        _reconnectTimer.Start();
                    }
                    else
                    {
                        Messages.Add(ChatMessage.SystemMessage($"❌ No se pudo conectar después de {MAX_RECONNECT_ATTEMPTS} intentos", true));
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Reemplaza el método ProcessIncomingMessage con este:
        private void ProcessIncomingMessage(string jsonMessage)
        {
            try
            {
                // Usar el serializador personalizado
                var message = WebSocketJsonSerializer.DeserializeWebSocketMessage(jsonMessage);

                if (message == null || string.IsNullOrEmpty(message.Type))
                    return;

                var type = message.Type;
                var username = message.Username ?? "Sistema";
                var messageText = message.Message ?? "";
                var timestamp = message.Timestamp ?? DateTime.Now.ToString("HH:mm:ss");
                var totalUsers = message.TotalUsers;

                if (totalUsers > 0)
                {
                    UserCount = totalUsers;
                }

                if (!string.IsNullOrWhiteSpace(messageText) || type == "welcome")
                {
                    var chatMessage = ChatMessage.FromServerJson(
                        type,
                        username,
                        messageText,
                        timestamp,
                        totalUsers,
                        username == _userName
                    );

                    Messages.Add(chatMessage);
                }

                if (type == "welcome" && !string.IsNullOrEmpty(message.YourUsername))
                {
                    System.Diagnostics.Debug.WriteLine($"Bienvenida confirmada para: {message.YourUsername}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error procesando mensaje: {ex.Message}");
            }
        }

        // Reemplaza el método SendWebSocketMessageAsync con este:
        private async Task SendWebSocketMessageAsync(object message)
        {
            if (_webSocket?.State != WebSocketState.Open || _isClosing)
                throw new InvalidOperationException("WebSocket no está conectado");

            // Usar el serializador personalizado
            var json = WebSocketJsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                _cancellationTokenSource.Token);
        }

        private async Task ConnectWebSocketAsync(string url)
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);
                IsConnected = true;

                // Iniciar recepción de mensajes
                _ = Task.Run(ReceiveMessagesAsync);

                // Iniciar heartbeat
                _heartbeatTimer.Start();

                ConnectionStatus = "Conectado";
                StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));

                //Messages.Add(ChatMessage.SystemMessage($"✅ Conectado como '{_userName}'"));

                // Detener reintentos
                _reconnectTimer.Stop();
            }
            catch (Exception ex)
            {
                IsConnected = false;
                throw new Exception($"Error conectando WebSocket: {ex.Message}");
            }
        }

        #endregion

        #region Recepción de Mensajes

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];

            while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested && !_isClosing)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                        if (!_isClosing) // Solo notificar si no estamos cerrando intencionalmente
                        {
                            await Application.Current.Dispatcher.InvokeAsync(HandleDisconnection);
                        }
                        break;
                    }

                    var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await Application.Current.Dispatcher.InvokeAsync(() => ProcessIncomingMessage(jsonMessage));
                }
                catch (Exception ex)
                {
                    if (!_cancellationTokenSource.Token.IsCancellationRequested && !_isClosing)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            HandleDisconnection();
                            Messages.Add(ChatMessage.SystemMessage($"🔴 Error de conexión: {ex.Message}", true));
                        });
                    }
                    break;
                }
            }
        }


        private void UpdateUserList(string username, int totalUsers, string messageType)
        {
            try
            {
                // Este método ahora es menos necesario porque no recibimos lista completa
                // pero podemos mantener una aproximación básica

                if (messageType == "system" && !string.IsNullOrEmpty(username) && username != "Sistema" && username != _userName)
                {
                    if (!ConnectedUsers.Any(u => u.Name == username))
                    {
                        ConnectedUsers.Add(new ChatUser
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = username
                        });
                    }
                }

                // Limpiar usuarios que ya no están (aproximación)
                while (ConnectedUsers.Count > totalUsers - 1) // -1 para excluirnos
                {
                    if (ConnectedUsers.Count > 0)
                        ConnectedUsers.RemoveAt(ConnectedUsers.Count - 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error actualizando lista de usuarios: {ex.Message}");
            }
        }
        #endregion

        #region Envío de Mensajes

        private bool CanExecuteSend(object parameter)
        {
            return CanSend && !_isClosing;
        }

        private async void ExecuteSend(object parameter)
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage) || _isClosing) return;

            var messageText = CurrentMessage.Trim();

            // El formato de envío NO cambia (el servidor espera lo mismo)
            var message = new
            {
                type = "message",
                message = messageText
            };

            try
            {
                await SendWebSocketMessageAsync(message);

                // Mensaje local (usando TotalUsers en lugar de OnlineCount)
                var localMessage = ChatMessage.FromServerJson(
                    "message",
                    _userName,
                    messageText,
                    DateTime.Now.ToString("HH:mm:ss"),
                    UserCount,  // UserCount ya tiene el valor actualizado
                    true
                );

                Messages.Add(localMessage);
                CurrentMessage = string.Empty;
            }
            catch (Exception ex)
            {
                if (!_isClosing)
                {
                    Messages.Add(ChatMessage.SystemMessage($"Error enviando mensaje: {ex.Message}", true));
                }
            }
        }


        #endregion

        #region Heartbeat y Reconexión

        private async void HeartbeatTimer_Tick(object sender, EventArgs e)
        {
            if (!IsConnected || _webSocket?.State != WebSocketState.Open || _isClosing)
            {
                _heartbeatTimer.Stop();
                return;
            }

            try
            {
                // Enviar ping para mantener conexión viva
                var ping = new { type = "ping" };
                await SendWebSocketMessageAsync(ping);
            }
            catch
            {
                if (!_isClosing)
                {
                    await Application.Current.Dispatcher.InvokeAsync(HandleDisconnection);
                }
            }
        }

        private async void ReconnectTimer_Tick(object sender, EventArgs e)
        {
            if (IsConnected || _isClosing)
            {
                _reconnectTimer.Stop();
                return;
            }

            _reconnectAttempts++;
            if (_reconnectAttempts > MAX_RECONNECT_ATTEMPTS)
            {
                _reconnectTimer.Stop();
                Messages.Add(ChatMessage.SystemMessage($"❌ No se pudo reconectar después de {MAX_RECONNECT_ATTEMPTS} intentos", true));
                return;
            }

            ConnectionStatus = $"Reconectando... ({_reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS})";
            StatusColor = new SolidColorBrush(Colors.Orange);

            try
            {
                await InitializeChatAsync();
            }
            catch
            {
                // Continuar reintentando
            }
        }

        private void HandleDisconnection()
        {
            if (_isClosing) return; // No hacer nada si estamos cerrando

            IsConnected = false;
            ConnectionStatus = "Desconectado";
            StatusColor = new SolidColorBrush(Colors.Red);
            _heartbeatTimer.Stop();

            Messages.Add(ChatMessage.SystemMessage("🔴 Desconectado del servidor"));

            // Iniciar reintentos
            if (_reconnectAttempts < MAX_RECONNECT_ATTEMPTS)
            {
                _reconnectTimer.Start();
            }
        }

        #endregion

        #region Comandos de Ventana

        private void ExecuteClose(object parameter)
        {
            // Marcar que estamos cerrando para evitar mensajes innecesarios
            _isClosing = true;

            // Desconectar y limpiar
            Cleanup();

            // Cerrar la ventana
            _window?.Close();
        }

        private void ExecuteMinimize(object parameter)
        {
            if (_window != null)
                _window.WindowState = WindowState.Minimized;
        }

        #endregion

        #region INotifyPropertyChanged

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}