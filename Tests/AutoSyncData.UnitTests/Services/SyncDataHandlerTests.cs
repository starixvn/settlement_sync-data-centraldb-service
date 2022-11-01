using AutoSyncData.Models;
using AutoSyncData.Services;
using Fanex.Caching;
using NSubstitute;
using System.Data;
using Xunit;

namespace AutoSyncData.UnitTests.Services;

public class SyncDataHandlerTests
{
    private readonly LogDetailService logDetailService;
    private readonly SyncDataService syncDataService;
    private readonly ICache cache;
    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

    public SyncDataHandlerTests()
    {
        logDetailService = Substitute.For<LogDetailService>();
        syncDataService = Substitute.For<SyncDataService>();
        cache = Substitute.For<ICache>();
    }

    [Fact]
    public async Task IsStartService_NotExistedKey_ReturnTrue()
    {
        // Arrange
        cache.ContainAsync(AutoSyncKey).Returns(false);
        var handler = new SyncDataHandlerImp(cache, logDetailService, syncDataService);

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
        var handler = new SyncDataHandlerImp(cache, logDetailService, syncDataService);

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
        var handler = new SyncDataHandlerImp(cache, logDetailService, syncDataService);

        // Act
        var result = await handler.IsStartService();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_Always_CallRightStep()
    {
        // Arrange
        var handler = new SyncDataHandlerImp(cache, logDetailService, syncDataService);

        // Act
        await handler.Handle("SiteDb01");

        // Assert
        Received.InOrder(() =>
        {
            logDetailService.GetCalculationLog(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>());
            logDetailService.GetMinMaxLogId(Arg.Any<DataTable>());
            syncDataService.InsertLog(Arg.Any<DataTable>(), Arg.Any<int>());
            syncDataService.UpdateLogStatus(Arg.Any<MinMaxLog>(), Arg.Any<string>());
        });
    }
}