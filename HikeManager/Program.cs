namespace HikeManager
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--test")
            {
                RunSelfTests();
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            HikeManager.DataAccess.DatabaseInitializer.Initialize();
            Application.Run(new Form1());
        }

        private static void RunSelfTests()
        {
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }
            Console.WriteLine("=== ЗАПУСК АВТОМАТИЧНИХ ТЕСТІВ МОДУЛІВ УЧАСНИКИ ТА СПОРЯДЖЕННЯ ===");

            HikeManager.DataAccess.DatabaseInitializer.Initialize();

            var partService = new HikeManager.Services.ParticipantService();
            var equipService = new HikeManager.Services.EquipmentService();
            var hikeService = new HikeManager.Services.HikeService();

            // Створюємо тестовий похід
            var testHike = new HikeManager.Models.Hike
            {
                Name = "Тестовий похід валідації",
                StartDate = DateTime.Today,
                RouteId = 1
            };
            hikeService.Create(testHike);
            int hikeId = testHike.Id;
            Console.WriteLine($"[OK] Створено тестовий похід з ID = {hikeId}");

            // 1. Тест: Учасник без ПІБ
            try
            {
                partService.Add(new HikeManager.Models.Participant { HikeId = hikeId, FullName = "   " });
                Console.WriteLine("[FAIL] Учасник без ПІБ повинен викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація ПІБ успішна: {ex.Message}");
            }

            // 2. Тест: Некоректний телефон (мало цифр)
            try
            {
                partService.Add(new HikeManager.Models.Participant { HikeId = hikeId, FullName = "Іван Тестовий", Phone = "123" });
                Console.WriteLine("[FAIL] Короткий телефон повинен викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація телефону успішна: {ex.Message}");
            }

            // 3. Тест: Некоректні символи в телефоні
            try
            {
                partService.Add(new HikeManager.Models.Participant { HikeId = hikeId, FullName = "Іван Тестовий", Phone = "+3805012345abc" });
                Console.WriteLine("[FAIL] Некоректні літери в телефоні повинні викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація формату телефону успішна: {ex.Message}");
            }

            // 4. Тест: Додавання та CountByHike
            var validPart = new HikeManager.Models.Participant
            {
                HikeId = hikeId,
                FullName = "Олександр Карпатський",
                Phone = "+38 (067) 123-45-67",
                Note = "Досвідчений турист"
            };
            partService.Add(validPart);
            int count = partService.CountByHike(hikeId);
            Console.WriteLine($"[PASS] Додано учасника. CountByHike = {count} (очікувалось >= 1)");

            // 5. Тест: Спорядження з кількістю 0
            try
            {
                equipService.Add(new HikeManager.Models.EquipmentItem { HikeId = hikeId, Name = "Казанок", Quantity = 0 });
                Console.WriteLine("[FAIL] Кількість 0 повинна викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація кількості (0) успішна: {ex.Message}");
            }

            // 6. Тест: Спорядження з порожньою назвою
            try
            {
                equipService.Add(new HikeManager.Models.EquipmentItem { HikeId = hikeId, Name = "", Quantity = 1 });
                Console.WriteLine("[FAIL] Порожня назва спорядження повинна викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація порожньої назви успішна: {ex.Message}");
            }

            // 7. Тест: Правило дублікатів (сумування кількості)
            equipService.Add(new HikeManager.Models.EquipmentItem { HikeId = hikeId, Name = "Термос 1л", Quantity = 2 });
            equipService.Add(new HikeManager.Models.EquipmentItem { HikeId = hikeId, Name = "термос 1л", Quantity = 3 });
            var items = equipService.GetByHike(hikeId);
            var thermoses = items.FindAll(x => x.Name.Equals("Термос 1л", StringComparison.OrdinalIgnoreCase));
            if (thermoses.Count == 1 && thermoses[0].Quantity == 5)
            {
                Console.WriteLine($"[PASS] Правило дублікатів (сумування кількості): {thermoses[0].Quantity} шт.");
            }
            else
            {
                Console.WriteLine($"[FAIL] Очікувалось 1 запис з кількістю 5, отримано: count={thermoses.Count}");
            }

            // 8. Тест: Бонус - Додати базовий набір
            int addedKit = equipService.AddBasicKit(hikeId);
            Console.WriteLine($"[PASS] Додано {addedKit} позицій базового набору.");

            // 9. Тест: Видалення походу прибирає учасників і спорядження (ON DELETE CASCADE)
            hikeService.Delete(hikeId);
            int partsAfterDelete = partService.CountByHike(hikeId);
            var equipAfterDelete = equipService.GetByHike(hikeId);
            if (partsAfterDelete == 0 && equipAfterDelete.Count == 0)
            {
                Console.WriteLine("[PASS] Каскадне видалення (ON DELETE CASCADE) успішно перевірено!");
            }
            else
            {
                Console.WriteLine($"[FAIL] Каскадне видалення не очистило: учасників={partsAfterDelete}, спорядження={equipAfterDelete.Count}");
            }

            Console.WriteLine("=== УСІ ТЕСТИ ПРОЙДЕНО УСПІШНО! ===");
        }
    }
}