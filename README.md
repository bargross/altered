# Altered

A lightweight .NET library for detecting and applying differences between objects.

## What is Altered?

Altered is a lightweight, zero‑dependency .NET library that answers the simple question: “What changed between these two objects?”

It generates a structured list of property differences between any two objects of the same type – and lets you apply those differences to other objects, serialize them for storage, or use them to drive custom logic.

Unlike manual property‑by‑property comparison (verbose, error‑prone, hard to maintain), Altered uses reflection to do the heavy lifting. But it goes far beyond that: it gives you full control over what to compare, how to compare it, and what to do with the result.

## 🤔 Why is it useful?

In almost every real‑world application, you need to track, communicate, or act on changes:

| Scenario | Without Altered | With Altered |
|----------|----------------|---------------|
| **Audit logging** | Write `if (old.Name != new.Name)` for every property – 50+ lines of repetitive, error‑prone code. | One line: `var diffs = DiffGenerator.Generate(old, new);` – automatically includes new properties. |
| **REST API PATCH** | Manually check which fields changed, update only those, and struggle with nested objects. | `DiffApplier.Apply(existing, updated)` – applies only changed properties, including deep objects. |
| **Undo / Redo** | Store full object copies (memory heavy) or write custom rollback logic per type. | Store a tiny list of `DiffEntry` – reversible, type‑agnostic, and memory‑efficient. |
| **Data synchronisation** | Write fragile diff‑and‑patch code for every entity. | Use the standard diff format, export to JSON, and reapply later – works across systems. |
| **Form change tracking** | Manually compare each field or rely on messy `INotifyPropertyChanged` implementations. | Generate a diff on every user action; instantly know what changed. |
| **Adding a new property** | Update every comparison method, audit log, and patch endpoint. | Do nothing – Altered automatically includes the new property (unless ignored). |

## 🎯 Who is the target audience, and why will they find it valuable?

### Backend API developers
Need to implement `PATCH` endpoints that only update changed fields (RFC 7386 style).  
`DiffApplier.Apply(existing, updates)` handles everything – nested objects, collections, and all. No more manual “if changed” checks.

### Developers building audit / compliance systems
Must log every change to sensitive data (finance, healthcare, GDPR).  
Altered generates a structured diff in one line, serializes to JSON for storage, and lets you ignore noisy properties (e.g., `LastModified`).

### Maintainers of large or legacy codebases
Have dozens of entities with repeated diff logic.  
Altered centralises change tracking: define type configurations once, reuse everywhere. Reduces boilerplate and maintenance headaches.

### Tooling / framework authors
Building an ORM, workflow engine, or state machine that needs change detection.  
Altered provides pluggable custom comparers (`RegisterComparer<MyType>`) to handle domain‑specific equality.

### Desktop / mobile app developers
Implementing undo/redo stacks, form “dirty” states, or change previews.  
Generate a diff on each user action, store it, and apply the reverse diff for undo – all with the same simple API.

### Anyone who adds new properties to models
Without Altered, you update every comparison method, audit log, and patch endpoint.  
With Altered, new properties are automatically included (unless you explicitly ignore them). **Your code evolves with your model – not against it.**

## 🔍 What makes Altered different?

- **No dependencies** – pure .NET, easy to integrate anywhere, no baggage.

- **Extensible by design** – custom comparers for any type, type‑specific configurations, and pluggable JSON converters.

- **Tested for real‑world needs** – handles nullable types, value types, reference types, records, and classes with inheritance.

- **Performance aware** – reflection results are cached per type; source generators are planned for v2 to eliminate runtime overhead.

- **Simple, consistent API** – one namespace, two main classes (`DiffGenerator`, `DiffApplier`), and intuitive extension methods.

- **JSON as a first‑class citizen** – built‑in serialization to/from JSON, file I/O, and try‑pattern helpers for safe deserialization.

- **Works the way you think** – you don't need to learn a new paradigm. It just does the obvious thing.

- **Evolves with your code** – add a property to a class, and Altered automatically includes it in diffs (unless you explicitly ignore it). No hidden surprises.

## 📌 In a nutshell

**Altered** turns object diffing from a chore into a joy.

It gives you a clean, standardised way to detect, represent, and apply changes across your entire .NET ecosystem – whether you're building a tiny console tool or a large enterprise system.

- **Simple to start** – one line of code gives you a structured diff.
- **Powerful when you need it** – custom comparers, type configurations, JSON serialisation.
- **Zero surprises** – no hidden magic, no external dependencies.

If you've ever written `if (old.Property != new.Property)` more than three times in a row, Altered is for you.

## Features

Core Features
- Detect changes between any two objects of the same type
- Generate structured diffs with property names, old values, and new values
- Apply diffs to any compatible object
- Ignore specific properties using the [IgnoreInDiff] attribute
- Works with any .NET type (classes, records, structs)
- No external dependencies

# 🎯 Type Configuration (Per-Type Rules)

Configure diff behavior per type using TypeConfigurator<T>:

```csharp
// Configure once, apply everywhere
var config = new TypeConfigurator<User>()
    .Ignore(u => u.Id)
    .Ignore(u => u.LastLogin);

DiffGenerator.Configure<User>(config);

// Or using a lambda (fluent API)
DiffGenerator.Configure<User>(cfg => cfg
    .Ignore(u => u.PasswordHash)
    .Ignore(u => u.TempData));
```
# 🔁 Multiple Configurations
Register different configurations for different types:

```csharp
// Configure User type
DiffGenerator.Configure<User>(cfg => cfg
    .Ignore(u => u.Id)
    .Ignore(u => u.CreatedAt));

// Configure Product type separately
DiffGenerator.Configure<Product>(cfg => cfg
    .Ignore(p => p.SKU)
    .Ignore(p => p.InternalNotes));

// Both configurations work automatically when diffing their respective types
var userChanges = DiffGenerator.Generate(user1, user2);        // Uses User config
var productChanges = DiffGenerator.Generate(product1, product2); // Uses Product config
```
# 🔍 Custom Comparers

Register custom comparison logic for specific types, overriding the default equality check:

```csharp
// Case-insensitive string comparison
DiffGenerator.RegisterComparer<string>((a, b) => 
    string.Equals(a, b, StringComparison.OrdinalIgnoreCase));

// Date-only comparison (ignore time component)
DiffGenerator.RegisterComparer<DateTime>((a, b) => a.Date == b.Date);

// Floating-point tolerance for 3D coordinates
DiffGenerator.RegisterComparer<double>((a, b) => Math.Abs(a - b) < 0.001);

// Custom object comparison
DiffGenerator.RegisterComparer<Address>((a, b) => 
    a.Street == b.Street && a.City == b.City);
```

Priority order: Custom comparers have the highest priority, followed by TypeConfigurator ignore rules, then [IgnoreInDiff] attributes, and finally the default equality comparison.

# 🔁 Multiple Configurations
Register different configurations for different types:

```csharp
// Configure User type
DiffGenerator.Configure<User>(cfg => cfg
    .Ignore(u => u.Id)
    .Ignore(u => u.CreatedAt));

// Configure Product type separately
DiffGenerator.Configure<Product>(cfg => cfg
    .Ignore(p => p.SKU)
    .Ignore(p => p.InternalNotes));

// Both configurations work automatically when diffing their respective types
var userChanges = DiffGenerator.Generate(user1, user2);        // Uses User config
var productChanges = DiffGenerator.Generate(product1, product2); // Uses Product config
```

# 📦 JSON-Serializable Output
Built-in JSON support makes Altered perfect for audit trails, API responses, and persistent storage:

```csharp
// Generate diffs
var diffs = DiffGenerator.Generate(original, modified);

// Serialize to JSON string
string json = diffs.ToJson();
Console.WriteLine(json);
// Output:
// [
//   {
//     "propertyName": "Age",
//     "oldValue": 30,
//     "newValue": 31
//   }
// ]

// Deserialize back
var restored = DiffJsonExtensions.FromJson(json);

// Save to a file (async)
await diffs.WriteToJsonFileAsync("audit/user123.json");

// Read from a file (async)
var loaded = await DiffJsonExtensions.ReadFromJsonFileAsync("audit/user123.json");

// Try‑pattern for safe deserialization
var (success, loadedDiffs) = DiffJsonExtensions.TryFromJson(maybeInvalidJson);
```

# Ignore Specific Properties
Use the [IgnoreInDiff] attribute to mark properties that should be excluded from all diff operations:

```csharp
public class Person
{
    [IgnoreInDiff]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    [IgnoreInDiff]
    public DateTime LastModified { get; set; }
}
```

# 🔄 Apply Diffs
Apply recorded changes to any compatible object:

```csharp
var target = new Person { Name = "Unknown", Age = 0 };
var diffs = new List<DiffEntry>
{
    new DiffEntry { PropertyName = "Age", NewValue = 31 }
};

DiffApplier.Apply(target, diffs);
Console.WriteLine(target.Age); // 31
```

# 🚀 Complete Example: Audit Logging
Here's a real‑world example showing how these features work together:

```csharp
// Setup: custom comparers and type configuration
DiffGenerator.RegisterComparer<DateTime>((a, b) => a.Date == b.Date);
DiffGenerator.Configure<User>(cfg => cfg
    .Ignore(u => u.PasswordHash)
    .Ignore(u => u.LastLoginIP));

// Your domain logic
var original = db.Users.Find(id);
var updated = MapFromRequest(original, request);

var changes = DiffGenerator.Generate(original, updated);
if (changes.Any())
{
    var auditEntry = new AuditLog
    {
        EntityType = nameof(User),
        EntityId = id,
        ChangedAt = DateTime.UtcNow,
        ChangesJson = changes.ToJson()   // JSON-serializable output
    };
    await db.AuditLogs.AddAsync(auditEntry);
    
    DiffApplier.Apply(original, changes);
    await db.SaveChangesAsync();
}
```