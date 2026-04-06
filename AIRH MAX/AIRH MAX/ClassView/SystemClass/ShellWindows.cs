using System.Diagnostics;

namespace AIRH_MAX.ClassView.SystemClass
{
    internal class ShellWindows
    {
        static Dictionary<string, string> Network = new Dictionary<string, string>
        {
            { "agregar_ubicacion_red_win", "shell:::{D4480A50-BA28-11d1-8E75-00C04FA31A86}" },
            { "propiedades_conexion_win", @"shell:::{241D7C96-F8BF-4F85-B01F-E2B043341A4B}\PropertiesPage" },
            { "lugares_red_win", @"shell:::{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}" },
            { "red_e_internet_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\3" },
            { "centro_comparticion_red_win", @"shell:::{8E908FC9-BECC-40f6-915B-F4CA0E70D03D}" },
            { "conexiones_red_win", @"shell:::{7007ACC7-3202-11D1-AAD2-00805FC1270E}" },
            { "configuracion_avanzada_compartir_win", @"shell:::{8E908FC9-BECC-40f6-915B-F4CA0E70D03D}\Advanced" },
            { "red_principal_win", @"shell:::{F02C1A0D-BE21-4586-A844-36FE4BEC8B6D}" },
            { "redes_centro_compartido_win", @"shell:::{8E908FC9-BECC-40f6-915B-F4CA0E70D03D}" },
            { "centro_red_trabajo", @"shell:::{208D2C60-3AEA-1069-A2DE-08002B30309D}" }
        };

        static Dictionary<string, string> EaseOfAccess = new Dictionary<string, string>
        {
            { "centro_accesibilidad_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}" }, 
            { "cognicion_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageQuestionsCognitive" }, 
            { "facilitar_enfoque_tareas_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageEasierToReadAndWrite" }, 
            { "facilitar_vision_computadora_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageEasierToSee" }, 
            { "facilitar_uso_teclado_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageKeyboardEasierToUse" }, 
            { "facilitar_uso_mouse_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageEasierToClick" }, 
            { "alternativas_texto_sonidos_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageEasierWithSounds" },
            { "usar_computadora_sin_pantalla_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageNoVisual" }, 
            { "usar_computadora_sin_mouse_teclado_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageNoMouseOrKeyboard" }, 
            { "vision_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageQuestionsEyesight" }, 
            { "facilidad_acceso_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\7" } 
        };

        static Dictionary<string, string> Troubleshooting = new Dictionary<string, string>
        {
            { "informacion_adicional_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\resultPage" }, 
            { "configuraciones_avanzadas_informes_problemas_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}\pageAdvSettings" },
            { "centro_solucion_problemas_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}" }, 
            { "solucionar_problemas_hardware_y_sonido_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\devices" }, 
            { "solucionar_problemas_red_e_internet_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\network" }, 
            { "solucionar_problemas_programas_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\applications" }, 
            { "solucionar_problemas_sistema_y_seguridad_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\system" } 
        };

        static Dictionary<string, string> ControlPanel = new Dictionary<string, string>
        {
            { "panel_control_win", @"shell:::{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}" },
            { "vista_categorias_panel_control_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}" },
            { "vista_iconos_panel_control_win", @"shell:::{21EC2020-3AEA-1069-A2DD-08002B30309D}" },
            { "panel_control_todas_tareas_win", @"shell:::{ED7BA470-8E54-465E-825C-99712043E01C}" },
            { "crear_plan_energia_win", @"shell:::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}\pageCreateNewPlan" },
            { "autoplay_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{9C60DE1E-E5FC-40F4-A487-460851A8D915}" },
            { "backup_restauracion_windows_7_win", @"shell:::{B98A2BEA-7D42-4558-8BD1-832F41BAC6FD}" },
            { "dispositivos_bluetooth_win", @"shell:::{28803F59-3A75-4058-995F-4EE5503B023C}" },
            { "bitLocker_cifrado_unidad_win", @"shell:::{D9EF8727-CAC2-4e60-809E-86F80A666C91}" },
            { "cambiar_seguridad_mantenimiento_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}\Settings" },
            { "cambiar_configuraciones_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\settingPage" },
            { "cambiar_nombre_win", @"shell:::{60632754-c523-4b62-b45c-4172da012619}\pageRenameMyAccount" },
            { "reloj_region_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\6" },
            { "color_apariencia_win", @"shell:::{ED834ED6-4B5A-4bfe-8F11-A626DCB6A921}\pageColorization" },
            { "gestion_color_win", @"shell:::{B2C761C6-29BC-4f19-9251-E6195265BAF1}" },
            { "carpeta_comandos_win", @"shell:::{437ff9c0-a07f-4fa0-af80-84b6c6440a16}" },
            { "carpetas_comunes_win", @"shell:::{d34a6ca6-62c2-4c34-8a7c-14709c1ad938}" },
            { "administrador_credenciales_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{1206F5F1-0569-412C-8FEC-3204630DFB70}" },
            { "firewall_windows_win", @"shell:::{4026492F-2F69-46B8-B9BF-5654FC07E423}\PageConfigureSettings" },
            { "programas_predeterminados_win", @"shell:::{2559a1f7-21d7-11d4-bdaf-00c04f60b9f0}" },
            { "fondo_escritorio_win", @"shell:::{ED834ED6-4B5A-4bfe-8F11-A626DCB6A921}\pageWallpaper" },
            { "administrador_dispositivos_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{74246BFC-4C96-11D0-ABEF-0020AF6B0B7A}" },
            { "apariencia_personalizacion_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\1" },
            { "pagina_apps_predeterminadas_win", @"shell:::{17cd9488-1228-4b2f-88ce-4298e93e0966}\pageDefaultProgram" },
            { "carpeta_delegada_win", @"shell:::{b155bdf8-02f0-451e-9a26-ae317cfd7779}" },
            { "dispositivos_impresoras_win", @"shell:::{A8A91A66-3A7D-4424-8D24-04E180695C7A}" },
            { "historial_archivos_win", @"shell:::{F6B6E965-E9B2-444B-9286-10C9152EDBC5}" },
            { "opciones_carpeta_win", @"shell:::{6DFD7C5C-2451-11d3-A299-00C04F8EF6AF}" },
            { "configuracion_fuente_win", @"shell:::{93412589-74D4-4e4e-AD0E-E0CB621440FD}" },
            { "carpeta_fuentes_win", @"shell:::{BD84B380-8CA2-1069-AB1D-08000948F534}" },
            { "carpetas_frecuentes_win", @"shell:::{3936E9E4-D92C-4EEE-A85A-BC16D5EA0819}" },
            { "explorador_juegos_win", @"shell:::{ED228FDF-9EA8-4870-83b1-96b02CFE0D52}" },
            { "ayuda_soporte_win", @"shell:::{2559a1f1-21d7-11d4-bdaf-00c04f60b9f0}" },
            { "opciones_indexacion_win", @"shell:::{87D66A43-7B11-4A28-9811-C86EE395ACF7}" },
            { "actualizaciones_instaladas_win", @"shell:::{d450a8a1-9568-45c7-9c0e-b4f9fb4537bd}" },
            { "opciones_internet_win", @"shell:::{A3DD4F92-658A-410F-84FD-6FBBBEF2FFFE}" },
            { "propiedades_teclado_win", @"shell:::{725BE8F7-668E-4C7B-8F90-46BDB0936430}" },
            { "informacion_ubicacion_win", @"shell:::{40419485-C444-4567-851A-2DD7BFA1684D}" },
            { "configuracion_ubicacion_win", @"shell:::{E9950154-C418-419e-A90A-20C5287AE24B}" },
            { "servidores_media_win", @"shell:::{289AF617-1CC3-42A6-926C-E6A863F0E3BA}" },
            { "propiedades_mouse_win", @"shell:::{6C8EEC18-8D75-41B2-A177-8831D59D2D50}" },
            { "netplwiz_win", @"shell:::{7A9D77BD-5403-11d2-8785-2E0420524153}" },
            { "iconos_area_notificacion_win", @"shell:::{05d7b0f4-2121-4eff-bf6b-ed3f69b894d9}" },
            { "archivos_offline_win", @"shell:::{AFDB1F70-2A4C-11d2-9039-00C04F8EEB3E}" },
            { "oneDrive_win", @"shell:::{018D5C66-4533-4307-9B53-224DE2ED1FE6}" },
            { "lápiz_tactil_win", @"shell:::{F82DF8F7-8B9F-442E-A48C-818EA735FF9B}" },
            { "personalizacion_win", @"shell:::{ED834ED6-4B5A-4bfe-8F11-A626DCB6A921}" },
            { "dispositivos_portatiles_win", @"shell:::{35786D3C-B075-49b9-88DD-029876E11C01}" },
            { "opciones_energia_win", @"shell:::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}" },
            { "versiones_previas_win", @"shell:::{f8c2ab3b-17bc-41da-9758-339d7dbf2d88}" },
            { "impresoras_win", @"shell:::{2227A280-3AEA-1069-A2DE-08002B30309D}" },
            { "configuracion_reporte_problemas_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}" },
            { "configuracion_informacion_problemas_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}\pageSettings" },
            { "programas_caracteristicas_win", @"shell:::{7b81be6a-ce2b-4676-a29e-eb907a5126c5}" },
            { "acceso_rapido_win", @"shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}" },
            { "carpetas_recientes_win", @"shell:::{22877a6d-37a1-461a-91b0-dbda5aaebc99}" },
            { "elementos_recientes_win", @"shell:::{4564b25e-30cd-4787-82ba-39e73a750b14}" },
            { "recuperacion_win", @"shell:::{9FE63AFD-59CF-4419-9775-ABCC3849F861}" },
            { "informacion_adicional_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\resultPage" },
            { "todas_categorias_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\listAllPage" },
            { "historial_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\historyPage" },
            { "busqueda_solucion_problemas_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\searchPage" },
            { "elementos_anclados_win", @"shell:::{1f3427c8-5c10-4210-aa03-2ee45287d668}" },
            { "navegador_predeterminado_win", @"shell:::{871C5380-42A0-1069-A2EA-08002B30309D}" },
            { "firewall_windows_defender_win", @"shell:::{4026492F-2F69-46B8-B9BF-5654FC07E423}" },
            { "apps_permitidas_win", @"shell:::{4026492F-2F69-46B8-B9BF-5654FC07E423}\pageConfigureApps" },
            { "restaurar_predeterminados_win", @"shell:::{4026492F-2F69-46B8-B9BF-5654FC07E423}\PageRestoreDefaults" },
            { "centro_movilidad_windows_win", @"shell:::{5ea4f148-308c-46d7-98a9-49041b1dd468}" },
            { "caracteristicas_windows_win", @"shell:::{67718415-c450-4f3c-bf8a-b487642dc39b}" },
            { "windows_to_go_win", @"shell:::{8E0C279D-0BD1-43C3-9EBD-31C3DC5B8A77}" },
            { "carpetas_trabajo_win", @"shell:::{ECDB0924-4208-451E-8EE0-373C0956DE16}" },
            { "papelera_reciclaje_win", @"shell:::{645FF040-5081-101B-9F08-00AA002F954E}" },
            { "region_win", @"shell:::{62D8ED13-C9D0-4CE8-A914-47DD628FB1B0}" },
            { "monitor_confiabilidad_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}\pageReliabilityView" },
            { "asistencia_remota_win", @"shell:::{C58C4893-3BE0-4B45-ABB5-A63E4B8C8651}\raPage" },
            { "conexiones_remotas_win", @"shell:::{241D7C96-F8BF-4F85-B01F-E2B043341A4B}" },
            { "impresoras_remotas_win", @"shell:::{863aa9fd-42df-457b-8e4d-0de1b8015c60}" },
            { "unidades_extraibles_win", @"shell:::{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}" },
            { "dispositivos_almacenamiento_removible_win", @"shell:::{a6482830-08eb-41e2-84c1-73920c2badb9}" },
            { "carpeta_resultados_win", @"shell:::{2965e715-eb66-4719-b53f-1672673bbefa}" },
            { "ejecutar_win", @"shell:::{2559a1f3-21d7-11d4-bdaf-00c04f60b9f0}" },
            { "buscar_explorador_win", @"shell:::{9343812e-1c37-4a49-a12e-4b2d810d956b}" },
            { "buscar_windows_win", @"shell:::{2559a1f8-21d7-11d4-bdaf-00c04f60b9f0}" },
            { "seguridad_mantenimiento_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}" },
            { "detalles_problema_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}\pageReportDetails" },
            { "reportes_problemas_win", @"shell:::{BB64F8A7-BEE7-4E1A-AB8D-7D8273F7FDB6}\pageProblems" },
            { "mostrar_escritorio_win", @"shell:::{3080F90D-D7AD-11D9-BD98-0000947B0257}" },
            { "sonido_win", @"shell:::{F2DDFC82-8F12-4CDD-B7DC-D4FE1425AA4D}" },
            { "reconocimiento_voz_win", @"shell:::{58E3C745-D971-4081-9034-86E34B30836A}" },
            { "espacios_almacenamiento_win", @"shell:::{F942C606-0914-47AB-BE56-1321B8035096}" },
            { "centro_sincronizacion_win", @"shell:::{9C73F5E5-7AE7-4E32-A8E8-8D23B85255BF}" },
            { "configuracion_sincronizacion_win", @"shell:::{2E9E59C0-B437-4981-A647-9C34B9B90891}" },
            { "sistema_win", @"shell:::{BB06C0E4-D293-4f75-8A90-CB05B6477EEE}" },
            { "iconos_sistema_win", @"shell:::{05d7b0f4-2121-4eff-bf6b-ed3f69b894d9}\SystemIcons" },
            { "restaurar_sistema_win", @"shell:::{3f6bc534-dfa1-4ab4-ae54-ef25a74e0107}" },
            { "configuracion_tablet_pc_win", @"shell:::{80F3F1D5-FECA-45F3-BC32-752C152E456E}" },
            { "propiedades_barra_tareas_win", @"shell:::{0DF44EAA-FF21-4412-828E-260A8728E7F1}" },
            { "texto_voz_win", @"shell:::{D17D1D6D-CC3F-4815-8FE3-607E7D5D10B3}" },
            { "este_dispositivo_win", @"shell:::{5b934b42-522b-4c34-bbfe-37a3ef7b9c90}" },
            { "este_equipo_win", @"shell:::{20D04FE0-3AEA-1069-A2D8-08002B30309D}" }
        };

        static Dictionary<string, string> Servicios = new Dictionary<string, string>
        {
            { "correo_win", @"shell:::{2559a1f5-21d7-11d4-bdaf-00c04f60b9f0}" }, 
            { "configuracion_fuente_win", @"shell:::{93412589-74D4-4E4E-AD0E-E0CB621440FD}" }, 
            { "galeria_win", @"shell:::{E88865EA-0E1C-4E20-9AA6-EDCD0212C87C}" }, 
            { "obtener_programas_win", @"shell:::{15eae92e-f17a-4431-9f28-805e482dafd4}" },
            { "hardware_y_sonido_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\2" },
            { "hyper_v_win", @"shell:::{0907616E-F5E6-48D8-9D61-A91C3D28106D}" }, 
            { "opciones_indexacion_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{87D66A43-7B11-4A28-9811-C86EE395ACF7}" }, 
            { "teclado_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{725BE8F7-668E-4C7B-8F90-46BDB0936430}" },
            { "linux_win", @"shell:::{B2B4A4D1-2754-4140-A2EB-9A76D9D7CDC6}" }, 
            { "administrar_cuentas_win", @"shell:::{60632754-c523-4b62-b45c-4172da012619}\pageAdminTasks" }, 
            { "opciones_streaming_medios_win", @"shell:::{8E908FC9-BECC-40f6-915B-F4CA0E70D03D}\ShareMedia" }, 
            { "imprimir_a_pdf_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{A8A91A66-3A7D-4424-8D24-04E180695C7A}\Provider%5CMicrosoft.Base.DevQueryObjects//DDO:%7BB02567CB-B5D8-11EE-8D8C-74D83E9479BB%7D" }, // Imprimir a PDF
            { "programas_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\8" }, 
            { "recuperacion_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{9FE63AFD-59CF-4419-9775-ABCC3849F861}" }, 
            { "configurar_teclas_filtro_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageFilterKeysSettings" }, 
            { "configurar_teclas_mouse_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageMouseKeysSettings" },
            { "configurar_teclas_repetir_lento_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageRepeatRateSlowKeysSettings" },
            { "configurar_teclas_adhesivas_win", @"shell:::{D555645E-D4F8-4c29-A827-D93C859C4F2A}\pageStickyKeysSettings" }, 
            { "sistema_y_seguridad_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\5" }, 
            { "configuracion_sistema_win", @"shell:::{025A5937-A6BE-4686-A844-36FE4BEC8B6D}\pageGlobalSettings" }, 
            { "mostrar_escritorios_win", @"shell:::{3080F90E-D7AD-11D9-BD98-0000947B0257}" }, 
            { "firewall_windows_defender_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{4026492F-2F69-46B8-B9BF-5654FC07E423}" }, 
            { "carpetas_trabajo_win", @"shell:::{26EE0668-A00A-44D7-9371-BEB064C98683}\0\::{ECDB0924-4208-451E-8EE0-373C0956DE16}" },
            { "carpeta_objetos_win", @"shell:::{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}" }, 
            { "herramientas_administrativas_win", @"shell:::{D20EA4E1-3957-11d2-A40B-0C5020524153}" }
        };

        public static string[] AllServicesPrompt()
        {
            string JoinIfNotEmpty(IEnumerable<string> items) => items.Any() ? string.Join("\n", items) : null;

            var parts = new[]
            {
                JoinIfNotEmpty(Network.Keys),
                JoinIfNotEmpty(EaseOfAccess.Keys),
                JoinIfNotEmpty(Troubleshooting.Keys),
                JoinIfNotEmpty(ControlPanel.Keys),
                JoinIfNotEmpty(Servicios.Keys)
            };

            return parts
                .Where(part => !string.IsNullOrEmpty(part))
                .SelectMany(part => part.Split('\n'))
                .ToArray();
        }

        public static string StartShell(string key)
        {
            Dictionary<string, string>[] DictionaryAll = [Network, EaseOfAccess, Troubleshooting, ControlPanel, Servicios];
            try
            {
                foreach (var Shells in DictionaryAll)
                {
                    if (Shells.TryGetValue(key, out string shellPath))
                    {
                        Process process = new Process();
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.FileName = shellPath;
                        process.Start();
                        return string.Empty;
                    }
                    else
                    {
                        //MainWindow.NotificacionEvent.Log = $"Error: La clave '{key}' no se encontró en el diccionario.";
                    }
                }
            }
            catch (Exception ex)
            {
                Views.MainWindow.NotificacionEvent.Log = $"Error al iniciar el proceso: {ex.Message}";
            }
            return string.Empty;
        }
    }
}