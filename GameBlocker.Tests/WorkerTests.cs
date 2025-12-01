using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Logging;
using Moq; // Mocking Library
using System.Collections.Generic;
using Xunit; // Testing Library

namespace GameBlocker.Tests;

public class WorkerTests
{
    // The [Fact] attribute marks this as a test runner entry point
    [Fact]
    public void RunCycle_ShouldKill_OnlyBlockedProcesses()
    {
        // 1. ARRANGE (Setup)

        // Mock the Logger (We don't care what it prints)
        var mockLogger = new Mock<ILogger<Worker>>();

        // Mock ConfigLoader (We don't need it for RunCycle, but constructor needs it)
        var mockConfigLoader = new Mock<IConfigLoader>();

        // Mock ProcessManager (The Star of the Show)
        var mockProcManager = new Mock<IProcessManager>();

        // Setup the Mock: When GetUserApps is called, return our fake list
        var fakeRunningApps = new List<ProcessInfo>
        {
            new ProcessInfo { ProcessName = "bf6", Id = 100 },
            new ProcessInfo { ProcessName = "notepad", Id = 200 }
        };

        mockProcManager
            .Setup(pm => pm.GetUserApps())
            .Returns(fakeRunningApps);

        // Create the Worker (Injecting our Mocks)
        var worker = new Worker(mockLogger.Object, mockProcManager.Object, mockConfigLoader.Object);

        // Define our Blocklist
        var blockList = new HashSet<string> { "bf6" };

        // 2. ACT (Run the logic)
        worker.RunCycle(blockList);

        // 3. ASSERT (Verify results)

        // Verify "bf6" was killed exactly ONCE
        mockProcManager.Verify(pm => pm.KillProcessByName("bf6"), Times.Once);

        // Verify "notepad" was NEVER killed
        mockProcManager.Verify(pm => pm.KillProcessByName("notepad"), Times.Never);
    }

    [Theory]
    [InlineData("dota2", "dota2", true)]    // Blocked process IS running -> Expect Kill
    [InlineData("dota2", "notepad", false)] // Blocked process NOT running -> Expect No Kill
    [InlineData("dota2", "DOTA2", true)]    // Case Insensitive check -> Expect Kill
    public void RunCycle_Theory_CheckKill(string blockedApp, string runningApp, bool shouldKill)
    {
        // 1. ARRANGE
        var mockProcManager = new Mock<IProcessManager>();

        // Setup the mock to return whatever 'runningApp' is passed in
        mockProcManager.Setup(pm => pm.GetUserApps())
            .Returns(new List<ProcessInfo> {
            new ProcessInfo { ProcessName = runningApp, Id = 1 }
            });

        var worker = new Worker(
            new Mock<ILogger<Worker>>().Object,
            mockProcManager.Object,
            new Mock<IConfigLoader>().Object
        );

        var blockList = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { blockedApp };

        // 2. ACT
        worker.RunCycle(blockList);

        // 3. ASSERT
        // We use 'Times.Once()' if true, or 'Times.Never()' if false
        var expectedTimes = shouldKill ? Times.Once() : Times.Never();

        // Did we try to kill the running app?
        mockProcManager.Verify(pm => pm.KillProcessByName(runningApp), expectedTimes);
    }
}