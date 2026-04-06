using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using PiperSharp.Models;
using System.Diagnostics;

namespace PiperSharp
{
    public static class PiperDownloader
    {
        private const string PIPER_REPO_URL = "https://github.com/rhasspy/piper";
        private const string MODEL_REPO_URL = "https://huggingface.co/rhasspy/piper-voices";
        private const string MODEL_LIST_URL = "https://huggingface.co/rhasspy/piper-voices/raw/main/voices.json";
        private const string MODEL_DOWNLOAD_URL =
            "https://huggingface.co/rhasspy/piper-voices/resolve/main/MODEL_FILE_URL?download=true";

        public static string DefaultLocation => Environment.CurrentDirectory + "\\tts\\";
        public static string DefaultModelLocation => Path.Join(DefaultLocation, "models");
        public static string DefaultPiperLocation => Path.Join(DefaultLocation, "piper");
        public static string DefaultPiperExecutableLocation => Path.Join(DefaultPiperLocation, PiperExecutable);
        public static string PiperExecutable => Environment.OSVersion.Platform == PlatformID.Win32NT ? "piper.exe" : "piper";

        private static Dictionary<string, VoiceModel>? _voiceModels;
        private static Regex RemoveLastSlash = new Regex(@"\/$", RegexOptions.Compiled);

        // Constructor estático para inicializar SevenZip
        static PiperDownloader()
        {
            // Configurar la ruta de 7z.dll (necesario para SharpSevenZip)
            // Puedes copiar 7z.dll a tu directorio de salida o especificar la ruta
            try
            {
                SharpSevenZip.SharpSevenZipBase.SetLibraryPath(@"x64\7z.dll");
            }
            catch
            {
                // Si no encuentra 7z.dll, SharpSevenZip usará modo managed (más lento)
                Debug.WriteLine("7z.dll not found, using managed mode");
            }
        }

        public static async Task<Stream> DownloadPiper(string version = "latest", string repo = PIPER_REPO_URL)
        {
            if (!Environment.Is64BitOperatingSystem)
                throw new NotSupportedException();
            var arch = typeof(object).Assembly.GetName().ProcessorArchitecture;
            var os = Environment.OSVersion.Platform;
            var fileName = os switch
            {
                PlatformID.Win32NT => "piper_windows_amd64.zip",
                PlatformID.Unix => arch == ProcessorArchitecture.Arm
                    ? "piper_linux_aarch64.tar.gz"
                    : "piper_linux_x86_64.tar.gz",
                PlatformID.MacOSX => arch == ProcessorArchitecture.Arm
                    ? "piper_macos_aarch64.tar.gz"
                    : "piper_macos_x64.tar.gz",
                _ => throw new NotSupportedException()
            };
            version = version == "latest" ? "latest/download" : $"download/{version}";
            var url = $"{RemoveLastSlash.Replace(repo, "")}/releases/{version}/{fileName}";
            var client = new HttpClient();
            var downloadStream = await client.GetStreamAsync(url);
            return downloadStream;
        }

        public static Task<string> ExtractPiper(this Task<Stream> downloadStream)
            => ExtractPiper(downloadStream, DefaultLocation);
        public static async Task<string> ExtractPiper(this Task<Stream> downloadStream, string extractTo)
            => ExtractPiper(await downloadStream, extractTo);

        public static string ExtractPiper(this Stream downloadStream, string extractTo)
        {
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            if (isWindows)
            {
                // Para Windows - usar ZipArchive nativo (más eficiente para ZIP)
                using var archive = new ZipArchive(downloadStream);
                archive.ExtractToDirectory(extractTo);
            }
            else
            {
                // Para Linux/macOS - usar SharpSevenZip para .tar.gz
                using (var extractor = new SharpSevenZip.SharpSevenZipExtractor(downloadStream))
                {
                    extractor.ExtractArchive(extractTo);

                    // Manejo de enlaces simbólicos (similar a la lógica original)
                    ProcessSymlinksForUnix(extractTo);
                }
            }
            return extractTo;
        }

        private static void ProcessSymlinksForUnix(string extractTo)
        {
            var piperPath = Path.Join(extractTo, "piper");
            if (!Directory.Exists(piperPath))
                return;

            // Buscar archivos que podrían necesitar ser copiados (simulación de enlaces simbólicos)
            var potentialLinks = FindPotentialSymlinks(piperPath);

            foreach (var link in potentialLinks)
            {
                try
                {
                    var sourceFile = Path.Join(piperPath, link.source);
                    var targetFile = Path.Join(piperPath, link.target);

                    if (File.Exists(sourceFile) && !File.Exists(targetFile))
                    {
                        File.Copy(sourceFile, targetFile, true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Warning: Could not process potential symlink {link.source} -> {link.target}: {ex.Message}");
                }
            }
        }

        private static List<(string source, string target)> FindPotentialSymlinks(string piperPath)
        {
            var potentialLinks = new List<(string source, string target)>();

            try
            {
                var files = Directory.GetFiles(piperPath, "*", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var relativePath = Path.GetRelativePath(piperPath, file);

                    // Detectar archivos que podrían ser enlaces simbólicos basado en patrones
                    if (IsPotentialSymlinkFile(fileName))
                    {
                        // Intentar encontrar el archivo destino basado en el nombre
                        var targetName = GuessTargetFromSource(fileName);
                        if (!string.IsNullOrEmpty(targetName))
                        {
                            potentialLinks.Add((relativePath, targetName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Error scanning for symlinks: {ex.Message}");
            }

            return potentialLinks;
        }

        private static bool IsPotentialSymlinkFile(string fileName)
        {
            // Patrones comunes de archivos que suelen ser enlaces simbólicos en paquetes Linux
            return fileName.StartsWith("lib") ||
                   fileName.EndsWith(".so") ||
                   fileName.Contains(".so.") ||
                   fileName.Length < 8; // Nombres cortos
        }

        private static string GuessTargetFromSource(string sourceName)
        {
            // Lógica simple para adivinar el destino del enlace
            if (sourceName.StartsWith("lib") && sourceName.Contains(".so"))
            {
                // Para librerías: libxyz.so.1 -> libxyz.so
                var baseName = sourceName.Split('.')[0] + ".so";
                return baseName;
            }
            return string.Empty;
        }

        public static async Task<VoiceModel?> GetModelByKey(string modelName)
        {
            await GetHuggingFaceModelList();
            return _voiceModels?.GetValueOrDefault(modelName);
        }

        public static async Task<VoiceModel?> TryGetModelByKey(string modelKey)
        {
            await GetHuggingFaceModelList();
            return _voiceModels?.GetValueOrDefault(modelKey);
        }

        public static async Task<Dictionary<string, VoiceModel>?> GetHuggingFaceModelList()
        {
            if (_voiceModels != null) return _voiceModels;
            var client = new HttpClient();
            var response = await client.GetAsync(MODEL_LIST_URL);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException();
            var data = await response.Content.ReadAsStringAsync();
            if (data is null) throw new ApplicationException();
            _voiceModels = JsonSerializer.Deserialize<Dictionary<string, VoiceModel>>(data);
            return _voiceModels;
        }

        public static async Task<VoiceModel> DownloadModelByKey(string modelKey)
        {
            var model = await GetModelByKey(modelKey);
            if (model is null)
            {
                throw new ArgumentException($"Model {modelKey} does not exist!", nameof(modelKey));
            }

            return await model.DownloadModel();
        }

        public static Task<VoiceModel> DownloadModel(this VoiceModel model)
            => model.DownloadModel(DefaultModelLocation);

        public static async Task<VoiceModel> DownloadModel(this VoiceModel model, string saveModelTo)
        {
            var path = Path.Join(saveModelTo, model.Key);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var client = new HttpClient();
            foreach (var file in model.Files.Keys)
            {
                var fileName = Path.GetFileName(file);
                var filePath = Path.Join(path, fileName);
                var downloadStream = await client.GetStreamAsync(MODEL_DOWNLOAD_URL.Replace("MODEL_FILE_URL", file));

                // Load Audio configuration from .onnx.json file
                if (fileName.EndsWith(".onnx.json"))
                {
                    var ms = new MemoryStream();
                    await downloadStream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var modelJson = await JsonSerializer.DeserializeAsync<VoiceModel>(ms);
                    model.Audio = modelJson!.Audio;
                    ms.Seek(0, SeekOrigin.Begin);
                    await using var fs = File.OpenWrite(filePath);
                    await ms.CopyToAsync(fs);
                    fs.Close();
                }
                else
                {
                    await using var fs = File.OpenWrite(filePath);
                    await downloadStream.CopyToAsync(fs);
                    fs.Close();
                }
                downloadStream.Close();
            }

            model.ModelLocation = path;
            await using var modelInfoFile = File.OpenWrite(Path.Join(path, "model.json"));
            await JsonSerializer.SerializeAsync<VoiceModel>(modelInfoFile, model, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            modelInfoFile.Close();

            return model;
        }
    }
}