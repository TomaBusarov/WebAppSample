using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Contoso.Mathlib.Tests
{
    [TestClass]
    public class RoundingsTests
    {
        [TestMethod]
        [Description("Rounds down for positive numbers with low decimal part.")]
        public void Floor_PositiveLow()
        {
            // Arrange
            double d = 123.123;

            // Act
            var res = Contoso.Mathlib.Roundings.Floor(d);

            // Assert
            Assert.AreEqual(123, res);
        }
        [TestMethod]
        [Description("Rounds down for positive numbers with high decimal part.")]
        public void Floor_PositiveHigh()
        {
            // Arrange
            double d = 123.999;

            // Act
            var res = Contoso.Mathlib.Roundings.Floor(d);

            // Assert
            Assert.AreEqual(123, res);
        }

        [TestMethod]
        [Description("Rounds down for negative numbers with high decimal part.")]
        public void Floor_NegativeHigh()
        {
            // Arrange
            double d = -123.999;

            // Act
            var res = Contoso.Mathlib.Roundings.Floor(d);

            // Assert
            Assert.AreEqual(-123, res);
        }

        [TestMethod]
        [Description("Rounds up for positive numbers with high decimal part.")]
        public void Ceiling_PositiveHigh()
        {
            // Arrange
            double d = 123.74;

            // Act
            var res = Contoso.Mathlib.Roundings.Ceiling(d);

            // Assert
            Assert.AreEqual(124, res);
        }

        [TestMethod]
        [Description("Rounds up for positive numbers with low decimal part.")]
        public void Ceiling_PositiveLow()
        {
            // Arrange
            double d = 123.499;

            // Act
            var res = Contoso.Mathlib.Roundings.Ceiling(d);

            // Assert
            Assert.AreEqual(124, res);
        }
    }
}
