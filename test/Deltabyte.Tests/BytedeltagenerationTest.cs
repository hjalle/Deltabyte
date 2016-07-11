using Deltabyte.Delta;
using Deltabyte.Models;
using System;
using System.IO;
using Xunit;

namespace ConsoleApplication
{
    public class BytedeltagenerationTest
    {
        private IDeltaGenerator basicDeltaGen;
        private UnsafeDeltaGenerator unsafeDeltaGen;

        public BytedeltagenerationTest()
        {
            basicDeltaGen = new BasicDeltaGenerator();
            unsafeDeltaGen = new UnsafeDeltaGenerator();
        }
        private void WriteToFile(string fileName, byte[] data)
        {
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                                              System.IO.FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
            return;
        }
        [Fact]
        public void ShouldGenerateCorrectDeltaWithSameLengthUnsafe()
        {
            var unsafeDeltaGen = new UnsafeDeltaGenerator();
            var tempPathName = Path.GetTempFileName();
            byte[] left = new byte[16];
            left[5] = 100;

            WriteToFile(tempPathName, left);

            byte[] right = new byte[16];
            right[5] = 0;

            ByteDelta delta = unsafeDeltaGen.ComputeDelta(left, right);

            using (var fs = new System.IO.FileStream(tempPathName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Write))
            {
                var bytesWritten = delta.ApplyDelta(fs);
                Assert.Equal(8, bytesWritten);
            }

            byte[] result = ReadFully(tempPathName);
            Assert.Equal(result, right);

        }
        [Fact]
        public void ShouldGenerateCorrectDeltaWithSameLength()
        {
            var tempPathName = Path.GetTempFileName();
            byte[] left = new byte[10];
            left[5] = 100;

            WriteToFile(tempPathName, left);

            byte[] right = new byte[10];
            right[5] = 0;

            ByteDelta delta = basicDeltaGen.ComputeDelta(left, right);

            using (var fs = new System.IO.FileStream(tempPathName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Write))
            {
                var bytesWritten = delta.ApplyDelta(fs);
                Assert.Equal(1, bytesWritten);
            }

            byte[] result = ReadFully(tempPathName);
            Assert.Equal(result, right);

        }
        [Fact]
        public void ShouldGenerateCorrectDeltaWhenRightIsLengthier()
        {
            var tempPathName = Path.GetTempFileName();
            byte[] left = new byte[10];
            left[5] = 100;

            WriteToFile(tempPathName, left);

            byte[] right = new byte[15];
            right[5] = 0;
            right[14] = 100;

            ByteDelta delta = basicDeltaGen.ComputeDelta(left, right);

            using (var fs = new System.IO.FileStream(tempPathName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Write))
            {
                var bytesWritten = delta.ApplyDelta(fs);
                Assert.Equal(6, bytesWritten);
            }

            byte[] result = ReadFully(tempPathName);
            Assert.Equal(result, right);

        }

        [Fact]
        public void ShouldGenerateCorrectDeltaWhenRightIsLengthierUnsafe()
        {
            var tempPathName = Path.GetTempFileName();
            byte[] left = new byte[16];
            left[5] = 100;

            WriteToFile(tempPathName, left);

            byte[] right = new byte[24];
            right[5] = 0;
            right[14] = 100;

            ByteDelta delta = unsafeDeltaGen.ComputeDelta(left, right);

            using (var fs = new System.IO.FileStream(tempPathName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Write))
            {
                var bytesWritten = delta.ApplyDelta(fs);
                Assert.Equal(24, bytesWritten);
            }

            byte[] result = ReadFully(tempPathName);
            Assert.Equal(result, right);

        }
        [Fact]
        public void ShouldGenerateCorrectDeltaWhenRightIsLengthierMultipleDiffs()
        {
            var tempPathName = Path.GetTempFileName();
            byte[] left = new byte[10];
            left[5] = 100;

            WriteToFile(tempPathName, left);

            byte[] right = new byte[1000];
            right[5] = 0;
            right[14] = 100;

            right[100] = 110;
            right[101] = 110;
            right[102] = 110;
            ByteDelta delta = basicDeltaGen.ComputeDelta(left, right);

            using (var fs = new System.IO.FileStream(tempPathName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Write))
            {
                var bytesWritten = delta.ApplyDelta(fs);
                Assert.Equal(991, bytesWritten);
            }

            byte[] result = ReadFully(tempPathName);
            Assert.Equal(result, right);
        }
        [Fact]
        public void ShouldGenerateCorrectDeltaWhenRightIsLengthierMultipleDiffsUnsafe()
        {
            var tempPathName = Path.GetTempFileName();
            byte[] left = new byte[16];
            left[5] = 100;

            WriteToFile(tempPathName, left);

            byte[] right = new byte[1024];
            right[5] = 100;
            right[14] = 100;

            right[100] = 110;
            right[101] = 110;
            right[102] = 110;
            ByteDelta delta = unsafeDeltaGen.ComputeDelta(left, right);

            using (var fs = new System.IO.FileStream(tempPathName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Write))
            {
                var bytesWritten = delta.ApplyDelta(fs);
                Assert.Equal(1016, bytesWritten);
            }

            byte[] result = ReadFully(tempPathName);
            for(var i = 0; i < result.Length; i++)
            {
                Assert.Equal(result[i], right[i]);
            }
            Assert.Equal(result, right);
        }


        public static byte[] ReadFully(string fileName)
        {
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open,
                                  System.IO.FileAccess.Read))
            {
                return ReadFully(fs);
            }
        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
