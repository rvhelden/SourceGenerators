---
marp: true

theme: gaia
class: lead
backgroundColor: #fff
backgroundImage: url('https://marp.app/assets/hero-background.svg')
---

![bg left fit](https://seinecle.github.io/gephi-tutorials/generated-html/working-from-the-source-en.adoc/images/use-the-source.jpg)
# Source Generators

---

# Waarom?

- Runtime reflectie vervangen door compile-time generatie
- Meta-programmeren
- Externe API typed genereren

---

# .NET 8

---

# C# 12 
## Primary constructors


```csharp
public class Person(string firstName, string lastName)
{
    public DateTime DateOfBirth { get; init; }

    public Person(string firstName, string lastName, DateTime dateOfBirth) 
        : this(firstName, lastName)
    {
        DateOfBirth = dateOfBirth;
    }

    public override string ToString() => $"{firstName} {lastName}";
}
```

---
## Alias any type

```csharp
using FirstName = System.String;
using LastName = System.String;
using Person = (string FirstName, string LastName);

Person person = ("John", "Doe");