using AutoSyncData.Services;
using System.Data;
using Xunit;

namespace AutoSyncData.UnitTests.Services
{
    public class LogDetailServiceTests
    {
        [Fact]
        public void GetLogIds_EmptyData_ReturnsEmpty()
        {
            // Arrange
            var table = new DataTable();
            var service = new QueryLogServiceImp();

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

            dt.Columns.Add("LogId");

            dt.Rows.Add(9876);
            dt.Rows.Add(51234);
            dt.Rows.Add(16789);

            var service = new QueryLogServiceImp();

            // Act
            var result = service.GetLogIds(dt);

            // Assert
            var expected = "9876,51234,16789";
            Assert.Equal(expected, result);
        }
    }
}