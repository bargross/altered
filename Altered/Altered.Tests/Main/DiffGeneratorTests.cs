using Altered.Core.Attributes;
using Altered.Core.Main;

namespace Altered.Tests.Main
{
    public class DiffGeneratorTests
    {
        [Fact]
        public void Generate_WithNoChanges_ReturnsEmptyList()
        {
            var obj = new Person { Name = "Alice", Age = 30 };
            var diffs = DiffGenerator.Generate(obj, obj);

            Assert.Empty(diffs);
        }

        [Fact]
        public void Generate_WithChangedProperty_ReturnsDiff()
        {
            var original = new Person { Name = "Alice", Age = 30 };
            var modified = new Person { Name = "Alice", Age = 31 };

            var diffs = DiffGenerator.Generate(original, modified);

            Assert.Single(diffs);
            Assert.Equal("Age", diffs[0].PropertyName);
            Assert.Equal(30, diffs[0].OldValue);
            Assert.Equal(31, diffs[0].NewValue);
        }

        [Fact]
        public void Generate_WithIgnoredProperty_SkipsIt()
        {
            var original = new PersonWithIgnored { Id = 1, Name = "Alice" };
            var modified = new PersonWithIgnored { Id = 2, Name = "Alice" };

            var diffs = DiffGenerator.Generate(original, modified);

            Assert.Single(diffs);
            Assert.Equal("Name", diffs[0].PropertyName);
        }

        [Fact]
        public void Apply_ChangesTargetObject()
        {
            var target = new Person { Name = "Alice", Age = 30 };
            var diffs = new List<DiffEntry>
        {
            new DiffEntry { PropertyName = "Age", NewValue = 31 }
        };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(31, target.Age);
            Assert.Equal("Alice", target.Name); // Unchanged
        }
    }

    // Test class with ignored property
    public class PersonWithIgnored
    {
        [IgnoreInDiff]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
