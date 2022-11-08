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
    private readonly SendMailService sendMailService;
    private readonly ICache cache;
    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";

    public SyncDataHandlerTests()
    {
        queryLogService = Substitute.For<QueryLogService>();
        syncDataService = Substitute.For<SyncDataService>();
        sendMailService = Substitute.For<SendMailService>();
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
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

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
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

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
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

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
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Act
        var result = await handler.IsStartService();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task StartHandle_SyncPendingTicketIsFailed_CallSendMail()
    {
        // Arrange
        syncDataService.RetrySyncCommandTicketToCentral(Arg.Any<SiteInfo>()).Returns(false);
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Act
        await handler.StartHandle(new SiteInfo());

        // Assert
        sendMailService.Received(1).SendMail(Arg.Any<string>());
    }

    [Fact]
    public async Task StartHandle_SyncPendingTicketSuccess_NotCallSendMail()
    {
        // Arrange
        syncDataService.RetrySyncCommandTicketToCentral(Arg.Any<SiteInfo>()).Returns(true);
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Act
        await handler.StartHandle(new SiteInfo());

        // Assert
        sendMailService.DidNotReceive().SendMail(Arg.Any<string>());
    }

    [Fact]
    public void StartHandle_SyncPendingTicketThrowException_RightExceptionMessage()
    {
        // Arrange
        syncDataService.RetrySyncCommandTicketToCentral(Arg.Any<SiteInfo>()).ThrowsAsync(new Exception());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Assert
        Assert.ThrowsAsync<Exception>(() => handler.StartHandle(new SiteInfo()));
    }

    [Fact]
    public async Task ProcessHandle_GetLatestLogIsNull_Returns()
    {
        // Arrange
        queryLogService.GetLatestLog(Arg.Any<string>(), Arg.Any<int>()).Returns(new DataTable());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Act
        await handler.ProcessHandle(new SiteInfo());

        // Assert
        await syncDataService.DidNotReceive().SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>());
    }

    [Fact]
    public async Task ProcessHandle_GetLatestLogIsNotNullAndSyncTicketSuccess_NotCallSendMail()
    {
        // Arrange
        var dt = new DataTable();
        dt.Columns.Add("LogId", typeof(long));
        dt.Rows.Add(9876);

        queryLogService.GetLatestLog(Arg.Any<string>(), Arg.Any<int>()).Returns(dt);
        syncDataService.SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>()).Returns(true);

        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Act
        await handler.ProcessHandle(new SiteInfo());

        // Assert
        await syncDataService.Received().SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>());
        sendMailService.DidNotReceive().SendMail(Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessHandle_GetLatestLogIsNotNullAndSyncTicketIsFailed_NotCallSendMail()
    {
        // Arrange
        var dt = new DataTable();
        dt.Columns.Add("LogId", typeof(long));
        dt.Rows.Add(9876);

        queryLogService.GetLogIds(Arg.Any<DataTable>()).Returns("12,24");
        queryLogService.GetLatestLog(Arg.Any<string>(), Arg.Any<int>()).Returns(dt);
        syncDataService.SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>()).Returns(false);

        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Act
        await handler.ProcessHandle(new SiteInfo());

        // Assert
        Received.InOrder(() =>
        {
            syncDataService.SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>());
            queryLogService.GetLogIds(Arg.Any<DataTable>());
            sendMailService.SendMail(Arg.Any<string>());
        });
    }

    [Fact]
    public void ProcessHandle_SyncPendingTicketThrowException_RightExceptionMessage()
    {
        // Arrange
        syncDataService.SyncCommandTicketToCentral(Arg.Any<DataTable>(), Arg.Any<SiteInfo>())
            .ThrowsAsync(new Exception());
        var handler = new SyncDataHandlerImp(cache, queryLogService, syncDataService, sendMailService);

        // Assert
        Assert.ThrowsAsync<Exception>(() => handler.ProcessHandle(new SiteInfo()));
    }
}