using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class DistributorTests
    {
        [TestMethod]
        public void DistributeToThreads_OneThreadCorrectFile_ReturnIsEmptyList()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            string hash1;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash1 = BitConverter.ToString(hashBytes);
                }
            }
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFilePath1).Name, new Dictionary<string, string>() { { testFilePath1, hash1 } });

            // Act
            List<string> actual = Distributor.DistributeToThreads(fileGroup);

            // Assert
            Assert.IsTrue(actual.Count == 0);
            File.Delete(testFilePath1);
        }

        [TestMethod]
        public void DistributeToThreads_OneThreadIncorrectFile_ReturnIsList()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            string hash1;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash1 = BitConverter.ToString(hashBytes);
                }
            }
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456"));
            }
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFilePath1).Name, new Dictionary<string, string>() { { testFilePath1, hash1 } });
            List<string> expected = new List<string>() { testFilePath1 };

            // Act
            List<string> actual = Distributor.DistributeToThreads(fileGroup);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (!actual.Except(expected).Any()));
            File.Delete(testFilePath1);
        }

        [TestMethod]
        public void DistributeToThreads_MultipleThreadsWithAllCorrectFiles_ReturnIsEmptyList()
        {
            // Arrange
            int numberOfFiles = 7;
            string[] testFiles = new string[numberOfFiles];
            for (int i = 0; i < testFiles.Length; i++)
            {
                testFiles[i] = "./Test" + (i + 1) + ".txt";
                using (FileStream fstream = new FileStream(testFiles[i], FileMode.Create))
                {
                    fstream.Write(Encoding.UTF8.GetBytes($"123456789_{i}"));
                }
            }
            string[] hashes = new string[numberOfFiles];
            using (MD5 md5 = MD5.Create())
            {
                for (int i = 0; i < testFiles.Length; i++)
                {
                    using (FileStream fstream = new FileStream(testFiles[i], FileMode.Open))
                    {
                        byte[] hashBytes = md5.ComputeHash(fstream);
                        hashes[i] = BitConverter.ToString(hashBytes);
                    }
                }
            }
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFiles[0]).Name, new Dictionary<string, string>());
            for (int i = 0; i < testFiles.Length; i++)
            {
                fileGroup.FilesHashes.Add(testFiles[i], hashes[i]);
            }

            // Act
            List<string> actual = Distributor.DistributeToThreads(fileGroup);

            // Assert
            Assert.IsTrue(actual.Count == 0);
            for (int i = 0; i < testFiles.Length; i++)
            {
                File.Delete(testFiles[i]);
            }
        }

        [TestMethod]
        public void DistributeToThreads_MultipleThreadsWithSomeIncorrectFiles_ReturnIsEmptyList()
        {
            // Arrange
            int numberOfFiles = 7;
            string[] testFiles = new string[numberOfFiles];
            for (int i = 0; i < testFiles.Length; i++)
            {
                testFiles[i] = "./Test" + (i + 1) + ".txt";
                using (FileStream fstream = new FileStream(testFiles[i], FileMode.Create))
                {
                    fstream.Write(Encoding.UTF8.GetBytes($"123456789_{i}"));
                }
            }
            string[] hashes = new string[numberOfFiles];
            using (MD5 md5 = MD5.Create())
            {
                for (int i = 0; i < testFiles.Length; i++)
                {
                    using (FileStream fstream = new FileStream(testFiles[i], FileMode.Open))
                    {
                        byte[] hashBytes = md5.ComputeHash(fstream);
                        hashes[i] = BitConverter.ToString(hashBytes);
                    }
                }
            }
            using (FileStream fstream = new FileStream(testFiles[1], FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes($"123456789_A"));
            }
            using (FileStream fstream = new FileStream(testFiles[5], FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes($"123456789_B"));
            }
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFiles[0]).Name, new Dictionary<string, string>());
            for (int i = 0; i < testFiles.Length; i++)
            {
                fileGroup.FilesHashes.Add(testFiles[i], hashes[i]);
            }
            List<string> expected = new List<string>() { testFiles[1], testFiles[5] };

            // Act
            List<string> actual = Distributor.DistributeToThreads(fileGroup);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (!actual.Except(expected).Any()));
            for (int i = 0; i < testFiles.Length; i++)
            {
                File.Delete(testFiles[i]);
            }
        }
    }
}
