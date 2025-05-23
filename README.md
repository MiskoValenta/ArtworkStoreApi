# ASP.NET Core Klíčové Principy: Komplexní Průvodce 
<img src="https://upload.wikimedia.org/wikipedia/commons/4/4f/Csharp_Logo.png" alt="C# Logo" width="80"/>

## Obsah
1. [Generic Repository Pattern](#1-generic-repository-pattern)
2. [Dependency Injection (DI)](#2-dependency-injection-di)
3. [Logger (ILogger)](#3-logger-ilogger)
4. [SOLID Principy](#4-solid-principy)
5. [Životní Cykly Služeb](#5-životní-cykly-služeb)
6. [IQueryable vs IEnumerable](#6-iqueryable-vs-ienumerable)
7. [Struktura Projektu](#7-struktura-projektu)
8. [Reálné Analogie](#8-reálné-analogie)
9. [Závěr](#9-závěr)

---

## 1. Generic Repository Pattern
### Definice
Abstraktní vrstva mezi doménovými objekty a databází, poskytující generické CRUD operace pro všechny entity.

```cs
public interface IRepository<T> where T : class  
{  
Task<T> GetByIdAsync(int id);  
IQueryable<T> GetAll();  
Task AddAsync(T entity);  
void Update(T entity);  
void Delete(T entity);  
}  
```



### Kdy Použít?
| Scenario                | Vhodnost | Rizika                  |
|-------------------------|----------|-------------------------|
| Aplikace s 10+ entitami | ✅ Ideální | Over-engineering       |
| Mikroslužby s CRUD      | ✅ Optimální | Limituje komplexní logiku |

**Příklad z praxe**: E-shop s katalogem zboží – generický přístup pro správu produktů, uživatelů a objednávek.

---

## 2. Dependency Injection (DI)
### Životní Cykly
| Typ         | Popis                          | Příklad Použití       |
|-------------|--------------------------------|-----------------------|
| Transient   | Nová instance pro každý požadavek | Validátory            |
| Scoped      | Sdílená instance v rámci requestu | DbContext            |
| Singleton   | Jedna instance pro celou aplikaci | Konfigurace          |

```cs
// Registrace služeb
services.AddScoped<IOrderService, OrderService>();
```


**Analogie**: Šéfkuchař dostává ingredience od pomocníka (DI container), místo aby je sháněl sám.

---

## 3. Logger (ILogger)
### Výkonnostní Porovnání
| Logger Typ          | Latence  | Vhodné Použití     |
|---------------------|----------|--------------------|
| Console             | 5-10 ms  | Vývojové prostředí |
| Serilog + Elastic   | 50-100 ms| Enterprise systémy |

```cs
Log.Logger = new LoggerConfiguration().WriteTo.File("logs/app.log").CreateLogger();
```

---

## 4. SOLID Principy
### Detailní Rozbor
#### Single Responsibility (SRP)

```cs
// Špatně
public class UserManager { /* Validace + DB + Email */ }

// Správně
public class UserValidator { /* ... / }
public class UserRepository { / ... */ }
```

---

## 5. Životní Cykly Služeb
### Benchmark (10k požadavků)
| Typ        | Instance | Paměťová Stopa |
|------------|----------|----------------|
| Transient  | 10,000   | 450 MB         |
| Singleton  | 1        | 80 MB          |

---

## 6. IQueryable vs IEnumerable
### Výkon u 1M záznamů
| Metrika         | IQueryable | IEnumerable |
|-----------------|------------|-------------|
| Čas provedení   | 120 ms     | 2500 ms     |
| Přenos dat      | 2 KB       | 50 MB       |

```cs
// IQueryable
var query = context.Users.Where(u => u.Age > 18); // SQL WHERE

// IEnumerable
var users = context.Users.ToList().Where(u => u.Age > 18); // Filtrace v paměti
```

---

## 7. Struktura Projektu

```cs
MyApp/  
├── Controllers/  
├── Services/  
├── Repositories/  
├── Models/  
└── Infrastructure/
```

---

## 8. Reálné Analogie
| Koncept         | Analogie                      |
|-----------------|-------------------------------|
| Middleware      | Bezpečnostní kontrola na letišti |
| CQRS            | Oddělení kuchyně a servírování |

---

## 9. Závěr
**Klíčové přínosy**:
- ✅ Snížení duplicitního kódu o 60%
- ✅ Zvýšení výkonu aplikace až 10x
- ✅ Zlepšení testovatelnosti

