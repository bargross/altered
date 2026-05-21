using Altered.Core.Configure;

namespace Altered.Tests.Configure
{
    public class TypeConfigurationManagerTests
    {
        private class SampleModel
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Email { get; set; } = string.Empty;
        }

        private class OtherModel
        {
            public string Title { get; set; } = string.Empty;
        }

        // -----------------------------------------------------------------------------------------
        // Configure<TValue>()
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_Generic_RegistersType()
        {
            var manager = new TypeConfigurationManager();

            manager.Configure<SampleModel>();

            Assert.True(manager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void Configure_Generic_CalledTwice_DoesNotThrow()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            var ex = Record.Exception(() => manager.Configure<SampleModel>());

            Assert.Null(ex);
        }

        [Fact]
        public void Configure_Generic_ReturnsSameInstance()
        {
            var manager = new TypeConfigurationManager();

            var result = manager.Configure<SampleModel>();

            Assert.Same(manager, result);
        }

        // -----------------------------------------------------------------------------------------
        // Configure<TValue>(TypeConfigurator)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_WithConfigurator_RegistersType()
        {
            var manager = new TypeConfigurationManager();
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            manager.Configure<SampleModel>(configurator);

            Assert.True(manager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void Configure_WithConfigurator_StoresProvidedConfigurator()
        {
            var manager = new TypeConfigurationManager();
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            manager.Configure<SampleModel>(configurator);

            Assert.True(manager.PropertyIsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void Configure_WithConfigurator_CalledTwice_DoesNotOverwrite()
        {
            var manager = new TypeConfigurationManager();
            var first = new TypeConfigurator();
            first.Configure<SampleModel>();
            first.Ignore<SampleModel>(x => x.Name);

            var second = new TypeConfigurator();
            second.Configure<SampleModel>();

            manager.Configure<SampleModel>(first);
            manager.Configure<SampleModel>(second);

            Assert.True(manager.PropertyIsIgnored<SampleModel>("Name"));
        }

        // -----------------------------------------------------------------------------------------
        // Configure(Type, TypeConfigurator?)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_Type_WithoutConfigurator_RegistersType()
        {
            var manager = new TypeConfigurationManager();

            manager.Configure(typeof(SampleModel));

            Assert.True(manager.IsTypeConfigured(typeof(SampleModel)));
        }

        [Fact]
        public void Configure_Type_WithConfigurator_RegistersType()
        {
            var manager = new TypeConfigurationManager();
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            manager.Configure(typeof(SampleModel), configurator);

            Assert.True(manager.IsTypeConfigured(typeof(SampleModel)));
        }

        [Fact]
        public void Configure_Type_CalledTwiceWithoutConfigurator_DoesNotThrow()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure(typeof(SampleModel));

            var ex = Record.Exception(() => manager.Configure(typeof(SampleModel)));

            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // IsTypeConfigured
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IsTypeConfigured_Generic_NotConfigured_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();

            Assert.False(manager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void IsTypeConfigured_Generic_AfterConfigure_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            Assert.True(manager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void IsTypeConfigured_Type_NotConfigured_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();

            Assert.False(manager.IsTypeConfigured(typeof(SampleModel)));
        }

        [Fact]
        public void IsTypeConfigured_Type_AfterConfigure_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure(typeof(SampleModel));

            Assert.True(manager.IsTypeConfigured(typeof(SampleModel)));
        }

        [Fact]
        public void IsTypeConfigured_DifferentTypeNotConfigured_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            Assert.False(manager.IsTypeConfigured<OtherModel>());
        }

        // -----------------------------------------------------------------------------------------
        // IgnoreProperties
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IgnoreProperties_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.IgnoreProperties<SampleModel>(x => x.Name));
        }

        [Fact]
        public void IgnoreProperties_TypeConfigured_AddsPropertyToIgnored()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.True(manager.PropertyIsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void IgnoreProperties_MultipleProperties_AddsAllToIgnored()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            manager.IgnoreProperties<SampleModel>(x => x.Name, x => x.Age);

            Assert.True(manager.PropertyIsIgnored<SampleModel>("Name"));
            Assert.True(manager.PropertyIsIgnored<SampleModel>("Age"));
        }

        [Fact]
        public void IgnoreProperties_ReturnsSameInstance()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            var result = manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.Same(manager, result);
        }

        // -----------------------------------------------------------------------------------------
        // IncludeProperties
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IncludeProperties_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.IncludeProperties<SampleModel>(x => x.Name));
        }

        [Fact]
        public void IncludeProperties_TypeConfigured_AddsPropertyToIncluded()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.True(manager.PropertyIsIncluded<SampleModel>("Name"));
        }

        [Fact]
        public void IncludeProperties_MultipleProperties_AddsAllToIncluded()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            manager.IncludeProperties<SampleModel>(x => x.Name, x => x.Age);

            Assert.True(manager.PropertyIsIncluded<SampleModel>("Name"));
            Assert.True(manager.PropertyIsIncluded<SampleModel>("Age"));
        }

        [Fact]
        public void IncludeProperties_ReturnsSameInstance()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            var result = manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.Same(manager, result);
        }

        // -----------------------------------------------------------------------------------------
        // PropertyIsIgnored
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void PropertyIsIgnored_Generic_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.PropertyIsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void PropertyIsIgnored_Generic_PropertyIgnored_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.True(manager.PropertyIsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void PropertyIsIgnored_Generic_PropertyNotIgnored_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.False(manager.PropertyIsIgnored<SampleModel>("Age"));
        }

        [Fact]
        public void PropertyIsIgnored_Type_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.PropertyIsIgnored(typeof(SampleModel), "Name"));
        }

        [Fact]
        public void PropertyIsIgnored_Type_PropertyIgnored_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.True(manager.PropertyIsIgnored(typeof(SampleModel), "Name"));
        }

        // -----------------------------------------------------------------------------------------
        // PropertyIsIncluded
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void PropertyIsIncluded_Generic_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.PropertyIsIncluded<SampleModel>("Name"));
        }

        [Fact]
        public void PropertyIsIncluded_Generic_PropertyIncluded_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.True(manager.PropertyIsIncluded<SampleModel>("Name"));
        }

        [Fact]
        public void PropertyIsIncluded_Generic_PropertyNotIncluded_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.False(manager.PropertyIsIncluded<SampleModel>("Age"));
        }

        [Fact]
        public void PropertyIsIncluded_Type_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.PropertyIsIncluded(typeof(SampleModel), "Name"));
        }

        [Fact]
        public void PropertyIsIncluded_Type_PropertyIncluded_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.True(manager.PropertyIsIncluded(typeof(SampleModel), "Name"));
        }

        // -----------------------------------------------------------------------------------------
        // IsUsingIgnore / IsUsingInclude (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IsUsingIgnore_TypeNotConfigured_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();

            Assert.False(manager.IsUsingIgnore<SampleModel>());
        }

        [Fact]
        public void IsUsingIgnore_TypeConfiguredWithIgnore_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.True(manager.IsUsingIgnore<SampleModel>());
        }

        [Fact]
        public void IsUsingIgnore_TypeConfiguredWithInclude_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.False(manager.IsUsingIgnore<SampleModel>());
        }

        [Fact]
        public void IsUsingInclude_TypeNotConfigured_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();

            Assert.False(manager.IsUsingInclude<SampleModel>());
        }

        [Fact]
        public void IsUsingInclude_TypeConfiguredWithInclude_ReturnsTrue()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IncludeProperties<SampleModel>(x => x.Name);

            Assert.True(manager.IsUsingInclude<SampleModel>());
        }

        [Fact]
        public void IsUsingInclude_TypeConfiguredWithIgnore_ReturnsFalse()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.IgnoreProperties<SampleModel>(x => x.Name);

            Assert.False(manager.IsUsingInclude<SampleModel>());
        }

        // -----------------------------------------------------------------------------------------
        // BlackList / WhiteList (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void BlackList_TypeNotConfigured_ThrowsInvalidOperationException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<InvalidOperationException>(() =>
                manager.BlackList(typeof(SampleModel), true));
        }

        [Fact]
        public void BlackList_TypeConfigured_SetsExclusionMode()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            manager.BlackList(typeof(SampleModel), true);

            Assert.True(manager.IsUsingIgnore<SampleModel>());
        }

        [Fact]
        public void WhiteList_TypeNotConfigured_ThrowsInvalidOperationException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<InvalidOperationException>(() =>
                manager.WhiteList(typeof(SampleModel), true));
        }

        [Fact]
        public void WhiteList_TypeConfigured_SetsInclusionMode()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            manager.WhiteList(typeof(SampleModel), true);

            Assert.True(manager.IsUsingInclude<SampleModel>());
        }

        [Fact]
        public void BlackList_AfterWhiteList_ThrowsInvalidOperationException()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.WhiteList(typeof(SampleModel), true);

            Assert.Throws<InvalidOperationException>(() =>
                manager.BlackList(typeof(SampleModel), true));
        }

        [Fact]
        public void WhiteList_AfterBlackList_ThrowsInvalidOperationException()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.BlackList(typeof(SampleModel), true);

            Assert.Throws<InvalidOperationException>(() =>
                manager.WhiteList(typeof(SampleModel), true));
        }

        // -----------------------------------------------------------------------------------------
        // ClearAll
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ClearAll_AfterConfigure_RemovesAllTypes()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.Configure<OtherModel>();

            manager.ClearAll();

            Assert.False(manager.IsTypeConfigured<SampleModel>());
            Assert.False(manager.IsTypeConfigured<OtherModel>());
        }

        [Fact]
        public void ClearAll_OnEmptyManager_DoesNotThrow()
        {
            var manager = new TypeConfigurationManager();

            var ex = Record.Exception(() => manager.ClearAll());

            Assert.Null(ex);
        }

        [Fact]
        public void ClearAll_AfterClearAll_CanReconfigure()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();
            manager.ClearAll();

            manager.Configure<SampleModel>();

            Assert.True(manager.IsTypeConfigured<SampleModel>());
        }

        // -----------------------------------------------------------------------------------------
        // Validate (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Validate_TypeNotConfigured_ThrowsArgumentException()
        {
            var manager = new TypeConfigurationManager();

            Assert.Throws<ArgumentException>(() =>
                manager.Validate(typeof(SampleModel)));
        }

        [Fact]
        public void Validate_TypeConfigured_DoesNotThrow()
        {
            var manager = new TypeConfigurationManager();
            manager.Configure<SampleModel>();

            var ex = Record.Exception(() => manager.Validate(typeof(SampleModel)));

            Assert.Null(ex);
        }
    }
}