using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FileIntegrityController
{
    public class PerformanceTests
    {
        private static int _writeBufferSize = 400000;

        public static void RunTests()
        {
            // Arrange
            Dictionary<string, string> filesHashes = new Dictionary<string, string>();
            Dictionary<string, bool> expected = new Dictionary<string, bool>();
            for (int i = 1; i <= 6; i++)
            {
                if (i % 2 == 0)
                    CreateCorrectFile(filesHashes, expected, i);
                else
                    CreateIncorrectFile(filesHashes, expected, i);
            }

            // Act & Assert
            FastDisksFewCores(filesHashes, expected);
            SlowDisksManyCores(filesHashes, expected);
            ManyDisksFewCores(filesHashes, expected);
            FewDisksManyCores(filesHashes, expected);

            // Cleanup
            foreach (var pair in filesHashes)
            {
                File.Delete(pair.Key);
            }
        }

        public static void FastDisksFewCores(Dictionary<string, string> filesHashes, Dictionary<string, bool> expected)
        {
            Console.WriteLine("Быстрые диски, мало ядер");
            // Последовательное выполнение
            Stopwatch sequentallWatch = new Stopwatch();
            Dictionary<string, bool> sequentallResult = new Dictionary<string, bool>();
            sequentallWatch.Start();
            foreach (var pair in filesHashes)
            {
                using (FileStream fileStream = new FileStream(pair.Key, FileMode.Open))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] byteHash = md5.ComputeHash(fileStream);
                        string hash = BitConverter.ToString(byteHash);
                        sequentallResult.Add(pair.Key, pair.Value == hash);
                    }
                }
            }
            sequentallWatch.Stop();
            Console.WriteLine($"Правильность последовательного выполнения: {(sequentallResult.Count == expected.Count) && (!expected.Except(sequentallResult).Any())}");
            Console.WriteLine($"Время последовательного выполнения: {sequentallWatch.ElapsedTicks}");

            // Параллельное выполнение
            Producer producer1 = new Producer(new FileGroup("1", new Dictionary<string, string>() { 
                {filesHashes.ToArray()[0].Key, filesHashes.ToArray()[0].Value },
                {filesHashes.ToArray()[1].Key, filesHashes.ToArray()[1].Value },
                {filesHashes.ToArray()[2].Key, filesHashes.ToArray()[2].Value }}), 4);
            Producer producer2 = new Producer(new FileGroup("2", new Dictionary<string, string>() {
                {filesHashes.ToArray()[3].Key, filesHashes.ToArray()[3].Value },
                {filesHashes.ToArray()[4].Key, filesHashes.ToArray()[4].Value },
                {filesHashes.ToArray()[5].Key, filesHashes.ToArray()[5].Value }}), 4);
            ConsumerController consumerController = new ConsumerController(new List<BufferBlock<(Task, Task)>>() { producer1.ProducerBuffer, producer2.ProducerBuffer }, 2);

            Stopwatch parallelWatch = new Stopwatch();
            parallelWatch.Start();
            Task<Dictionary<string, bool>> producerTask1 = Task.Run(() => producer1.Execute());
            Task<Dictionary<string, bool>> producerTask2 = Task.Run(() => producer2.Execute());
            producerTask1.Wait();
            producerTask2.Wait();
            parallelWatch.Stop();
            Dictionary<string, bool> actualDisk1 = producerTask1.Result;
            Dictionary<string, bool> actualDisk2 = producerTask2.Result;
            var parallelResult = new Dictionary<string, bool>[] { actualDisk1, actualDisk2 }.SelectMany(dict => dict)
                         .ToDictionary(pair => pair.Key, pair => pair.Value);
            Console.WriteLine($"Правильность параллельного выполнения: {(parallelResult.Count == expected.Count) && (!expected.Except(parallelResult).Any())}");
            Console.WriteLine($"Время параллельного выполнения: {parallelWatch.ElapsedTicks}");
            Console.WriteLine("");
        }

        public static void SlowDisksManyCores(Dictionary<string, string> filesHashes, Dictionary<string, bool> expected)
        {
            Console.WriteLine("Медленные диски, много ядер");
            // Последовательное выполнение
            Stopwatch sequentallWatch = new Stopwatch();
            Dictionary<string, bool> sequentallResult = new Dictionary<string, bool>();
            sequentallWatch.Start();
            foreach (var pair in filesHashes)
            {
                using (FileStream fileStream = new FileStream(pair.Key, FileMode.Open))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] byteHash = md5.ComputeHash(fileStream);
                        string hash = BitConverter.ToString(byteHash);
                        sequentallResult.Add(pair.Key, pair.Value == hash);
                    }
                }
            }
            sequentallWatch.Stop();
            Console.WriteLine($"Правильность последовательного выполнения: {(sequentallResult.Count == expected.Count) && (!expected.Except(sequentallResult).Any())}");
            Console.WriteLine($"Время последовательного выполнения: {sequentallWatch.ElapsedTicks}");

            // Параллельное выполнение
            Producer producer1 = new Producer(new FileGroup("1", new Dictionary<string, string>() {
                {filesHashes.ToArray()[0].Key, filesHashes.ToArray()[0].Value },
                {filesHashes.ToArray()[1].Key, filesHashes.ToArray()[1].Value },
                {filesHashes.ToArray()[2].Key, filesHashes.ToArray()[2].Value }}), 1);
            Producer producer2 = new Producer(new FileGroup("2", new Dictionary<string, string>() {
                {filesHashes.ToArray()[3].Key, filesHashes.ToArray()[3].Value },
                {filesHashes.ToArray()[4].Key, filesHashes.ToArray()[4].Value },
                {filesHashes.ToArray()[5].Key, filesHashes.ToArray()[5].Value }}), 1);
            ConsumerController consumerController = new ConsumerController(new List<BufferBlock<(Task, Task)>>() { producer1.ProducerBuffer, producer2.ProducerBuffer }, 4);

            Stopwatch parallelWatch = new Stopwatch();
            parallelWatch.Start();
            Task<Dictionary<string, bool>> producerTask1 = Task.Run(() => producer1.Execute());
            Task<Dictionary<string, bool>> producerTask2 = Task.Run(() => producer2.Execute());
            producerTask1.Wait();
            producerTask2.Wait();
            parallelWatch.Stop();
            Dictionary<string, bool> actualDisk1 = producerTask1.Result;
            Dictionary<string, bool> actualDisk2 = producerTask2.Result;
            var parallelResult = new Dictionary<string, bool>[] { actualDisk1, actualDisk2 }.SelectMany(dict => dict)
                         .ToDictionary(pair => pair.Key, pair => pair.Value);
            Console.WriteLine($"Правильность параллельного выполнения: {(parallelResult.Count == expected.Count) && (!expected.Except(parallelResult).Any())}");
            Console.WriteLine($"Время параллельного выполнения: {parallelWatch.ElapsedTicks}");
            Console.WriteLine("");
        }

        public static void ManyDisksFewCores(Dictionary<string, string> filesHashes, Dictionary<string, bool> expected)
        {
            Console.WriteLine("Много дисков, мало ядер");
            // Последовательное выполнение
            Stopwatch sequentallWatch = new Stopwatch();
            Dictionary<string, bool> sequentallResult = new Dictionary<string, bool>();
            sequentallWatch.Start();
            foreach (var pair in filesHashes)
            {
                using (FileStream fileStream = new FileStream(pair.Key, FileMode.Open))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] byteHash = md5.ComputeHash(fileStream);
                        string hash = BitConverter.ToString(byteHash);
                        sequentallResult.Add(pair.Key, pair.Value == hash);
                    }
                }
            }
            sequentallWatch.Stop();
            Console.WriteLine($"Правильность последовательного выполнения: {(sequentallResult.Count == expected.Count) && (!expected.Except(sequentallResult).Any())}");
            Console.WriteLine($"Время последовательного выполнения: {sequentallWatch.ElapsedTicks}");

            // Параллельное выполнение
            Producer producer1 = new Producer(new FileGroup("1", new Dictionary<string, string>() {
                {filesHashes.ToArray()[0].Key, filesHashes.ToArray()[0].Value },
                {filesHashes.ToArray()[1].Key, filesHashes.ToArray()[1].Value },}), 4);
            Producer producer2 = new Producer(new FileGroup("2", new Dictionary<string, string>() {
                {filesHashes.ToArray()[2].Key, filesHashes.ToArray()[2].Value },
                {filesHashes.ToArray()[3].Key, filesHashes.ToArray()[3].Value },}), 4);
            Producer producer3 = new Producer(new FileGroup("3", new Dictionary<string, string>() {
                {filesHashes.ToArray()[4].Key, filesHashes.ToArray()[4].Value },
                {filesHashes.ToArray()[5].Key, filesHashes.ToArray()[5].Value },}), 4);
            ConsumerController consumerController = new ConsumerController(new List<BufferBlock<(Task, Task)>>() { producer1.ProducerBuffer, producer2.ProducerBuffer, producer3.ProducerBuffer }, 2);

            Stopwatch parallelWatch = new Stopwatch();
            parallelWatch.Start();
            Task<Dictionary<string, bool>> producerTask1 = Task.Run(() => producer1.Execute());
            Task<Dictionary<string, bool>> producerTask2 = Task.Run(() => producer2.Execute());
            Task<Dictionary<string, bool>> producerTask3 = Task.Run(() => producer3.Execute());
            producerTask1.Wait();
            producerTask2.Wait();
            producerTask3.Wait();
            parallelWatch.Stop();
            Dictionary<string, bool> actualDisk1 = producerTask1.Result;
            Dictionary<string, bool> actualDisk2 = producerTask2.Result;
            Dictionary<string, bool> actualDisk3 = producerTask3.Result;
            var parallelResult = new Dictionary<string, bool>[] { actualDisk1, actualDisk2, actualDisk3 }.SelectMany(dict => dict)
                         .ToDictionary(pair => pair.Key, pair => pair.Value);
            Console.WriteLine($"Правильность параллельного выполнения: {(parallelResult.Count == expected.Count) && (!expected.Except(parallelResult).Any())}");
            Console.WriteLine($"Время параллельного выполнения: {parallelWatch.ElapsedTicks}");
            Console.WriteLine("");
        }

        public static void FewDisksManyCores(Dictionary<string, string> filesHashes, Dictionary<string, bool> expected)
        {
            Console.WriteLine("Мало дисков, много ядер");
            // Последовательное выполнение
            Stopwatch sequentallWatch = new Stopwatch();
            Dictionary<string, bool> sequentallResult = new Dictionary<string, bool>();
            sequentallWatch.Start();
            foreach (var pair in filesHashes)
            {
                using (FileStream fileStream = new FileStream(pair.Key, FileMode.Open))
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        byte[] byteHash = md5.ComputeHash(fileStream);
                        string hash = BitConverter.ToString(byteHash);
                        sequentallResult.Add(pair.Key, pair.Value == hash);
                    }
                }
            }
            sequentallWatch.Stop();
            Console.WriteLine($"Правильность последовательного выполнения: {(sequentallResult.Count == expected.Count) && (!expected.Except(sequentallResult).Any())}");
            Console.WriteLine($"Время последовательного выполнения: {sequentallWatch.ElapsedTicks}");

            // Параллельное выполнение
            Producer producer1 = new Producer(new FileGroup("1", new Dictionary<string, string>() {
                {filesHashes.ToArray()[0].Key, filesHashes.ToArray()[0].Value },
                {filesHashes.ToArray()[1].Key, filesHashes.ToArray()[1].Value },
                {filesHashes.ToArray()[2].Key, filesHashes.ToArray()[2].Value },
                {filesHashes.ToArray()[3].Key, filesHashes.ToArray()[3].Value },
                {filesHashes.ToArray()[4].Key, filesHashes.ToArray()[4].Value },
                {filesHashes.ToArray()[5].Key, filesHashes.ToArray()[5].Value }}), 4);
            ConsumerController consumerController = new ConsumerController(new List<BufferBlock<(Task, Task)>>() { producer1.ProducerBuffer }, 4);

            Stopwatch parallelWatch = new Stopwatch();
            parallelWatch.Start();
            Task<Dictionary<string, bool>> producerTask1 = Task.Run(() => producer1.Execute());
            producerTask1.Wait();
            parallelWatch.Stop();
            Dictionary<string, bool> parallelResult = producerTask1.Result;
            Console.WriteLine($"Правильность параллельного выполнения: {(parallelResult.Count == expected.Count) && (!expected.Except(parallelResult).Any())}");
            Console.WriteLine($"Время параллельного выполнения: {parallelWatch.ElapsedTicks}");
            Console.WriteLine("");
        }

        public static void CreateCorrectFile(Dictionary<string, string> filesHashes, Dictionary<string, bool> expected, int number)
        {
            string testFilePath = "./Test" + number + ".txt";
            using (FileStream fstream = new FileStream(testFilePath, FileMode.Create))
            {
                byte[] buffer = new byte[200000];
                (new Random()).NextBytes(buffer);
                fstream.Write(buffer);
                expected.Add(fstream.Name, true);
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashes.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
        }

        public static void CreateIncorrectFile(Dictionary<string, string> filesHashes, Dictionary<string, bool> expected, int number)
        {
            string testFilePath = "./Test" + number + ".txt";
            using (FileStream fstream = new FileStream(testFilePath, FileMode.Create))
            {
                byte[] buffer = new byte[_writeBufferSize];
                (new Random()).NextBytes(buffer);
                fstream.Write(buffer);
                expected.Add(fstream.Name, false);
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashes.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            using (FileStream fstream = new FileStream(testFilePath, FileMode.Create))
            {
                byte[] buffer = new byte[_writeBufferSize];
                (new Random()).NextBytes(buffer);
                fstream.Write(buffer);
            }
        }
    }
}
