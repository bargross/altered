using Moq;
using Altered.Core.Configure;

namespace Altered.Tests.Configure
{
    public class TypeConfigurationManagerTests
    {
        [Fact]
        public void ShouldConfigureTypeWithoutConfigurator()
        {
            // Arrange
            var manager = new TypeConfigurationManager();

            // Act
            var result = manager.Configure<string>();

            // Assert
            Assert.NotNull(result);
            Assert.True(manager.IsTypeConfigured<string>());
        }

        [Fact]
        public void ShouldConfigureTypeWithConfigurator()
        {
            // Arrange
            var manager = new TypeConfigurationManager();
            var configuratorMock = new Mock<TypeConfigurator>();

            // Act
            var result = manager.Configure<string>(configuratorMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.True(manager.IsTypeConfigured<string>());
        }

        [Fact]
        public void ShouldThrowExceptionWhenConfigureWithoutConfigurator()
        {
            // Arrange
            var manager = new TypeConfigurationManager();
            var configuratorMock = new Mock<TypeConfigurator>();

            configuratorMock.Setup(c => c.Configure(It.IsAny<Type>())).Throws<ArgumentException>();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => manager.Configure<string>(configuratorMock.Object));
        }

        [Fact]
        public void ShouldThrowExceptionWhenIgnorePropertiesWithoutConfigure()
        {
            // Arrange
            var manager = new TypeConfigurationManager();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => manager.IgnoreProperties<string>(x => x.Length));
        }

        [Fact]
        public void ShouldIgnoreProperties()
        {
            // Arrange
            var manager = new TypeConfigurationManager();

            manager.Configure<string>();

            // Act
            var result = manager.IgnoreProperties<string>(x => x.Length);

            // Assert
            Assert.NotNull(result);
            Assert.True(manager.IsTypeConfigured<string>());
            Assert.True(manager.PropertyIsIgnored<string>(nameof(string.Length)));
        }

        [Fact]
        public void ShouldIncludeProperties()
        {
            // Arrange
            var manager = new TypeConfigurationManager();
        
            manager.Configure<string>();

            // Act
            var result = manager.IncludeProperties<string>(x => x.Length);

            // Assert
            Assert.NotNull(result);
            Assert.True(manager.IsTypeConfigured<string>());
            Assert.False(manager.PropertyIsIgnored<string>(nameof(string.Length)));
        }

        [Fact]
        public void ShouldThrowExceptionWhenIncludePropertiesWithoutConfigure()
        {
            // Arrange
            var manager = new TypeConfigurationManager();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => manager.IncludeProperties<string>(x => x.Length));
        }

        [Fact]
        public void ShouldPropertyIsIncluded()
        {
            // Arrange
            var manager = new TypeConfigurationManager();
        
            manager.Configure<string>();

            // Act
            manager.IncludeProperties<string>(x => x.Length);

            // Assert
            Assert.True(manager.PropertyIsIncluded<string>(nameof(string.Length)));
        }

        [Fact]
        public void ShouldPropertyIsIgnored()
        {
            // Arrange
            var manager = new TypeConfigurationManager();
        
            manager.Configure<string>();
        
            // Act
            manager.IgnoreProperties<string>(X => X.Length);

            // Assert
            Assert.True(manager.PropertyIsIgnored<string>(nameof(string.Length)));
        }
    }
}

