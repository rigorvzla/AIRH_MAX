using System;

namespace PiperSharp.Models
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public string[] MissingFiles { get; set; } = Array.Empty<string>();
        public string ModelName { get; set; }
        public string ModelDirectory { get; set; }
    }
}
