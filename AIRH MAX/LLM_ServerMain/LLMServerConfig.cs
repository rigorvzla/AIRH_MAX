using LLM_ServerMain;

/// <summary>
/// Configuración del servidor LLM
/// </summary>
public class LLMServerConfig
{
    public ServerMode Mode { get; set; } = ServerMode.LocalExecutable; // ✅ CAMBIADO
    public string ModelName { get; set; }
    public string ApiKey { get; set; }
    public string Endpoint { get; set; }
    public int Port { get; set; } = 8766;
    public string ServerExePath { get; set; } = "llm\\llm-server.exe"; // ✅ CAMBIADO: ServerScriptPath → ServerExePath
    public string ModelsDirectory { get; set; } = "llm\\models";
    public int ContextSize { get; set; } = 4096;
    public bool UseGPU { get; set; } = false;
    public int MaxTokens { get; set; } = 512;
    public float Temperature { get; set; } = 0.1f;
    public string SystemPrompt { get; set; }
    public int GpuLayers { get; set; } = 0;
}