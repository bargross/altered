using Altered.Main;

namespace Altered.Tests
{
    public class DiffApplierTests
    {
        // Helper to create a DiffEntry with optional type hint
        private DiffEntry CreateDiff(string propertyName, object newValue, string typeHint = null)
        {
            return new DiffEntry
            {
                PropertyName = propertyName,
                NewValue = newValue,
                NewValueTypeHint = typeHint // assumes this property exists
            };
        }

        // -----------------------------------------------------------------
        // Basic application tests
        // -----------------------------------------------------------------

        [Fact]
        public void Apply_WithValidDiff_SetsProperty()
        {
            var target = new TestTarget { IntProp = 10 };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.IntProp), 20)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(20, target.IntProp);
        }

        [Fact]
        public void Apply_WithMultipleDiffs_AppliesAll()
        {
            var target = new TestTarget { IntProp = 10, StringProp = "old" };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.IntProp), 20),
                CreateDiff(nameof(TestTarget.StringProp), "new")
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(20, target.IntProp);
            Assert.Equal("new", target.StringProp);
        }

        [Fact]
        public void Apply_WithNullNewValue_SetsNullForNullable()
        {
            var target = new TestTarget { NullableInt = 5 };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.NullableInt), null)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Null(target.NullableInt);
        }

        [Fact]
        public void Apply_WithNullNewValue_OnNonNullable_ThrowsOrFails()
        {
            var target = new TestTarget { IntProp = 10 };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.IntProp), null)
            };

            // If an exception is thrown, catch it and verify; if not, ensure the value remains unchanged.
            Exception exception = Record.Exception(() => DiffApplier.Apply(target, diffs));

            if (exception != null)
            {
                // It should be ArgumentException or TargetInvocationException wrapping it.
                Assert.IsAssignableFrom<ArgumentException>(exception);
            }
            else
            {
                // If no exception, the value should not have changed.
                Assert.Equal(10, target.IntProp);
            }
        }

        [Fact]
        public void Apply_OnReadOnlyProperty_WithPrivateSetter_StillSetsViaReflection()
        {
            var target = new TestTarget();
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.ReadOnlyProp), "new value")
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal("new value", target.ReadOnlyProp); // passes
        }

        [Fact]
        public void Apply_OnNonExistentProperty_Skips()
        {
            var target = new TestTarget();
            var diffs = new List<DiffEntry>
            {
                CreateDiff("NonExistent", "anything")
            };

            // Should not throw
            var exception = Record.Exception(() => DiffApplier.Apply(target, diffs));
            Assert.Null(exception);
        }

        // -----------------------------------------------------------------
        // Type coercion tests (with and without hint)
        // -----------------------------------------------------------------

        [Fact]
        public void Apply_IntToLong_WithHint_Converts()
        {
            var target = new TestTarget { LongProp = 0 };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.LongProp), 42, "System.Int64")
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(42L, target.LongProp);
        }

        [Fact]
        public void Apply_IntToLong_WithoutHint_DoesNotConvert()
        {
            var target = new TestTarget { LongProp = 0 };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.LongProp), 42) // no hint
            };

            DiffApplier.Apply(target, diffs);

            // IsTypeCompatible returns false because long is not assignable from int, so no change
            Assert.Equal(0L, target.LongProp);
        }

        [Fact]
        public void Apply_DoubleToFloat_WithHint_Converts()
        {
            var target = new TestTarget { FloatProp = 0f };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.FloatProp), 1.23, "System.Single")
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(1.23f, target.FloatProp);
        }

        [Fact]
        public void Apply_DoubleToFloat_WithoutHint_DoesNotConvert()
        {
            var target = new TestTarget { FloatProp = 0f };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.FloatProp), 1.23) // double
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(0f, target.FloatProp);
        }

        [Fact]
        public void Apply_IntToEnum_WithHint_Converts()
        {
            var target = new TestTarget { EnumProp = Status.Inactive };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.EnumProp), 2, typeof(Status).AssemblyQualifiedName)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(Status.Pending, target.EnumProp);
        }

        [Fact]
        public void Apply_IntToEnum_WithoutHint_DoesNotConvert()
        {
            var target = new TestTarget { EnumProp = Status.Inactive };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.EnumProp), 2)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(Status.Inactive, target.EnumProp);
        }

        [Fact]
        public void Apply_WithHintButInvalidConversion_FallsThroughAndFails()
        {
            var target = new TestTarget { DateTimeProp = DateTime.Parse("2020-01-01") };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.DateTimeProp), "not a date", "System.DateTime")
            };

            DiffApplier.Apply(target, diffs);

            // Conversion fails (FormatException), then compatibility check fails (string != DateTime)
            // so the value should remain unchanged.
            Assert.Equal(DateTime.Parse("2020-01-01"), target.DateTimeProp);
        }

        // -----------------------------------------------------------------
        // Argument validation
        // -----------------------------------------------------------------

        [Fact]
        public void Apply_WithNullTarget_ThrowsArgumentNullException()
        {
            var diffs = new List<DiffEntry> { CreateDiff("Prop", 1) };
            Assert.Throws<ArgumentNullException>("target", () => DiffApplier.Apply<TestTarget>(null, diffs));
        }

        [Fact]
        public void Apply_WithNullDiffs_ThrowsArgumentNullException()
        {
            var target = new TestTarget();
            Assert.Throws<ArgumentNullException>("diffs", () => DiffApplier.Apply(target, null));
        }

        [Fact]
        public void Apply_WithNullEntryInDiffs_ThrowsArgumentException()
        {
            var target = new TestTarget();
            var diffs = new List<DiffEntry> { null };
            var ex = Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
            Assert.Contains("Null value in different entries list", ex.Message);
        }

        [Fact]
        public void Apply_WithEntryHavingEmptyPropertyName_ThrowsArgumentException()
        {
            var target = new TestTarget();
            var diffs = new List<DiffEntry> { CreateDiff("", 1) };
            var ex = Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
            Assert.Contains("property name as null, empty or white space", ex.Message);
        }

        [Fact]
        public void Apply_WithEntryHavingNullPropertyName_ThrowsArgumentException()
        {
            var target = new TestTarget();
            var diffs = new List<DiffEntry> { CreateDiff(null, 1) };
            var ex = Assert.Throws<ArgumentException>(() => DiffApplier.Apply(target, diffs));
            Assert.Contains("property name as null", ex.Message);
        }

        // -----------------------------------------------------------------
        // Edge cases: property not writable, write-only, etc.
        // -----------------------------------------------------------------

        [Fact]
        public void Apply_OnWriteOnlyProperty_Skips()
        {
            // Write-only property has no getter, but it's writable; we can test that it is skipped because it's not in the dictionary?
            // The dictionary is built from GetProperties with BindingFlags.Public | Instance, which includes write-only if it has set.
            // Actually we don't filter writable in GetWritableProperties; we get all public properties, then we check CanWrite later.
            // So write-only is included, but when we try to set, it works because it has setter.
            // The property can be set, so the diff will apply. That's fine.
            // But there is no way to read it to verify. We can test that it doesn't throw.
            var target = new TestTarget();
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.WriteOnlyProp), "new value")
            };

            var ex = Record.Exception(() => DiffApplier.Apply(target, diffs));
            Assert.Null(ex);
            // We cannot assert value because property is write-only.
        }

        // -----------------------------------------------------------------
        // Complex object assignment (reference types)
        // -----------------------------------------------------------------

        [Fact]
        public void Apply_WithAddressObject_AssignsReference()
        {
            var target = new TestTarget { AddressProp = null };
            var address = new Address { Street = "Main St", City = "Springfield" };

            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.AddressProp), address)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Same(address, target.AddressProp);
        }

        [Fact]
        public void Apply_WithNullForReferenceType_AssignsNull()
        {
            var target = new TestTarget { AddressProp = new Address() };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.AddressProp), null)
            };

            DiffApplier.Apply(target, diffs);

            Assert.Null(target.AddressProp);
        }

        // -----------------------------------------------------------------
        // IsTypeCompatible (internal method) – can be tested via public Apply if needed
        // but we can also directly test the internal method if we have InternalsVisibleTo.
        // We'll rely on integration tests.
        // -----------------------------------------------------------------

        // -----------------------------------------------------------------
        // Testing with multiple diffs where some fail (should not stop others)
        // -----------------------------------------------------------------

        [Fact]
        public void Apply_WithSomeFailingDiffs_AppliesSuccessfulOnes()
        {
            var target = new TestTarget { IntProp = 10, LongProp = 0 };
            var diffs = new List<DiffEntry>
            {
                CreateDiff(nameof(TestTarget.IntProp), 20), // valid
                CreateDiff(nameof(TestTarget.LongProp), 42), // incompatible, no hint -> will be ignored
                CreateDiff(nameof(TestTarget.StringProp), "new") // valid
            };

            DiffApplier.Apply(target, diffs);

            Assert.Equal(20, target.IntProp);
            Assert.Equal(0L, target.LongProp); // unchanged
            Assert.Equal("new", target.StringProp);
        }

        // -----------------------------------------------------------------
        // Performance / large diff list (optional – we skip)
        // -----------------------------------------------------------------
    }
}