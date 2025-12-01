using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace GameBlocker.Tests;

public class WorkerTests
{
    [Fact]
    public void RunCycle_ShouldKill_OnlyBlockedProcesses()
    {
        // 1. ARRANGE
        var mockLogger = new Mock<ILogger<Worker>>();
        var mockProcManager = new Mock<IProcessManager>();

        // --- FIXED: Setup Monitor instead of Options ---
        var testConfig = new AppConfig
        {
            IsEnabled = true,
            BlockedProcesses = new List<string> { "bf6" }
        };

        var mockMonitor = new Mock<IOptionsMonitor<AppConfig>>();
        mockMonitor.Setup(m => m.CurrentValue).Returns(testConfig);
        // -----------------------------------------------

        var fakeRunningApps = new List<ProcessInfo>
        {
            new ProcessInfo { ProcessName = "bf6", Id = 100 },
            new ProcessInfo { ProcessName = "notepad", Id = 200 }
        };

        mockProcManager
            .Setup(pm => pm.GetUserApps())
            .Returns(fakeRunningApps);

        var worker = new Worker(mockLogger.Object, mockProcManager.Object, mockMonitor.Object);

        // We manually create the blocklist for RunCycle to test the killing logic
        var blockList = new HashSet<string>(testConfig.BlockedProcesses);

        // 2. ACT
        worker.RunCycle(blockList);

        // 3. ASSERT
        mockProcManager.Verify(pm => pm.KillProcessByName("bf6"), Times.Once);
        mockProcManager.Verify(pm => pm.KillProcessByName("notepad"), Times.Never);
    }

    [Theory]
    [InlineData("bf6", "bf6", true)]
    [InlineData("bf6", "notepad", false)]
    [InlineData("bf6", "BF6", true)]
    public void RunCycle_Theory_CheckKill(string blockedApp, string runningApp, bool shouldKill)
    {
        // 1. ARRANGE
        var mockProcManager = new Mock<IProcessManager>();

        mockProcManager.Setup(pm => pm.GetUserApps())
            .Returns(new List<ProcessInfo> {
                new ProcessInfo { ProcessName = runningApp, Id = 1 }
            });

        var mockMonitor = new Mock<IOptionsMonitor<AppConfig>>();
        mockMonitor.Setup(m => m.CurrentValue).Returns(new AppConfig()); // Return empty config

        var worker = new Worker(
            new Mock<ILogger<Worker>>().Object,
            mockProcManager.Object,
            mockMonitor.Object // Pass the mock object
        );

        var blockList = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { blockedApp };

        // 2. ACT
        worker.RunCycle(blockList);

        // 3. ASSERT
        var expectedTimes = shouldKill ? Times.Once() : Times.Never();
        mockProcManager.Verify(pm => pm.KillProcessByName(runningApp), expectedTimes);
    }
}