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
            Dictionary<string, string> expected = new Dictionary<string, string>();
            expected.Add("TestPath", "TestHash");
            JsonSerializer.Serialize(expected, new JsonSerializerOptions { WriteIndented = true });
            using (FileStream fstream = new FileStream(jsonPath, FileMode.Create))
            {
                fstream.Seek(0, SeekOrigin.End);
                var options = new JsonSerializerOptions { WriteIndented = true };
                fstream.Write(JsonSerializer.SerializeToUtf8Bytes(expected, options));
            }

            // Act
            Dictionary<string, string> actual = Parser.ParseJSON(jsonPath);

            // Assert
            Assert.IsTrue(expected.Count == actual.Count && !expected.Except(actual).Any());
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
        public void GetDriveInfo_ExistingFile_ReturnIsDriveName()
        {
            // Arrange
            string testFilePath = "./Test.txt";
            using (FileStream fstream = new FileStream(testFilePath, FileMode.Create)) { }
            string expected = (new DriveInfo((new FileInfo(testFilePath)).Directory.Root.FullName)).Name;

            // Act
            string actual = Parser.GetDriveName(testFilePath);

            // Assert
            Assert.IsTrue(expected == actual);
            File.Delete(testFilePath);
        }

        [TestMethod]
        public void GetDriveInfo_NotexistingFile_ReturnIsNull()
        {
            // Arrange
            string fakePath = "Incorrect path";

            // Act
            string actual = Parser.GetDriveName(fakePath);

            // Assert
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void SortFilesByDisks_CorrectDictionary_ReturnIsFileGroupArray()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create)) { }
            string testFilePath2 = "./Test2.txt";
            using (FileStream fstream = new FileStream(testFilePath2, FileMode.Create)) { }

            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add(testFilePath1, "Test1");
            input.Add(testFilePath2, "Test2");
            List<FileGroup> expected = new List<FileGroup>();
            expected.Add(new FileGroup((new StorageInfo()).GetDiskSerialNumber(Parser.GetDriveName(testFilePath1)), input));

            // Act
            List<FileGroup> actual = Parser.SortFilesByDisks(input);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (actual[0].Equals(expected[0])));
            File.Delete(testFilePath1);
            File.Delete(testFilePath2);
        }

        [TestMethod]
        public void SortFilesByDisks_DictionaryWithOneIncorrectValue_ReturnIsFileGroupArrayWithoutIncorrectValue()
        {
            // Arrange
            string testFilePath1 = "./Test1.txt";
            using (FileStream fstream = new FileStream(testFilePath1, FileMode.Create)) { }
            string testFilePath2 = "Incorrect data";

            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add(testFilePath1, "Test1");
            input.Add(testFilePath2, "Test2");
            List<FileGroup> expected = new List<FileGroup>();
            expected.Add(new FileGroup((new StorageInfo()).GetDiskSerialNumber(Parser.GetDriveName(testFilePath1)), new Dictionary<string, string>() { { testFilePath1, "Test1" } }));

            // Act
            List<FileGroup> actual = Parser.SortFilesByDisks(input);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (actual[0].Equals(expected[0])));
            File.Delete(testFilePath1);
        }

        [TestMethod]
        public void SortFilesByDisks_DictionaryWithIncorrectValues_ReturnIsEmptyList()
        {
            // Arrange
            string testFilePath1 = "Incorrect data 1";
            string testFilePath2 = "Incorrect data 2";

            Dictionary<string, string> input = new Dictionary<string, string>();
            input.Add(testFilePath1, "Test1");
            input.Add(testFilePath2, "Test2");

            // Act
            List<FileGroup> actual = Parser.SortFilesByDisks(input);

            // Assert
            Assert.IsTrue(actual.Count == 0);
        }

        [TestMethod]
        public void SortFilesByDisks_EmptyDictionary_ReturnIsEmptyList()
        {
            // Arrange
            Dictionary<string, string> input = new Dictionary<string, string>();

            // Act
            List<FileGroup> actual = Parser.SortFilesByDisks(input);

            // Assert
            Assert.IsTrue(actual.Count == 0);
        }
    }
}