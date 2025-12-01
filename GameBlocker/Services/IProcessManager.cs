using System.Diagnostics;

namespace GameBlocker.Services;

public interface IProcessManager
{
    public List<ProcessInfo> GetUserApps();

    public void KillProcessByName(string targetName);
}