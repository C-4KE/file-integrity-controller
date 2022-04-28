using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void ParseJSON_ExistingFileWithCorrectData_DictionaryWithCorrectPairs()
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
    }
}
