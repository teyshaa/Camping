using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HikeManager.Models;
using HikeManager.DataAccess;

namespace HikeManager.Services
{
    public class ParticipantService : IParticipantService
    {
        private readonly ParticipantRepository _repository;

        public ParticipantService(ParticipantRepository? repository = null)
        {
            _repository = repository ?? new ParticipantRepository();
        }

        public List<Participant> GetByHike(int hikeId)
        {
            return _repository.GetByHike(hikeId);
        }

        public int CountByHike(int hikeId)
        {
            return _repository.CountByHike(hikeId);
        }

        public void Add(Participant p)
        {
            Validate(p);
            _repository.Insert(p);
        }

        public void Update(Participant p)
        {
            Validate(p);
            _repository.Update(p);
        }

        public void Save(Participant p)
        {
            if (p.Id == 0)
                Add(p);
            else
                Update(p);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        private void Validate(Participant p)
        {
            p.FullName = p.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(p.FullName))
            {
                throw new ValidationException("ПІБ учасника є обов'язковим.");
            }
            if (p.FullName.Length > 100)
            {
                throw new ValidationException("ПІБ учасника не може перевищувати 100 символів.");
            }

            if (!string.IsNullOrWhiteSpace(p.Phone))
            {
                p.Phone = p.Phone.Trim();

                int digitCount = Regex.Matches(p.Phone, @"\d").Count;
                if (digitCount < 10 || digitCount > 15)
                {
                    throw new ValidationException("Телефон повинен містити від 10 до 15 цифр.");
                }

                if (!Regex.IsMatch(p.Phone, @"^[\d\+\s\(\)\-]+$"))
                {
                    throw new ValidationException("Некоректний формат телефону. Дозволені лише цифри, +, пробіли, дужки та дефіси.");
                }
            }
        }
    }
}