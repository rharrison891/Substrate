# Notify Attribute
The [Notify] attribute generates boilerplate for property change
notification, so you don’t have to manually write INotifyPropertyChanged
logic.
It is designed for MVVM scenarios where you want clean view-models without repeating the same patterns.
## ✨ What it does
Normally you would write:
```csharp 
public class Person : INotifyPropertyChanged { private string _name;
public string Name
{
    get => _name;
    set
    {
        if (_name == value) return;

        _name = value;
        OnPropertyChanged(nameof(Name));
    }
}
public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged(string propertyName)
    => PropertyChanged?.Invoke(this, new(propertyName));
}
```
With [Notify], you simply declare the backing field:
```csharp 
public partial class Person 
{ 
    [Notify] 
    private string _name; 
} 
```
The generator creates the property, backing logic, and notification code automatically.
## 🎯 Benefits
No duplicated OnPropertyChanged code
Strongly-typed, compiler-generated properties
Easier refactoring (rename the field → property updates automatically)
Less noise in your view-models
## 🔎 Generated property
From the field:
```[Notify] private string _name; ```
The generator produces something like:
```csharp
public string Name { 
    get => _name; 
    set 
    { 
        if (_name == value) return;
        _name = value;
        OnPropertyChanged(nameof(Name));
    }
} 
```
The exact output may vary slightly, but the behavior is always the same.

## 📌 Notes
Works in any class marked partial
Requires INotifyPropertyChanged support in your base type
Generated code is placed in a .g.cs file — do not edit it

## 📄 How Notify Works 
When you mark a backing field with [Notify], the generator:

- Ensures the type implements INotifyPropertyChanged
- Generates a public property for the field
- Raises OnPropertyChanged when the value changes
- Skips notifications when the value hasn’t changed
- (Optional) generates strongly-typed change hooks


If hooks are enabled for the field, the following methods are available:

- OnXChanging(oldValue, ref newValue, ref cancel)
- OnXChanged(oldValue, newValue)

Use these to:

- validate and cancel changes
- modify the incoming value (coercion)
- react to changes after assignment

The final property change event is raised only after the value is successfully updated.