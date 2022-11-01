using AutoSyncData.Services;
using System.Data;
using Xunit;

namespace AutoSyncData.UnitTests.Services
{
    public class LogDetailServiceTests
    {
        [Fact]
        public void GetMinMaxLogId_EmptyData_ReturnMinMaxIsZero()
        {
            // Arrange
            var table = new DataTable();
            var service = new LogDetailServiceImp();

            // Act
            var result = service.GetMinMaxLogId(table);

            // Assert
            Assert.Equal(0, result.MinLogId);
            Assert.Equal(0, result.MaxLogId);
        }

        [Fact]
        public void GetMinMaxLogId_FakeData_ReturnsRightMinMax()
        {
            // Arrange
            var dt = new DataTable();

            dt.Columns.Add("LogId");
            dt.Columns.Add("SessionId");

            dt.Rows.Add(9, 1111);
            dt.Rows.Add(5, 1111);
            dt.Rows.Add(1, 1111);

            var service = new LogDetailServiceImp();

            // Act
            var result = service.GetMinMaxLogId(dt);

            // Assert
            Assert.Equal(1, result.MinLogId);
            Assert.Equal(9, result.MaxLogId);
            Assert.Equal(1111, result.SessionId);
        }
    }
}