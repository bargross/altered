using System;
using System.Collections.Generic;
using Xunit;
using Altered.Core.Configure;

namespace Altered.Core.Tests.Configure
{
    public class ComparerManagerTests
    {
        // Test delegates
        private static bool StringComparer(string a, string b) => a == b;
        private static bool IntComparer(int a, int b) => a == b;
        private static bool ObjectComparer(object a, object b) => a.Equals(b);

        #region Constructor Tests

        [Fact]
        public void DefaultConstructor_InitializesEmptyDictionary()
        {
            var manager = new ComparerManager();
            Assert.False(manager.IsRegistered<string>());
        }

        [Fact]
        public void Constructor_WithDictionary_StoresProvidedDictionary()
        {
            var dict = new Dictionary<Type, Delegate>
            {
                [typeof(string)] = (Func<string, string, bool>)StringComparer
            };
            var manager = new ComparerManager(dict);
            Assert.True(manager.IsRegistered<string>());
        }

        #endregion

        #region Register Tests

        [Fact]
        public void Register_NewType_AddsComparer()
        {
            var manager = new ComparerManager();
            manager.Register<string>(StringComparer);
            Assert.True(manager.IsRegistered<string>());
            Assert.Same(StringComparer, manager.Get<string>());
        }

        [Fact]
        public void Register_AlreadyRegisteredType_ThrowsArgumentException()
        {
            var manager = new ComparerManager();
            manager.Register<string>(StringComparer);

            var ex = Assert.Throws<ArgumentException>(() =>
                manager.Register<string>((a, b) => a == b));
            Assert.Contains("Type String already has a comparer registered.", ex.Message);
        }

        [Fact]
        public void Register_AllowsNullComparer()
        {
            var manager = new ComparerManager();
            manager.Register<string>(null);
            Assert.True(manager.IsRegistered<string>());
            Assert.Null(manager.Get<string>());
        }

        #endregion

        #region Replace Tests

        [Fact]
        public void Replace_ExistingType_OverwritesComparer()
        {
            var manager = new ComparerManager();
            manager.Register<string>(StringComparer);
            Func<string, string, bool> newComparer = (a, b) => a.Length == b.Length;

            manager.Replace<string>(newComparer);

            Assert.Same(newComparer, manager.Get<string>());
        }

        [Fact]
        public void Replace_TypeNotRegistered_ThrowsArgumentException()
        {
            var manager = new ComparerManager();

            var ex = Assert.Throws<ArgumentException>(() =>
                manager.Replace<string>(StringComparer));
            Assert.Contains("No Comparer for type String has been registered.", ex.Message);
        }

        #endregion

        #region IsRegistered Tests

        [Fact]
        public void IsRegistered_Generic_ReturnsTrueForRegisteredType()
        {
            var manager = new ComparerManager();
            manager.Register<int>(IntComparer);
            Assert.True(manager.IsRegistered<int>());
        }

        [Fact]
        public void IsRegistered_Generic_ReturnsFalseForUnregisteredType()
        {
            var manager = new ComparerManager();
            Assert.False(manager.IsRegistered<object>());
        }

        [Fact]
        public void IsRegistered_TypeParameter_ReturnsTrueForRegisteredType()
        {
            var manager = new ComparerManager();
            manager.Register<object>(ObjectComparer);
            Assert.True(manager.IsRegistered(typeof(object)));
        }

        [Fact]
        public void IsRegistered_TypeParameter_ReturnsFalseForUnregisteredType()
        {
            var manager = new ComparerManager();
            Assert.False(manager.IsRegistered(typeof(string)));
        }

        #endregion

        #region Get Tests

        [Fact]
        public void Get_Generic_ReturnsComparerWhenRegistered()
        {
            var manager = new ComparerManager();
            manager.Register<int>(IntComparer);
            var comparer = manager.Get<int>();
            Assert.Same(IntComparer, comparer);
        }

        [Fact]
        public void Get_Generic_WhenNotRegistered_ThrowsArgumentException()
        {
            var manager = new ComparerManager();
            var ex = Assert.Throws<ArgumentException>(() => manager.Get<decimal>());

            Assert.Contains("No Comparer for type Decimal has been registered.", ex.Message);
        }

        [Fact]
        public void Get_TypeParameter_ReturnsComparerWhenRegistered()
        {
            var manager = new ComparerManager();
            manager.Register<object>(ObjectComparer);
            var comparer = manager.Get(typeof(object));
            Assert.Same(ObjectComparer, comparer);
        }

        [Fact]
        public void Get_TypeParameter_WhenNotRegistered_ThrowsArgumentException()
        {
            var manager = new ComparerManager();
            var ex = Assert.Throws<ArgumentException>(() => manager.Get(typeof(Version)));
            Assert.Contains("No Comparer for type Version has been registered.", ex.Message);
        }

        #endregion

        #region Edge Cases & Type Safety

        [Fact]
        public void RegisterAndGet_ExactTypeMatching_NoInheritance()
        {
            var manager = new ComparerManager();
            manager.Register<object>(ObjectComparer);
            Assert.False(manager.IsRegistered<string>()); // string != object
            Assert.Throws<ArgumentException>(() => manager.Get<string>());
        }

        [Fact]
        public void Replace_WithNullComparer_StoresNull()
        {
            var manager = new ComparerManager();
            manager.Register<string>(StringComparer);
            manager.Replace<string>(null);
            Assert.Null(manager.Get<string>());
        }

        #endregion
    }
}