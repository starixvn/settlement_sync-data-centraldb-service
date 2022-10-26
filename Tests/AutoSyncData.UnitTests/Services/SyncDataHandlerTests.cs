using AutoSyncData.Models;
using AutoSyncData.Services;
using Fanex.Caching;
using NSubstitute;
using System.Diagnostics;
using Xunit;

namespace AutoSyncData.UnitTests.Services;

public class SyncDataHandlerTests
{
    private readonly LogDetailService logDetailService;
    private readonly ValidationService validationService;
    private readonly SyncDataService syncDataService;
    private readonly SendMailService sendMailService;
    private readonly ICache cache;

    private const string AutoSyncKey = "Settlement-Auto-Sync-Sportsbook";


    public SyncDataHandlerTests()
    {
        logDetailService = Substitute.For<LogDetailService>();
        validationService = Substitute.For<ValidationService>();
        syncDataService = Substitute.For<SyncDataService>();
        sendMailService = Substitute.For<SendMailService>();
        cache = Substitute.For<ICache>();
    }

    [Fact]
    public async Task IsStartService_NotKey_ReturnTrue()
    {
        // Arrange
        var handler = new SyncDataHandlerImp(cache, logDetailService, validationService, syncDataService, sendMailService);

        // Act
        var result = await handler.IsStartService();

        // Assert 
        Assert.True(result);
    }

    [Fact]
    public async Task Handle_EmptyLogDetail_ReturnsDelayTime()
    {
        // Arrange
        const string server = "1";
        logDetailService.GetLatestLog(Arg.Any<string>()).Returns(new LogDetail());
        var handler = new SyncDataHandlerImp(cache, logDetailService, validationService, syncDataService, sendMailService);

        // Act
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await handler.Handle(server);
        stopWatch.Stop();
        var timeSpan = stopWatch.Elapsed;

        // Assert
        var totalDelayTime = timeSpan.Milliseconds;
        Assert.True(totalDelayTime > 500);
    }

    [Fact]
    public async Task Handl_ExistedLogDetailAndTicketInvalid_CallRightSomeMethods()
    {
        // Arrange
        const string server = "1";
        var logDetail = new LogDetail
        {
            LogId = 1,
            MatchId = 1
        };
        logDetailService.GetLatestLog(Arg.Any<string>()).Returns(logDetail);
        validationService.ValidateTickets(Arg.Any<LogDetail>()).Returns(false);
        var handler = new SyncDataHandlerImp(cache, logDetailService, validationService, syncDataService, sendMailService);

        // Act
        await handler.Handle(server);

        // Assert
        await syncDataService.Received(2).UpdateLogDetail(Arg.Any<LogDetail>(), Arg.Any<byte>(), Arg.Any<string>());
        sendMailService.Received(1).SendMail(Arg.Any<LogDetail>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Process_ExistedLogDetailAndTicketValidAndSyncFail_CallRightSomeMethods()
    {
        // Arrange
        const string server = "1";
        var logDetail = new LogDetail
        {
            LogId = 1,
            MatchId = 1
        };

        logDetailService.GetLatestLog(Arg.Any<string>()).Returns(logDetail);
        validationService.ValidateTickets(Arg.Any<LogDetail>()).Returns(true);
        syncDataService.SyncData(Arg.Any<LogDetail>()).Returns(new Result { ErrCode = 1, ErrMsg = "Message" });

        var handler = new SyncDataHandlerImp(cache, logDetailService, validationService, syncDataService, sendMailService);

        // Act
        await handler.Handle(server);

        // Assert
        await syncDataService.Received(3).UpdateLogDetail(Arg.Any<LogDetail>(), Arg.Any<byte>(), Arg.Any<string>());
        sendMailService.Received(1).SendMail(Arg.Any<LogDetail>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Process_ExistedLogDetailAndTicketValidAndSyncSuccess_CallRightSomeMethods()
    {
        // Arrange
        const string server = "1";
        var logDetail = new LogDetail
        {
            LogId = 1,
            MatchId = 1
        };

        logDetailService.GetLatestLog(Arg.Any<string>()).Returns(logDetail);
        validationService.ValidateTickets(Arg.Any<LogDetail>()).Returns(true);
        syncDataService.SyncData(Arg.Any<LogDetail>()).Returns(new Result { ErrCode = 0, ErrMsg = string.Empty });

        var handler = new SyncDataHandlerImp(cache, logDetailService, validationService, syncDataService, sendMailService);
        // Act
        await handler.Handle(server);

        // Assert
        await syncDataService.Received(3).UpdateLogDetail(Arg.Any<LogDetail>(), Arg.Any<byte>(), Arg.Any<string>());
        sendMailService.DidNotReceive().SendMail(Arg.Any<LogDetail>(), Arg.Any<string>());
    }





}
