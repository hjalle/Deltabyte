using Deltabyte.Delta;
using Deltabyte.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ConsoleApplication
{
    public class Benchmark
    {

        public static Random r = new Random();

        private static void WriteToFile(string fileName, byte[] data)
        {
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create,
                                              System.IO.FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
            return;
        }
        public static void Main(string[] args)
        {
            var iterations = 100;
            var sizeInBytes = 10 * 1024 * 1024;

            Stopwatch sw = new Stopwatch();
            var leftArrays = new byte[iterations][];
            FillArrays(leftArrays, sizeInBytes);
            var rightArrays = ModifyAndCloneArrays(leftArrays,0.01,5);


            var fileNames = CreateAndFillFiles(leftArrays);
            IDeltaGenerator generator = new BasicDeltaGenerator();
            TestWithDelta(leftArrays,rightArrays, fileNames,generator);
            DeleteFiles(fileNames);

            

            fileNames = CreateAndFillFiles(leftArrays);
            TestWithoutDelta(leftArrays, rightArrays, fileNames);
            DeleteFiles(fileNames);

            fileNames = CreateAndFillFiles(leftArrays);
            generator = new UnsafeDeltaGenerator();
            TestWithDelta(leftArrays, rightArrays, fileNames, generator);
            DeleteFiles(fileNames);

        }

        private static void TestWithoutDelta(byte[][] leftArrays, byte[][] rightArrays, List<string> fileNames)
        {

            var sw = new Stopwatch();
            sw.Start();
            var tempPathName = Path.GetTempFileName();
            var bytesWritten = 0;
            for (var i = 0; i < leftArrays.Length; i++)
            {
                using (var fs = new System.IO.FileStream(fileNames[i], System.IO.FileMode.Open,
                                      System.IO.FileAccess.Write))
                {
                    fs.Write(rightArrays[i], 0, rightArrays[i].Length);
                    bytesWritten += rightArrays[i].Length;
                }

            }
            sw.Stop();
            Console.WriteLine("[No delta]: Time taken: " + sw.ElapsedMilliseconds);
            Console.WriteLine("[No delta]: Bytes written: " + bytesWritten);

        }

        private static void DeleteFiles(List<string> fileNames)
        {
            foreach(var file in fileNames)
            {
                File.Delete(file);
            }
        }

        private static List<string> CreateAndFillFiles(byte[][] leftArrays)
        {
            List<string> filenames = new List<string>();
            for (var i = 0; i < leftArrays.Length; i++)
            {
                var tempPathName = Path.GetTempFileName();
                WriteToFile(tempPathName, leftArrays[i]);
                filenames.Add(tempPathName);
            }
            return filenames;
        }


        private static void TestWithDelta(byte[][] leftArrays, byte[][] rightArrays, List<string> fileNames, IDeltaGenerator generator)
        {
            var tempPathName = Path.GetTempFileName();
            Stopwatch sw = new Stopwatch();
            Stopwatch totSw = new Stopwatch();
            totSw.Start();
            long totWritten = 0;
            for(var i = 0; i < leftArrays.Length; i++)
            {
                sw.Start();
                ByteDelta delta = generator.ComputeDelta(leftArrays[i], rightArrays[i]);
                sw.Stop();
                using (var fs = new System.IO.FileStream(fileNames[i], System.IO.FileMode.Open,
                                      System.IO.FileAccess.Write))
                {
                    totWritten += delta.ApplyDelta(fs);
                }

            }
            totSw.Stop();
            Console.WriteLine($"[{generator.GetType().Name}] Time taken deltaHandling: " + sw.ElapsedMilliseconds);
            Console.WriteLine($"[{generator.GetType().Name}] Total Time taken: " + totSw.ElapsedMilliseconds);
            Console.WriteLine($"[{generator.GetType().Name}] Bytes written: " + totWritten);
        }

        private static byte[][] ModifyAndCloneArrays(byte[][] leftArrays, double percentageOfArrayModified, int numberOfModifications)
        {
            var rightArrays = new byte[leftArrays.Length][];
            for (var i = 0; i < rightArrays.Length; i++)
            {
                rightArrays[i] = new byte[leftArrays[i].Length];
                Array.Copy(leftArrays[i], rightArrays[i], rightArrays[i].Length);
                ModifyArrayInPlace(rightArrays[i], percentageOfArrayModified, numberOfModifications);
            }
            return rightArrays;
        }

        private static void ModifyArrayInPlace(byte[] array, double percentageOfArrayModified, int numberOfModifications)
        {
            var bytesPerModification = (array.Length * percentageOfArrayModified) / numberOfModifications;
            for (var i = 0; i < numberOfModifications; i++)
            {
                var randomData = new byte[(int)bytesPerModification];
                r.NextBytes(randomData);
                Array.Copy(randomData, 0, array, (((int)bytesPerModification - 1) * i + 1),randomData.Length);
            }
        }

        private static void FillArrays(byte[][] leftArrays, int sizeInBytes)
        {
            for (var i = 0; i < leftArrays.Length; i++)
            {
                Random r = new Random();
                leftArrays[i] = new byte[sizeInBytes];
                r.NextBytes(leftArrays[i]);
            }
        }
    }
}
