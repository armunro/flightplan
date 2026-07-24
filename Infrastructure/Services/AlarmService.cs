using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlightPlan.Core.Models;
using FlightPlan.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FlightPlan.Infrastructure.Services
{
    public interface IAlarmService
    {
        IEnumerable<AlarmItem> GetAllAlarms();
        AlarmItem GetAlarmById(Guid id);
        void AddAlarm(AlarmItem alarm);
        void UpdateAlarm(AlarmItem alarm);
        void DeleteAlarm(Guid id);
    }

    public class AlarmService : IAlarmService
    {
        private readonly IStorageService _storageService;
        private List<AlarmItem> _alarms = new List<AlarmItem>();
        private readonly string _filePath;

        public AlarmService(IStorageService storageService)
        {
            _storageService = storageService;
            _filePath = _storageService.GetAlarmsPath();
            LoadAlarms();
        }

        private void LoadAlarms()
        {
            if (!File.Exists(_filePath))
            {
                _alarms = new List<AlarmItem>();
                return;
            }

            try
            {
                var yaml = File.ReadAllText(_filePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
                _alarms = deserializer.Deserialize<List<AlarmItem>>(yaml) ?? new List<AlarmItem>();
            }
            catch
            {
                _alarms = new List<AlarmItem>();
            }
        }

        private void SaveAlarms()
        {
            try
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
                var yaml = serializer.Serialize(_alarms);
                File.WriteAllText(_filePath, yaml);
            }
            catch
            {
                // Log error
            }
        }

        public IEnumerable<AlarmItem> GetAllAlarms()
        {
            return _alarms.OrderByDescending(a => a.CreatedAt);
        }

        public AlarmItem GetAlarmById(Guid id)
        {
            return _alarms.FirstOrDefault(a => a.Id == id);
        }

        public void AddAlarm(AlarmItem alarm)
        {
            _alarms.Add(alarm);
            SaveAlarms();
        }

        public void UpdateAlarm(AlarmItem alarm)
        {
            var existing = _alarms.FirstOrDefault(a => a.Id == alarm.Id);
            if (existing != null)
            {
                _alarms.Remove(existing);
                _alarms.Add(alarm);
                SaveAlarms();
            }
        }

        public void DeleteAlarm(Guid id)
        {
            var alarm = _alarms.FirstOrDefault(a => a.Id == id);
            if (alarm != null)
            {
                _alarms.Remove(alarm);
                SaveAlarms();
            }
        }
    }
}
