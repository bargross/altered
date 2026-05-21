using Altered.Core.Attributes;
using Altered.Core.Configure;
using Altered.Core.Main;
using System.Linq.Expressions;
using System.Reflection;

namespace Altered.Tests.Main
{
    public class DiffGeneratorTests
    {
        private class SampleModel
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public string Email { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        private class ModelWithIgnoredAttribute
        {
            public string Name { get; set; } = string.Empty;
            [IgnoreInDiff]
            public string IgnoredProperty { get; set; } = string.Empty;
        }

        private class ModelWithReadOnlyProperty
        {
            public string Name { get; set; } = string.Empty;
            public string ReadOnly => "constant";
        }

        private class OtherModel
        {
            public string Title { get; set; } = string.Empty;
        }

        public DiffGeneratorTests()
        {
            DiffGenerator._typeConfiguratorManager = new();
            DiffGenerator._comparerManager = new();
        }

        // -----------------------------------------------------------------------------------------
        // Generate
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Generate_BothObjectsNull_ReturnsEmptyList()
        {
            var result = DiffGenerator.Generate<SampleModel>(null!, null!);

            Assert.Empty(result);
        }

        [Fact]
        public void Generate_OriginalNull_ModifiedNotNull_ThrowsArgumentNullException()
        {
            var modified = new SampleModel { Name = "Alice" };

            Assert.Throws<ArgumentNullException>(() =>
                DiffGenerator.Generate<SampleModel>(null!, modified));
        }

        [Fact]
        public void Generate_ModifiedNull_OriginalNotNull_ThrowsArgumentNullException()
        {
            var original = new SampleModel { Name = "Alice" };

            Assert.Throws<ArgumentNullException>(() =>
                DiffGenerator.Generate<SampleModel>(original, null!));
        }

        [Fact]
        public void Generate_IdenticalObjects_NoSelectors_ReturnsEmptyList()
        {
            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Alice", Age = 30 };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Empty(result);
        }

        [Fact]
        public void Generate_DifferentObjects_NoSelectors_ReturnsAllDiffs()
        {
            var original = new SampleModel { Name = "Alice", Age = 30, Email = "a@a.com" };
            var modified = new SampleModel { Name = "Bob", Age = 31, Email = "b@b.com" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void Generate_SinglePropertyChanged_NoSelectors_ReturnsSingleDiff()
        {
            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Bob", Age = 30 };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Single(result);
        }

        [Fact]
        public void Generate_SinglePropertyChanged_NoSelectors_DiffHasCorrectPropertyName()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Equal("Name", result[0].PropertyName);
        }

        [Fact]
        public void Generate_SinglePropertyChanged_NoSelectors_DiffHasCorrectOldValue()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Equal("Alice", result[0].OldValue);
        }

        [Fact]
        public void Generate_SinglePropertyChanged_NoSelectors_DiffHasCorrectNewValue()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Equal("Bob", result[0].NewValue);
        }

        [Fact]
        public void Generate_SelectorsProvided_IgnoreNull_ThrowsInvalidOperationException()
        {
            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Bob", Age = 31 };

            Assert.Throws<InvalidOperationException>(() =>
                DiffGenerator.Generate(original, modified, null, x => x.Name));
        }

        [Fact]
        public void Generate_WithIgnoreTrueAndSelector_SkipsIgnoredProperty()
        {
            DiffGenerator.Configure<SampleModel>();

            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Bob", Age = 31 };

            var result = DiffGenerator.Generate(original, modified, true, x => x.Name);

            Assert.DoesNotContain(result, d => d.PropertyName == "Name");
        }

        [Fact]
        public void Generate_WithIgnoreTrueAndSelector_StillReturnsNonIgnoredChangedProperties()
        {
            DiffGenerator.Configure<SampleModel>();

            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Bob", Age = 31 };

            var result = DiffGenerator.Generate(original, modified, true, x => x.Name);

            Assert.Contains(result, d => d.PropertyName == "Age");
        }

        [Fact]
        public void Generate_WithIgnoreFalseAndSelector_SkipsNonSelectedProperties()
        {
            DiffGenerator.Configure<SampleModel>();

            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Bob", Age = 31 };

            var result = DiffGenerator.Generate(original, modified, false, x => x.Name);

            Assert.DoesNotContain(result, d => d.PropertyName == "Age");
        }

        [Fact]
        public void Generate_WithIgnoreFalseAndSelector_OnlyReturnsSelectedChangedProperty()
        {
            DiffGenerator.Configure<SampleModel>();

            var original = new SampleModel { Name = "Alice", Age = 30 };
            var modified = new SampleModel { Name = "Bob", Age = 31 };

            var result = DiffGenerator.Generate(original, modified, false, x => x.Name);

            Assert.Single(result);
            Assert.Equal("Name", result[0].PropertyName);
        }

        [Fact]
        public void Generate_WithIgnoreTrueAndMultipleSelectors_SkipsAllIgnoredProperties()
        {
            DiffGenerator.Configure<SampleModel>();

            var original = new SampleModel { Name = "Alice", Age = 30, Email = "a@a.com" };
            var modified = new SampleModel { Name = "Bob", Age = 31, Email = "b@b.com" };

            var result = DiffGenerator.Generate(original, modified, true, x => x.Name, x => x.Age);

            Assert.DoesNotContain(result, d => d.PropertyName == "Name");
            Assert.DoesNotContain(result, d => d.PropertyName == "Age");
        }

        [Fact]
        public void Generate_WithIgnoreFalseAndMultipleSelectors_OnlyReturnsSelectedChangedProperties()
        {
            DiffGenerator.Configure<SampleModel>();

            var original = new SampleModel { Name = "Alice", Age = 30, Email = "a@a.com" };
            var modified = new SampleModel { Name = "Bob", Age = 31, Email = "b@b.com" };

            var result = DiffGenerator.Generate(original, modified, false, x => x.Name, x => x.Age);

            Assert.Contains(result, d => d.PropertyName == "Name");
            Assert.Contains(result, d => d.PropertyName == "Age");
            Assert.DoesNotContain(result, d => d.PropertyName == "Email");
        }

        [Fact]
        public void Generate_WithIgnoreAttributeOnProperty_NoSelectors_SkipsAttributedProperty()
        {
            var original = new ModelWithIgnoredAttribute { Name = "Alice", IgnoredProperty = "X" };
            var modified = new ModelWithIgnoredAttribute { Name = "Alice", IgnoredProperty = "Y" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.DoesNotContain(result, d => d.PropertyName == "IgnoredProperty");
        }

        [Fact]
        public void Generate_WithReadOnlyProperty_NoSelectors_SkipsReadOnlyProperty()
        {
            var original = new ModelWithReadOnlyProperty { Name = "Alice" };
            var modified = new ModelWithReadOnlyProperty { Name = "Alice" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.DoesNotContain(result, d => d.PropertyName == "ReadOnly");
        }

        [Fact]
        public void Generate_WithCustomComparer_ComparerReturnsTrue_TreatsValuesAsEqual()
        {
            DiffGenerator.RegisterComparer<string>((a, b) => true);

            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.DoesNotContain(result, d => d.PropertyName == "Name");
        }

        [Fact]
        public void Generate_WithCustomComparer_ComparerReturnsFalse_TreatsValuesAsDifferent()
        {
            DiffGenerator.RegisterComparer<string>((a, b) => false);

            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Alice" };

            var result = DiffGenerator.Generate(original, modified);

            Assert.Contains(result, d => d.PropertyName == "Name");
        }

        [Fact]
        public void Generate_SelectorsWithoutPriorConfigure_IgnoreNull_ThrowsInvalidOperationException()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            Assert.Throws<InvalidOperationException>(() =>
                DiffGenerator.Generate(original, modified, null, x => x.Name));
        }

        // -----------------------------------------------------------------------------------------
        // Configure (no action)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_Generic_RegistersTypeInManager()
        {
            DiffGenerator.Configure<SampleModel>();

            Assert.True(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void Configure_Generic_ResetsManagerBeforeConfiguring()
        {
            DiffGenerator.Configure<SampleModel>();
            DiffGenerator.Configure<SampleModel>();

            Assert.True(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void Configure_Generic_DoesNotThrow()
        {
            var ex = Record.Exception(() => DiffGenerator.Configure<SampleModel>());

            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // Configure (with action)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_WithAction_RegistersTypeInManager()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Ignore<SampleModel>(x => x.Name));

            Assert.True(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void Configure_WithAction_AppliesIgnoreConfiguration()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Ignore<SampleModel>(x => x.Name));

            Assert.True(DiffGenerator._typeConfiguratorManager.PropertyIsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void Configure_WithAction_ResetsManagerBeforeConfiguring()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Ignore<SampleModel>(x => x.Name));
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Ignore<SampleModel>(x => x.Age));

            Assert.True(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        // -----------------------------------------------------------------------------------------
        // Configure (with TypeConfigurator)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Configure_WithConfigurator_RegistersTypeInManager()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();

            DiffGenerator.Configure<SampleModel>(configurator);

            Assert.True(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void Configure_WithNullConfigurator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DiffGenerator.Configure<SampleModel>((TypeConfigurator)null!));
        }

        [Fact]
        public void Configure_WithConfigurator_AppliesIgnoredProperties()
        {
            var configurator = new TypeConfigurator();
            configurator.Configure<SampleModel>();
            configurator.Ignore<SampleModel>(x => x.Name);

            DiffGenerator.Configure<SampleModel>(configurator);

            Assert.True(DiffGenerator._typeConfiguratorManager.PropertyIsIgnored<SampleModel>("Name"));
        }

        // -----------------------------------------------------------------------------------------
        // RegisterComparer
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void RegisterComparer_ValidComparer_RegistersSuccessfully()
        {
            DiffGenerator.RegisterComparer<SampleModel>((a, b) => a.Name == b.Name);

            Assert.True(DiffGenerator._comparerManager.IsRegistered<SampleModel>());
        }

        [Fact]
        public void RegisterComparer_NullComparer_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DiffGenerator.RegisterComparer<SampleModel>(null!));
        }

        [Fact]
        public void RegisterComparer_DuplicateType_ThrowsArgumentException()
        {
            DiffGenerator.RegisterComparer<SampleModel>((a, b) => true);

            Assert.Throws<ArgumentException>(() =>
                DiffGenerator.RegisterComparer<SampleModel>((a, b) => false));
        }

        // -----------------------------------------------------------------------------------------
        // ClearAll
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ClearAll_AfterConfigure_RemovesAllConfigurations()
        {
            DiffGenerator.Configure<SampleModel>();

            DiffGenerator.ClearAll();

            Assert.False(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void ClearAll_CalledOnEmptyManager_DoesNotThrow()
        {
            var ex = Record.Exception(() => DiffGenerator.ClearAll());

            Assert.Null(ex);
        }

        [Fact]
        public void ClearAll_CalledTwice_DoesNotThrow()
        {
            DiffGenerator.Configure<SampleModel>();
            DiffGenerator.ClearAll();

            var ex = Record.Exception(() => DiffGenerator.ClearAll());

            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // GenerateDiffs (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void GenerateDiffs_BothNull_ReturnsEmptyList()
        {
            var result = DiffGenerator.GenerateDiffs<SampleModel>(
                null!, null!,
                Array.Empty<Expression<Func<SampleModel, object>>>());

            Assert.Empty(result);
        }

        [Fact]
        public void GenerateDiffs_OriginalNull_ThrowsArgumentNullException()
        {
            var modified = new SampleModel { Name = "Alice" };

            Assert.Throws<ArgumentNullException>(() =>
                DiffGenerator.GenerateDiffs<SampleModel>(
                    null!, modified,
                    Array.Empty<Expression<Func<SampleModel, object>>>()));
        }

        [Fact]
        public void GenerateDiffs_ModifiedNull_ThrowsArgumentNullException()
        {
            var original = new SampleModel { Name = "Alice" };

            Assert.Throws<ArgumentNullException>(() =>
                DiffGenerator.GenerateDiffs<SampleModel>(
                    original, null!,
                    Array.Empty<Expression<Func<SampleModel, object>>>()));
        }

        [Fact]
        public void GenerateDiffs_SelectorsProvidedWithIgnoreNull_ThrowsInvalidOperationException()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            Assert.Throws<InvalidOperationException>(() =>
                DiffGenerator.GenerateDiffs(
                    original, modified,
                    new Expression<Func<SampleModel, object>>[] { x => x.Name },
                    null));
        }

        [Fact]
        public void GenerateDiffs_NoSelectors_IgnoreNull_DoesNotThrow()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            var ex = Record.Exception(() =>
                DiffGenerator.GenerateDiffs(
                    original, modified,
                    Array.Empty<Expression<Func<SampleModel, object>>>(),
                    null));

            Assert.Null(ex);
        }

        [Fact]
        public void GenerateDiffs_WithIgnoreTrue_TypeNotConfigured_ThrowsInvalidOperationException()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            Assert.Throws<InvalidOperationException>(() =>
                DiffGenerator.GenerateDiffs(
                    original, modified,
                    new Expression<Func<SampleModel, object>>[] { x => x.Name },
                    true));
        }

        [Fact]
        public void GenerateDiffs_WithIgnoreFalse_TypeNotConfigured_ThrowsInvalidOperationException()
        {
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            Assert.Throws<InvalidOperationException>(() =>
                DiffGenerator.GenerateDiffs(
                    original, modified,
                    new Expression<Func<SampleModel, object>>[] { x => x.Name },
                    false));
        }

        // -----------------------------------------------------------------------------------------
        // ApplyPropertySelectors (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ApplyPropertySelectors_NoSelectors_DoesNotConfigureType()
        {
            DiffGenerator.ApplyPropertySelectors<SampleModel>(
                Array.Empty<Expression<Func<SampleModel, object>>>());

            Assert.False(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void ApplyPropertySelectors_SelectorsProvided_TypeNotConfigured_ConfiguresType()
        {
            DiffGenerator.ApplyPropertySelectors<SampleModel>(
                new Expression<Func<SampleModel, object>>[] { x => x.Name });

            Assert.True(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void ApplyPropertySelectors_TypeAlreadyConfigured_DoesNotThrow()
        {
            DiffGenerator.Configure<SampleModel>();

            var ex = Record.Exception(() =>
                DiffGenerator.ApplyPropertySelectors<SampleModel>(
                    new Expression<Func<SampleModel, object>>[] { x => x.Name }));

            Assert.Null(ex);
        }

        [Fact]
        public void ApplyPropertySelectors_UsingIgnore_AddsToIgnoredProperties()
        {
            DiffGenerator.Configure<SampleModel>();
            DiffGenerator._typeConfiguratorManager.BlackList(typeof(SampleModel), true);

            DiffGenerator.ApplyPropertySelectors<SampleModel>(
                new Expression<Func<SampleModel, object>>[] { x => x.Name });

            Assert.True(DiffGenerator._typeConfiguratorManager.PropertyIsIgnored<SampleModel>("Name"));
        }

        [Fact]
        public void ApplyPropertySelectors_UsingInclude_AddsToIncludedProperties()
        {
            DiffGenerator.Configure<SampleModel>();
            DiffGenerator._typeConfiguratorManager.WhiteList(typeof(SampleModel), true);

            DiffGenerator.ApplyPropertySelectors<SampleModel>(
                new Expression<Func<SampleModel, object>>[] { x => x.Name });

            Assert.True(DiffGenerator._typeConfiguratorManager.PropertyIsIncluded<SampleModel>("Name"));
        }

        // -----------------------------------------------------------------------------------------
        // ShouldSkipProperty (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ShouldSkipProperty_PropertyWithIgnoreAttribute_ReturnsTrue()
        {
            var prop = typeof(ModelWithIgnoredAttribute).GetProperty("IgnoredProperty")!;

            var result = DiffGenerator.ShouldSkipProperty<ModelWithIgnoredAttribute>(prop);

            Assert.True(result);
        }

        [Fact]
        public void ShouldSkipProperty_NormalProperty_TypeNotConfigured_ReturnsFalse()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;

            var result = DiffGenerator.ShouldSkipProperty<SampleModel>(prop);

            Assert.False(result);
        }

        [Fact]
        public void ShouldSkipProperty_IgnoredPropertyInManager_ReturnsTrue()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Ignore<SampleModel>(x => x.Name));
            var prop = typeof(SampleModel).GetProperty("Name")!;

            var result = DiffGenerator.ShouldSkipProperty<SampleModel>(prop);

            Assert.True(result);
        }

        [Fact]
        public void ShouldSkipProperty_NonIgnoredPropertyInManager_ReturnsFalse()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Ignore<SampleModel>(x => x.Name));
            var prop = typeof(SampleModel).GetProperty("Age")!;

            var result = DiffGenerator.ShouldSkipProperty<SampleModel>(prop);

            Assert.False(result);
        }

        [Fact]
        public void ShouldSkipProperty_IncludedPropertyInManager_ReturnsFalse()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Include<SampleModel>(x => x.Name));

            var prop = typeof(SampleModel).GetProperty("Name")!;

            var result = DiffGenerator.ShouldSkipProperty<SampleModel>(prop);

            Assert.False(result);
        }

        [Fact]
        public void ShouldSkipProperty_NotIncludedPropertyInManager_ReturnsTrue()
        {
            DiffGenerator.Configure<SampleModel>(cfg => cfg.Include<SampleModel>(x => x.Name));

            var prop = typeof(SampleModel).GetProperty("Age")!;

            var result = DiffGenerator.ShouldSkipProperty<SampleModel>(prop);

            Assert.True(result);
        }

        // -----------------------------------------------------------------------------------------
        // TryBuildDiffEntry (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void TryBuildDiffEntry_ValuesAreEqual_ReturnsFalse()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Alice" };

            var result = DiffGenerator.TryBuildDiffEntry(prop, original, modified, out _);

            Assert.False(result);
        }

        [Fact]
        public void TryBuildDiffEntry_ValuesAreEqual_OutEntryIsNull()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Alice" };

            DiffGenerator.TryBuildDiffEntry(prop, original, modified, out var entry);

            Assert.Null(entry);
        }

        [Fact]
        public void TryBuildDiffEntry_ValuesAreDifferent_ReturnsTrue()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            var result = DiffGenerator.TryBuildDiffEntry(prop, original, modified, out _);

            Assert.True(result);
        }

        [Fact]
        public void TryBuildDiffEntry_ValuesAreDifferent_EntryHasCorrectPropertyName()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            DiffGenerator.TryBuildDiffEntry(prop, original, modified, out var entry);

            Assert.Equal("Name", entry!.PropertyName);
        }

        [Fact]
        public void TryBuildDiffEntry_ValuesAreDifferent_EntryHasCorrectOldValue()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            DiffGenerator.TryBuildDiffEntry(prop, original, modified, out var entry);

            Assert.Equal("Alice", entry!.OldValue);
        }

        [Fact]
        public void TryBuildDiffEntry_ValuesAreDifferent_EntryHasCorrectNewValue()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = "Alice" };
            var modified = new SampleModel { Name = "Bob" };

            DiffGenerator.TryBuildDiffEntry(prop, original, modified, out var entry);

            Assert.Equal("Bob", entry!.NewValue);
        }

        [Fact]
        public void TryBuildDiffEntry_OldValueNullNewValueSet_ReturnsTrue()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = null! };
            var modified = new SampleModel { Name = "Bob" };

            var result = DiffGenerator.TryBuildDiffEntry(prop, original, modified, out _);

            Assert.True(result);
        }

        [Fact]
        public void TryBuildDiffEntry_BothValuesNull_ReturnsFalse()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;
            var original = new SampleModel { Name = null! };
            var modified = new SampleModel { Name = null! };

            var result = DiffGenerator.TryBuildDiffEntry(prop, original, modified, out _);

            Assert.False(result);
        }

        // -----------------------------------------------------------------------------------------
        // AreEqual (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void AreEqual_BothNull_ReturnsTrue()
        {
            var result = DiffGenerator.AreEqual(null, null);

            Assert.True(result);
        }

        [Fact]
        public void AreEqual_OriginalNullModifiedSet_ReturnsFalse()
        {
            var result = DiffGenerator.AreEqual(null, "value");

            Assert.False(result);
        }

        [Fact]
        public void AreEqual_OriginalSetModifiedNull_ReturnsFalse()
        {
            var result = DiffGenerator.AreEqual("value", null);

            Assert.False(result);
        }

        [Fact]
        public void AreEqual_EqualStringValues_ReturnsTrue()
        {
            var result = DiffGenerator.AreEqual("Alice", "Alice");

            Assert.True(result);
        }

        [Fact]
        public void AreEqual_DifferentStringValues_ReturnsFalse()
        {
            var result = DiffGenerator.AreEqual("Alice", "Bob");

            Assert.False(result);
        }

        [Fact]
        public void AreEqual_EqualValueTypes_ReturnsTrue()
        {
            var result = DiffGenerator.AreEqual(42, 42);

            Assert.True(result);
        }

        [Fact]
        public void AreEqual_DifferentValueTypes_ReturnsFalse()
        {
            var result = DiffGenerator.AreEqual(42, 43);

            Assert.False(result);
        }

        [Fact]
        public void AreEqual_WithRegisteredComparerReturningTrue_ReturnsTrue()
        {
            DiffGenerator.RegisterComparer<string>((a, b) => true);

            var result = DiffGenerator.AreEqual("Alice", "Bob");

            Assert.True(result);
        }

        [Fact]
        public void AreEqual_WithRegisteredComparerReturningFalse_ReturnsFalse()
        {
            DiffGenerator.RegisterComparer<string>((a, b) => false);

            var result = DiffGenerator.AreEqual("Alice", "Alice");

            Assert.False(result);
        }

        // -----------------------------------------------------------------------------------------
        // InvokeCustomComparer (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void InvokeCustomComparer_RegisteredComparerReturnsTrue_ReturnsTrue()
        {
            DiffGenerator.RegisterComparer<string>((a, b) => true);

            var result = DiffGenerator.InvokeCustomComparer(typeof(string), "Alice", "Bob");

            Assert.True(result);
        }

        [Fact]
        public void InvokeCustomComparer_RegisteredComparerReturnsFalse_ReturnsFalse()
        {
            DiffGenerator.RegisterComparer<string>((a, b) => false);

            var result = DiffGenerator.InvokeCustomComparer(typeof(string), "Alice", "Alice");

            Assert.False(result);
        }

        [Fact]
        public void InvokeCustomComparer_UnregisteredType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                DiffGenerator.InvokeCustomComparer(typeof(string), "Alice", "Bob"));
        }

        // -----------------------------------------------------------------------------------------
        // ClearAllConfigurations (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ClearAllConfigurations_AfterConfigure_RemovesAllTypes()
        {
            DiffGenerator.Configure<SampleModel>();

            DiffGenerator.ClearAllConfigurations();

            Assert.False(DiffGenerator._typeConfiguratorManager.IsTypeConfigured<SampleModel>());
        }

        [Fact]
        public void ClearAllConfigurations_CalledOnEmptyManager_DoesNotThrow()
        {
            var ex = Record.Exception(() => DiffGenerator.ClearAllConfigurations());

            Assert.Null(ex);
        }
    }
}