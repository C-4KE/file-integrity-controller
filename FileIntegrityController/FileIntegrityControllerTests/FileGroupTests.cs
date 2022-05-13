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
            FileGroup firstGroup = new FileGroup("ABCDEF123456", new Dictionary<string, string>() { { "1", "1" } });
            FileGroup secondGroup = new FileGroup("ABCDEF123456", new Dictionary<string, string>() { { "1", "1" } });

            // Act
            bool actual = firstGroup.Equals(secondGroup);

            // Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void Equals_UnequalObjects_ReturnIsFalse()
        {
            // Arrange
            FileGroup firstGroup = new FileGroup("ABCDEF123456", new Dictionary<string, string>() { { "1", "1" } });
            FileGroup secondGroup = new FileGroup("ABCDEF123456", new Dictionary<string, string>() { { "1", "1" }, { "2", "2" } });

            // Act
            bool actual = firstGroup.Equals(secondGroup);

            // Assert
            Assert.IsFalse(actual);
        }
    }
}