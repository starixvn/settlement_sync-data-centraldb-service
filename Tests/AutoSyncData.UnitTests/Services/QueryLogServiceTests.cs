using AutoSyncData.Services;
using Fanex.Data.Repository;
using NSubstitute;
using System.Data;
using Xunit;

namespace AutoSyncData.UnitTests.Services
{
    public class QueryLogServiceTests
    {
        private readonly IDynamicRepository repository;

        public QueryLogServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
        }

        [Fact]
        public void GetLogIds_EmptyData_ReturnsEmpty()
        {
            // Arrange
            var table = new DataTable();
            var service = new QueryLogServiceImp(repository);

            // Act
            var result = service.GetLogIds(table);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetLogIds_FakeData_ReturnsExpectedStringLogIds()
        {
            // Arrange
            var dt = new DataTable();

            dt.Columns.Add("LogId", typeof(long));

            dt.Rows.Add(9876);
            dt.Rows.Add(51234);
            dt.Rows.Add(16789);

            var service = new QueryLogServiceImp(repository);

            // Act
            var result = service.GetLogIds(dt);

            // Assert
            const string expected = "9876,51234,16789";
            Assert.Equal(expected, result);
        }
    }
}