using System.IO;
using System.Text;
using System.Threading;
using ElMariachi.Http.Header.Managed;
using ElMariachi.Http.Server.Models.ResponseContent;
using Xunit;

namespace ElMariachi.Http.Server.Test.Models.ResponseContent
{

    public class FileContentTest
    {
        private const string FILE_CONTENT = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private readonly string _filePath;
        private static readonly ReaderWriterLock _testingFileLock = new ReaderWriterLock(); // NOTE: required for preventing concurrent access on testing file when tests are run in parallel

        public FileContentTest()
        {
            _testingFileLock.AcquireReaderLock(10000);
            
            _filePath = Path.Combine(Path.GetTempPath(), "Test.txt");
            File.WriteAllText(_filePath, FILE_CONTENT, Encoding.ASCII);
        }

        ~FileContentTest()
        {
            File.Delete(_filePath);
            _testingFileLock.ReleaseLock();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(26)]
        [InlineData(34)]
        public void WhenNotRange_TheWholeContentIsCopiedWhateverTheValueOfBufferSize(int bufferSize)
        {
            var fileContent = new FileContent(_filePath)
            {
                BufferSize = bufferSize
            };

            var memoryStream = new MemoryStream();
            fileContent.CopyToStream(memoryStream);

            memoryStream.Position = 0;
            var readToEnd = new StreamReader(memoryStream, Encoding.ASCII).ReadToEnd();
            Assert.Equal(FILE_CONTENT, readToEnd);
        }

        [Theory]
        [InlineData(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 5)]
        [InlineData(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 26)]
        [InlineData(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 27)]
        [InlineData(1, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(1, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 5)]
        [InlineData(1, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 50)]
        [InlineData(2, "CDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(2, "CDEFGHIJKLMNOPQRSTUVWXYZ", 5)]
        [InlineData(2, "CDEFGHIJKLMNOPQRSTUVWXYZ", 50)]
        [InlineData(3, "DEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(3, "DEFGHIJKLMNOPQRSTUVWXYZ", 18)]
        [InlineData(3, "DEFGHIJKLMNOPQRSTUVWXYZ", 52)]
        [InlineData(5, "FGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(5, "FGHIJKLMNOPQRSTUVWXYZ", 26)]
        [InlineData(5, "FGHIJKLMNOPQRSTUVWXYZ", 55)]
        [InlineData(10, "KLMNOPQRSTUVWXYZ", 1)]
        [InlineData(10, "KLMNOPQRSTUVWXYZ", 17)]
        [InlineData(10, "KLMNOPQRSTUVWXYZ", 26)]
        [InlineData(10, "KLMNOPQRSTUVWXYZ", 27)]
        [InlineData(10, "KLMNOPQRSTUVWXYZ", 50)]
        [InlineData(25, "Z", 34)]
        public void WithRangeStart_ReturnedContentStartsAtStartIndex(int start, string expectedContent, int bufferSize)
        {
            var range = new Range
            {
                Start = start,
                End = null
            };
            var fileContent = new FileContent(_filePath, range: range)
            {
                BufferSize = bufferSize
            };

            var memoryStream = new MemoryStream();
            fileContent.CopyToStream(memoryStream);

            memoryStream.Position = 0;
            var readToEnd = new StreamReader(memoryStream, Encoding.ASCII).ReadToEnd();
            Assert.Equal(expectedContent, readToEnd);
        }

        [Theory]
        [InlineData(1, "Z", 1)]
        [InlineData(1, "Z", 5)]
        [InlineData(1, "Z", 26)]
        [InlineData(1, "Z", 50)]
        [InlineData(2, "YZ", 1)]
        [InlineData(2, "YZ", 5)]
        [InlineData(2, "YZ", 26)]
        [InlineData(2, "YZ", 145)]
        [InlineData(3, "XYZ", 1)]
        [InlineData(3, "XYZ", 5)]
        [InlineData(3, "XYZ", 26)]
        [InlineData(3, "XYZ", 87)]
        [InlineData(5, "VWXYZ", 1)]
        [InlineData(5, "VWXYZ", 13)]
        [InlineData(5, "VWXYZ", 26)]
        [InlineData(10, "QRSTUVWXYZ", 34)]
        [InlineData(10, "QRSTUVWXYZ", 1)]
        [InlineData(10, "QRSTUVWXYZ", 9)]
        [InlineData(10, "QRSTUVWXYZ", 26)]
        [InlineData(10, "QRSTUVWXYZ", 27)]
        [InlineData(25, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(25, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 3)]
        [InlineData(25, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 26)]
        [InlineData(25, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 27)]
        [InlineData(26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 3)]
        [InlineData(26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 26)]
        [InlineData(26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 27)]
        public void WithRangeEnd_ReturnedContentCorrespondsToNumberOfBytesFromTheEndOfTheFile(int end, string expectedContent, int bufferSize)
        {
            var range = new Range
            {
                Start = null,
                End = end
            };
            var fileContent = new FileContent(_filePath, range: range)
            {
                BufferSize = bufferSize
            };

            var memoryStream = new MemoryStream();
            fileContent.CopyToStream(memoryStream);

            memoryStream.Position = 0;
            var readToEnd = new StreamReader(memoryStream, Encoding.ASCII).ReadToEnd();
            Assert.Equal(expectedContent, readToEnd);
        }

        [Theory]
        [InlineData(0, 25, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(0, 25, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 9)]
        [InlineData(0, 25, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 26)]
        [InlineData(0, 25, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 27)]
        [InlineData(0, 25, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 200)]
        [InlineData(0, 24, "ABCDEFGHIJKLMNOPQRSTUVWXY", 1)]
        [InlineData(0, 24, "ABCDEFGHIJKLMNOPQRSTUVWXY", 26)]
        [InlineData(0, 24, "ABCDEFGHIJKLMNOPQRSTUVWXY", 27)]
        [InlineData(0, 24, "ABCDEFGHIJKLMNOPQRSTUVWXY", 200)]
        [InlineData(1, 24, "BCDEFGHIJKLMNOPQRSTUVWXY", 1)]
        [InlineData(1, 24, "BCDEFGHIJKLMNOPQRSTUVWXY", 26)]
        [InlineData(1, 24, "BCDEFGHIJKLMNOPQRSTUVWXY", 27)]
        [InlineData(1, 24, "BCDEFGHIJKLMNOPQRSTUVWXY", 200)]
        [InlineData(0, 0, "A", 200)]
        [InlineData(1, 1, "B", 200)]
        [InlineData(0, 1, "AB", 200)]
        [InlineData(25, 25, "Z", 200)]
        [InlineData(24, 25, "YZ", 200)]
        [InlineData(12, 13, "MN", 200)]
        public void WithRange_ReturnedContentCorrespondsToBytesBetweenGivenStartIndexAndGivenEndIndex(int start, int end, string expectedContent, int bufferSize)
        {
            var range = new Range
            {
                Start = start,
                End = end
            };
            var fileContent = new FileContent(_filePath, range: range)
            {
                BufferSize = bufferSize
            };

            var memoryStream = new MemoryStream();
            fileContent.CopyToStream(memoryStream);

            memoryStream.Position = 0;
            var readToEnd = new StreamReader(memoryStream, Encoding.ASCII).ReadToEnd();
            Assert.Equal(expectedContent, readToEnd);
        }

        [Theory]
        [InlineData(0, 26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", 1)]
        [InlineData(1, 26, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 2)]
        [InlineData(1, 26, "BCDEFGHIJKLMNOPQRSTUVWXYZ", 30)]
        [InlineData(25, 200, "Z", 1)]
        public void WithRange_IfEndRangeIsGreaterThanOrEqualToFileSizeThenTheEndOfFileIsReturned(int start, int end, string expectedContent, int bufferSize)
        {
            Assert.True(end >= FILE_CONTENT.Length, "Invalid test input data, end should be greater or equal to file size.");

            var range = new Range
            {
                Start = start,
                End = end
            };
            var fileContent = new FileContent(_filePath, range: range)
            {
                BufferSize = bufferSize
            };

            var memoryStream = new MemoryStream();
            fileContent.CopyToStream(memoryStream);

            memoryStream.Position = 0;
            var readToEnd = new StreamReader(memoryStream, Encoding.ASCII).ReadToEnd();
            Assert.Equal(expectedContent, readToEnd);
        }

        [Theory]
        [InlineData(0, 25, false)]
        [InlineData(0, 26, false)]
        [InlineData(null, 26, false)]
        [InlineData(null, 25, true)]
        [InlineData(0, 24, true)]
        [InlineData(1, 25, true)]
        public void WithRange_IsPartialContentIsFalseIfRangeCoversTheEntireFile(int? start, int? end, bool expectedPartialContent)
        {
            using var fileContent = new FileContent(_filePath, range: new Range(start, end));
            Assert.Equal(expectedPartialContent, fileContent.IsPartialContent);
        }

        [Fact]
        public void WithoutRange_TheEntireFileIsReturned()
        {
            var fileContent = new FileContent(_filePath);
            Assert.False(fileContent.IsPartialContent);

            var memoryStream = new MemoryStream();
            fileContent.CopyToStream(memoryStream);

            memoryStream.Position = 0;
            var readToEnd = new StreamReader(memoryStream, Encoding.ASCII).ReadToEnd();
            Assert.Equal(FILE_CONTENT, readToEnd);
        }
    }
}
