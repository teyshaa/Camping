
using System;
using System.Linq;
using System.Text;
using HikeManager.Models;

namespace HikeManager.Services
{
    public class HikeReportExporter
    {
        private readonly ParticipantService _participants = new();
        private readonly EquipmentService _equipment = new();
        private readonly ExpenseService _expenses = new();
        private readonly RouteService _routes = new();

        public string Generate(Hike hike)
        {
            if (hike == null || hike.Id <= 0)
                throw new ValidationException("Не вибрано похід.");

            var route = _routes.GetById(hike.RouteId);

            if (route == null)
                throw new ValidationException("Маршрут не знайдено.");

            var participants = _participants.GetByHike(hike.Id);
            var equipment = _equipment.GetByHike(hike.Id);
            var expenses = _expenses.GetByHike(hike.Id);

            var budget = BudgetCalculator.Calculate(
                expenses, participants.Count);

            var sb = new StringBuilder();

            sb.AppendLine("ЗВІТ ПРО ГІРСЬКИЙ ПОХІД");
            sb.AppendLine(new string('=', 45));
            sb.AppendLine();

            sb.AppendLine("1. ІНФОРМАЦІЯ ПРО ПОХІД");
            sb.AppendLine($"Назва: {hike.Name}");
            sb.AppendLine($"Дата початку: {hike.StartDate:dd.MM.yyyy}");
            sb.AppendLine($"Маршрут: {route.Name}");
            sb.AppendLine($"Місцевість: {route.Location ?? "Не вказано"}");
            sb.AppendLine($"Складність: {route.Difficulty}");
            sb.AppendLine($"Тривалість: {route.DurationDays} дн.");
            sb.AppendLine();

            sb.AppendLine("2. УЧАСНИКИ");
            sb.AppendLine($"Кількість: {participants.Count}");

            if (participants.Count == 0)
                sb.AppendLine("Учасників немає.");

            foreach (var p in participants)
            {
                sb.AppendLine($"- {p.FullName}");
                if (!string.IsNullOrWhiteSpace(p.Phone))
                    sb.AppendLine($"  Телефон: {p.Phone}");
            }

            sb.AppendLine();
            sb.AppendLine("3. СПОРЯДЖЕННЯ");

            if (equipment.Count == 0)
                sb.AppendLine("Спорядження не додано.");

            foreach (var item in equipment)
                sb.AppendLine($"- {item.Name}: {item.Quantity} шт.");

            sb.AppendLine();
            sb.AppendLine("4. ВИТРАТИ");

            if (expenses.Count == 0)
                sb.AppendLine("Витрат немає.");

            foreach (var e in expenses)
            {
                sb.AppendLine(
                    $"- {e.Category}: {e.Amount:N2} грн");

                if (!string.IsNullOrWhiteSpace(e.Description))
                    sb.AppendLine($"  Опис: {e.Description}");
            }

            sb.AppendLine();
            sb.AppendLine("5. БЮДЖЕТ");
            sb.AppendLine(
                $"Загальний бюджет: {budget.TotalBudget:N2} грн");

            if (budget.HasNoParticipants)
                sb.AppendLine("На людину: немає учасників");
            else
                sb.AppendLine(
                    $"На людину: {budget.CostPerPerson:N2} грн");

            sb.AppendLine();
            sb.AppendLine("Витрати за категоріями:");

            foreach (var category in ExpenseService.Categories)
            {
                decimal amount = budget.CategoryTotals
                    .GetValueOrDefault(category, 0m);

                sb.AppendLine($"- {category}: {amount:N2} грн");
            }

            sb.AppendLine();
            sb.AppendLine(new string('=', 45));
            sb.AppendLine(
                $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}");

            return sb.ToString();
        }
    }
}
