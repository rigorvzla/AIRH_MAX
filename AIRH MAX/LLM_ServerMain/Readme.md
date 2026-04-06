# 🧠 LLM_ServerMain - Servidor Unificado para LLM (WebSocket) y OCR

## 📦 Descripción General

**LLM_ServerMain** es una librería unificada que integra **dos servidores independientes** para potenciar AV-AIRH MAX:

- **llama-server** para **OCR** (reconocimiento óptico de caracteres) - Proceso independiente
- **LLM WebSocket Server (Python compilado a EXE)** para **chat y razonamiento** - Conexión vía WebSocket

Ambos servidores funcionan de manera **independiente** pero son gestionados desde una misma API en .NET, permitiendo al asistente **leer texto de imágenes** y **mantener conversaciones inteligentes** con un solo punto de control.

### 🎯 ¿Para quién es?

- ✅ AV-AIRH MAX y sus componentes de visión/OCR
- ✅ Aplicaciones que necesitan **chat vía WebSocket + reconocimiento de imágenes**
- ✅ Proyectos offline que requieren **privacidad total**
- ✅ Desarrolladores que integran el **LLM Server Python** en .NET

---

## ✨ Características Principales

| Característica | Estado | Descripción |
| :--- | :--- | :--- |
| **LLM vía WebSocket** | ✅ | Conexión con servidor Python compilado |
| **OCR con llama-server** | ✅ | Extrae texto de imágenes usando visión |
| **Servidores independientes** | ✅ | OCR y LLM funcionan por separado |
| **Gestión unificada** | ✅ | Una API controla ambos servidores |
| **100% offline** | ✅ | Sin dependencia de APIs externas |
| **Auto-detección GPU** | ✅ | Usa CUDA si está disponible |
| **Health checks** | ✅ | Monitoreo de estado de servidores |
| **Auto-reinicio** | ✅ | Recuperación automática de fallos |

---
