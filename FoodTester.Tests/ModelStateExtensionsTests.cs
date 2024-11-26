using FoodTester.Api.Utility.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;
using Assert = Xunit.Assert;

namespace FoodTester.Tests
{
    public class ModelStateExtensionsTests
    {
        [Fact]
        public void GetErrorMessage_ReturnsFormattedErrorMessage()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Key", "Error message");

            // Act
            var result = ModelStateExtensions.GetErrorMessage(modelState);

            // Assert
            Assert.Contains("Error message", result);
        }
    }
}
