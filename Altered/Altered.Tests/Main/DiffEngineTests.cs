using Altered.Configure;
using Altered.Main;
using System.Linq.Expressions;

namespace Altered.Tests
{
    public class DiffEngineTests
    {
        private DiffEngine CreateEngine() => new DiffEngine();

        // ------------------- Configuration Tests -------------------

        [Fact]
        public void ConfigureType_WithNoConfig_CreatesDefaultConfig()
        {
            var engine = CreateEngine();
            engine.ConfigureType<Person>();

            var diffs = engine.Generate(new Person(), new Person());
            Assert.Empty(diffs);
        }

        [Fact]
        public void ConfigureTypeWithAction_AppliesConfiguratorActions()
        {
            var engine = CreateEngine();
            // Use the correct Ignore<Person> syntax
            engine.ConfigureTypeWithAction<Person>(c => c.Ignore<Person>(p => p.Age));

            var original = new Person { Name = "John", Age = 30 };
            var modified = new Person { Name = "John", Age = 31 };
            var diffs = engine.Generate(original, modified);

            Assert.Empty(diffs); // Age is ignored
        }

        [Fact]
        public void ConfigureTypeWithAction_ThrowsOnNullAction()
        {
            var engine = CreateEngine();
            Assert.Throws<ArgumentNullException>(() => engine.ConfigureTypeWithAction<Person>(null));
        }

        [Fact]
        public void ConfigureTypeWithConfigurator_AppliesGivenConfigurator()
        {
            var engine = CreateEngine();
            var config = new TypeConfigurator();
            config.Ignore<Person>(p => p.Age); // correct method
            engine.ConfigureTypeWithConfigurator<Person>(config);

            var diffs = engine.Generate(new Person { Age = 30 }, new Person { Age = 31 });
            Assert.Empty(diffs);
        }

        [Fact]
        public void ConfigureTypeWithConfigurator_ThrowsOnNullConfigurator()
        {
            var engine = CreateEngine();
            Assert.Throws<ArgumentNullException>(() => engine.ConfigureTypeWithConfigurator<Person>(null));
        }

        // ------------------- Property Selectors & Ignore/Include -------------------

        [Fact]
        public void Generate_WithIgnoreTrueAndSelectors_IgnoresSpecifiedProperties()
        {
            var engine = CreateEngine();
            var original = new Person { Name = "John", Age = 30 };
            var modified = new Person { Name = "Jane", Age = 31 };

            var diffs = engine.Generate(original, modified,
                propertySelectors: new Expression<Func<Person, object>>[] { p => p.Name },
                ignore: true);

            Assert.Single(diffs);
            Assert.Equal("Age", diffs[0].PropertyName);
        }

        [Fact]
        public void Generate_WithIgnoreFalseAndSelectors_IncludesOnlySpecifiedProperties()
        {
            var engine = CreateEngine();
            var original = new Person { Name = "John", Age = 30 };
            var modified = new Person { Name = "Jane", Age = 31 };

            var diffs = engine.Generate(original, modified,
                propertySelectors: new Expression<Func<Person, object>>[] { p => p.Name },
                ignore: false);

            Assert.Single(diffs);
            Assert.Equal("Name", diffs[0].PropertyName);
        }

        [Fact]
        public void Generate_WithSelectorsButNoIgnore_ThrowsInvalidOperationException()
        {
            var engine = CreateEngine();
            Assert.Throws<InvalidOperationException>(() =>
                engine.Generate(new Person(), new Person(),
                    propertySelectors: new Expression<Func<Person, object>>[] { p => p.Name },
                    ignore: null));
        }

        [Fact]
        public void Generate_WithNoSelectors_ComparesAllProperties()
        {
            var engine = CreateEngine();
            var original = new Person { Name = "John", Age = 30 };
            var modified = new Person { Name = "Jane", Age = 31 };

            var diffs = engine.Generate(original, modified);
            Assert.Equal(2, diffs.Count);
            Assert.Contains(diffs, d => d.PropertyName == "Name");
            Assert.Contains(diffs, d => d.PropertyName == "Age");
        }

        // ------------------- Custom Comparer Tests -------------------

        [Fact]
        public void RegisterCustomComparer_OverridesEqualityForType()
        {
            var engine = CreateEngine();
            engine.RegisterCustomComparer<Address>((a, b) => a.Street == b.Street && a.City == b.City);

            var original = new Person
            {
                HomeAddress = new Address { Street = "Main St", City = "Springfield" }
            };
            var modified = new Person
            {
                HomeAddress = new Address { Street = "Main St", City = "Springfield" }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Empty(diffs);
        }

        [Fact]
        public void RegisterCustomComparer_ThrowsOnNullComparer()
        {
            var engine = CreateEngine();
            Assert.Throws<ArgumentNullException>(() => engine.RegisterCustomComparer<Address>(null));
        }

        [Fact]
        public void CustomComparer_IsInvokedForNestedObjects()
        {
            var engine = CreateEngine();
            engine.RegisterCustomComparer<Address>((a, b) => a.Street == b.Street);

            var original = new Person
            {
                HomeAddress = new Address { Street = "Main St", City = "Springfield" }
            };
            var modified = new Person
            {
                HomeAddress = new Address { Street = "Main St", City = "Shelbyville" }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Empty(diffs);
        }

        // ------------------- Deep Equality for Nested Objects -------------------

        [Fact]
        public void Generate_WithNestedObjects_ComparesRecursively()
        {
            var engine = CreateEngine();
            var original = new Person
            {
                Name = "John",
                HomeAddress = new Address { Street = "Main St", City = "Springfield" }
            };
            var modified = new Person
            {
                Name = "John",
                HomeAddress = new Address { Street = "Main St", City = "Shelbyville" }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Single(diffs);
            Assert.Equal("HomeAddress", diffs[0].PropertyName);
        }

        [Fact]
        public void Generate_WithIdenticalNestedObjects_ProducesNoDiff()
        {
            var engine = CreateEngine();
            var original = new Person
            {
                HomeAddress = new Address { Street = "Main St", City = "Springfield" }
            };
            var modified = new Person
            {
                HomeAddress = new Address { Street = "Main St", City = "Springfield" }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Empty(diffs);
        }

        [Fact]
        public void Generate_WithDeepNestedDiff_FindsDiff()
        {
            var engine = CreateEngine();
            var original = new Person
            {
                Name = "John",
                HomeAddress = new Address { Street = "Main St", City = "Springfield" },
                WorkAddress = new Address { Street = "Oak Ave", City = "Capital City" }
            };
            var modified = new Person
            {
                Name = "John",
                HomeAddress = new Address { Street = "Main St", City = "Springfield" },
                WorkAddress = new Address { Street = "Pine Rd", City = "Capital City" }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Single(diffs);
            Assert.Equal("WorkAddress", diffs[0].PropertyName);
        }

        // ------------------- Collection Support -------------------

        [Fact]
        public void Generate_WithDifferentLengthCollections_ProducesDiff()
        {
            var engine = CreateEngine();
            var original = new Person
            {
                PreviousAddresses = new List<Address>
                {
                    new Address { Street = "Main St", City = "Springfield" }
                }
            };
            var modified = new Person
            {
                PreviousAddresses = new List<Address>
                {
                    new Address { Street = "Main St", City = "Springfield" },
                    new Address { Street = "Second St", City = "Shelbyville" }
                }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Single(diffs);
            Assert.Equal("PreviousAddresses", diffs[0].PropertyName);
        }

        [Fact]
        public void Generate_WithIdenticalCollections_ProducesNoDiff()
        {
            var engine = CreateEngine();
            var original = new Person
            {
                PreviousAddresses = new List<Address>
                {
                    new Address { Street = "Main St", City = "Springfield" }
                }
            };
            var modified = new Person
            {
                PreviousAddresses = new List<Address>
                {
                    new Address { Street = "Main St", City = "Springfield" }
                }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Empty(diffs);
        }

        [Fact]
        public void Generate_WithCollectionElementDifferences_ProducesDiff()
        {
            var engine = CreateEngine();
            var original = new Person
            {
                PreviousAddresses = new List<Address>
                {
                    new Address { Street = "Main St", City = "Springfield" }
                }
            };
            var modified = new Person
            {
                PreviousAddresses = new List<Address>
                {
                    new Address { Street = "Other St", City = "Springfield" }
                }
            };

            var diffs = engine.Generate(original, modified);
            Assert.Single(diffs);
            Assert.Equal("PreviousAddresses", diffs[0].PropertyName);
        }

        // ------------------- Circular Reference Handling -------------------

        [Fact]
        public void Generate_WithCircularReferences_DoesNotStackOverflow()
        {
            var engine = CreateEngine();
            var original = new CircularRef { Id = "A" };
            original.Next = original;

            var modified = new CircularRef { Id = "A" };
            modified.Next = modified;

            var diffs = engine.Generate(original, modified);
            Assert.Empty(diffs);
        }

        [Fact]
        public void Generate_WithCircularReferenceDifferentValues_FindsDiff()
        {
            var engine = CreateEngine();
            var original = new CircularRef { Id = "A" };
            original.Next = new CircularRef { Id = "B" };

            var modified = new CircularRef { Id = "A" };
            modified.Next = new CircularRef { Id = "C" };

            var diffs = engine.Generate(original, modified);
            Assert.Single(diffs);
            Assert.Equal("Next", diffs[0].PropertyName);
        }

        // ------------------- Edge Cases -------------------

        [Fact]
        public void Generate_WithBothNull_ReturnsEmptyList()
        {
            var engine = CreateEngine();
            var diffs = engine.Generate<Person>(null, null);
            Assert.Empty(diffs);
        }

        [Fact]
        public void Generate_WithOriginalNull_Throws()
        {
            var engine = CreateEngine();
            Assert.Throws<ArgumentNullException>(() => engine.Generate<Person>(null, new Person()));
        }

        [Fact]
        public void Generate_WithModifiedNull_Throws()
        {
            var engine = CreateEngine();
            Assert.Throws<ArgumentNullException>(() => engine.Generate<Person>(new Person(), null));
        }

        // ------------------- ShouldSkipProperty Tests -------------------

        [Fact]
        public void ShouldSkipProperty_ForWriteOnly_ReturnsTrue()
        {
            var engine = CreateEngine();
            var prop = typeof(WriteOnlyClass).GetProperty("WriteOnlyProp");
            var result = engine.ShouldSkipProperty<WriteOnlyClass>(prop);
            Assert.True(result);
        }

        [Fact]
        public void ShouldSkipProperty_WithIgnoreInDiffAttribute_ReturnsTrue()
        {
            var engine = CreateEngine();
            var prop = typeof(PersonWithIgnored).GetProperty("IgnoredProp");
            var result = engine.ShouldSkipProperty<PersonWithIgnored>(prop);
            Assert.True(result);
        }

        [Fact]
        public void ShouldSkipProperty_WithConfiguredIgnore_ReturnsTrue()
        {
            var engine = CreateEngine();
            engine.ConfigureTypeWithAction<Person>(c => c.Ignore<Person>(p => p.Age));
            var prop = typeof(Person).GetProperty("Age");
            var result = engine.ShouldSkipProperty<Person>(prop);
            Assert.True(result);
        }

        [Fact]
        public void ShouldSkipProperty_WithConfiguredInclude_ReturnsFalseForIncluded()
        {
            var engine = CreateEngine();
            engine.ConfigureTypeWithAction<Person>(c => c.Include<Person>(p => p.Name));
            var prop = typeof(Person).GetProperty("Name");
            var result = engine.ShouldSkipProperty<Person>(prop);
            Assert.False(result);
        }

        [Fact]
        public void ShouldSkipProperty_WithConfiguredInclude_ReturnsTrueForNotIncluded()
        {
            var engine = CreateEngine();
            engine.ConfigureTypeWithAction<Person>(c => c.Include<Person>(p => p.Name));
            var prop = typeof(Person).GetProperty("Age");
            var result = engine.ShouldSkipProperty<Person>(prop);
            Assert.True(result);
        }

        // ------------------- ClearAllConfigurations -------------------

        [Fact]
        public void ClearAllConfigurations_ResetsComparersAndTypeConfigs()
        {
            var engine = CreateEngine();
            engine.RegisterCustomComparer<Address>((a, b) => true);
            engine.ConfigureTypeWithAction<Person>(c => c.Ignore<Person>(p => p.Age));

            engine.ClearAllConfigurations();

            Assert.False(engine._comparerManager.IsRegistered<Address>());

            var diffs = engine.Generate(new Person { Age = 30 }, new Person { Age = 31 });
            Assert.Single(diffs);
            Assert.Equal("Age", diffs[0].PropertyName);
        }

        // ------------------- InvokeCustomComparer -------------------

        [Fact]
        public void InvokeCustomComparer_ReturnsBoolean()
        {
            var engine = CreateEngine();
            engine.RegisterCustomComparer<Address>((a, b) => a.Street == b.Street);
            var addr1 = new Address { Street = "Main St" };
            var addr2 = new Address { Street = "Main St" };

            var result = engine.InvokeCustomComparer(typeof(Address), addr1, addr2);
            Assert.True(result);
        }
    }
}