using Altered.Attributes;
using Altered.Main;

namespace Altered.Tests
{
    // Simple enum for enum conversion tests
    public enum Status { Inactive, Active, Pending }

    // Target class with various property types to test coercion and compatibility
    public class TestTarget
    {
        public int IntProp { get; set; }
        public long LongProp { get; set; }
        public float FloatProp { get; set; }
        public double DoubleProp { get; set; }
        public decimal DecimalProp { get; set; }
        public string StringProp { get; set; }
        public Status EnumProp { get; set; }
        public int? NullableInt { get; set; }
        public object ObjectProp { get; set; } // any type
        public DateTime DateTimeProp { get; set; }
        public bool BoolProp { get; set; }
        public Address AddressProp { get; set; } // nested complex type
        public string ReadOnlyProp { get; private set; } = "ReadOnly";
        public string WriteOnlyProp { private get; set; } // no getter, should be skipped
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public override bool Equals(object obj) => obj is Address a && Street == a.Street && City == a.City;
        public override int GetHashCode() => HashCode.Combine(Street, City);
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Address HomeAddress { get; set; }
        public List<Address> PreviousAddresses { get; set; }
        public Address WorkAddress { get; set; }
    }

    public class CircularRef
    {
        public string Id { get; set; }
        public CircularRef Next { get; set; }
    }

    public class PersonWithIgnored
    {
        [IgnoreInDiff]
        public string IgnoredProp { get; set; }
        public string IncludedProp { get; set; }
    }

    public class WriteOnlyClass
    {
        private string _hidden;
        public string WriteOnlyProp { set => _hidden = value; }
    }

    public enum TestEnum { Value1, Value2, Value3 }

    public class CustomObject
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    // Helper to create a DiffEntry with all fields
    public static class DiffEntryBuilder
    {
        public static DiffEntry Create(string propertyName, object oldValue = null, object newValue = null)
        {
            return new DiffEntry
            {
                PropertyName = propertyName,
                OldValue = oldValue,
                NewValue = newValue
            };
        }
    }
}
