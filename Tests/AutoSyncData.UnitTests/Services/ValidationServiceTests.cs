using AutoSyncData.Criteria;
using AutoSyncData.Models;
using AutoSyncData.Services;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace AutoSyncData.UnitTests.Services
{
    public class ValidationServiceTests
    {
        private readonly IOptions<AppSettingsKey> appSettingsKey;

        public ValidationServiceTests()
        {
            var settings = new AppSettingsKey()
            {
                RetryInterval = 1,
                RetryCount = 1,
                EnvironmentName = "Local",
            };
            appSettingsKey = Options.Create(settings);
        }

        [Fact]
        public async Task ValidateTickets_IsPassed_ReturnsTrue()
        {
            // Arrange
            var logDetail = new LogDetail();
            var repository = Substitute.For<IDynamicRepository>();
            await repository.ExecuteAsync(Arg.Do<ValidateTicketsCriteria>(x =>
            {
                x.IsPassed = true;
            }));

            var service = new ValidationServiceImp(repository, appSettingsKey);

            // Act
            var actual = await service.ValidateTickets(logDetail);

            // Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task ValidateTickets_IsNotPassed_ReturnsFalse()
        {
            // Arrange
            var logDetail = new LogDetail();
            var repository = Substitute.For<IDynamicRepository>();
            await repository.ExecuteAsync(Arg.Do<ValidateTicketsCriteria>(x =>
            {
                x.IsPassed = false;
            }));

            var service = new ValidationServiceImp(repository, appSettingsKey);

            // Act
            var actual = await service.ValidateTickets(logDetail);

            // Assert
            Assert.False(actual);
        }
    }
}