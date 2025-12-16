using GameBlocker.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameBlocker.Tests
{
    public class ScannerTests
    {
        [Fact]
        public void shouldScanForGames()
        {
            var mockLogger = new Mock<ILogger<GameScannerService>>();

            var Scanner = new GameScannerService(mockLogger.Object);

            var exeFilesReturned = Scanner.ScanGames();

            //Should not return empty files
            Assert.NotNull(exeFilesReturned);

        }
    }
}
