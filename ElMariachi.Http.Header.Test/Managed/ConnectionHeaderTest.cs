using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class ConnectionHeaderTest
    {

        [Fact]
        public void HeaderName()
        {
            Assert.Equal("Connection", new ConnectionHeader().Name);
        }

        [Fact]
        public void InitialValues()
        {
            Assert.Null(new ConnectionHeader().RawValue);
        }

        [Fact]
        public void SetRawValue_Null()
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = null
            };

            Assert.False(connectionHeader.Close);
            Assert.False(connectionHeader.KeepAlive);

            Assert.Null(connectionHeader.RawValue);
        }

        [Fact]
        public void SetRawValue_Empty()
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = ""
            };

            Assert.False(connectionHeader.Close);
            Assert.False(connectionHeader.KeepAlive);

            Assert.Equal("", connectionHeader.RawValue);
        }

        [Fact]
        public void SetRawValue_ManagedValuesAreAppendedBeforeExtraItems()
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = "a,b"
            };

            Assert.False(connectionHeader.Close);
            Assert.False(connectionHeader.KeepAlive);

            connectionHeader.Close = true;
            Assert.Equal("close,a,b", connectionHeader.RawValue);

            connectionHeader.KeepAlive = true;
            Assert.Equal("close,keep-alive,a,b", connectionHeader.RawValue);

            connectionHeader.Close = false;
            Assert.Equal("keep-alive,a,b", connectionHeader.RawValue);

            connectionHeader.Close = true;
            Assert.Equal("close,keep-alive,a,b", connectionHeader.RawValue);

            connectionHeader.KeepAlive = false;
            Assert.Equal("close,a,b", connectionHeader.RawValue);

            connectionHeader.Close = false;
            Assert.Equal("a,b", connectionHeader.RawValue);
        }

        [Theory]
        [InlineData(" A     ,  bbb , cDe, FFFF ,gG ")]
        [InlineData(" -\"come together\"- ")]
        [InlineData(" -\"come together\"- ,  -\"come together again\"- ,   -\"come together once again\"- ")]
        public void RawValue_IsPreservedWhenManagedValuesAreReverted(string headerValue)
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = headerValue
            };

            Assert.Equal(headerValue, connectionHeader.RawValue);

            connectionHeader.Close = !connectionHeader.Close;
            connectionHeader.Close = !connectionHeader.Close;
            connectionHeader.KeepAlive = !connectionHeader.KeepAlive;
            connectionHeader.KeepAlive = !connectionHeader.KeepAlive;

            Assert.Equal(headerValue, connectionHeader.RawValue);
        }

        [Theory]
        [InlineData("close", true, false)]
        [InlineData("\"close\"", true, false)]
        [InlineData("  \"close\"  ", true, false)]
        [InlineData("   c\"lose\"   ", true, false)]
        [InlineData("   c\"LoSe\"   ", true, false)]
        [InlineData("  \" close\"  ", false, false)]
        [InlineData("  \"close \"  ", false, false)]
        [InlineData("keep-alive", false, true)]
        [InlineData("\"keep-alive\"", false, true)]
        [InlineData("  \"keep-alive\"  ", false, true)]
        [InlineData("   ke\"ep-a\"live   ", false, true)]
        [InlineData("   kE\"ep-A\"liVe   ", false, true)]
        [InlineData("  \" keep-alive\"  ", false, false)]
        [InlineData("  \"keep-alive \"  ", false, false)]
        [InlineData(" A     ,  bbb , close , cDe, FFFF ,gG , keep-alive", true, true)]
        [InlineData(" A     ,  bbb , \"close\" , cDe, FFFF ,gG , keep-alive", true, true)]
        [InlineData(" A     ,  bbb , close , cDe, FFFF ,gG , \"keep-alive\"", true, true)]
        [InlineData(" A     ,  bbb , \"close\" , cDe, FFFF ,gG , \"keep-alive\"", true, true)]
        public void SetRawValue_ManagedValuesAreDetected_And_RawValueIsPreserved(string rawValue, bool expectedClose, bool expectedKeepAlive)
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = rawValue,
            };

            Assert.Equal(expectedClose, connectionHeader.Close);
            Assert.Equal(expectedKeepAlive, connectionHeader.KeepAlive);

            Assert.Equal(rawValue, connectionHeader.RawValue);
        }

        [Fact]
        public void SetRawValue_WhenManagedValuesAreInRawValue_Then_RevertingManagedValuesRestoreOriginalRawValue()
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = " A     ,  bbb , close , cDe, FFFF ,gG , keep-alive",
            };

            Assert.Equal(" A     ,  bbb , close , cDe, FFFF ,gG , keep-alive", connectionHeader.RawValue);

            connectionHeader.Close = false;
            Assert.Equal(" A     ,  bbb , cDe, FFFF ,gG , keep-alive", connectionHeader.RawValue);

            connectionHeader.KeepAlive = false;
            Assert.Equal(" A     ,  bbb , cDe, FFFF ,gG ", connectionHeader.RawValue);

            connectionHeader.KeepAlive = true;
            Assert.Equal(" A     ,  bbb , cDe, FFFF ,gG , keep-alive", connectionHeader.RawValue);

            connectionHeader.Close = true;
            Assert.Equal(" A     ,  bbb , close , cDe, FFFF ,gG , keep-alive", connectionHeader.RawValue);
        }

        [Theory]
        [InlineData("\"A\"", false, false)]
        public void SetRawValue_WithExtraItemsAsDelimitedString(string rawValue, bool expectedClose, bool expectedKeepAlive)
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = rawValue
            };

            Assert.Equal(expectedClose, connectionHeader.Close);
            Assert.Equal(expectedKeepAlive, connectionHeader.KeepAlive);

            Assert.Equal(rawValue, connectionHeader.RawValue);
        }

        [Fact]
        public void WhenRawValueContainsOnlyManagedValues_Then_RemovingManagedValuesSetsRawValueToNull()
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = " keep-alive, close "
            };

            Assert.True(connectionHeader.Close);
            Assert.True(connectionHeader.KeepAlive);

            connectionHeader.Close = false;

            Assert.Equal(" keep-alive", connectionHeader.RawValue);

            connectionHeader.KeepAlive = false;

            Assert.Null(connectionHeader.RawValue);
        }

        [Fact]
        public void SettingKeepAliveToFalsePreservesClose()
        {
            var connectionHeader = new ConnectionHeader
            {
                RawValue = " keep-alive , close "
            };

            connectionHeader.KeepAlive = false;
            Assert.Equal(" close ", connectionHeader.RawValue);
        }

        [Fact]
        public void Close_SetsOrUnsetRawValue()
        {
            var connectionHeader = new ConnectionHeader();

            connectionHeader.Close = true;
            Assert.Equal("close", connectionHeader.RawValue);

            connectionHeader.Close = false;
            Assert.Null(connectionHeader.RawValue);

            connectionHeader.Close = true;
            connectionHeader.Close = true;
            Assert.Equal("close", connectionHeader.RawValue);
        }

        [Fact]
        public void KeepAlive_SetsOrUnsetRawValue()
        {
            var connectionHeader = new ConnectionHeader();

            connectionHeader.KeepAlive = true;
            Assert.Equal("keep-alive", connectionHeader.RawValue);

            connectionHeader.KeepAlive = false;
            Assert.Null(connectionHeader.RawValue);

            connectionHeader.KeepAlive = true;
            connectionHeader.KeepAlive = true;
            Assert.Equal("keep-alive", connectionHeader.RawValue);
        }

        [Fact]
        public void SetRawValue_WithDuplicatedCloseValues() // This test case is more a side effect result of the global expectation which is to always preserve the raw value set
        {
            var connectionHeader = new ConnectionHeader();

            connectionHeader.RawValue = "close , dummy , close, Close , cLOse";

            Assert.True(connectionHeader.Close);

            Assert.Equal("close , dummy , close, Close , cLOse", connectionHeader.RawValue);
        }


        [Fact]
        public void SetRawValue_WithDuplicatedKeepAliveValues() // This test case is more a side effect result of the global expectation which is to always preserve the raw value set
        {
            var connectionHeader = new ConnectionHeader();

            connectionHeader.RawValue = "abc, keep-alive  , Keep-Alive , dummy , keep-alive, kEEp-alive , some, keep-Alive, keep-alive  ";

            Assert.True(connectionHeader.KeepAlive);

            Assert.Equal("abc, keep-alive  , Keep-Alive , dummy , keep-alive, kEEp-alive , some, keep-Alive, keep-alive  ", connectionHeader.RawValue);

            connectionHeader.KeepAlive = false;

            Assert.Equal("abc, Keep-Alive , dummy , keep-alive, kEEp-alive , some, keep-Alive, keep-alive  ", connectionHeader.RawValue);

            connectionHeader.KeepAlive = true;

            Assert.Equal("abc, keep-alive  , Keep-Alive , dummy , keep-alive, kEEp-alive , some, keep-Alive, keep-alive  ", connectionHeader.RawValue);
        }

        [Fact]
        public void SetRawValue_AlwaysResetManagedValues()
        {
            var connectionHeader = new ConnectionHeader();

            var header = "dummy , keep-alive";
            connectionHeader.RawValue = header;
            Assert.True(connectionHeader.KeepAlive);
            Assert.False(connectionHeader.Close);
            Assert.Equal(header, connectionHeader.RawValue);

            header = "abc, any ,lose, dummy 1, clos";
            connectionHeader.RawValue = header;
            Assert.False(connectionHeader.KeepAlive);
            Assert.False(connectionHeader.Close);
            Assert.Equal(header, connectionHeader.RawValue);

            header = "close, dummy , keep-alive";
            connectionHeader.RawValue = "close, dummy , keep-alive";
            Assert.True(connectionHeader.KeepAlive);
            Assert.True(connectionHeader.Close);
            Assert.Equal(header, connectionHeader.RawValue);

            header = "close, dummy , keep not alive";
            connectionHeader.RawValue = header;
            Assert.False(connectionHeader.KeepAlive);
            Assert.True(connectionHeader.Close);
            Assert.Equal(header, connectionHeader.RawValue);

            header = "close, dummy , keep-alive";
            connectionHeader.RawValue = header;
            Assert.True(connectionHeader.KeepAlive);
            Assert.True(connectionHeader.Close);
            Assert.Equal(header, connectionHeader.RawValue);

            header = null;
            connectionHeader.RawValue = header;
            Assert.False(connectionHeader.KeepAlive);
            Assert.False(connectionHeader.Close);
            Assert.Equal(header, connectionHeader.RawValue);
        }


    }
}
