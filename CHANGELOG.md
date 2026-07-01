# Changelog

## 1.0.2 - 2026-05-22
All notable changes to the **Altered** library are documented in this file.

### 🔄 Removed

- **Static `Ignore` and `Include` properties** on `DiffGenerator` – these global flags have been removed to eliminate mutable static state and improve thread safety.

### ✨ Added

- **`bool ignore` parameter** on `DiffGenerator.Generate<TValue>()`:
  - When `ignore = true`, the provided `propertySelectors` are treated as a **blacklist** (those properties are excluded from diff).
  - When `ignore = false`, the property selectors act as a **whitelist** (only the specified properties are compared).
  - This parameter is required whenever `propertySelectors` are supplied, removing ambiguity about how selectors should be applied.

- **Thread‑safe internal state** – all configuration and comparer storage now uses thread‑safe collections (`ConcurrentDictionary`, `ConcurrentBag`). Multiple threads can safely generate diffs, register comparers, and clear configurations concurrently.

- **Mutual exclusion enforcement** in `TypeConfigurator` – attempting to call `Ignore` after `Include` (or vice versa) on the same configurator now throws a clear `InvalidOperationException`, preventing inconsistent modes.

- **Full XML documentation** for all public APIs, including the new `Generate` overload.

### 🐛 Fixed

- **`Include` mode behavior** – previously, properties marked with `Include` were incorrectly skipped. Now they correctly form a whitelist, so only included properties are compared in the diff.

- **Property selector application** – when `ignore = false`, the system now correctly limits diff generation to the selected properties, without inadvertently excluding them.

- **TypeConfigurator state corruption** – switching between blacklist and whitelist modes no longer leaves the configurator in an invalid state; instead, an exception is thrown to alert the caller.

- **Static state pollution** – removed all static flags that could cause cross‑test or cross‑thread interference. Configuration is now fully encapsulated per call or per registered type.

### ⚠️ Breaking Changes

- **`DiffGenerator.Generate` signature changed** – you must now pass the `ignore` parameter explicitly when providing property selectors.

```csharp
// Before (PR #7 and earlier)
var diffs = DiffGenerator.Generate(original, modified, p => p.Name);

// After (PR #8)
var diffs = DiffGenerator.Generate(original, modified, ignore: true, p => p.Name);   // blacklist
var diffs = DiffGenerator.Generate(original, modified, ignore: false, p => p.Name);  // whitelist
```

- `DiffGenerator.Ignore` **and** `DiffGenerator.Include` **properties removed** – any code that set or read these static flags must be updated to use the new ignore parameter on Generate.

- `TypeConfigurator` now throws when mixing `Ignore` (black listing) and `Include` (white listing) – if your code previously called both methods on the same configurator (even accidentally), it will now throw. Ensure you use only one mode per configurator.

### 🧪 Testing
- Massively expanded test coverage – added hundreds of unit tests covering all public APIs, edge cases, and thread‑safety scenarios.

- New test categories:

 - Blacklist mode (ignore = true) with property selectors.

 - Whitelist mode (ignore = false) with property selectors.

 - Concurrent calls to Generate, Configure, ClearAll.

 - Invalid mode mixing and exception validation.


### 📚 Documentation

- Updated the `README.md` with new code examples and API usage guidelines.

- Added inline XML comments for all public methods, including the new Generate overload.

---

# Changelog

## [1.0.3] – 2026-07-01

### Changed

- **Namespace consolidation**  
  All types previously under `Altered.Core.*` (e.g., `Altered.Core.Main`, `Altered.Core.Configure`, `Altered.Core.Converters`) have been moved to the root `Altered.*` namespace. This simplifies usage and reduces nesting.  
  *Breaking change:* If you referenced any `Altered.Core.*` namespace directly, update your `using` statements to `Altered.*`.

### Fixed

- **`Configure<T>()` no longer resets other type configurations**  
  Previously, calling `Configure<T>()` or `Configure<T>(action)` replaced the entire `TypeConfigurationManager`, erasing all previously configured types. Now only the specified type is added or updated, preserving existing configurations.

- **Duplicate property names in `ConcurrentBag`**  
  `TypeConfigurator` now uses a thread‑safe `ConcurrentDictionary` for ignored/included property names, preventing duplicates and improving lookup performance from O(n) to O(1).

- **`Clear()` fully resets the configurator**  
  `ClearAllProperties()` now clears both ignore and include lists and resets the `_isInclusion` / `_isExclusion` mode flags, returning the configurator to a clean state.

- **Correct error message in `BlackListProperties`**  
  The exception thrown when switching to blacklist mode while already in whitelist mode now accurately describes the conflict.

- **`TryFromJson` returns `true` for valid empty JSON**  
  `TryFromJson` now returns `(true, [])` for `"[]"` – a valid JSON payload that deserializes to an empty list is considered a success, not a failure.

- **`ClearAll()` resets comparer manager**  
  `DiffGenerator.ClearAll()` now clears both the type configuration manager and the custom comparer manager, allowing re‑registration of comparers after a reset without duplicate‑registration exceptions.

- **`ComparerManager._customComparers` is now private**  
  The internal field has been made `private` / `internal readonly` to prevent external mutation and improve encapsulation.

- **`InvokeCustomComparer` performance**  
  Replaced `DynamicInvoke` with a compiled expression‑based cache per type, eliminating reflection overhead and boxing on each call.

- **Removed dead code**  
  The unused `ValidateConfigurationType` method and the unused `propertyNames` / `type` variables in `AddToCollection` have been removed.

- **Struct constraint removed**  
  The `where TValue : class` constraint is removed from `Generate` methods, enabling full support for structs and value types as documented in the README.

---

**Upgrade recommended** – especially if you use multiple type configurations, custom comparers, or struct types. Update your `using` statements to reflect the new namespace structure.
