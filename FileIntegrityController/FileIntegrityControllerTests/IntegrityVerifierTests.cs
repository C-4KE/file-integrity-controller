using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class IntegrityVerifierTests
    {
        [TestMethod]
        public void VerifyFile_ExistingFileWithCorrectHash_ReturnIsTrue()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            string hash;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash = BitConverter.ToString(hashBytes);
                }
            }
            KeyValuePair<string, string> fileHash = new KeyValuePair<string, string>(testFilePath1, hash);

            // Act
            bool actual = IntegrityVerifier.VerifyFile(fileHash);

            // Assert
            Assert.IsTrue(actual);
            File.Delete(testFilePath1);
        }

        [TestMethod]
        public void VerifyFile_ExistingFileWithIncorrectHash_ReturnIsFalse()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            string hash;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash = BitConverter.ToString(hashBytes);
                }
            }
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123"));
            }
            KeyValuePair<string, string> fileHash = new KeyValuePair<string, string>(testFilePath1, hash);

            // Act
            bool actual = IntegrityVerifier.VerifyFile(fileHash);

            // Assert
            Assert.IsFalse(actual);
            File.Delete(testFilePath1);
        }

        [TestMethod]
        public void VerifyFile_NotexistingFile_ReturnIsFalse()
        {
            // Arrange
            string fakeFilePath = "FakePath";
            string fakeHash = "FakeHash";
            KeyValuePair<string, string> fileHash = new KeyValuePair<string, string>(fakeFilePath, fakeHash);

            // Act
            bool actual = IntegrityVerifier.VerifyFile(fileHash);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void VerifyGroupSingleThread_AllFilesAreValid_ReturnIsEmptyList()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            string hash1, hash2;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash1 = BitConverter.ToString(hashBytes);
                }
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash2 = BitConverter.ToString(hashBytes);
                }
            }
            Dictionary<string, string> filesHashes = new Dictionary<string, string>();
            filesHashes.Add(testFilePath1, hash1);
            filesHashes.Add(testFilePath2, hash2);
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFilePath1).Name, filesHashes);

            // Act
            List<string> actual = IntegrityVerifier.VerifyGroupSingleThread(fileGroup);

            // Assert
            Assert.IsTrue(actual.Count == 0);
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);
        }

        [TestMethod]
        public void VerifyGroupSingleThread_SomeFilesAreInvalid_ReturnIsListWithInvalidFiles()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            string hash1, hash2;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash1 = BitConverter.ToString(hashBytes);
                }
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash2 = BitConverter.ToString(hashBytes);
                }
            }
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABC"));
            }
            Dictionary<string, string> filesHashes = new Dictionary<string, string>();
            filesHashes.Add(testFilePath1, hash1);
            filesHashes.Add(testFilePath2, hash2);
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFilePath1).Name, filesHashes);
            List<string> expected = new List<string>();
            expected.Add(testFilePath2);

            // Act
            List<string> actual = IntegrityVerifier.VerifyGroupSingleThread(fileGroup);

            // Assert
            Assert.IsTrue(expected.Count == actual.Count && !expected.Except(actual).Any());
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);
        }

        [TestMethod]
        public void VerifyGroupSingleThread_AllFilesAreInvalid_ReturnIsListWithInvalidFiles()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456789"));
            }
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABCDEF"));
            }
            string hash1, hash2;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream fstream = new FileStream(testFilePath1, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash1 = BitConverter.ToString(hashBytes);
                }
                using (FileStream fstream = new FileStream(testFilePath2, FileMode.Open))
                {
                    byte[] hashBytes = md5.ComputeHash(fstream);
                    hash2 = BitConverter.ToString(hashBytes);
                }
            }
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("123456"));
            }
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create))
            {
                fstream.Write(Encoding.UTF8.GetBytes("ABC"));
            }
            Dictionary<string, string> filesHashes = new Dictionary<string, string>();
            filesHashes.Add(testFilePath1, hash1);
            filesHashes.Add(testFilePath2, hash2);
            FileGroup fileGroup = new FileGroup(Parser.GetDriveInfo(testFilePath1).Name, filesHashes);
            List<string> expected = new List<string>();
            expected.Add(testFilePath1);
            expected.Add(testFilePath2);

            // Act
            List<string> actual = IntegrityVerifier.VerifyGroupSingleThread(fileGroup);

            // Assert
            Assert.IsTrue(expected.Count == actual.Count && !expected.Except(actual).Any());
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);
        }
    }
}
