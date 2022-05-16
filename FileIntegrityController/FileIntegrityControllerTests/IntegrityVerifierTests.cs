using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class IntegrityVerifierTests
    {
        [TestMethod]
        public void VerifyHash_ExistingFileWithCorrectHash_ReturnIsTrue()
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

            // Act
            bool actual;
            using (FileStream fileStream = new FileStream(testFilePath1, FileMode.Open))
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] buffer = new byte[4096];
                    int readAmount = 0;
                    while ((readAmount = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        md5.TransformBlock(buffer, 0, readAmount, buffer, 0);
                    }
                    actual = IntegrityVerifier.VerifyHash(md5, hash);
                }
            }

            // Assert
            Assert.IsTrue(actual);
            File.Delete(testFilePath1);
        }

        [TestMethod]
        public void VerifyHash_ExistingFileWithIncorrectHash_ReturnIsFalse()
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

            // Act
            bool actual;
            using (FileStream fileStream = new FileStream(testFilePath1, FileMode.Open))
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] buffer = new byte[4096];
                    int readAmount = 0;
                    while ((readAmount = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        md5.TransformBlock(buffer, 0, readAmount, buffer, 0);
                    }
                    actual = IntegrityVerifier.VerifyHash(md5, hash);
                }
            }

            // Assert
            Assert.IsFalse(actual);
            File.Delete(testFilePath1);
        }
    }
}