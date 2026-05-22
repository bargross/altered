# Changelog

All notable changes to the **Altered** library are documented in this file.

## [Unreleased] – PR #8 (Thread Safety & API Refinements)

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

## 🧪 Testing
- Massively expanded test coverage – added hundreds of unit tests covering all public APIs, edge cases, and thread‑safety scenarios.

- New test categories:

 - Blacklist mode (ignore = true) with property selectors.

 - Whitelist mode (ignore = false) with property selectors.

 - Concurrent calls to Generate, Configure, ClearAll.

 - Invalid mode mixing and exception validation.


## 📚 Documentation

- Updated the `README.md` with new code examples and API usage guidelines.

- Added inline XML comments for all public methods, including the new Generate overload.