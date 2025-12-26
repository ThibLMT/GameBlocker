using System;
using System.IO;
using static System.Net.WebRequestMethods;

namespace GameBlocker.Services
{
    public class GameScannerService
    {

        // Field to hold the logger
        private readonly ILogger<GameScannerService> _logger;

        // Constructor
        public GameScannerService(ILogger<GameScannerService> logger)
        {
            _logger = logger;
        }

        public Dictionary<string, List<String>> ScanGames(string gamesDirectory)
        {
            try
            {
                var exeFilesPaths = Directory.EnumerateFiles(gamesDirectory, "*.exe", SearchOption.AllDirectories);
                _logger.LogDebug("Successfully scanned this many .exe: {count}, wit this being the first one: {firstExe}", exeFilesPaths.Count(), exeFilesPaths.First());

                // We now need to filter the .exe and group them
                var groups = exeFilesPaths.GroupBy(file =>
                {
                    // Logic to find the "Game Name" (First folder after root)
                    var relative = Path.GetRelativePath(gamesDirectory, file);
                    var parts = relative.Split(Path.DirectorySeparatorChar);
                    string gameName = parts[0];

                    if (gameName.Equals("steamapps", StringComparison.OrdinalIgnoreCase) && parts.Length > 2)
                    {
                        // A:\Games\steamapps\common\Battlefield 6 -> Game Name is "Battlefield 6"
                        gameName = parts[2];
                    }
                    return gameName;
                });

                return groups.OrderBy(group => group.Key).ToDictionary(
                    group => group.Key,
                    group => group.ToList()
                );
            }
            catch (global::System.Exception e)
            {
                throw e;
            }
        }
    }
}
