using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class ProducerConsumerTests
    {
        [TestMethod]
        public void Execute_OneProducerAllFilesAreValid_ReturnIsDictionary()
        {
            // Arrange
            Dictionary<string, string> filesHashes = new Dictionary<string, string>();
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashes.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashes.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            FileGroup fileGroup = new FileGroup("DISK1", filesHashes);
            Dictionary<string, bool> expected = new Dictionary<string, bool>() { { filesHashes.Keys.ToArray()[0], true }, { filesHashes.Keys.ToArray()[1], true } };
            BufferBlock<Task<KeyValuePair<string, bool>>> consumerBuffer = new BufferBlock<Task<KeyValuePair<string, bool>>>();
            Producer producer = new Producer(fileGroup, consumerBuffer);
            Consumer consumer = new Consumer(consumerBuffer, new List<BufferBlock<Task<KeyValuePair<string, bool>>>>() { producer.ProducerBuffer });

            // Act
            Task consumerTask = Task.Run(() => consumer.Execute());
            Task<Dictionary<string, bool>> producerTask = Task.Run(() => producer.Execute());
            consumerTask.Wait();
            producerTask.Wait();
            Dictionary<string, bool> actual = producerTask.Result;
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);

            // Assert
            Assert.IsTrue(expected.Count == actual.Count && !expected.Except(actual).Any());
        }

        [TestMethod]
        public void Execute_OneProducerSomeFilesAreInvalid_ReturnIsDictionary()
        {
            // Arrange
            Dictionary<string, string> filesHashes = new Dictionary<string, string>();
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashes.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashes.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCD"));
            }
            FileGroup fileGroup = new FileGroup("DISK1", filesHashes);
            Dictionary<string, bool> expected = new Dictionary<string, bool>() { { filesHashes.Keys.ToArray()[0], true }, { filesHashes.Keys.ToArray()[1], false } };
            BufferBlock<Task<KeyValuePair<string, bool>>> consumerBuffer = new BufferBlock<Task<KeyValuePair<string, bool>>>();
            Producer producer = new Producer(fileGroup, consumerBuffer);
            Consumer consumer = new Consumer(consumerBuffer, new List<BufferBlock<Task<KeyValuePair<string, bool>>>>() { producer.ProducerBuffer });

            // Act
            Task consumerTask = Task.Run(() => consumer.Execute());
            Task<Dictionary<string, bool>> producerTask = Task.Run(() => producer.Execute());
            consumerTask.Wait();
            producerTask.Wait();
            Dictionary<string, bool> actual = producerTask.Result;
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);

            // Assert
            Assert.IsTrue(expected.Count == actual.Count && !expected.Except(actual).Any());
        }

        [TestMethod]
        public void Execute_ManyProducersAllFilesAreValid_ReturnIsDictionary()
        {
            // Arrange
            Dictionary<string, string> filesHashesDisk1 = new Dictionary<string, string>();
            Dictionary<string, string> filesHashesDisk2 = new Dictionary<string, string>();
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashesDisk1.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashesDisk2.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            string testFilePath3 = "./Test3.txt";
            using (FileStream fstream = new FileStream(testFilePath3, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF5"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath3, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashesDisk2.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            FileGroup fileGroupDisk1 = new FileGroup("DISK1", filesHashesDisk1);
            FileGroup fileGroupDisk2 = new FileGroup("DISK2", filesHashesDisk2);
            Dictionary<string, bool> expectedDisk1 = new Dictionary<string, bool>() { { filesHashesDisk1.Keys.ToArray()[0], true } };
            Dictionary<string, bool> expectedDisk2 = new Dictionary<string, bool>() { { filesHashesDisk2.Keys.ToArray()[0], true }, { filesHashesDisk2.Keys.ToArray()[1], true } };
            BufferBlock<Task<KeyValuePair<string, bool>>> consumerBuffer = new BufferBlock<Task<KeyValuePair<string, bool>>>();
            Producer producerDisk1 = new Producer(fileGroupDisk1, consumerBuffer);
            Producer producerDisk2 = new Producer(fileGroupDisk2, consumerBuffer);
            Consumer consumer = new Consumer(consumerBuffer, new List<BufferBlock<Task<KeyValuePair<string, bool>>>>() { producerDisk1.ProducerBuffer, producerDisk2.ProducerBuffer });

            // Act
            Task consumerTask = Task.Run(() => consumer.Execute());
            Task<Dictionary<string, bool>> producerTask1 = Task.Run(() => producerDisk1.Execute());
            Task<Dictionary<string, bool>> producerTask2 = Task.Run(() => producerDisk2.Execute());
            consumerTask.Wait();
            producerTask1.Wait();
            producerTask2.Wait();
            Dictionary<string, bool> actualDisk1 = producerTask1.Result;
            Dictionary<string, bool> actualDisk2 = producerTask2.Result;
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);
            File.Delete(testFilePath3);

            // Assert
            Assert.IsTrue(expectedDisk1.Count == actualDisk1.Count && !expectedDisk1.Except(actualDisk1).Any()
                       && expectedDisk2.Count == actualDisk2.Count && !expectedDisk2.Except(actualDisk2).Any());
        }

        [TestMethod]
        public void Execute_ManyProducersSomeFilesAreInvalid_ReturnIsDictionary()
        {
            // Arrange
            Dictionary<string, string> filesHashesDisk1 = new Dictionary<string, string>();
            Dictionary<string, string> filesHashesDisk2 = new Dictionary<string, string>();
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashesDisk1.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashesDisk2.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            string testFilePath3 = "./Test3.txt";
            using (FileStream fstream = new FileStream(testFilePath3, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF5"));
            }
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath3, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    filesHashesDisk2.Add(fstream.Name, BitConverter.ToString(hashBytes));
                }
            }
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABC"));
            }
            using (FileStream fstream = new FileStream(testFilePath3, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("AB"));
            }
            FileGroup fileGroupDisk1 = new FileGroup("DISK1", filesHashesDisk1);
            FileGroup fileGroupDisk2 = new FileGroup("DISK2", filesHashesDisk2);
            Dictionary<string, bool> expectedDisk1 = new Dictionary<string, bool>() { { filesHashesDisk1.Keys.ToArray()[0], false } };
            Dictionary<string, bool> expectedDisk2 = new Dictionary<string, bool>() { { filesHashesDisk2.Keys.ToArray()[0], true }, { filesHashesDisk2.Keys.ToArray()[1], false } };
            BufferBlock<Task<KeyValuePair<string, bool>>> consumerBuffer = new BufferBlock<Task<KeyValuePair<string, bool>>>();
            Producer producerDisk1 = new Producer(fileGroupDisk1, consumerBuffer);
            Producer producerDisk2 = new Producer(fileGroupDisk2, consumerBuffer);
            Consumer consumer = new Consumer(consumerBuffer, new List<BufferBlock<Task<KeyValuePair<string, bool>>>>() { producerDisk1.ProducerBuffer, producerDisk2.ProducerBuffer });

            // Act
            Task consumerTask = Task.Run(() => consumer.Execute());
            Task<Dictionary<string, bool>> producerTask1 = Task.Run(() => producerDisk1.Execute());
            Task<Dictionary<string, bool>> producerTask2 = Task.Run(() => producerDisk2.Execute());
            consumerTask.Wait();
            producerTask1.Wait();
            producerTask2.Wait();
            Dictionary<string, bool> actualDisk1 = producerTask1.Result;
            Dictionary<string, bool> actualDisk2 = producerTask2.Result;
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);
            File.Delete(testFilePath3);

            // Assert
            Assert.IsTrue(expectedDisk1.Count == actualDisk1.Count && !expectedDisk1.Except(actualDisk1).Any()
                       && expectedDisk2.Count == actualDisk2.Count && !expectedDisk2.Except(actualDisk2).Any());
        }
    }
}
