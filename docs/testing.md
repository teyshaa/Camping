# Документація процесу тестування та виконаного рефакторингу

Цей документ фіксує результати виконання етапу 7 «Тестування та забезпечення якості» для настільного застосунку **HikeManager**.

---

## 1. Загальний огляд проведеної роботи

У рамках етапу 7 було створено тестовий проєкт **`HikeManager.Tests`** (xUnit, .NET 8 Windows), який підключено до загального solution `HikeManager.sln`.

Реалізовано:
- **130 автоматизованих тестів** (модульні, інтеграційні та крайові випадки).
- Повне тестування на реальній локальній СКБД SQLite без сторонніх мок-бібліотек.
- Виявлено та виправлено реальний дефект зовнішніх ключів при видаленні маршрутів.
- Проведено 3 архітектурні рефакторинги методом TDD (спочатку тест, потім рефакторинг, потім усі тести зелені).

---

## 2. Приклади виконаного рефакторингу

### Рефакторинг №1: Конфігурований шлях до бази даних для ізоляції тестів

* **Проблема:**  
  У класі `DbConnectionFactory` шлях до БД був захардкоджений як `Path.Combine(AppContext.BaseDirectory, "hikemanager.db")` через статичні поля `readonly`. Через це будь-який тест, що викликав репозиторій або ініціалізатор, модифікував реальну робочу базу даних застосунку, а паралельні або повторні запуски тестів залишали за собою «сміття» та конфліктували між собою.

* **Фрагмент ДО:**
  ```csharp
  public static class DbConnectionFactory
  {
      private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "hikemanager.db");
      public static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

      public static string DatabasePath => DbPath;
      ...
  ```

* **Фрагмент ПІСЛЯ:**
  ```csharp
  public static class DbConnectionFactory
  {
      private static readonly string DefaultDbPath = Path.Combine(AppContext.BaseDirectory, "hikemanager.db");
      private static string? _customDbPath;
      private static string? _customConnectionString;

      public static string DatabasePath => _customDbPath ?? DefaultDbPath;
      public static string ConnectionString => _customConnectionString ?? $"Data Source={DatabasePath};Version=3;";

      public static void SetCustomDatabasePath(string path)
      {
          _customDbPath = path;
          _customConnectionString = $"Data Source={path};Version=3;";
      }

      public static void ResetToDefault()
      {
          _customDbPath = null;
          _customConnectionString = null;
      }
      ...
  ```

* **Чому це краще:**  
  Для кінцевого користувача поведінка не змінилася — застосунок продовжує використовувати файл `hikemanager.db` у своїй папці. Однак тепер тестовий клас `SqliteTestDatabase` на час виконання тесту перемикає фабрику на унікальний тимчасовий файл у `Path.GetTempPath()`, а при завершенні викликає `SQLiteConnection.ClearAllPools()` та видаляє тимчасову БД. Це забезпечує 100% ізоляцію тестів.

---

### Рефакторинг №2: Впровадження залежностей (Constructor Injection) в `ExpenseService`

* **Проблема:**  
  У класі `ExpenseService` екземпляр `ExpenseRepository` створювався напряму при оголошенні поля: `private readonly ExpenseRepository repository = new ExpenseRepository();`. Це суперечило принципу інверсії залежностей (Dependency Inversion Principle, SOLID) та робило сервіс негнучким для конфігурації у тестових сценаріях.

* **Фрагмент ДО:**
  ```csharp
  public class ExpenseService : IExpenseService
  {
      private readonly ExpenseRepository repository = new ExpenseRepository();

      public static readonly string[] Categories = ...
  ```

* **Фрагмент ПІСЛЯ:**
  ```csharp
  public class ExpenseService : IExpenseService
  {
      private readonly ExpenseRepository _repository;

      public static readonly string[] Categories = ...

      public ExpenseService(ExpenseRepository? repository = null)
      {
          _repository = repository ?? new ExpenseRepository();
      }
  ```

* **Чому це краще:**  
  Сервіс тепер узгоджений за стилем з іншими сервісами проєкту (`RouteService`, `ParticipantService`, `EquipmentService`). Збережено повну зворотну сумісність з WinForms (де викликається конструктор без параметрів `new ExpenseService()`), але водночас з'явилася можливість передавати налаштований або тестовий репозиторій.

---

### Рефакторинг №3: Впровадження залежностей у `HikeReportExporter`

* **Проблема:**  
  Клас формування друкованого звіту `HikeReportExporter` напряму ініціалізував у своїх полях чотири сервіси: `_participants = new();`, `_equipment = new();`, `_expenses = new();`, `_routes = new();`. Через це клас неможливо було протестувати ізольовано.

* **Фрагмент ДО:**
  ```csharp
  public class HikeReportExporter
  {
      private readonly ParticipantService _participants = new();
      private readonly EquipmentService _equipment = new();
      private readonly ExpenseService _expenses = new();
      private readonly RouteService _routes = new();
  ```

* **Фрагмент ПІСЛЯ:**
  ```csharp
  public class HikeReportExporter
  {
      private readonly ParticipantService _participants;
      private readonly EquipmentService _equipment;
      private readonly ExpenseService _expenses;
      private readonly RouteService _routes;

      public HikeReportExporter(
          ParticipantService? participants = null,
          EquipmentService? equipment = null,
          ExpenseService? expenses = null,
          RouteService? routes = null)
      {
          _participants = participants ?? new();
          _equipment = equipment ?? new();
          _expenses = expenses ?? new();
          _routes = routes ?? new();
      }
  ```

* **Чому це краще:**  
  Класи зв'язані слабко (loose coupling). Експортер тепер піддається модульному тестуванню, дозволяючи тестувати генерацію тексту звіту з кастомними або ізольованими джерелами даних.

---

## 3. Виявлена проблема та виправлений баг (Foreign Key Bug)

* **Опис проблеми:**  
  При написанні тестів на остаточне видалення маршрутів (`HardDelete`) було виявлено, що метод `RouteRepository.CountHikesUsingRoute` рахував походи з фільтром `WHERE RouteId = @RouteId AND IsDeleted = 0;`.
  Якщо в системі існував похід у статусі історії / архівований (`IsDeleted = 1`), який використовував цей маршрут, метод повертав `0`.
  Внаслідок цього `RouteService.HardDelete` дозволяв видалення маршруту, викликаючи SQL `DELETE FROM ROUTES WHERE Id = @Id;`.
  Оскільки в SQLite увімкнено обмеження зовнішніх ключів (`PRAGMA foreign_keys = ON;`), а таблиця `HIKES` посилається на `ROUTES(Id)`, СКБД миттєво аварійно зупиняла виконання програми з винятком `System.Data.SQLite.SQLiteException: FOREIGN KEY constraint failed`.

* **Тест, що демонстрував баг:**  
  `RouteServiceTests.HardDelete_RouteUsedInHistoricalHike_ThrowsValidationException` падав із `SQLiteException`.

* **Виправлення (коміт `fix: prevent hard-deleting routes referenced by historical hikes`):**  
  У `RouteRepository.CountHikesUsingRoute` прибрано умову `AND IsDeleted = 0`, оскільки зовнішній ключ у БД тримається незалежно від того, чи є похід активним чи архівним:
  ```sql
  SELECT COUNT(*) FROM HIKES WHERE RouteId = @RouteId;
  ```
  Тепер сервіс коректно виявляє факт використання маршруту і викидає зрозумілий для користувача `ValidationException`:
  > *«Неможливо остаточно видалити маршрут, оскільки він використовується у поході(ах).»*
  Застосунок більше не зазнає аварійного збою.

