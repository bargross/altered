# Altered

A lightweight .NET library for detecting and applying differences between objects.

## What is it?

Altered tells you what changed between two objects of the same type. It generates a list of property differences (name, old value, new value) that you can inspect, store, or apply to another object.

## Why is it useful?

Ever needed to:
- Log exactly what a user changed in a form?
- Build an audit trail of entity changes?
- Implement undo/redo functionality?
- Create a patch endpoint without writing manual property comparisons?

Altered handles this automatically using reflection. No more writing `if (old.Name != new.Name)` for every property.

## Features

- Compare any two objects of the same type
- Get back a simple list of what changed
- Apply changes to other objects
- Ignore specific properties with an attribute
- No external dependencies

## Quick Example

```csharp
var original = new Person { Name = "Alice", Age = 30 };
var modified = new Person { Name = "Alice", Age = 31 };

var changes = DiffGenerator.Generate(original, modified);

// changes contains:
// { PropertyName = "Age", OldValue = 30, NewValue = 31 }

foreach (var change in changes)
{
    Console.WriteLine($"{change.PropertyName}: {change.OldValue} → {change.NewValue}");
}
```