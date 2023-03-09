# Code Style Guide
## Class names for specific types
### Services
Services should be single instance classes that are used to access external resources such as web requests and files.

In `StandardModule.cs` we are registering every class which matches the pattern `*Service` as a single instance:
```csharp
builder.RegisterAssemblyTypes(ThisAssembly)
    .Where(t => t.Name.EndsWith("Service"))
    .SingleInstance();
```

These class types should all be placed inside the `Services` directory of the project.

### View Models
View Models are multi-instance classes which are tied to a View. These should be named the same as the view that they correspond to.

In `StandardModule.cs` we are registering every class which matches the pattern `*ViewModel`:
```csharp
builder.RegisterAssemblyTypes(ThisAssembly)
    .Where(t => t.Name.EndsWith("ViewModel"));
```

These class types should all be placed inside the `ViewModels` directory of the project.

## Prefer using explicit types and new()
Yes:
```csharp
Foo bar = new();
```

No:
```csharp
Foo bar = new Foo();
var bar = new Foo();
```

## Prefer adding braces to statements
This one will fail the code quality check in Codacy if not followed.

Yes:
```csharp
if (something)
{
  return true;
}
```

No:
```csharp
if (something)
  return true;
```