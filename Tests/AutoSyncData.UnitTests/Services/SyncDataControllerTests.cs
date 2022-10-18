using AutoSyncData.Models;
using AutoSyncData.Services;
using NSubstitute;
using System.Diagnostics;
using Xunit;

namespace AutoSyncData.UnitTests.Services;

public class SyncDataControllerTests
{
    private readonly LogDetailService logDetailService;

    private readonly ValidationService validationService;

    private readonly SyncDataService syncDataService;

    private readonly SendMailService sendMailService;

    private readonly IMaintenanceService maintenanceService;
    private readonly ControlSyncService controlSyncService;

    public SyncDataControllerTests()
    {
        logDetailService = Substitute.For<LogDetailService>();
        validationService = Substitute.For<ValidationService>();
        syncDataService = Substitute.For<SyncDataService>();
        sendMailService = Substitute.For<SendMailService>();
        maintenanceService = Substitute.For<IMaintenanceService>();
        controlSyncService = Substitute.For<ControlSyncService>();
    }

    [Fact]
    public async Task Process_NotMaintenanceAndNotLatestLog_ReturnsDelayTime()
    {
        // Arrange
        const string server = "1";
        logDetailService.GetLatestLog(Arg.Any<string>()).Returns(new LogDetail());
        controlSyncService.IsStartService().Returns(false);
        var service = new SyncDataControllerImp(logDetailService, validationService, syncDataService, sendMailService, maintenanceService, controlSyncService);

        // Act

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await service.Process(server);
        stopWatch.Stop();
        var timeSpan = stopWatch.Elapsed;
        // Assert
        var totalDelayTime = timeSpan.Milliseconds;
        Assert.True(totalDelayTime > 500);
    }

    [Fact]
    public async Task Process_ExistedLogDetailAndTicketInvalid_CallRightSomeMethods()
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
        controlSyncService.IsStartService().Returns(false);
        var service = new SyncDataControllerImp(logDetailService, validationService, syncDataService, sendMailService, maintenanceService, controlSyncService);
        // Act
        await service.Process(server);

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

        controlSyncService.IsStartService().Returns(false);
        var service = new SyncDataControllerImp(logDetailService, validationService, syncDataService, sendMailService, maintenanceService, controlSyncService);

        // Act
        await service.Process(server);

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


        controlSyncService.IsStartService().Returns(false);
        var service = new SyncDataControllerImp(logDetailService, validationService, syncDataService, sendMailService, maintenanceService, controlSyncService);
        // Act
        await service.Process(server);

        // Assert
        await syncDataService.Received(3).UpdateLogDetail(Arg.Any<LogDetail>(), Arg.Any<byte>(), Arg.Any<string>());
        sendMailService.DidNotReceive().SendMail(Arg.Any<LogDetail>(), Arg.Any<string>());
    }
}