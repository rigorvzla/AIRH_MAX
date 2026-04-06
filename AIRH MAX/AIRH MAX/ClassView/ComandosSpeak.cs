using AIRH_MAX.ClassView.SystemClass;
using AIRH_MAX.Models;
using AIRH_MAX.Views;
using InputSimulator_RV;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace AIRH_MAX.ClassView
{
    internal class ComandosSpeak
    {
        private static string Reproductor(ModelResponse comando)
        {
            switch (comando.user_command)
            {
                case "ajustar_volumen":
                    if (comando.user_command_extra.command_a == string.Empty || comando.user_command_extra.command_a == null)
                    {
                        var key = comando.user_command_extra.command_b == "sube_volumen"
                            ? InputSimulator.Keyboard.VirtualKeyShort.VOLUME_UP
                            : InputSimulator.Keyboard.VirtualKeyShort.VOLUME_DOWN;

                        for (int i = 0; i < 3; i++)
                            InputSimulator.Keyboard.PRESSKEY(key);
                    }
                    else
                    {
                        AudioManager.VolumenPorcentual(Convert.ToInt32(comando.user_command_extra.command_a.Replace("%", "")));
                    }
                    break;
                case "reproducir_multimedia":
                    if (!string.IsNullOrEmpty(comando.user_command_extra.command_a))
                    {
                        var multimedia = DB_Lite.EjecutarMultimedia(comando.user_command_extra.command_a, CultureInfo.CurrentCulture.TextInfo.ToTitleCase(comando.user_command_extra.command_b.ToLower()));
                        if (multimedia == null)
                        {
                            return $"{comando.user_command_extra.command_b} no encontrado";
                        }

                        try
                        {
                            Process.Start(new ProcessStartInfo(multimedia) { UseShellExecute = true });
                        }
                        catch (Exception ex)
                        {
                            return $"Error al reproducir {comando.user_command_extra.command_b}: {ex.Message}";
                        }
                    }
                    else
                    {
                        InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.PLAY);
                    }
                    break;
                case "pausar_no_pausar_multimedia":
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.MEDIA_PLAY_PAUSE);
                    break;
                case "siguiente_multimedia":
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.MEDIA_NEXT_TRACK);
                    break;
                case "anterior_multimedia":
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.MEDIA_PREV_TRACK);
                    break;
                case "activar_desactivar_sonido":
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.VOLUME_MUTE);
                    break;
                case "repetir_multimedia":
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.MEDIA_STOP);
                    Task.Delay(100).Wait();
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.MEDIA_PLAY_PAUSE);
                    break;
            }

            return string.Empty;
        }
        private static string Compresor(ModelResponse comando)
        {
            var commands = comando.user_command switch
            {
                "descomprimir_extraer_archivo" => "3",
                "comprimir_archivo_directorio" => "1",
                _ => null // Caso por defecto si no coincide ningún comando
            };

            if (commands != null)
            {
                SevenPack seven = new(Convert.ToInt16(commands));
                seven.Show();
            }
            return string.Empty;
        }
        private static async Task<string> Conversor(ModelResponse comando)
        {
            var commands = comando.user_command switch
            {
                "conversor_mp3" => new[] { "1" },
                "conversor_mp4" => new[] { "2" },
                "conversor_jpg" => new[] { "3" },
                _ => null // Caso por defecto si no coincide ningún comando
            };

            if (commands != null)
            {
                string path = Engrane.ElementPath();
                var convertWindow = new ConvertMediaPack(Convert.ToInt16(commands[0]), path);
                convertWindow.Show();
                await convertWindow.StartConversionAsync();
                convertWindow.Close();

            }
            return string.Empty;
        }
        private static string WebExplorer(ModelResponse comando)
        {
            Action action = comando.user_command switch
            {
                "pagina_anterior_web" => () => InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.BROWSER_BACK),
                "pagina_siguiente_web" => () => InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.BROWSER_FORWARD),
                "siguiente_pestaña_web" => () => SendKeys.SendWait("^{TAB}"),
                "pestaña_anterior_web" => () => SendKeys.SendWait("^+{TAB}"),
                "aumentar_tamaño_web" => () => SendKeys.SendWait("^{ADD}"),
                "reducir_tamaño_web" => () => SendKeys.SendWait("^{SUBTRACT}"),
                "tamaño_normal_web" => () => SendKeys.SendWait("^0"),
                "nueva_pestaña_web" => () => SendKeys.SendWait("^t"),
                "cambiar_pestaña_web" => () => SendKeys.SendWait($"^{comando.user_command_extra.command_a}"),
                "agregar_marcadores_web" => () => SendKeys.SendWait("^d"),
                "ventana_privada_web" => () => SendKeys.SendWait("^+n"),
                "cerrar_pestaña_web" => () => SendKeys.SendWait("^w"),
                "recargar_pagina_web" => () => InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.BROWSER_REFRESH),
                "guardar_pagina_web" => async () => await WebStore.GuardarWeb(),
                _ => null // Caso por defecto si no coincide ningún comando
            };

            action?.Invoke();
            return string.Empty;
        }
        private static string Sistema(ModelResponse comando)
        {
            string respuesta = string.Empty;
            Action action = comando.user_command switch
            {
                "temperatura_CPU_equipo" or "temperatura_GPU_grafica" => () =>
                {
                    respuesta = "Dame un momento";
                    if (comando.user_command == "temperatura_CPU_equipo")
                    {
                        Engrane.EXE("MAX.exe", "TC");
                    }
                    else
                    {
                        Engrane.EXE("MAX.exe", "TG");
                    }
                }
                ,
                "apagar_monitor" => () => MonitorOff.ApagadoMonitor(),
                "accion_sistema_vaciar_papelera" => () => Papelera.limpieza(),
                "accion_sistema_imprimir" => () => SendKeys.SendWait("^p"),
                "accion_sistema_guardar_documento" => () => SendKeys.SendWait("^g"),
                "accion_sistema_copiar" => () => SendKeys.SendWait("^c"),
                "accion_sistema_cortar" => () => SendKeys.SendWait("^x"),
                "accion_sistema_pegar" => () => SendKeys.SendWait("^v"),
                "cerrar_ventana" => () => SendKeys.SendWait("%{F4}"),
                "minimizar_ventana" => () => InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.DOWN, InputSimulator.Keyboard.ScanCodeShort.LWIN),
                "maximizar_ventana" => () => InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.UP, InputSimulator.Keyboard.ScanCodeShort.LWIN),
                "maximizar_todo" or "minimizar_todo" => () => InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_D, InputSimulator.Keyboard.ScanCodeShort.LWIN),
                "acoplar_derecha" => () => InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.RIGHT, InputSimulator.Keyboard.ScanCodeShort.LWIN),
                "acoplar_izquierda" => () => InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.LEFT, InputSimulator.Keyboard.ScanCodeShort.LWIN),
                "caracteristicas_del_equipo" => () => InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.PAUSE, InputSimulator.Keyboard.ScanCodeShort.LWIN),
                "ver_propiedades" => () => SendKeys.SendWait("%{ENTER}"),
                "cambiar_programa" => () =>
                {
                    if (comando.user_command_extra.command_a == string.Empty)
                    {
                        SendKeys.SendWait("%{TAB}");
                    }
                    else if (comando.user_command_extra.command_a == "1")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_1, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "2")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_2, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "3")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_3, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "4")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_4, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "5")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_5, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "6")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_6, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "7")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_7, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "8")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_8, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "9")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_9, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                    else if (comando.user_command_extra.command_a == "10")
                    {
                        InputSimulator.Keyboard.KEYSTROKE(InputSimulator.Keyboard.VirtualKeyShort.LWIN, InputSimulator.Keyboard.VirtualKeyShort.KEY_0, InputSimulator.Keyboard.ScanCodeShort.LWIN);
                    }
                }
                ,
                "hora_actual" => () => respuesta = DateTime.Now.ToString("hh:mm tt"),
                "fecha_actual" => () => respuesta = DateTime.Now.Date.ToString("d"),
                "administrador_tareas" => () => SendKeys.SendWait("^+{ESC}"),
                "apagar_equipo" => () =>
                {
                    if (comando.user_command_extra.command_a == string.Empty || comando.user_command_extra.command_a == null)
                    {
                        respuesta = $"En 30 segundos, el equipo se apagará.";
                        Process.Start("shutdown.exe", $"-s -t 30");
                    }
                    else
                    {
                        var unidades = new Dictionary<string, Func<long, (long segundos, string descripcion)>>
                        {
                            ["segundos"] = secs => (secs, $"{secs} segundo(s)"),
                            ["minutos"] = mins => (mins * 60, $"{mins} minuto(s)"),
                            ["horas"] = horas => (horas * 3600, $"{horas} hora(s)"),
                            ["dias"] = dias => (dias * 86400, $"{dias} día(s)")
                        };

                        // Obtener valores del comando
                        string unidad = comando.user_command_extra.command_b?.Trim().ToLower()
                            ?? throw new ArgumentNullException(nameof(comando.user_command_extra.command_b), "La unidad de tiempo no puede ser nula.");

                        if (!long.TryParse(comando.user_command_extra.command_a, out long valor) || valor < 0)
                            throw new ArgumentException("El valor de tiempo debe ser un número positivo.");

                        if (!unidades.TryGetValue(unidad, out var transformador))
                            throw new ArgumentException($"Unidad de tiempo no válida: {unidad}");

                        var (segundos, descripcion) = transformador(valor);

                        // Calcular fecha de apagado
                        DateTime dueDate = DateTime.Now.AddSeconds(segundos);

                        // Respuesta al usuario
                        respuesta = $"En {descripcion}, el equipo se apagará.";

                        Process.Start("shutdown.exe", $"-s -t {segundos}");
                    }
                }
                ,
                "cancelar_apagado_reinicio" => () =>
                {
                    respuesta = "Acción cancelada";
                    Process.Start("shutdown.exe", "/a");
                }
                ,
                "reiniciar_equipo" => () =>
                {
                    if (comando.user_command_extra.command_a == string.Empty || comando.user_command_extra.command_a == null)
                    {
                        respuesta = $"En 30 segundos, el equipo se apagará.";
                        Process.Start("shutdown.exe", $"-r -t 30");
                    }
                    else
                    {
                        var unidades = new Dictionary<string, Func<long, (long segundos, string descripcion)>>
                        {
                            ["segundos"] = secs => (secs, $"{secs} segundo(s)"),
                            ["minutos"] = mins => (mins * 60, $"{mins} minuto(s)"),
                            ["horas"] = horas => (horas * 3600, $"{horas} hora(s)"),
                            ["dias"] = dias => (dias * 86400, $"{dias} día(s)")
                        };

                        // Obtener valores del comando
                        string unidad = comando.user_command_extra.command_b?.Trim().ToLower()
                            ?? throw new ArgumentNullException(nameof(comando.user_command_extra.command_b), "La unidad de tiempo no puede ser nula.");

                        if (!long.TryParse(comando.user_command_extra.command_a, out long valor) || valor < 0)
                            throw new ArgumentException("El valor de tiempo debe ser un número positivo.");

                        if (!unidades.TryGetValue(unidad, out var transformador))
                            throw new ArgumentException($"Unidad de tiempo no válida: {unidad}");

                        var (segundos, descripcion) = transformador(valor);

                        // Calcular fecha de apagado
                        DateTime dueDate = DateTime.Now.AddSeconds(segundos);

                        // Respuesta al usuario
                        respuesta = $"En {descripcion}, el equipo se apagará.";

                        Process.Start("shutdown.exe", $"-r -t {segundos}");
                    }
                }
                ,
                _ => null // Caso por defecto si no coincide ningún comando
            };

            action?.Invoke();
            return respuesta;
        }
        private static string Accion(ModelResponse comando)
        {
            Action action = comando.user_command switch
            {
                "captura_monitor_actual" => () =>
                {
                    Screenshots.Screen_Primary();
                    _ = Engrane.MP3_Player(Path.Combine(Environment.CurrentDirectory, "Sonidos", "shot.mp3"));
                }
                ,
                "captura_monitor_todos" => () =>
                {
                    Screenshots.Screen_All();
                    _ = Engrane.MP3_Player(Path.Combine(Environment.CurrentDirectory, "Sonidos", "shot.mp3"));
                }
                ,
                "accion_sistema_eliminar" => () =>
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.DELETE),
                "menu_inicio" => () =>
                    SendKeys.SendWait("^{ESC}"),
                "carpeta_anterior" => () =>//no asignado
                    InputSimulator.Keyboard.PRESSKEY(InputSimulator.Keyboard.VirtualKeyShort.BACK),
                _ => null // Caso por defecto si no coincide ningún comando
            };

            action?.Invoke();
            return string.Empty;
        }
        private static string Raton(ModelResponse comando)
        {
            Action action = comando.user_command switch
            {
                "raton_click_izquierdo" => () =>
                {
                    InputSimulator.MouseController.Click.Izquierdo();
                    _ = Engrane.MP3_Player(Path.Combine(Environment.CurrentDirectory, "Sonidos", "single-click.mp3"));
                }
                ,
                "raton_click_derecho" => () =>
                {
                    InputSimulator.MouseController.Click.Derecho();
                    _ = Engrane.MP3_Player(Path.Combine(Environment.CurrentDirectory, "Sonidos", "single-click.mp3"));
                }
                ,
                "raton_click_doble_izquierdo" => () =>
                {
                    InputSimulator.MouseController.Click.Izquierdo();
                    InputSimulator.MouseController.Click.Izquierdo();
                    _ = Engrane.MP3_Player(Path.Combine(Environment.CurrentDirectory, "Sonidos", "click-double.mp3"));
                }
                ,
                "raton_click_doble_derecho" => () =>
                {
                    InputSimulator.MouseController.Click.Derecho();
                    InputSimulator.MouseController.Click.Derecho();
                    _ = Engrane.MP3_Player(Path.Combine(Environment.CurrentDirectory, "Sonidos", "click-double.mp3"));
                }
                ,
                "mouse_scroll_abajo" => () =>
               InputSimulator.MouseController.ScrollSimulator.ScrollDown(-3),
                "mouse_scroll_subir" => () =>
                    InputSimulator.MouseController.ScrollSimulator.ScrollUp(3),
                "raton_click_mantener_izquierdo" => () =>
                    InputSimulator.MouseController.Click.IzquierdoMantener(),
                "raton_click_soltar_izquierdo" => () =>
                    InputSimulator.MouseController.Click.IzquierdoSoltar(),
                _ => null // Caso por defecto si no coincide ningún comando
            };

            action?.Invoke();
            return string.Empty;
        }
        private static string Lectura(ModelResponse comando)
        {
            string respuesta = string.Empty;
            Action action = comando.user_command switch
            {
                "lectura_leer" => () =>
                {
                    SendKeys.SendWait("^c");
                    respuesta = Clipboard.GetText();
                }
                ,
                "lectura_detener" => () =>
                {
                    if (Engrane.File_Config().VozIA)
                    {
                        _ = Engrane._piper.ReinitializeAsync(Engrane.File_Config().Voz);
                    }
                    else
                    {
                        Engrane.AIRH_Voz.Pause();
                        Engrane.AIRH_Voz.SpeakAsyncCancelAll();
                        Engrane.AIRH_Voz.Resume();
                    }
                }
                ,
                _ => null // Caso por defecto si no coincide ningún comando
            };

            action?.Invoke();
            return respuesta;
        }
        private static async Task<string> ComandosOnlineAsync(ModelResponse comando)
        {
            string respuesta = string.Empty;

            Func<Task> action = comando.user_command switch
            {
                "online_clima" => () =>
                {
                    respuesta = SolicitudOnline.ObtenerClima(); // ✅ Sin await
                    return Task.CompletedTask;
                }
                ,

                "ubicacion_actual" => () =>
                {
                    respuesta = "Tu ubicación aproximada según tu IP es la siguiente";
                    Engrane.EXE($"https://www.google.com/maps/@{GeolocationCoordinates.GetCoordinatesForGoogleMaps()}z?hl=es");
                    return Task.CompletedTask;
                }
                ,

                "buscar_internet" => () =>
                {
                    respuesta = "Entendido";
                    SendKeys.SendWait("^c");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://www.google.co.ve/search?q={comando.user_command_extra.command_a}",
                        UseShellExecute = true
                    });
                    return Task.CompletedTask;
                }
                ,

                "descargar_multimedia" => () =>
                {
                    if (comando.user_command_extra.command_b == "audio")
                    {
                        if (string.IsNullOrEmpty(comando.user_command_extra.command_a))
                        {
                            YouTubePack youTube = new YouTubePack(1, ClassView.SolicitudOnline.GetWebURL(), Engrane.File_Config().Dir_Musica, Engrane.File_Config().Asistente);
                            youTube.Show();
                        }
                        else
                        {
                            YouTubePack youTube = new YouTubePack(3, comando.user_command_extra.command_a + " " + (comando.user_command_extra.command_b ?? ""), Engrane.File_Config().Dir_Musica, Engrane.File_Config().Asistente);
                            youTube.Show();
                        }
                    }
                    else if (comando.user_command_extra.command_b == "video")
                    {
                        if (string.IsNullOrEmpty(comando.user_command_extra.command_a))
                        {
                            YouTubePack youTube = new YouTubePack(2, ClassView.SolicitudOnline.GetWebURL(), Engrane.File_Config().Dir_Videos, Engrane.File_Config().Asistente);
                            youTube.Show();
                        }
                        else
                        {
                            YouTubePack youTube = new YouTubePack(4, comando.user_command_extra.command_a + " " + (comando.user_command_extra.command_b ?? ""), Engrane.File_Config().Dir_Videos, Engrane.File_Config().Asistente);
                            youTube.Show();
                        }
                    }
                    respuesta = "Descargando multimedia...";
                    return Task.CompletedTask;
                }
                ,

                "buscar_internet_imagen" => () =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://www.google.com/search?tbm=isch&q={comando.user_command_extra.command_a}",
                        UseShellExecute = true
                    });
                    return Task.CompletedTask;
                }
                ,

                "buscar_internet_video" => () =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"https://www.youtube.com/results?search_query={comando.user_command_extra.command_a}",
                        UseShellExecute = true
                    });
                    return Task.CompletedTask;
                }
                ,

                "traducir_texto" => async () =>
                {
                    SendKeys.SendWait("^c");
                    await Task.Delay(100); // Esperar a que se copie al portapapeles
                    string texto = System.Windows.Clipboard.GetText();
                    string traduccion = await Py.Script_EXE(Py.Traductor, texto);
                    respuesta = traduccion;
                }
                ,

                "medir_velocidad_descarga" => () =>
                {
                    respuesta = "Calculando velocidad de descarga";
                    MedirVelocidad velocidad = new MedirVelocidad("D");
                    velocidad.Show();
                    return Task.CompletedTask;
                }
                ,

                "medir_velocidad_subida" => () =>
                {
                    respuesta = "Calculando velocidad de subida";
                    MedirVelocidad velocidad = new MedirVelocidad("U");
                    velocidad.Show();
                    return Task.CompletedTask;
                }
                ,

                "medir_velocidad_ping" => () =>
                {
                    respuesta = "Calculando latencia";
                    MedirVelocidad velocidad = new MedirVelocidad("P");
                    velocidad.Show();
                    return Task.CompletedTask;
                }
                ,

                "ofertas_epicgames" => () =>
                {
                    _ = GameSearch.ObtenerJuegosEpicGratis();
                    return Task.CompletedTask;
                }
                ,

                "ofertas_steam" => () =>
                {
                    _ = GameSearch.ObtenerOfertasSteam();
                    return Task.CompletedTask;
                }
                ,
                "whatsapp_mensaje" => async () => //revision
                {
                    var telefono = DB_Lite.ConsultarContactos(comando.user_command_extra.command_a);
                    string args = @$"""{telefono}"" ""{comando.user_command_extra.command_b}""";
                    string result = await Py.Script_EXE(Py.WhatsApp, args);

                    if (result.Equals("Success"))
                    {
                        respuesta = "mensaje enviado";
                    }
                    else
                    {
                        respuesta = "fallo al enviar el mensaje";
                    }
                }
                ,
                _ => () =>
                {
                    return Task.CompletedTask;
                }
            };

            // ✅ SOLO UNA LLAMADA - y siempre ejecutar (no verificar null)
            await action();

            return respuesta;
        }
        private static string Directorios(ModelResponse comando)
        {
            string respuesta = string.Empty;
            Action action = comando.user_command switch
            {
                "asistente_manual" => () =>
                    Engrane.EXE(Path.Combine(Environment.CurrentDirectory, "Manual.pdf")),
                "directorio_carpeta_notas_voz" => () =>
                    Engrane.EXE(Engrane.File_Config().Dir_Notas),
                "directorio_carpeta_documentos" => () =>
                    Engrane.EXE(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
                "directorio_carpeta_videos" => () =>
                    Engrane.EXE(Engrane.File_Config().Dir_Videos),
                "directorio_carpeta_captura_pantalla" => () =>
                    Engrane.EXE(Engrane.File_Config().Dir_Pantalla_Capturas),
                "directorio_carpeta_musica" => () =>
                    Engrane.EXE(Engrane.File_Config().Dir_Musica),
                "directorio_carpeta_imagenes" => () =>
                    Engrane.EXE(Engrane.File_Config().Dir_Imagenes),
                "temporizador_alarma" => () =>
                    Temporizador.RegistroTemporizador(comando.user_command_extra),
                "cerrar_asistente" => () =>
                    App.Current.Shutdown(),
                _ => null // Caso por defecto si no coincide ningún comando
            };

            action?.Invoke();
            return respuesta;
        }
        private static string Comandos_Contacto(ModelResponse comando)
        {
            if (!comando.user_command.StartsWith("contacto_", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var telefono = DB_Lite.ConsultarContactos(comando.user_command);

            if (telefono != "NaN")
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    telefono,
                    "AV-AIRH MAX",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
            }
            return telefono;
        }

        private static string Comandos_Discord(ModelResponse comando)
        {
            if (!comando.user_command.StartsWith("discord_", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var canal = DB_Lite.ConsultarDiscord(comando.user_command);

            if (canal != "NaN")
            {
                var mensaje = new Models.DiscordMessage
                {
                    WebhookUrl = canal,
                    Mensaje = comando.user_command_extra.command_a,
                    Asistente = Engrane.File_Config().Asistente,
                    Usuario = Engrane.File_Config().Usuario
                };

                Task.Run(async () => await Services.DiscordMessage.SendEmbedAsync(mensaje, Theme.MessageDiscord.Type.Info));
            }

            return comando.model_response; // Si necesitas retornar esto
        }
        private static string Comandos_Usuario(ModelResponse comando)
        {
            return DB_Lite.EjecutarComandoPersonal(comando.user_command);
        }
        private static string Comandos_Gamer(ModelResponse comando)
        {
            return GamePass.Engrane.GamerCommand(comando.user_command);
        }
        private static string Comandos_Arduino(ModelResponse comando)
        {
            return DB_Lite.EjecutarAccionArduino(comando.user_command);
        }
        private static string Comandos_WindowsShell(ModelResponse comando)
        {
            return string.Empty;
            //return ShellWindows.StartShell(comando.user_command);
            //analizar si se elimina
        }
        public static async Task<string> Comandos_IA(ModelResponse comando)
        {
            string resultado;

            resultado = Raton(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Directorios(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Accion(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Lectura(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Compresor(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = await Conversor(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Reproductor(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = WebExplorer(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Sistema(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = await ComandosOnlineAsync(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Comandos_Contacto(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Comandos_Usuario(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Comandos_Gamer(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Comandos_Arduino(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Comandos_Discord(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            resultado = Comandos_WindowsShell(comando);
            if (!string.IsNullOrWhiteSpace(resultado)) return resultado;

            return string.Empty;
        }
    }
}