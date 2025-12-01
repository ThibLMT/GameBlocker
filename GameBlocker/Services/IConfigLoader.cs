using GameBlocker.Models;

namespace GameBlocker.Services
{
    public interface IConfigLoader
    {
        AppConfig LoadConfig(string filePath);
    }
}