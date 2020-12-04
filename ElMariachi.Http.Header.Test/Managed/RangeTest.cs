using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class RangeTest
    {
        [Fact]
        public void RangeIsAStruct()
        {
            var range = new Range(5, 10);

            Assert.Equal(5, range.Start);
            Assert.Equal(10, range.End);

            var range2 = range;
            range2.Start = 25;
            range2.End = 50;

            Assert.Equal(5, range.Start);
            Assert.Equal(10, range.End);

            Assert.Equal(25, range2.Start);
            Assert.Equal(50, range2.End);
        }

        [Fact]
        public void InitialValues()
        {
            var range = new Range();
            Assert.Null(range.Start);
            Assert.Null(range.End);
        }

        [Fact]
        public void Range_NotifiesOnStartChanged()
        {
            var range = new Range();

            var called = false;
            range.Changed += (sender, args) =>
            {
                Assert.Equal(nameof(Range.Start), args.PropertyName);
                called = true;
            };
            range.Start++;
            Assert.True(called);
        }

        [Fact]
        public void Range_NotifiesOnEndChanged()
        {
            var range = new Range();

            var called = false;
            range.Changed += (sender, args) =>
            {
                Assert.Equal(nameof(Range.End), args.PropertyName);
                called = true;
            };
            range.End++;
            Assert.True(called);
        }

    }
}
