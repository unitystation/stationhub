# Code Style Guide
## Class names for specific types
### Services
Services should be single instance classes that are used to access external resources such as web requests and files.

In `StandardModule.cs` we are registering each service we use as a single instance:
```csharp
builder.RegisterType<EnvironmentService>().As<IEnvironmentService>().SingleInstance();
builder.RegisterType<PreferencesService>().As<IPreferencesService>().SingleInstance();
[...]
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
This makes code review easier when reading through the changes without an IDE open.

Yes:
```csharp
Foo bar = new();
Example two = SomeClass.SomeMethodThatReturnsSomething();
```

No:
```csharp
Foo bar = new Foo();
var baz = new Foo();
var two = SomeClass.SomeMethodThatReturnsSomething();
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