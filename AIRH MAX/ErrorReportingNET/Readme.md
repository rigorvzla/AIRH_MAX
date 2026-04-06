# 📡 ErrorReportingNET - Reporte de Errores para AV-AIRH

## 📦 Descripción General

**ErrorReportingNET** es una librería liviana para **capturar y reportar errores** en aplicaciones .NET, enviando notificaciones automáticas a **Telegram** (mediante bot) o **Discord** (mediante webhook). Diseñada específicamente para AV-AIRH MAX y sus componentes, permite monitorear fallos en tiempo real sin necesidad de logs locales.

### 🎯 ¿Para quién es?

- ✅ Desarrolladores que necesitan monitorear errores en producción
- ✅ Aplicaciones .NET que requieren alertas inmediatas
- ✅ Proyectos AV-AIRH MAX y sus componentes
- ✅ Equipos que desean centralizar reportes en Telegram/Discord

---

## ✨ Características Principales

| Característica | Estado | Descripción |
| :--- | :--- | :--- |
| **Telegram Bot** | ✅ | Envía errores a tu chat de Telegram |
| **Discord Webhook** | ✅ | Publica errores en canal de Discord |
| **Múltiples canales** | ✅ | Envía a Telegram y Discord simultáneamente |
| **Stack trace completo** | ✅ | Incluye línea, archivo y pila de llamadas |
| **Contexto adicional** | ✅ | SO, versión app, usuario, fecha/hora |
| **Filtrado por nivel** | ✅ | Error, Warning, Info, Debug |
| **Rate limiting** | ✅ | Evita spam en canales |
| **Modo silencioso** | ✅ | Reportes locales sin enviar |
| **Callback personalizado** | ✅ | Procesa errores antes de enviar |

---
