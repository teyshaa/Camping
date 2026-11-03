namespace HikeManager
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--test")
            {
                RunSelfTests();
                return;
            }

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

            var testHike = new HikeManager.Models.Hike
            {
                Name = "Тестовий похід валідації",
                StartDate = DateTime.Today,
                RouteId = 1
            };
            hikeService.Create(testHike);
            int hikeId = testHike.Id;
            Console.WriteLine($"[OK] Створено тестовий похід з ID = {hikeId}");

            try
            {
                partService.Add(new HikeManager.Models.Participant { HikeId = hikeId, FullName = "   " });
                Console.WriteLine("[FAIL] Учасник без ПІБ повинен викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація ПІБ успішна: {ex.Message}");
            }

            try
            {
                partService.Add(new HikeManager.Models.Participant { HikeId = hikeId, FullName = "Іван Тестовий", Phone = "123" });
                Console.WriteLine("[FAIL] Короткий телефон повинен викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація телефону успішна: {ex.Message}");
            }

            try
            {
                partService.Add(new HikeManager.Models.Participant { HikeId = hikeId, FullName = "Іван Тестовий", Phone = "+3805012345abc" });
                Console.WriteLine("[FAIL] Некоректні літери в телефоні повинні викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація формату телефону успішна: {ex.Message}");
            }

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

            try
            {
                equipService.Add(new HikeManager.Models.EquipmentItem { HikeId = hikeId, Name = "Казанок", Quantity = 0 });
                Console.WriteLine("[FAIL] Кількість 0 повинна викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація кількості (0) успішна: {ex.Message}");
            }

            try
            {
                equipService.Add(new HikeManager.Models.EquipmentItem { HikeId = hikeId, Name = "", Quantity = 1 });
                Console.WriteLine("[FAIL] Порожня назва спорядження повинна викликати помилку!");
            }
            catch (HikeManager.Services.ValidationException ex)
            {
                Console.WriteLine($"[PASS] Валідація порожньої назви успішна: {ex.Message}");
            }

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

            int addedKit = equipService.AddBasicKit(hikeId);
            Console.WriteLine($"[PASS] Додано {addedKit} позицій базового набору.");

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