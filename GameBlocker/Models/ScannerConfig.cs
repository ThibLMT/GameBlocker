namespace GameBlocker.Models
{
    public class ScannerConfig
    {
        public List<string> IgnoredFiles { get; set; } = new();
        public List<string> IgnoredKeywords { get; set; } = new();
    }
}
