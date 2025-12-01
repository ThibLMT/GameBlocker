using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options; // REQUIRED for Options.Create
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

        // --- CHANGED: Setup Config using Options.Create ---
        // We don't mock the loader anymore. We just create the data.
        var myConfig = new AppConfig
        {
            BlockedProcesses = new List<string> { "bf6" }
        };

        // Wrap it in the IOptions container
        IOptions<AppConfig> configOptions = Options.Create(myConfig);
        // --------------------------------------------------

        var fakeRunningApps = new List<ProcessInfo>
        {
            new ProcessInfo { ProcessName = "bf6", Id = 100 },
            new ProcessInfo { ProcessName = "notepad", Id = 200 }
        };

        mockProcManager
            .Setup(pm => pm.GetUserApps())
            .Returns(fakeRunningApps);

        // --- CHANGED: Pass configOptions instead of mockConfigLoader ---
        var worker = new Worker(mockLogger.Object, mockProcManager.Object, configOptions);

        // Define our blocklist (The worker extracts this from config internally now, 
        // but RunCycle accepts it as a param for easier testing of logic)
        var blockList = new HashSet<string>(myConfig.BlockedProcesses);

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

        // --- CHANGED: Create dummy options ---
        var dummyConfig = Options.Create(new AppConfig());

        var worker = new Worker(
            new Mock<ILogger<Worker>>().Object,
            mockProcManager.Object,
            dummyConfig // Pass the dummy
        );

        var blockList = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { blockedApp };

        // 2. ACT
        worker.RunCycle(blockList);

        // 3. ASSERT
        var expectedTimes = shouldKill ? Times.Once() : Times.Never();
        mockProcManager.Verify(pm => pm.KillProcessByName(runningApp), expectedTimes);
    }
}