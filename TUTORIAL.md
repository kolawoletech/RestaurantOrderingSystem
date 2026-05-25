# Restaurant Ordering System — Tutorial Workbook

> A complete, enterprise-style C# WinForms (.NET Framework 4.7.2) project
> written for students moving from **beginner** to **intermediate** level.
> Used as the running example for Eduvos *Basic C# Programming*.

---

## 1. Project overview

You are building a small but realistic **point-of-sale system** for a
restaurant. Two staff roles use the application:

| Role         | What they can do                                                       |
| ------------ | ---------------------------------------------------------------------- |
| **Admin**    | Manage the menu (add / update / delete / search meals, manage stock).  |
| **Cashier**  | Take customer orders, calculate totals, print receipts, run kitchen.   |

The system is **deliberately structured like a real enterprise project**:
data, models, services, interfaces, exceptions, utilities, and forms are
kept in separate folders/namespaces. The persistence layer is in-memory
today but can be swapped to SQL Server later **without changing any of
the form code**.

> Demo accounts (seeded in [Data/UserRepository.cs](Data/UserRepository.cs)):
> `admin / admin123`  •  `cashier / cashier123`

---

## 2. Architecture (text diagram)

```
+---------------------------------------------------------------+
|                        PRESENTATION                           |
|   SplashForm  ->  LoginForm  ->  DashboardForm                |
|                                  |                            |
|                  +---------------+---------------+            |
|                  v               v               v            |
|       MenuManagementForm   OrderingForm    KitchenForm        |
+----------------------+-----------------+----------------------+
                       |                 |
                       v                 v
+---------------------------------------------------------------+
|                          SERVICES                             |
|     AuthService    MenuService    OrderService                |
|              (wired through ServiceContainer)                 |
+----------------------+-----------------+----------------------+
                       |                 |
                       v                 v
+---------------------------------------------------------------+
|                            DATA                               |
|   UserRepository    MenuRepository    OrderRepository         |
|             (in-memory today, SQL tomorrow)                   |
+----------------------+----------------------------------------+
                       |
                       v
+---------------------------------------------------------------+
|              MODELS  (POCO domain objects)                    |
|   Person -> Employee -> Admin / Cashier                       |
|   MenuItem (abstract) -> FoodItem / DrinkItem                 |
|   Order (with OrderStatus lifecycle), OrderItem               |
|                                                               |
|              INTERFACES        EXCEPTIONS                     |
|   IOrderable                   OutOfStockException            |
|   IRepository<T>               InvalidOrderStateException     |
|   IAuthService                 InvalidLoginException          |
+---------------------------------------------------------------+
```

Why this layering matters:

- **Forms** only know about **Services** (not about repositories or lists).
- **Services** only know about **Repositories** (interface, not concrete).
- **Repositories** own the data.
- **Models** are pure C# classes with no UI references.
- **Exceptions** are domain-specific types in their own namespace — the UI
  catches them by type, not by parsing strings.

This is **separation of concerns**. Each layer can be replaced or
unit-tested in isolation.

---

## 3. Folder structure

```
RestaurantOrderingSystem/
│
├── Program.cs                  ← application entry point + navigation flow
├── App.config                  ← .NET configuration file
├── RestaurantOrderingSystem.csproj
│
├── Forms/                      ← every screen the user sees
│   ├── SplashForm.cs / .Designer.cs
│   ├── LoginForm.cs
│   ├── DashboardForm.cs
│   ├── MenuManagementForm.cs
│   ├── OrderingForm.cs
│   ├── KitchenForm.cs          ← order-status workflow (Pending → Completed)
│   └── ReceiptPreviewForm.cs
│
├── Models/                     ← domain objects (pure C# — no UI)
│   ├── Person.cs               (abstract)
│   ├── Employee.cs             (abstract)
│   ├── Admin.cs   /  Cashier.cs
│   ├── MealCategory.cs         (enum)
│   ├── OrderStatus.cs          (enum — Pending/Preparing/Ready/Completed/Cancelled)
│   ├── MenuItem.cs             (abstract; carries stock + low-stock threshold)
│   ├── FoodItem.cs  /  DrinkItem.cs
│   ├── OrderItem.cs
│   └── Order.cs                (state machine + stock reservation)
│
├── Interfaces/                 ← contracts only
│   ├── IOrderable.cs
│   ├── IRepository.cs          (generic CRUD)
│   └── IAuthService.cs
│
├── Exceptions/                 ← custom exception types
│   ├── OutOfStockException.cs
│   ├── InvalidOrderStateException.cs
│   └── InvalidLoginException.cs
│
├── Data/                       ← persistence layer (in-memory)
│   ├── UserRepository.cs
│   ├── MenuRepository.cs       (seeds stock + thresholds)
│   └── OrderRepository.cs
│
├── Services/                   ← business logic
│   ├── AuthService.cs
│   ├── MenuService.cs          (overloaded AddFood / AddDrink)
│   ├── OrderService.cs         (Submit, MarkReady, MarkCompleted, Cancel)
│   └── ServiceContainer.cs     ← composition root
│
├── Utilities/                  ← reusable helpers
│   ├── Validator.cs
│   └── ReceiptFormatter.cs
│
├── Resources/                  ← icons, images, branding
└── Properties/                 ← AssemblyInfo + project resources
```

---

## 4. Class diagram (text form)

```
            ┌──────────────┐
            │  «abstract»  │
            │   Person     │
            ├──────────────┤
            │ FullName     │
            │ Username     │
            │ + Describe() │
            └──────┬───────┘
                   │
            ┌──────┴───────┐
            │  «abstract»  │
            │  Employee    │
            ├──────────────┤
            │ EmployeeId   │
            │ Password     │
            │ VerifyPwd()  │
            └──────┬───────┘
        ┌──────────┴──────────┐
        │                     │
   ┌────┴────┐           ┌────┴────┐
   │  Admin  │           │ Cashier │
   └─────────┘           └─────────┘


            ┌────────────────────────┐         «interface»
            │     «abstract»         │◀────────  IOrderable
            │     MenuItem           │
            ├────────────────────────┤
            │ MealId, MealName       │
            │ Category, Price        │
            │ IsAvailable            │
            │ StockQuantity (priv.set)
            │ LowStockThreshold      │
            │ + IsInStock            │
            │ + IsLowStock           │
            │ + IsOrderable          │
            │ + UpdateStock(delta)   │  throws OutOfStockException
            │ + GetLineTotal(qty)    │
            └──────────┬─────────────┘
              ┌────────┴────────┐
              │                 │
        ┌─────┴─────┐     ┌─────┴─────┐
        │ FoodItem  │     │ DrinkItem │
        │ + IsVeg   │     │ + Volume  │
        └───────────┘     └───────────┘


   ┌─────────────────────────┐    1   *   ┌────────────┐    *  1   ┌──────────┐
   │   Order                 │◇──────────│  OrderItem │──────────▶│ MenuItem │
   ├─────────────────────────┤            └────────────┘           └──────────┘
   │ OrderNumber, Items      │
   │ Status: OrderStatus     │
   │ CreatedAt, SubmittedAt, │
   │ CompletedAt             │
   │ + AddItem / RemoveItem  │  throw OutOfStockException
   │ + MarkReady             │  throw InvalidOrderStateException
   │ + MarkCompleted, Cancel │
   └─────────────────────────┘

         OrderStatus lifecycle:
            Pending ──▶ Preparing ──▶ Ready ──▶ Completed
                \_______________ Cancelled
```

---

## 5. Development roadmap (phases)

| Phase | Goal                                       | Output                          |
|------:|--------------------------------------------|---------------------------------|
| 1     | Project skeleton + folder structure        | Empty solution, splash form     |
| 2     | Models + Interfaces                        | OOP hierarchy compiles          |
| 3     | Data layer + Services                      | Repos seeded with sample data   |
| 4     | Login + Dashboard                          | Sign-in flow, role routing      |
| 5     | Menu management (Admin)                    | CRUD grid + form                |
| 6     | Ordering + Receipt (Cashier)               | Add to order, totals, receipt   |
| 7     | Order status + Kitchen workflow            | State machine, KitchenForm      |
| 8     | Inventory                                  | Stock + thresholds, reservation |
| 9     | Custom exceptions + try/catch in UI        | Type-specific error handling    |
| 10    | (Stretch) SQL Server backend, reporting    | Real persistence                |

---

## 6. Visual Studio setup

1. **File → New → Project → Windows Forms App (.NET Framework)**.
   Pick **.NET Framework 4.7.2**, name it `RestaurantOrderingSystem`.
2. Right-click the project → **Properties → Application → Target framework**:
   confirm 4.7.2.
3. In Solution Explorer right-click the project and **Add → New Folder** for
   each of: `Forms`, `Models`, `Interfaces`, `Exceptions`, `Data`,
   `Services`, `Utilities`, `Resources`.
4. Drag `SplashForm` into `Forms/` (Visual Studio will fix its namespace).
5. For each `.cs` file in this tutorial, **Add → Existing Item** (or
   **Add → Class**) and paste in the contents.
6. **Build → Build Solution** (Ctrl + Shift + B). The output window should
   say *Build: 1 succeeded*.
7. Press **F5** to run.

---

## 7. Form-by-form walkthrough

### 7.1 [SplashForm](Forms/SplashForm.cs)

- Borderless, dark-themed window shown for ~2 seconds at startup.
- Uses a `System.Windows.Forms.Timer` — once it ticks, the form closes
  with `DialogResult.OK` and `Program.cs` moves on to the login screen.
- Demonstrates: **timers**, **partial classes** (`SplashForm.cs` +
  `SplashForm.Designer.cs`), and the **OnShown / OnFormClosed** lifecycle
  hooks instead of wiring `Load` event handlers.

### 7.2 [LoginForm](Forms/LoginForm.cs)

- Receives the shared `ServiceContainer` so it can call `Auth.Authenticate(...)`.
- On success, stores the authenticated `Employee` in `ctx.CurrentUser` and
  closes with `DialogResult.OK`.
- On failure, `AuthService` throws **`InvalidLoginException`** which the
  form catches in a **specific `catch` clause before** the general
  `catch (Exception)` — the PDF §2.4.1 ordering rule made concrete.
- Demonstrates: **constructor injection**, **event handlers**, **custom
  exception handling**, **AcceptButton** for the Enter key.

### 7.3 [DashboardForm](Forms/DashboardForm.cs)

- Shows the user's name and role in a coloured header strip.
- Tiles are built with a `FlowLayoutPanel` so they reflow on resize.
- The **Menu management** tile is only added if `ctx.CurrentUser is Admin`
  — this is **role-based navigation** in action.
- A **Kitchen queue** tile opens the new `KitchenForm`.
- Includes a `MenuStrip` (`File`, `Go`, `Help`) for keyboard-friendly access.
- Sign-out returns `DialogResult.Retry` which `Program.cs` reads to loop
  back to the login screen.

### 7.4 [MenuManagementForm](Forms/MenuManagementForm.cs)

- Combines a `DataGridView`, `TextBox`, `ComboBox`, `NumericUpDown`,
  `CheckBox`, and a search box.
- Selecting a row populates the form (with `txtId.ReadOnly = true` so the
  primary key can't be edited).
- The `ComboBox` is bound to the `MealCategory` enum using
  `Enum.GetValues(typeof(MealCategory))`.
- When the user picks `Drink`, the **vegetarian** checkbox hides and the
  **volume** field appears — demonstrating dynamic UI based on data type.
- New: **Stock qty** and **Low-stock threshold** inputs flow through to
  the overloaded `MenuService.AddFood` / `AddDrink` methods.
- Grid colours rows that are **out of stock** (red) or **low stock**
  (amber) so the admin sees them at a glance.

### 7.5 [OrderingForm](Forms/OrderingForm.cs)

- Builds an `Order` in memory while the cashier picks meals.
- Uses an inner `MenuItemBox` class to override `ToString()` for the
  combo without polluting the domain model — combo entries show live
  stock count.
- Subtotal / VAT / Total are recomputed by reading properties on the
  `Order` itself — there's no duplicated math in the form.
- Submitting the order persists it (via `OrderService`) and opens
  `ReceiptPreviewForm` showing the formatted receipt.
- **Three-tier catch block** on `Add`: `OutOfStockException` first,
  then `InvalidOrderStateException`, then generic `Exception` — each
  with its own message style.
- Submit uses **`try / finally`** (PDF §2.4.2) to re-enable the button
  whether the call succeeded or threw.
- Opens with a **using alias** at the top of the file
  (`using MenuItem = RestaurantOrderingSystem.Models.MenuItem;`) — the
  PDF §2.5.2 alias pattern, used here to resolve the name clash with
  `System.Windows.Forms.MenuItem`.

### 7.6 [KitchenForm](Forms/KitchenForm.cs) *(new)*

- Lists every order with a filter for `OrderStatus`. Rows are colour-coded
  by status (yellow = Pending, blue = Preparing, green = Ready, red =
  Cancelled).
- Three action buttons — **Mark Ready**, **Mark Completed**, **Cancel
  Order** — each calling the matching method on `OrderService`.
- Illegal transitions throw `InvalidOrderStateException`, which the form
  catches **specifically** and turns into a friendly message that
  reminds the user of the lifecycle flow.
- Demonstrates: a **state machine**, a generic `Action<>` delegate
  parameter to remove duplicated `try/catch` blocks, and exception-type-
  based UI feedback.

### 7.7 [ReceiptPreviewForm](Forms/ReceiptPreviewForm.cs)

- Simple read-only `TextBox` in a monospaced font to mimic a paper receipt.
- The actual formatting lives in [`ReceiptFormatter`](Utilities/ReceiptFormatter.cs)
  — a static utility class that the form never has to know how to print.
- Now prints the order's `Status` line as part of the header.

---

## 8. OOP concepts: where they appear

| Concept                  | Where to look                                                                                                  |
| ------------------------ | -------------------------------------------------------------------------------------------------------------- |
| **Abstract class**       | [Person](Models/Person.cs), [Employee](Models/Employee.cs), [MenuItem](Models/MenuItem.cs)                     |
| **Inheritance**          | `Admin : Employee : Person`, `FoodItem : MenuItem`, `OutOfStockException : Exception`                          |
| **Polymorphism**         | `RoleName`, `DisplayLabel` overridden per subclass; UI catches by exception subtype                            |
| **Interface**            | [IOrderable](Interfaces/IOrderable.cs), [IRepository<T>](Interfaces/IRepository.cs), [IAuthService](Interfaces/IAuthService.cs) |
| **Encapsulation**        | `private set` properties; `Order.Status` / `MenuItem.StockQuantity` change only via controlled methods         |
| **State machine**        | `OrderStatus` + `Order.MarkReady / MarkCompleted / Cancel` with `RequireStatus` guards                         |
| **Constructors**         | All models validate in their ctor — invariants enforced on day one                                             |
| **Method overloading**   | [MenuService.AddFood / AddDrink](Services/MenuService.cs) — two real overloads per method (L10 §2.3.1)         |
| **Constructor overloads**| [InvalidLoginException](Exceptions/InvalidLoginException.cs), [InvalidOrderStateException](Exceptions/InvalidOrderStateException.cs) |
| **Custom exceptions**    | [Exceptions/](Exceptions/) folder — three custom types inheriting from `Exception` (L11 §2.4.3)                |
| **Multiple catch order** | [OrderingForm.BtnAdd_Click](Forms/OrderingForm.cs), [KitchenForm.Apply](Forms/KitchenForm.cs), [LoginForm.BtnLogin_Click](Forms/LoginForm.cs) |
| **finally**              | [OrderingForm.BtnSubmit_Click](Forms/OrderingForm.cs) re-enables Submit button whatever happens                 |
| **Namespaces**           | `RestaurantOrderingSystem.{Models,Services,Data,Forms,Interfaces,Exceptions,Utilities}` (L12 §2.5.1)           |
| **Using alias**          | `using MenuItem = RestaurantOrderingSystem.Models.MenuItem;` in OrderingForm/MenuManagementForm (L12 §2.5.2)   |
| **Static**               | [ReceiptFormatter](Utilities/ReceiptFormatter.cs) is `static`                                                  |
| **Generics**             | `IRepository<T>` is generic — same contract for any entity type                                                |
| **Enum**                 | [MealCategory](Models/MealCategory.cs), [OrderStatus](Models/OrderStatus.cs)                                   |
| **Event-driven**         | Every form: `Click`, `SelectionChanged`, `TextChanged`, `Tick`                                                 |

### Example: polymorphism in action

```csharp
MenuItem item = new DrinkItem("D001", "Coca-Cola", 18.50m, true, 330);
Console.WriteLine(item.DisplayLabel);     // "Coca-Cola (330ml)"

item = new FoodItem("F003", "Caesar Salad", MealCategory.Starter, 55m, true, true);
Console.WriteLine(item.DisplayLabel);     // "Caesar Salad (V)"
```

The variable's type is `MenuItem`, but the **subclass** decides what to
print. That is polymorphism.

### Example: method overloading (Lesson 10)

```csharp
// Two real overloads — same name, different signatures.
public void AddFood(string id, string name, MealCategory category,
                    decimal price, bool available, bool vegetarian)
{
    AddFood(id, name, category, price, available, vegetarian, 0, 5);
}

public void AddFood(string id, string name, MealCategory category,
                    decimal price, bool available, bool vegetarian,
                    int stockQuantity, int lowStockThreshold)
{
    _repo.Add(new FoodItem(id, name, category, price, available,
                           vegetarian, stockQuantity, lowStockThreshold));
}
```

The compiler picks which `AddFood` to invoke based purely on the
arguments at the call site. The short overload delegates to the long
one so the actual work is written once.

---

## 9. Exception handling philosophy

Four rules (built on PDF §2.4):

1. **Throw at the boundary** with a **specific custom exception** when
   the program enters an impossible state. The three custom types live
   in [Exceptions/](Exceptions/):

   - `OutOfStockException` — `MenuItem.UpdateStock` and `Order.AddItem`
     refuse to push stock below zero.
   - `InvalidOrderStateException` — `Order.MarkReady / MarkCompleted /
     Cancel` reject illegal transitions.
   - `InvalidLoginException` — `AuthService.Authenticate` throws instead
     of returning `null`, so callers can't "forget" to handle bad login.

2. **Catch at the UI** with multiple `catch` clauses ordered
   **most-specific first**:

```csharp
try
{
    _currentOrder.AddItem(box.Item, (int)numQty.Value);
}
catch (OutOfStockException ex)
{
    MessageBox.Show(ex.Message + "\nAsk the kitchen to restock.",
                    "Out of stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
catch (InvalidOrderStateException ex)
{
    MessageBox.Show(ex.Message, "Order locked", ...);
}
catch (Exception ex)
{
    MessageBox.Show(ex.Message, "Cannot add item", ...);
}
```

3. **Use `finally` for cleanup**. `OrderingForm.BtnSubmit_Click` disables
   the Submit button before the work starts and re-enables it inside
   `finally`, so a thrown exception can't leave the UI permanently
   stuck.

4. **Validate early via a helper**. [Validator](Utilities/Validator.cs)
   centralises the "is this empty / is this a positive number" checks so
   form code reads cleanly.

---

## 10. Order lifecycle (state machine)

`OrderStatus` is an enum with five values. `Order` exposes `Status` as
`public get; private set;` — callers can read it but cannot bypass the
transition rules.

```
   Pending  ──Submit()──▶  Preparing  ──MarkReady()──▶  Ready  ──MarkCompleted()──▶  Completed
       │                                                                │
       └──────────────────── Cancel() ─────────────────── Cancelled ◀───┘
```

Key invariants enforced inside `Order`:

- `AddItem / RemoveItem / Clear` only work while `Status == Pending`.
- `MarkReady` requires `Preparing`; `MarkCompleted` requires `Ready`.
- `Cancel` is rejected on `Completed` / `Cancelled`, and only **refunds
  stock** when the order was still `Pending` (because the kitchen
  hadn't used any ingredients yet).

This is the textbook benefit of encapsulation: the object guarantees
its own correctness, so no form or future API can corrupt it.

---

## 11. Inventory management

`MenuItem` carries two stock fields (read-only externally; set via
`UpdateStock(delta)`):

| Property             | Meaning                                            |
| -------------------- | -------------------------------------------------- |
| `StockQuantity`      | How many units the kitchen has on hand.            |
| `LowStockThreshold`  | When `StockQuantity` drops to this, UI warns.      |
| `IsInStock`          | `StockQuantity > 0`                                |
| `IsLowStock`         | `StockQuantity <= LowStockThreshold`               |
| `IsOrderable`        | `IsAvailable && IsInStock` — used by the cashier.  |

`Order.AddItem` reserves stock by calling `UpdateStock(-qty)`.
`RemoveItem` and `Clear` return it. `Cancel` returns it **only** if the
order was still `Pending`. `MenuService.Restock(id, qty)` lets the
admin top items back up.

The cashier's combo box uses `MenuService.GetAvailable()`, which checks
`IsOrderable`, so an out-of-stock meal disappears from the picker
automatically.

---

## 12. Data storage and SQL migration plan

Right now the system stores data in `List<T>` inside three repository
classes. To switch to SQL Server **you would not touch a single form**:

1. Create a `SqlMenuRepository : IRepository<MenuItem>` that calls
   `SqlConnection` / `SqlCommand` (or Entity Framework).
2. In `ServiceContainer`, replace `new MenuRepository()` with
   `new SqlMenuRepository(connectionString)`.
3. Done. Every form, every service, keeps compiling.

That is the entire point of programming to **interfaces** rather than
concrete classes.

---

## 13. Source control (Git)

```bash
git init
git add .
git commit -m "feat: phase 2 — models, services, full UI"
git branch -M main
git remote add origin https://github.com/<you>/restaurant-ordering-system.git
git push -u origin main
```

Tips for students:

- Commit **per phase**, not at the end. A reader should be able to walk
  the history and see your project grow.
- Add a `.gitignore` (already in this repo) so `bin/`, `obj/`, and `.vs/`
  never reach GitHub.
- Use **descriptive commit messages** in the imperative mood:
  ✅ `add login form with role routing`
  ❌ `update`

---

## 14. Common student mistakes

1. **Putting business logic in `Form1_Load`.** Forms should call services,
   not contain SQL or list mutation.
2. **Re-creating repositories in every form.** Use a single
   `ServiceContainer` passed into each form's constructor.
3. **Swallowing exceptions silently.** A `catch { }` block with no message
   hides bugs forever — at minimum, show the message.
4. **Catching `Exception` before the specific type.** The compiler won't
   stop you, but the specific `catch` becomes unreachable. PDF §2.4.1:
   *specific first, general last*.
5. **Hard-coding prices in the form code.** Prices live on the model, not
   on the screen.
6. **Forgetting `using` blocks.** Modal forms shown with `ShowDialog()`
   should be wrapped in `using` so their resources are disposed.
7. **Editing the designer file by hand.** For complex forms in this project
   we built the UI **in code** (`BuildUi()`) on purpose — easier to read
   and source-control than `.Designer.cs`.
8. **Bypassing a state machine.** Don't add a `public set` to
   `Order.Status` "just so the form can change it." The whole point of
   the private setter is that the form can't.

---

## 15. Student exercises

1. Add a third role `Manager` that can see both menu management AND a new
   "sales report" tile. (Adds: subclass, role check, new form.)
2. ~~Add stock and prevent ordering when zero.~~ ✅ *done — see §11.*
3. Add a `Discount` field to `OrderItem` and adjust `Order.Subtotal`.
4. Persist meals to a JSON file on disk so changes survive a restart.
   Implement `JsonMenuRepository : IRepository<MenuItem>` and swap it in.
5. Add unit tests for `Order` (subtotal, tax, total, lifecycle
   transitions) using MSTest. Bonus: assert that `MarkCompleted` throws
   `InvalidOrderStateException` when called on a `Pending` order.
6. Add a fourth `OrderStatus` value `OnHold` and decide where it fits
   in the lifecycle. Update `KitchenForm` accordingly.

## Stretch goals

- Migrate to **SQL Server LocalDB** with Entity Framework 6.
- Add a `Customer` model and an order-history screen.
- Implement password hashing (PBKDF2) in `Employee`.
- Theme the app with light / dark mode toggle.
- Print the receipt to a real printer using `PrintDocument`.

---

## 16. Deployment / handing in

1. **Clean Solution** (Build → Clean).
2. Switch to **Release** configuration.
3. **Build** — the `.exe` lives in `bin\Release\`.
4. Zip the entire solution folder **excluding** `bin/`, `obj/`, `.vs/`.
5. Push to GitHub. Tag the submission with `git tag v1.0`.

---

## 17. Quick reference of WinForms controls used

| Control          | First appearance                          | What it teaches                |
| ---------------- | ----------------------------------------- | ------------------------------ |
| `Label`          | every form                                | display-only text              |
| `TextBox`        | LoginForm, MenuManagementForm             | user text input                |
| `Button`         | every form                                | `Click` events                 |
| `MenuStrip`      | DashboardForm                             | classic top-of-window menus    |
| `Panel`          | DashboardForm header, MenuManagement form | grouping + colour blocks       |
| `FlowLayoutPanel`| DashboardForm tiles                       | automatic layout/reflow        |
| `DataGridView`   | MenuManagementForm, OrderingForm, KitchenForm | tabular data + row colouring |
| `ComboBox`       | MenuManagementForm, OrderingForm, KitchenForm | enum / list selection      |
| `NumericUpDown`  | MenuManagementForm, OrderingForm          | constrained numeric input      |
| `CheckBox`       | MenuManagementForm                        | boolean fields                 |
| `Timer`          | SplashForm                                | scheduled work on the UI thread |

---

## 18. Mapping to the Eduvos curriculum (Basic C# Programming)

The project deliberately demonstrates concepts as students learn them.
Browse the matching code with the table below.

### Week 4 — Lessons 10, 11, 12

| Lesson | Topic                              | Where to see it in this project                                                                                                                                  |
| -----: | ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **10** | Method overloading (§2.3.1)        | [MenuService.AddFood / AddDrink](Services/MenuService.cs) — two genuine overloads each (not default arguments).                                                  |
| **10** | Constructor overloading            | [InvalidLoginException](Exceptions/InvalidLoginException.cs) (3 ctors), [InvalidOrderStateException](Exceptions/InvalidOrderStateException.cs) (2 ctors).        |
| **11** | `try / catch` ordering (§2.4.1)    | [OrderingForm.BtnAdd_Click](Forms/OrderingForm.cs), [KitchenForm.Apply](Forms/KitchenForm.cs), [LoginForm.BtnLogin_Click](Forms/LoginForm.cs) — specific → general. |
| **11** | `finally` block (§2.4.2)           | [OrderingForm.BtnSubmit_Click](Forms/OrderingForm.cs) — Submit button always re-enabled.                                                                          |
| **11** | Custom exceptions (§2.4.3)         | [Exceptions/](Exceptions/) — three custom types inheriting from `Exception`, mirroring the PDF's `SillyException` pattern.                                       |
| **12** | Declaring namespaces (§2.5.1)      | One namespace per folder; `RestaurantOrderingSystem.Exceptions` introduced specifically for this lesson.                                                         |
| **12** | `using` directive + alias (§2.5.2) | [OrderingForm.cs:11](Forms/OrderingForm.cs) — `using MenuItem = RestaurantOrderingSystem.Models.MenuItem;` resolves a name clash with `System.Windows.Forms.MenuItem`. |

### Earlier-week material (used as supporting infrastructure)

- **Enums** — `MealCategory`, `OrderStatus`.
- **Inheritance + abstract classes** — `Person → Employee → Admin/Cashier`,
  `MenuItem → FoodItem/DrinkItem`.
- **Encapsulation** — private setters everywhere; `Order.Status` and
  `MenuItem.StockQuantity` are the strongest examples.
- **Interfaces** — `IOrderable`, `IRepository<T>`, `IAuthService`.

---

Good luck — and remember: **clean code is just kind code for the next
person reading it (often you, in three weeks).**
