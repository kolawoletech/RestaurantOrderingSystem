# Restaurant Ordering System — Tutorial Workbook

> A complete, enterprise-style C# WinForms (.NET Framework 4.7.2) project
> written for students moving from **beginner** to **intermediate** level.

---

## 1. Project overview

You are building a small but realistic **point-of-sale system** for a
restaurant. Two staff roles use the application:

| Role         | What they can do                                          |
| ------------ | --------------------------------------------------------- |
| **Admin**    | Manage the menu (add / update / delete / search meals).   |
| **Cashier**  | Take customer orders, calculate totals, print receipts.   |

The system is **deliberately structured like a real enterprise project**:
data, models, services, interfaces, utilities, and forms are kept in
separate folders. The persistence layer is in-memory today but can be
swapped to SQL Server later **without changing any of the form code**.

> Demo accounts (seeded in [Data/UserRepository.cs](Data/UserRepository.cs)):
> `admin / admin123`  •  `cashier / cashier123`

---

## 2. Architecture (text diagram)

```
+---------------------------------------------------------------+
|                        PRESENTATION                           |
|   SplashForm  ->  LoginForm  ->  DashboardForm                |
|                                  |                            |
|                  +---------------+----------------+           |
|                  v                                v           |
|        MenuManagementForm                  OrderingForm       |
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
|   Order, OrderItem                                            |
|                                                               |
|              INTERFACES                                       |
|   IOrderable, IRepository<T>, IAuthService                    |
+---------------------------------------------------------------+
```

Why this layering matters:

- **Forms** only know about **Services** (not about repositories or lists).
- **Services** only know about **Repositories** (interface, not concrete).
- **Repositories** own the data.
- **Models** are pure C# classes with no UI references.

This is **separation of concerns**. Each layer can be replaced or unit-tested
in isolation.

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
│   └── ReceiptPreviewForm.cs
│
├── Models/                     ← domain objects (pure C# — no UI)
│   ├── Person.cs               (abstract)
│   ├── Employee.cs             (abstract)
│   ├── Admin.cs   /  Cashier.cs
│   ├── MealCategory.cs         (enum)
│   ├── MenuItem.cs             (abstract)
│   ├── FoodItem.cs  /  DrinkItem.cs
│   ├── OrderItem.cs
│   └── Order.cs
│
├── Interfaces/                 ← contracts only
│   ├── IOrderable.cs
│   ├── IRepository.cs          (generic CRUD)
│   └── IAuthService.cs
│
├── Data/                       ← persistence layer (in-memory)
│   ├── UserRepository.cs
│   ├── MenuRepository.cs
│   └── OrderRepository.cs
│
├── Services/                   ← business logic
│   ├── AuthService.cs
│   ├── MenuService.cs
│   ├── OrderService.cs
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


            ┌────────────────────┐         «interface»
            │     «abstract»     │◀────────  IOrderable
            │     MenuItem       │
            ├────────────────────┤
            │ MealId, MealName   │
            │ Category, Price    │
            │ IsAvailable        │
            │ + GetLineTotal()   │
            └──────────┬─────────┘
              ┌────────┴────────┐
              │                 │
        ┌─────┴─────┐     ┌─────┴─────┐
        │ FoodItem  │     │ DrinkItem │
        │ + IsVeg   │     │ + Volume  │
        └───────────┘     └───────────┘


   ┌─────────────┐    1   *   ┌────────────┐    *  1   ┌──────────┐
   │   Order     │◇──────────│  OrderItem │──────────▶│ MenuItem │
   └─────────────┘            └────────────┘           └──────────┘
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
| 7     | Polish: validation, exception handling     | Friendly errors, no crashes     |
| 8     | (Stretch) SQL Server backend, reporting    | Real persistence                |

---

## 6. Visual Studio setup

1. **File → New → Project → Windows Forms App (.NET Framework)**.
   Pick **.NET Framework 4.7.2**, name it `RestaurantOrderingSystem`.
2. Right-click the project → **Properties → Application → Target framework**:
   confirm 4.7.2.
3. In Solution Explorer right-click the project and **Add → New Folder** for
   each of: `Forms`, `Models`, `Interfaces`, `Data`, `Services`, `Utilities`,
   `Resources`.
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
- On failure, displays an error label rather than crashing.
- Demonstrates: **constructor injection**, **event handlers**, **validation
  via exception handling**, **AcceptButton** for the Enter key.

### 7.3 [DashboardForm](Forms/DashboardForm.cs)

- Shows the user's name and role in a coloured header strip.
- Tiles are built with a `FlowLayoutPanel` so they reflow on resize.
- The **Menu management** tile is only added if `ctx.CurrentUser is Admin`
  — this is **role-based navigation** in action.
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
- All persistence goes through `MenuService` → `MenuRepository`; the form
  never touches a `List<T>` directly.

### 7.5 [OrderingForm](Forms/OrderingForm.cs)

- Builds an `Order` in memory while the cashier picks meals.
- Uses an inner `MenuItemBox` class to override `ToString()` for the
  combo without polluting the domain model.
- Subtotal / VAT / Total are recomputed by reading properties on the
  `Order` itself — there's no duplicated math in the form.
- Submitting the order persists it (via `OrderService`) and opens
  `ReceiptPreviewForm` showing the formatted receipt.

### 7.6 [ReceiptPreviewForm](Forms/ReceiptPreviewForm.cs)

- Simple read-only `TextBox` in a monospaced font to mimic a paper receipt.
- The actual formatting lives in [`ReceiptFormatter`](Utilities/ReceiptFormatter.cs)
  — a static utility class that the form never has to know how to print.

---

## 8. OOP concepts: where they appear

| Concept             | Where to look                                                     |
| ------------------- | ----------------------------------------------------------------- |
| **Abstract class**  | [Person](Models/Person.cs), [Employee](Models/Employee.cs), [MenuItem](Models/MenuItem.cs) |
| **Inheritance**     | `Admin : Employee : Person`, `FoodItem : MenuItem`                |
| **Polymorphism**    | `RoleName`, `DisplayLabel` overridden per subclass                 |
| **Interface**       | [IOrderable](Interfaces/IOrderable.cs), [IRepository<T>](Interfaces/IRepository.cs) |
| **Encapsulation**   | `private set` properties + validation in constructors             |
| **Constructors**    | All models validate in their ctor — invariants enforced on day one |
| **Static**          | [ReceiptFormatter](Utilities/ReceiptFormatter.cs) is `static`     |
| **Lists**           | Used throughout the repositories — see [MenuRepository](Data/MenuRepository.cs) |
| **Generics**        | `IRepository<T>` is generic — same contract for any entity type   |
| **Enum**            | [MealCategory](Models/MealCategory.cs)                            |
| **Event-driven**    | Every form: `Click`, `SelectionChanged`, `TextChanged`, `Tick`    |

### Example: polymorphism in action

```csharp
MenuItem item = new DrinkItem("D001", "Coca-Cola", 18.50m, true, 330);
Console.WriteLine(item.DisplayLabel);     // "Coca-Cola (330ml)"

item = new FoodItem("F003", "Caesar Salad", MealCategory.Starter, 55m, true, true);
Console.WriteLine(item.DisplayLabel);     // "Caesar Salad (V)"
```

The variable's type is `MenuItem`, but the **subclass** decides what to
print. That is polymorphism.

---

## 9. Exception handling philosophy

We follow three rules:

1. **Throw at the boundary**. Constructors and services throw with clear
   messages when something is wrong (`ArgumentException`,
   `InvalidOperationException`).
2. **Catch at the UI**. Forms wrap calls in `try { ... } catch (Exception ex)`
   and display the message to the user via `MessageBox` or a status label.
   The app must never crash from a bad input.
3. **Validate early via a helper**. [Validator](Utilities/Validator.cs)
   centralises the "is this empty / is this a positive number" checks so
   form code reads cleanly:

```csharp
try
{
    Validator.RequireText(txtName.Text, "Name");
    _ctx.MenuService.AddFood(...);
}
catch (Exception ex)
{
    MessageBox.Show(this, ex.Message, "Cannot add", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
```

---

## 10. Data storage and SQL migration plan

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

## 11. Source control (Git)

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

## 12. Common student mistakes

1. **Putting business logic in `Form1_Load`.** Forms should call services,
   not contain SQL or list mutation.
2. **Re-creating repositories in every form.** Use a single
   `ServiceContainer` passed into each form's constructor.
3. **Swallowing exceptions silently.** A `catch { }` block with no message
   hides bugs forever — at minimum, show the message.
4. **Hard-coding prices in the form code.** Prices live on the model, not
   on the screen.
5. **Forgetting `using` blocks.** Modal forms shown with `ShowDialog()`
   should be wrapped in `using` so their resources are disposed.
6. **Editing the designer file by hand.** For complex forms in this project
   we built the UI **in code** (`BuildUi()`) on purpose — easier to read
   and source-control than `.Designer.cs`.

---

## 13. Student exercises

1. Add a third role `Manager` that can see both menu management AND a new
   "sales report" tile. (Adds: subclass, role check, new form.)
2. Add a `Quantity in stock` field to `MenuItem`, decrement it when a
   meal is added to an order, and prevent ordering when it hits zero.
3. Add a `Discount` field to `OrderItem` and adjust `Order.Subtotal`.
4. Persist meals to a JSON file on disk so changes survive a restart.
   Implement `JsonMenuRepository : IRepository<MenuItem>` and swap it in.
5. Add unit tests for `Order` (subtotal, tax, total) using MSTest.

## Stretch goals

- Migrate to **SQL Server LocalDB** with Entity Framework 6.
- Add a `Customer` model and an order-history screen.
- Implement password hashing (PBKDF2) in `Employee`.
- Theme the app with light / dark mode toggle.
- Print the receipt to a real printer using `PrintDocument`.

---

## 14. Deployment / handing in

1. **Clean Solution** (Build → Clean).
2. Switch to **Release** configuration.
3. **Build** — the `.exe` lives in `bin\Release\`.
4. Zip the entire solution folder **excluding** `bin/`, `obj/`, `.vs/`.
5. Push to GitHub. Tag the submission with `git tag v1.0`.

---

## 15. Quick reference of WinForms controls used

| Control          | First appearance                          | What it teaches                |
| ---------------- | ----------------------------------------- | ------------------------------ |
| `Label`          | every form                                | display-only text              |
| `TextBox`        | LoginForm, MenuManagementForm             | user text input                |
| `Button`         | every form                                | `Click` events                 |
| `MenuStrip`      | DashboardForm                             | classic top-of-window menus    |
| `Panel`          | DashboardForm header, MenuManagement form | grouping + colour blocks       |
| `FlowLayoutPanel`| DashboardForm tiles                       | automatic layout/reflow        |
| `DataGridView`   | MenuManagementForm, OrderingForm          | tabular data                   |
| `ComboBox`       | MenuManagementForm, OrderingForm          | enum / list selection          |
| `NumericUpDown`  | MenuManagementForm, OrderingForm          | constrained numeric input      |
| `CheckBox`       | MenuManagementForm                        | boolean fields                 |
| `Timer`          | SplashForm                                | scheduled work on the UI thread |

---

Good luck — and remember: **clean code is just kind code for the next
person reading it (often you, in three weeks).**
