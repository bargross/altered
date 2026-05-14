using System.Linq.Expressions;
using Altered.Core.Configure;
using Moq;
using Altered.Core.Extensions;

namespace Altered.Tests.Configure
{
    public class TypeConfiguratorTests
    {
        [Fact]
        public void ShouldConfigureType()
        {
            // Arrange
            var configurator = new TypeConfigurator();

            // Act
            configurator.Configure<string>();

            // Assert
            Assert.NotNull(configurator.Type);
        }

        [Fact]
        public void ShouldConfigureTypeWithExistingType()
        {
            // Arrange
            var type = typeof(string);
            var configurator = new TypeConfigurator(type);

            // Act
            configurator.Configure<string>();

            // Assert
            Assert.Equal(type, configurator.Type);
        }

        [Fact]
        public void ShouldIgnoreProperty()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expression = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.Ignore(expression);

            // Assert
            Assert.Contains(expression.GetPropertyName(), configurator.GetIgnoredProperties());
        }

        [Fact]
        public void ShouldIgnoreManyProperties()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expressions = new[] { Mock.Of<Expression<Func<string, object>>>() };
            var expression2 = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.IgnoreMany(expressions);

            // Assert
            Assert.Contains(expressions[0].GetPropertyName(), configurator.GetIgnoredProperties());
            Assert.Contains(expression2.GetPropertyName(), configurator.GetIgnoredProperties());
        }

        [Fact]
        public void ShouldIncludeProperty()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expression = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.Include(expression);

            // Assert
            Assert.Contains(expression.GetPropertyName(), configurator.GetIncludedProperties());
        }

        [Fact]
        public void ShouldIncludeManyProperties()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expressions = new[] { Mock.Of<Expression<Func<string, object>>>() };
            var expression2 = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.IncludeMany(expressions);

            // Assert
            Assert.Contains(expressions[0].GetPropertyName(), configurator.GetIncludedProperties());
            Assert.Contains(expression2.GetPropertyName(), configurator.GetIncludedProperties());
        }

        [Fact]
        public void ShouldThrowArgumentExceptionWhenIgnoringAndIncludingPropertiesSimultaneously()
        {
            // Arrange
            var configurator = new TypeConfigurator();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => configurator.Ignore(Mock.Of<Expression<Func<string, object>>>()));
            Assert.Throws<ArgumentException>(() => configurator.Include(Mock.Of<Expression<Func<string, object>>>()));
        }

        [Fact]
        public void ShouldClearIgnoredProperties()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expression = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.Ignore(expression);
            configurator.Clear();

            // Assert
            Assert.Empty(configurator.GetIgnoredProperties());
        }

        [Fact]
        public void ShouldGetIgnoredProperties()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expression = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.Ignore(expression);

            // Assert
            Assert.Contains(expression.GetPropertyName(), configurator.GetIgnoredProperties());
        }

        [Fact]
        public void ShouldGetIncludedProperties()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            var expression = Mock.Of<Expression<Func<string, object>>>();

            // Act
            configurator.Include(expression);

            // Assert
            Assert.Contains(expression.GetPropertyName(), configurator.GetIncludedProperties());
        }
    }
}
