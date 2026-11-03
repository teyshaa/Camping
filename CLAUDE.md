# HikeManager — Project Context & Guide for Claude

Цей документ містить повний контекст проєкту **HikeManager**, його архітектуру, базу даних, контракти сервісів та поточний стан коду.

---

## 1. Загальний огляд проєкту
* **Назва проєкту:** HikeManager
* **Тип:** Настільний застосунок (Desktop Application)
* **Призначення:** Централізоване планування та організація гірських походів: ведення каталогу маршрутів, планування походів, керування групами учасників, формування списків спорядження, розрахунок витрат (загального бюджету та частки на людину) та збереження історії.

---

## 2. Технологічний стек та правила
* **Мова:** C# (C# 12)
* **Платформа:** .NET 8.0 Windows (`net8.0-windows`)
* **UI-фреймворк:** Windows Forms (WinForms)
* **База даних:** Локальна SQLite (`hikemanager.db` через `AppContext.BaseDirectory`)
* **Пакет БД:** `System.Data.SQLite.Core` (1.0.119)
* **Правила архітектури:**
  1. Кожен екран у `Views/` є `UserControl` з методом `public void LoadHike(int hikeId)` (для `ParticipantsView`, `EquipmentView`, `ExpensesView`).
  2. Форми викликають лише сервіси. SQL у `Views/` не пишемо.
  3. Помилки валідації сервіс кидає як `ValidationException`, а форма показує текст українською.
  4. Усі запити в БД параметризовані. Дати зберігаємо як `yyyy-MM-dd` через `CultureInfo.InvariantCulture`.
  5. На кожному підключенні обов'язково викликається `PRAGMA foreign_keys = ON;` (реалізовано в `DbConnectionFactory`).

---

## 3. Спільні контракти сервісів (`Services/`)

```csharp
public interface IRouteService          // Учасник 1
{
    List<Route> GetAll();
    Route? GetById(int id);
    void Add(Route route);              // при невалідних даних кидає ValidationException
    void Update(Route route);
    void Delete(int id);
}

public interface IHikeService           // Учасник 2
{
    List<Hike> GetActive();
    List<Hike> GetHistory();
    Hike? GetById(int id);
    void Create(Hike hike);
    void Update(Hike hike);
}

public interface IParticipantService    // Учасник 3
{
    List<Participant> GetByHike(int hikeId);
    int CountByHike(int hikeId);
    void Add(Participant p); void Update(Participant p); void Delete(int id);
}

public interface IEquipmentService      // Учасник 3
{
    List<EquipmentItem> GetByHike(int hikeId);
    void Add(EquipmentItem i); void Update(EquipmentItem i); void Delete(int id);
}

public interface IExpenseService        // Учасник 4
{
    List<Expense> GetByHike(int hikeId);
    void Add(Expense e); void Update(Expense e); void Delete(int id);
}
```

---

## 4. Схема бази даних (SQLite)

```sql
CREATE TABLE IF NOT EXISTS ROUTES (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Location TEXT,
    Difficulty TEXT NOT NULL,
    DurationDays INTEGER NOT NULL,
    LengthKm REAL
);

CREATE TABLE IF NOT EXISTS HIKES (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    StartDate TEXT NOT NULL,
    RouteId INTEGER NOT NULL,
    FOREIGN KEY(RouteId) REFERENCES ROUTES(Id)
);

CREATE TABLE IF NOT EXISTS PARTICIPANTS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    HikeId INTEGER NOT NULL,
    FullName TEXT NOT NULL,
    Phone TEXT,
    Note TEXT,
    FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS EQUIPMENT_ITEMS (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    HikeId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Quantity INTEGER DEFAULT 1,
    FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS EXPENSES (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    HikeId INTEGER NOT NULL,
    Category TEXT NOT NULL,
    Amount REAL NOT NULL,
    Description TEXT,
    FOREIGN KEY(HikeId) REFERENCES HIKES(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_participants_hikeid ON PARTICIPANTS(HikeId);
CREATE INDEX IF NOT EXISTS idx_equipment_hikeid ON EQUIPMENT_ITEMS(HikeId);
CREATE INDEX IF NOT EXISTS idx_expenses_hikeid ON EXPENSES(HikeId);
```

---

## 5. Команди збірки та тестування
```powershell
dotnet build HikeManager/HikeManager.csproj
dotnet run --project HikeManager/HikeManager.csproj
```
