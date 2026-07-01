using Altered.Configure;

namespace Altered.Tests.Configure
{
    public class TypeConfiguratorTests
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
        public void Configure_Generic_SetsType()
        {
            var configurator = new TypeConfigurator();

            configurator.Configure<SampleModel>();

            Assert.Equal(typeof(SampleModel), configurator.Type);
        }

        [Fact]
        public void Configure_Generic_CalledTwiceWithSameType_DoesNotThrow()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            var ex = Record.Exception(() => configurator.Configure<SampleModel>());

            Assert.Null(ex);
        }

        [Fact]
        public void Configure_Generic_CalledWithDifferentType_ThrowsInvalidCastException()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            Assert.Throws<InvalidCastException>(() => configurator.Configure<OtherModel>());
        }

        [Fact]
        public void Configure_Generic_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.Configure<SampleModel>();

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // Configure(Type)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_Type_SetsType()
        {
            var configurator = new TypeConfigurator();

            configurator.Configure(typeof(SampleModel));

            Assert.Equal(typeof(SampleModel), configurator.Type);
        }

        [Fact]
        public void Configure_Type_CalledWithDifferentType_ThrowsInvalidCastException()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure(typeof(SampleModel));

            Assert.Throws<InvalidCastException>(() => configurator.Configure(typeof(OtherModel)));
        }

        [Fact]
        public void Configure_Type_CalledTwiceWithSameType_DoesNotThrow()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure(typeof(SampleModel));

            var ex = Record.Exception(() => configurator.Configure(typeof(SampleModel)));

            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // Type property
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Type_BeforeConfigure_ThrowsArgumentNullException()
        {
            var configurator = new TypeConfigurator();

            Assert.Throws<ArgumentNullException>(() => configurator.Type);
        }

        [Fact]
        public void Type_AfterConfigure_ReturnsConfiguredType()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            Assert.Equal(typeof(SampleModel), configurator.Type);
        }

        // -----------------------------------------------------------------------------------------
        // Ignore
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Ignore_SingleProperty_AddsToIgnoredProperties()
        {
            var configurator = new TypeConfigurator();

            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.Contains("Name", configurator.IgnoredProperties);
        }

        [Fact]
        public void Ignore_SetsExclusionMode()
        {
            var configurator = new TypeConfigurator();

            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.True(configurator._isExclusion);
        }

        [Fact]
        public void Ignore_AfterInclude_ThrowsInvalidOperationException()
        {
            var configurator = new TypeConfigurator();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.Throws<InvalidOperationException>(() =>
                configurator.Ignore<SampleModel>(x => x.Age));
        }

        [Fact]
        public void Ignore_DifferentTypeThanConfigured_ThrowsArgumentException()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            Assert.Throws<ArgumentException>(() =>
                configurator.Ignore<OtherModel>(x => x.Title));
        }

        [Fact]
        public void Ignore_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.Ignore<SampleModel>(x => x.Name);

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // IgnoreMany
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IgnoreMany_MultipleProperties_AddsAllToIgnoredProperties()
        {
            var configurator = new TypeConfigurator();

            configurator.IgnoreMany<SampleModel>(x => x.Name, x => x.Age, x => x.Email);

            Assert.Contains("Name", configurator.IgnoredProperties);
            Assert.Contains("Age", configurator.IgnoredProperties);
            Assert.Contains("Email", configurator.IgnoredProperties);
        }

        [Fact]
        public void IgnoreMany_CalledTwice_AccumulatesAllProperties()
        {
            var configurator = new TypeConfigurator();

            configurator.IgnoreMany<SampleModel>(x => x.Name);
            configurator.IgnoreMany<SampleModel>(x => x.Age);

            Assert.Contains("Name", configurator.IgnoredProperties);
            Assert.Contains("Age", configurator.IgnoredProperties);
        }

        [Fact]
        public void IgnoreMany_AfterInclude_ThrowsInvalidOperationException()
        {
            var configurator = new TypeConfigurator();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.Throws<InvalidOperationException>(() =>
                configurator.IgnoreMany<SampleModel>(x => x.Age, x => x.Email));
        }

        [Fact]
        public void IgnoreMany_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.IgnoreMany<SampleModel>(x => x.Name, x => x.Age);

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // Include
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Include_SingleProperty_AddsToIncludedProperties()
        {
            var configurator = new TypeConfigurator();

            configurator.Include<SampleModel>(x => x.Name);

            Assert.Contains("Name", configurator.IncludedProperties);
        }

        [Fact]
        public void Include_SetsInclusionMode()
        {
            var configurator = new TypeConfigurator();

            configurator.Include<SampleModel>(x => x.Name);

            Assert.True(configurator._isInclusion);
        }

        [Fact]
        public void Include_AfterIgnore_ThrowsInvalidOperationException()
        {
            var configurator = new TypeConfigurator();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.Throws<InvalidOperationException>(() =>
                configurator.Include<SampleModel>(x => x.Age));
        }

        [Fact]
        public void Include_DifferentTypeThanConfigured_ThrowsArgumentException()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            Assert.Throws<ArgumentException>(() =>
                configurator.Include<OtherModel>(x => x.Title));
        }

        [Fact]
        public void Include_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.Include<SampleModel>(x => x.Name);

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // IncludeMany
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IncludeMany_MultipleProperties_AddsAllToIncludedProperties()
        {
            var configurator = new TypeConfigurator();

            configurator.IncludeMany<SampleModel>(x => x.Name, x => x.Age, x => x.Email);

            Assert.Contains("Name", configurator.IncludedProperties);
            Assert.Contains("Age", configurator.IncludedProperties);
            Assert.Contains("Email", configurator.IncludedProperties);
        }

        [Fact]
        public void IncludeMany_CalledTwice_AccumulatesAllProperties()
        {
            var configurator = new TypeConfigurator();

            configurator.IncludeMany<SampleModel>(x => x.Name);
            configurator.IncludeMany<SampleModel>(x => x.Age);

            Assert.Contains("Name", configurator.IncludedProperties);
            Assert.Contains("Age", configurator.IncludedProperties);
        }

        [Fact]
        public void IncludeMany_AfterIgnore_ThrowsInvalidOperationException()
        {
            var configurator = new TypeConfigurator();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.Throws<InvalidOperationException>(() =>
                configurator.IncludeMany<SampleModel>(x => x.Age, x => x.Email));
        }

        [Fact]
        public void IncludeMany_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.IncludeMany<SampleModel>(x => x.Name, x => x.Age);

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // IsIgnored
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IsIgnored_Generic_IgnoredProperty_ReturnsTrue()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.True(configurator.IsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void IsIgnored_Generic_NotIgnoredProperty_ReturnsFalse()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.False(configurator.IsIgnored<SampleModel>("Age"));
        }

        [Fact]
        public void IsIgnored_Generic_DifferentType_ReturnsFalse()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.False(configurator.IsIgnored<OtherModel>("Name"));
        }

        [Fact]
        public void IsIgnored_Type_IgnoredProperty_ReturnsTrue()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.True(configurator.IsIgnored(typeof(SampleModel), "Name"));
        }

        [Fact]
        public void IsIgnored_Type_DifferentType_ReturnsFalse()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            Assert.False(configurator.IsIgnored(typeof(OtherModel), "Name"));
        }

        // -----------------------------------------------------------------------------------------
        // IsIncluded
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IsIncluded_Generic_IncludedProperty_ReturnsTrue()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.True(configurator.IsIncluded<SampleModel>("Name"));
        }

        [Fact]
        public void IsIncluded_Generic_NotIncludedProperty_ReturnsFalse()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.False(configurator.IsIncluded<SampleModel>("Age"));
        }

        [Fact]
        public void IsIncluded_Generic_DifferentType_ReturnsFalse()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.False(configurator.IsIncluded<OtherModel>("Name"));
        }

        [Fact]
        public void IsIncluded_Type_IncludedProperty_ReturnsTrue()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.True(configurator.IsIncluded(typeof(SampleModel), "Name"));
        }

        [Fact]
        public void IsIncluded_Type_DifferentType_ReturnsFalse()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Include<SampleModel>(x => x.Name);

            Assert.False(configurator.IsIncluded(typeof(OtherModel), "Name"));
        }

        // -----------------------------------------------------------------------------------------
        // BlackList
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void BlackList_SetTrue_SetsExclusionMode()
        {
            var configurator = new TypeConfigurator();

            configurator.BlackList(true);

            Assert.True(configurator._isExclusion);
        }

        [Fact]
        public void BlackList_SetTrue_DoesNotSetInclusionMode()
        {
            var configurator = new TypeConfigurator();

            configurator.BlackList(true);

            Assert.False(configurator._isInclusion);
        }

        [Fact]
        public void BlackList_CalledTwiceWithTrue_DoesNotThrow()
        {
            var configurator = new TypeConfigurator();
            configurator.BlackList(true);

            var ex = Record.Exception(() => configurator.BlackList(true));

            Assert.Null(ex);
        }

        [Fact]
        public void BlackList_WhenAlreadyWhiteListed_ThrowsInvalidOperationException()
        {
            var configurator = new TypeConfigurator();
            configurator.WhiteList(true);

            Assert.Throws<InvalidOperationException>(() => configurator.BlackList(true));
        }

        [Fact]
        public void BlackList_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.BlackList(true);

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // WhiteList
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void WhiteList_SetTrue_SetsInclusionMode()
        {
            var configurator = new TypeConfigurator();

            configurator.WhiteList(true);

            Assert.True(configurator._isInclusion);
        }

        [Fact]
        public void WhiteList_SetTrue_DoesNotSetExclusionMode()
        {
            var configurator = new TypeConfigurator();

            configurator.WhiteList(true);

            Assert.False(configurator._isExclusion);
        }

        [Fact]
        public void WhiteList_CalledTwiceWithTrue_DoesNotThrow()
        {
            var configurator = new TypeConfigurator();
            configurator.WhiteList(true);

            var ex = Record.Exception(() => configurator.WhiteList(true));

            Assert.Null(ex);
        }

        [Fact]
        public void WhiteList_WhenAlreadyBlackListed_ThrowsInvalidOperationException()
        {
            var configurator = new TypeConfigurator();
            configurator.BlackList(true);

            Assert.Throws<InvalidOperationException>(() => configurator.WhiteList(true));
        }

        [Fact]
        public void WhiteList_ReturnsSameInstance()
        {
            var configurator = new TypeConfigurator();

            var result = configurator.WhiteList(true);

            Assert.Same(configurator, result);
        }

        // -----------------------------------------------------------------------------------------
        // Clear
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Clear_AfterIgnore_RemovesIgnoredProperties()
        {
            var configurator = new TypeConfigurator();
            configurator.Ignore<SampleModel>(x => x.Name);
            configurator.Ignore<SampleModel>(x => x.Age);

            configurator.Clear();

            Assert.Empty(configurator.IgnoredProperties);
        }

        [Fact]
        public void Clear_OnEmptyConfigurator_DoesNotThrow()
        {
            var configurator = new TypeConfigurator();

            var ex = Record.Exception(() => configurator.Clear());

            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // Fluent chaining
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void FluentChain_Configure_IgnoreMany_AddsAllProperties()
        {
            var configurator = new TypeConfigurator();

            configurator
                .Configure<SampleModel>()
                .IgnoreMany<SampleModel>(x => x.Name, x => x.Age);

            Assert.Contains("Name", configurator.IgnoredProperties);
            Assert.Contains("Age", configurator.IgnoredProperties);
        }

        [Fact]
        public void FluentChain_Configure_IncludeMany_AddsAllProperties()
        {
            var configurator = new TypeConfigurator();

            configurator
                .Configure<SampleModel>()
                .IncludeMany<SampleModel>(x => x.Name, x => x.Age);

            Assert.Contains("Name", configurator.IncludedProperties);
            Assert.Contains("Age", configurator.IncludedProperties);
        }

        [Fact]
        public void FluentChain_Ignore_Ignore_AccumulatesProperties()
        {
            var configurator = new TypeConfigurator();

            configurator
                .Ignore<SampleModel>(x => x.Name)
                .Ignore<SampleModel>(x => x.Age);

            Assert.Contains("Name", configurator.IgnoredProperties);
            Assert.Contains("Age", configurator.IgnoredProperties);
        }

        [Fact]
        public void FluentChain_Include_Include_AccumulatesProperties()
        {
            var configurator = new TypeConfigurator();

            configurator
                .Include<SampleModel>(x => x.Name)
                .Include<SampleModel>(x => x.Age);

            Assert.Contains("Name", configurator.IncludedProperties);
            Assert.Contains("Age", configurator.IncludedProperties);
        }
    }
}