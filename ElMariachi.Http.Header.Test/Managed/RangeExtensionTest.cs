using ElMariachi.Http.Header.Managed;
using Xunit;

namespace ElMariachi.Http.Header.Test.Managed
{
    public class RangeExtensionTest
    {

        [Fact]
        public void IsValidReturnsFalseWhenStartAndEndAreNull()
        {
            var range = new Range(null, null);
            Assert.False(range.IsValid());
        }   
        
        [Fact]
        public void IsValidReturnsFalseWhenStartIsGreaterThanEnd()
        {
            var range = new Range(1, 0);
            Assert.False(range.IsValid());

            range = new Range(456, 455);
            Assert.False(range.IsValid());

            range = new Range(1456, 0);
            Assert.False(range.IsValid());
        }   
        
        [Fact]
        public void IsValidReturnsFalseWhenStartOrEndIsNegative()
        {
            var range = new Range(-1, 151);
            Assert.False(range.IsValid());

            range = new Range(null, -54);
            Assert.False(range.IsValid());

            range = new Range(-5000, -4000);
            Assert.False(range.IsValid());
        }
        
        [Fact]
        public void IsValidReturnsTrueStartAndEndAreEqual()
        {
            var range = new Range(0, 0);
            Assert.True(range.IsValid());

            range = new Range(456, 456);
            Assert.True(range.IsValid());
        }

        [Fact]
        public void IsValidReturnsTrueStartIsLowerThanEndAndBothPositive()
        {
            var range = new Range(0, 1);
            Assert.True(range.IsValid());

            range = new Range(6, 7);
            Assert.True(range.IsValid());

            range = new Range(8464, 8797);
            Assert.True(range.IsValid());
        }
    }
}
