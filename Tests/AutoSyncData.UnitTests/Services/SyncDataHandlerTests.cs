using AutoSyncData.Models;
using AutoSyncData.Services;
using Fanex.Caching;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharpRaven;
using System.Data;
using Xunit;

namespace AutoSyncData.UnitTests.Services;

public class SyncDataHandlerTests
{
    private readonly QueryLogService queryLogService;
    private readonly SyncDataService syncDataService;
    private readonly ICache cache;
    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

    public SyncDataHandlerTests()
    {
        queryLogService = Substitute.For<QueryLogService>();
        syncDataService = Substitute.For<SyncDataService>();
        cache = Substitute.For<ICache>();

        LogManager
            .SetDefaultLogCategory("NAP-Settlement-LOCAL")
            .Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn("http://2dfee6c5bbc844558ad3bef5f4b59ee1@10.18.200.131:800/9")
                })
            });
    }

    [Fact]
    public async Task IsStartService_NotExistedKey_ReturnTrue()
    {
        // Arrange
        cache.ContainAsync(AutoSyncKey).Returns(false);
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Act
        var result = await handler.IsStartService();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsStartService_IsExistedKeyAndFalse_ReturnFalse()
    {
        // Arrange
        cache.ContainAsync(AutoSyncKey).Returns(true);
        cache.TryGetAsync<bool>(AutoSyncKey).Returns((false, false));
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Act
        var result = await handler.IsStartService();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsStartService_IsExistedKeyAndTrue_ReturnTrue()
    {
        // Arrange
        cache.ContainAsync(AutoSyncKey).Returns(true);
        cache.TryGetAsync<bool>(AutoSyncKey).Returns((false, true));
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Act
        var result = await handler.IsStartService();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsStartService_ExceptionGetCache_ReturnTrue()
    {
        // Arrange
        cache.ContainAsync(Arg.Any<string>()).ThrowsAsync(new Exception());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Act
        var result = await handler.IsStartService();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StartHandle_SyncPendingTicketThrowException_RightExceptionNull()
    {
        // Arrange
        syncDataService.RetrySyncCommandTicketToCentral(Arg.Any<SiteInfo>()).ThrowsAsync(new Exception());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Assert
        var exception = await Record.ExceptionAsync(() => handler.StartHandle(new SiteInfo()));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ProcessHandle_GetLatestLogIsNull_Returns()
    {
        // Arrange
        queryLogService.GetLatestLog(Arg.Any<string>(), Arg.Any<int>()).Returns(new DataTable());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Act
        await handler.ProcessHandle(new SiteInfo());

        // Assert
        await syncDataService.DidNotReceive().SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>());
    }

    [Fact]
    public async Task ProcessHandle_SyncPendingTicketThrowException_RightExceptionMessage()
    {
        // Arrange
        syncDataService.SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>())
            .ThrowsAsync(new Exception());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService);

        // Assert
        var exception = await Record.ExceptionAsync(() => handler.ProcessHandle(new SiteInfo()));
        Assert.Null(exception);
    }
}