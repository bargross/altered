using Altered.Core.Configure;
using Altered.Core.Extensions;
using System.Linq.Expressions;

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
            Expression<Func<string, object>> expression = x => x.Length;

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
            Expression<Func<string, object>> expression = x => x.Length;
            var expressions = new[] { expression };

            // Act
            configurator.IgnoreMany(expressions);

            // Assert
            Assert.Contains(expressions[0].GetPropertyName(), configurator.GetIgnoredProperties());
            Assert.Contains(expression.GetPropertyName(), configurator.GetIgnoredProperties());
        }

        [Fact]
        public void ShouldIncludeProperty()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            Expression<Func<string, object>> expression = x => x.Length;

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
            Expression<Func<string, object>> expression = x => x.Length;
            var expressions = new[] { expression };

            // Act
            configurator.IncludeMany(expressions);

            // Assert
            Assert.Contains(expressions[0].GetPropertyName(), configurator.GetIncludedProperties());
            Assert.Contains(expression.GetPropertyName(), configurator.GetIncludedProperties());
        }

        [Fact]
        public void ShouldThrowArgumentExceptionWhenIgnoringAndThenIncludingPropertiesSimultaneously()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            Expression<Func<string, object>> expression = x => x.Length;

            configurator.Ignore(expression);

            Action action = () => configurator.Include(expression);

            // Act and Assert
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ShouldThrowArgumentExceptionWhenIncludingAndThenIgnoringPropertiesSimultaneously()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            Expression<Func<string, object>> expression = x => x.Length;

            configurator.Include(expression);

            Action action = () => configurator.Ignore(expression);

            // Act and Assert
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ShouldClearIgnoredProperties()
        {
            // Arrange
            var configurator = new TypeConfigurator();
            Expression<Func<string, object>> expression = x => x.Length;

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
            Expression<Func<string, object>> expression = x => x.Length;

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
            Expression<Func<string, object>> expression = x => x.Length;

            // Act
            configurator.Include(expression);

            // Assert
            Assert.Contains(expression.GetPropertyName(), configurator.GetIncludedProperties());
        }
    }
}
