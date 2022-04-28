using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.Linq;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ParseJSON_ExistingFileWithCorrectData_ReturnIsDictionaryWithCorrectPairs()
        {
            // Arrange
            string jsonPath = "./TestJSON.json";
            Dictionary<string, string> expexted = new Dictionary<string, string>();
            expexted.Add("TestPath", "TestHash");
            JsonSerializer.Serialize(expexted, new JsonSerializerOptions { WriteIndented = true });
            using (FileStream fstream = new FileStream(jsonPath, FileMode.Create))
            {
                fstream.Seek(0, SeekOrigin.End);
                var options = new JsonSerializerOptions { WriteIndented = true };
                fstream.Write(JsonSerializer.SerializeToUtf8Bytes(expexted, options));
            }

            // Act
            Dictionary<string, string> actual = Parser.ParseJSON(jsonPath);

            // Assert
            Assert.IsTrue(expexted.Count == actual.Count && !expexted.Except(actual).Any());
            File.Delete(jsonPath);
        }

        [TestMethod]
        public void ParseJSON_ExistingFileWithIncorrectData_ReturnIsNull()
        {
            // Arrange
            string jsonPath = "./TestJSON.json";
            using (FileStream fstream = new FileStream(jsonPath, FileMode.Create))
            {
                fstream.Seek(0, SeekOrigin.End);
                fstream.Write(Encoding.UTF8.GetBytes("Incorrect data"));
            }

            // Act
            Dictionary<string, string> actual = Parser.ParseJSON(jsonPath);

            // Assert
            Assert.IsNull(actual);
            File.Delete(jsonPath);
        }

        [TestMethod]
        public void ParseJSON_ExistingFileWithoutData_ReturnIsNull()
        {
            // Arrange
            string jsonPath = "./TestJSON.json";
            using (FileStream fstream = new FileStream(jsonPath, FileMode.Create)) { }

            // Act
            Dictionary<string, string> actual = Parser.ParseJSON(jsonPath);

            // Assert
            Assert.IsNull(actual);
            File.Delete(jsonPath);
        }

        [TestMethod]
        public void ParseJSON_NotexistingFile_ReturnIsNull()
        {
            // Arrange
            string jsonPath = "./TestJSON.json";

            // Act
            Dictionary<string, string> actual = Parser.ParseJSON(jsonPath);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetDriveInfo_ExistingFile_ReturnIsDriveInfo()
        {
            // Arrange
            string testFilePath = "./Test.txt";
            using (FileStream fstream = new FileStream(testFilePath, FileMode.Create)) { }
            DriveInfo expected = new DriveInfo((new FileInfo(testFilePath)).Directory.Root.FullName);

            // Act
            DriveInfo actual = Parser.GetDriveInfo(testFilePath);

            // Assert
            Assert.IsTrue(DriveInfoEquality(expected, actual));
            File.Delete(testFilePath);
        }

        private bool DriveInfoEquality(DriveInfo first, DriveInfo second)
        {
            return (first.AvailableFreeSpace == second.AvailableFreeSpace) &&
                   (first.DriveFormat == second.DriveFormat) &&
                   (first.DriveType == second.DriveType) &&
                   (first.IsReady == second.IsReady) &&
                   (first.Name == second.Name) &&
                   (first.TotalFreeSpace == second.TotalFreeSpace) &&
                   (first.TotalSize == second.TotalSize) &&
                   (first.VolumeLabel == second.VolumeLabel);
        }

        [TestMethod]
        public void GetDriveInfo_NotexistingFile_ReturnIsNull()
        {
            // Arrange
            string fakePath = "Incorrect path";

            // Act
            DriveInfo actual = Parser.GetDriveInfo(fakePath);

            // Assert
            Assert.IsNull(actual);
        }
    }
}
