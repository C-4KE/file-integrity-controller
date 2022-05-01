using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using FileIntegrityController;

namespace FileIntegrityControllerTests
{
    [TestClass]
    public class FileGroupTests
    {
        [TestMethod]
        public void Equals_EqualObjects_ReturnIsTrue()
        {
            // Arrange
            FileGroup firstGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" } });
            FileGroup secondGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" } });

            // Act
            bool actual = firstGroup.Equals(secondGroup);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Equals_UnequalObjects_ReturnIsFalse()
        {
            // Arrange
            FileGroup firstGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" } });
            FileGroup secondGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" } });

            // Act
            bool actual = firstGroup.Equals(secondGroup);

            // Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void Split_OnePart_ReturnIsList()
        {
            // Arrange
            FileGroup fileGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" } });
            List<FileGroup> expected = new List<FileGroup>();
            expected.Add(fileGroup);

            // Act
            List<FileGroup> actual = fileGroup.Split(1);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (!actual.Except(expected).Any()));
        }

        [TestMethod]
        public void Split_MultiplePartsWithEqualNumberOfElements_ReturnIsList()
        {
            // Arrange
            FileGroup fileGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" }, { "3", "3" }, { "4", "4" } });
            List<FileGroup> expected = new List<FileGroup>();
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" } }));
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "3", "3" }, { "4", "4" } }));

            // Act
            List<FileGroup> actual = fileGroup.Split(2);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (!actual.Except(expected).Any()));
        }

        [TestMethod]
        public void Split_MultiplePartsWithUnequalNumberOfElements_ReturnIsList()
        {
            // Arrange
            FileGroup fileGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" }, { "3", "3" }, { "4", "4" }, { "5", "5" } });
            List<FileGroup> expected = new List<FileGroup>();
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" } }));
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "3", "3" }, { "4", "4" } }));
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "5", "5" } }));

            // Act
            List<FileGroup> actual = fileGroup.Split(3);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (!actual.Except(expected).Any()));
        }

        [TestMethod]
        public void Split_MultiplePartsWithUnequalNumberOfElementsThatCantBeSplitByParts_ReturnIsListSplittedByPartsMinusOne()
        {
            // Arrange
            FileGroup fileGroup = new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" }, { "3", "3" }, { "4", "4" } });
            List<FileGroup> expected = new List<FileGroup>();
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" } }));
            expected.Add(new FileGroup("D:\\", new Dictionary<string, string>() { { "3", "3" }, { "4", "4" } }));

            // Act
            List<FileGroup> actual = fileGroup.Split(3);

            // Assert
            Assert.IsTrue((actual.Count == expected.Count) && (!actual.Except(expected).Any()));
        }
    }
}
