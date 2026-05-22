using Altered.Core.Main;
using System.Reflection;

namespace Altered.Tests.Main
{
    public class DiffApplierTests
    {
        private class SampleModel
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public string? NullableString { get; set; }
            public string ReadOnly => "constant";
            private string Private { get; set; } = string.Empty;
        }

        private class OtherModel
        {
            public string Title { get; set; } = string.Empty;
        }

        // -----------------------------------------------------------------------------------------
        // Apply
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Apply_TargetNull_ThrowsArgumentNullException()
        {
            var diffs = new List<DiffEntry> { new("Name", "Alice", "Bob") };

            Assert.Throws<ArgumentNullException>(() =>
                DiffApplier.Apply<SampleModel>(null!, diffs));
        }

        [Fact]
        public void Apply_DiffsNull_ThrowsArgumentNullException()
        {
            var target = new SampleModel();

            Assert.Throws<ArgumentNullException>(() =>
                DiffApplier.Apply(target, null!));
        }

        [Fact]
        public void Apply_ValidDiff_AppliesPropertyChange()
        {
            var target = new SampleModel { Name = "Alice" };
            var diffs = new List<DiffEntry> { new("Name", "Alice", "Bob") };

            DiffApplier.Apply(target, diffs);

            Assert.Equal("Bob", target.Name);
        }

        [Fact]
        public void Apply_MultipleDiffs_AppliesAllChanges()
        {
            var target = new SampleModel { Name = "Alice", Age = 25 };
            var diffs = new List<DiffEntry>
            {
                new("Name", "Alice", "Bob"),
                new("Age", 25, 30)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal("Bob", target.Name);
            Assert.Equal(30, target.Age);
        }

        [Fact]
        public void Apply_EmptyDiffs_DoesNotModifyTarget()
        {
            var target = new SampleModel { Name = "Alice", Age = 25 };
            var diffs = new List<DiffEntry>();

            DiffApplier.Apply(target, diffs);

            Assert.Equal("Alice", target.Name);
            Assert.Equal(25, target.Age);
        }

        [Fact]
        public void Apply_DiffWithNullEntry_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { null! };

            Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
        }

        [Fact]
        public void Apply_DiffWithNullPropertyName_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new(null!, "Alice", "Bob") };

            Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
        }

        [Fact]
        public void Apply_DiffWithEmptyPropertyName_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new(string.Empty, "Alice", "Bob") };

            Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
        }

        [Fact]
        public void Apply_DiffWithWhitespacePropertyName_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new("   ", "Alice", "Bob") };

            Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
        }

        [Fact]
        public void Apply_DiffWithNonExistentProperty_DoesNotThrow()
        {
            var target = new SampleModel { Name = "Alice" };
            var diffs = new List<DiffEntry> { new("NonExistent", "old", "new") };

            var ex = Record.Exception(() => DiffApplier.Apply(target, diffs));

            Assert.Null(ex);
        }

        [Fact]
        public void Apply_DiffWithNonExistentProperty_DoesNotModifyTarget()
        {
            var target = new SampleModel { Name = "Alice" };
            var diffs = new List<DiffEntry> { new("NonExistent", "old", "new") };

            DiffApplier.Apply(target, diffs);

            Assert.Equal("Alice", target.Name);
        }

        [Fact]
        public void Apply_DiffWithReadOnlyProperty_DoesNotThrow()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new("ReadOnly", "old", "new") };

            var ex = Record.Exception(() => DiffApplier.Apply(target, diffs));

            Assert.Null(ex);
        }

        [Fact]
        public void Apply_DiffWithIncompatibleType_DoesNotModifyTarget()
        {
            var target = new SampleModel { Name = "Alice" };
            var diffs = new List<DiffEntry> { new("Name", "Alice", 12345) };

            DiffApplier.Apply(target, diffs);

            Assert.Equal("Alice", target.Name);
        }

        [Fact]
        public void Apply_DiffWithNullNewValue_SetsPropertyToNull()
        {
            var target = new SampleModel { NullableString = "Alice" };
            var diffs = new List<DiffEntry> { new("NullableString", "Alice", null) };

            DiffApplier.Apply(target, diffs);

            Assert.Null(target.NullableString);
        }

        // -----------------------------------------------------------------------------------------
        // ApplyDiffs (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ApplyDiffs_TargetNull_ThrowsArgumentNullException()
        {
            var diffs = new List<DiffEntry> { new("Name", "Alice", "Bob") };

            Assert.Throws<ArgumentNullException>(() =>
                DiffApplier.ApplyDiffs<SampleModel>(null!, diffs));
        }

        [Fact]
        public void ApplyDiffs_ValidArguments_AppliesAllDiffs()
        {
            var target = new SampleModel { Name = "Alice", Age = 25 };
            var diffs = new List<DiffEntry>
            {
                new("Name", "Alice", "Bob"),
                new("Age", 25, 30)
            };

            DiffApplier.ApplyDiffs(target, diffs);

            Assert.Equal("Bob", target.Name);
            Assert.Equal(30, target.Age);
        }

        // -----------------------------------------------------------------------------------------
        // ValidateArguments (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ValidateArguments_TargetNull_ThrowsArgumentNullException()
        {
            var diffs = new List<DiffEntry>();

            Assert.Throws<ArgumentNullException>(() =>
                DiffApplier.ValidateArguments<SampleModel>(null!, diffs));
        }

        [Fact]
        public void ValidateArguments_DiffsNull_ThrowsArgumentNullException()
        {
            var target = new SampleModel();

            Assert.Throws<ArgumentNullException>(() =>
                DiffApplier.ValidateArguments(target, null!));
        }

        [Fact]
        public void ValidateArguments_DiffsContainsNullEntry_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { null! };

            Assert.Throws<ArgumentException>(() =>
                DiffApplier.ValidateArguments(target, diffs));
        }

        [Fact]
        public void ValidateArguments_DiffsContainsNullPropertyName_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new(null!, "old", "new") };

            Assert.Throws<ArgumentException>(() =>
                DiffApplier.ValidateArguments(target, diffs));
        }

        [Fact]
        public void ValidateArguments_DiffsContainsEmptyPropertyName_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new(string.Empty, "old", "new") };

            Assert.Throws<ArgumentException>(() =>
                DiffApplier.ValidateArguments(target, diffs));
        }

        [Fact]
        public void ValidateArguments_DiffsContainsWhitespacePropertyName_ThrowsArgumentException()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new("   ", "old", "new") };

            Assert.Throws<ArgumentException>(() =>
                DiffApplier.ValidateArguments(target, diffs));
        }

        [Fact]
        public void ValidateArguments_ValidArguments_DoesNotThrow()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry> { new("Name", "Alice", "Bob") };

            var ex = Record.Exception(() =>
                DiffApplier.ValidateArguments(target, diffs));

            Assert.Null(ex);
        }

        [Fact]
        public void ValidateArguments_EmptyDiffs_DoesNotThrow()
        {
            var target = new SampleModel();
            var diffs = new List<DiffEntry>();

            var ex = Record.Exception(() =>
                DiffApplier.ValidateArguments(target, diffs));

            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // GetWritableProperties (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void GetWritableProperties_ReturnsOnlyPublicInstanceProperties()
        {
            var result = DiffApplier.GetWritableProperties<SampleModel>();

            Assert.True(result.ContainsKey("Name"));
            Assert.True(result.ContainsKey("Age"));
        }

        [Fact]
        public void GetWritableProperties_DoesNotIncludePrivateProperties()
        {
            var result = DiffApplier.GetWritableProperties<SampleModel>();

            Assert.False(result.ContainsKey("Private"));
        }

        [Fact]
        public void GetWritableProperties_IncludesReadOnlyProperties()
        {
            var result = DiffApplier.GetWritableProperties<SampleModel>();

            Assert.True(result.ContainsKey("ReadOnly"));
        }

        [Fact]
        public void GetWritableProperties_ReturnsCorrectPropertyInfoType()
        {
            var result = DiffApplier.GetWritableProperties<SampleModel>();

            Assert.IsAssignableFrom<PropertyInfo>(result["Name"]);
        }

        [Fact]
        public void GetWritableProperties_EmptyClass_ReturnsEmptyDictionary()
        {
            var result = DiffApplier.GetWritableProperties<EmptyModel>();

            Assert.Empty(result);
        }

        private class EmptyModel { }

        // -----------------------------------------------------------------------------------------
        // TryApplyDiff (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void TryApplyDiff_PropertyExists_AppliesValue()
        {
            var target = new SampleModel { Name = "Alice" };
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("Name", "Alice", "Bob");

            DiffApplier.TryApplyDiff(properties, diff, target);

            Assert.Equal("Bob", target.Name);
        }

        [Fact]
        public void TryApplyDiff_PropertyDoesNotExist_DoesNotThrow()
        {
            var target = new SampleModel { Name = "Alice" };
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("NonExistent", "old", "new");

            var ex = Record.Exception(() =>
                DiffApplier.TryApplyDiff(properties, diff, target));

            Assert.Null(ex);
        }

        [Fact]
        public void TryApplyDiff_PropertyDoesNotExist_DoesNotModifyTarget()
        {
            var target = new SampleModel { Name = "Alice" };
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("NonExistent", "old", "new");

            DiffApplier.TryApplyDiff(properties, diff, target);

            Assert.Equal("Alice", target.Name);
        }

        [Fact]
        public void TryApplyDiff_ReadOnlyProperty_DoesNotThrow()
        {
            var target = new SampleModel();
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("ReadOnly", "old", "new");

            var ex = Record.Exception(() =>
                DiffApplier.TryApplyDiff(properties, diff, target));

            Assert.Null(ex);
        }

        [Fact]
        public void TryApplyDiff_IncompatibleType_DoesNotModifyTarget()
        {
            var target = new SampleModel { Name = "Alice" };
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("Name", "Alice", 99999);

            DiffApplier.TryApplyDiff(properties, diff, target);

            Assert.Equal("Alice", target.Name);
        }

        [Fact]
        public void TryApplyDiff_NullNewValue_SetsPropertyToNull()
        {
            var target = new SampleModel { NullableString = "Alice" };
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("NullableString", "Alice", null);

            DiffApplier.TryApplyDiff(properties, diff, target);

            Assert.Null(target.NullableString);
        }

        [Fact]
        public void TryApplyDiff_ValueTypeProperty_AppliesCorrectly()
        {
            var target = new SampleModel { Age = 25 };
            var properties = DiffApplier.GetWritableProperties<SampleModel>();
            var diff = new DiffEntry("Age", 25, 30);

            DiffApplier.TryApplyDiff(properties, diff, target);

            Assert.Equal(30, target.Age);
        }

        // -----------------------------------------------------------------------------------------
        // IsTypeCompatible (internal)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void IsTypeCompatible_NullNewValue_ReturnsTrue()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;

            var result = DiffApplier.IsTypeCompatible(prop, null);

            Assert.True(result);
        }

        [Fact]
        public void IsTypeCompatible_MatchingType_ReturnsTrue()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;

            var result = DiffApplier.IsTypeCompatible(prop, "Bob");

            Assert.True(result);
        }

        [Fact]
        public void IsTypeCompatible_IncompatibleType_ReturnsFalse()
        {
            var prop = typeof(SampleModel).GetProperty("Name")!;

            var result = DiffApplier.IsTypeCompatible(prop, 12345);

            Assert.False(result);
        }

        [Fact]
        public void IsTypeCompatible_SubclassOfPropertyType_ReturnsTrue()
        {
            var prop = typeof(ModelWithBaseProperty).GetProperty("Value")!;

            var result = DiffApplier.IsTypeCompatible(prop, new DerivedClass());

            Assert.True(result);
        }

        [Fact]
        public void IsTypeCompatible_ValueTypeMatchingExactType_ReturnsTrue()
        {
            var prop = typeof(SampleModel).GetProperty("Age")!;

            var result = DiffApplier.IsTypeCompatible(prop, 42);

            Assert.True(result);
        }

        [Fact]
        public void IsTypeCompatible_ValueTypeMismatch_ReturnsFalse()
        {
            var prop = typeof(SampleModel).GetProperty("Age")!;

            var result = DiffApplier.IsTypeCompatible(prop, "not an int");

            Assert.False(result);
        }

        private class BaseClass { }
        private class DerivedClass : BaseClass { }
        private class ModelWithBaseProperty
        {
            public BaseClass Value { get; set; } = new();
        }
    }
}