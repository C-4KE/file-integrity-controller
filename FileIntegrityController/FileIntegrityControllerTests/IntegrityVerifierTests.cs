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
            FileStream fileStream = new FileStream(testFilePath1, FileMode.Open);

            // Act
            bool actual = IntegrityVerifier.VerifyFile(fileStream, hash);

            // Assert
            Assert.IsTrue(actual);
            fileStream.Close();
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
            FileStream fileStream = new FileStream(testFilePath1, FileMode.Open);

            // Act
            bool actual = IntegrityVerifier.VerifyFile(fileStream, hash);

            // Assert
            Assert.IsFalse(actual);
            fileStream.Close();
            File.Delete(testFilePath1);
        }
    }
}