using System;
using Xunit;
using static SimpleEventSourcing.Extensions;

namespace SimpleEventSourcing.UnitTests
{
    public class Extensions
    {
        [Fact]
        public void NullObjectIsNotAValidGuid()
        {
            // Arrange

            // Act
            bool isValid = IsValidGuid(null);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void StringEmptyIsNotAValidGuid()
        {
            // Arrange

            // Act
            bool isValid = IsValidGuid(string.Empty);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void WhitespaceStringIsNotAValidGuid()
        {
            // Arrange

            // Act
            bool isValid = IsValidGuid("   ");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void NewGuidIsAValidGuid()
        {
            // Arrange

            // Act
            bool isValid = IsValidGuid(Guid.NewGuid().ToString());

            // Assert
            Assert.True(isValid);
        }
    }
}
