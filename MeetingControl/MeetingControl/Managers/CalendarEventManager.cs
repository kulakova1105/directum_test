using MeetingControl.Models;
using MeetingControl.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MeetingControl.Managers
{
    /// <summary>
    /// Менеджер событий
    /// </summary>
    internal class CalendarEventManager
    {
        private Dictionary<int, CalendarEvent> calendarEvents = new Dictionary<int, CalendarEvent>();
        private int idNumerator = 0;
        private readonly Action<string> writeTextAction;
        private readonly ReminderService reminderService;
        private const string exportDirectoryName = "..//..//..//export";
        private const string dateTimeFormat = "dd.MM.yyyy HH:mm";
        private const string reminderKeyFormat = "calendar_event_reminder_{0}";

        /// <summary>
        /// Ctor
        /// </summary>
        public CalendarEventManager(Action<string> writeTextAction)
        {
            if (writeTextAction == null)
            {
                throw new ArgumentNullException("writeTextAction");
            }

            this.writeTextAction = writeTextAction;
            this.reminderService = new ReminderService();
        }

        /// <summary>
        /// Сохранить
        /// </summary>
        /// <param name="calendarEvent">Событие</param>
        public void Save(CalendarEvent calendarEvent)
        {
            if (calendarEvent == null)
            {
                throw new ArgumentNullException("calendarEvent");
            }

            var checkResult = Check(calendarEvent);

            if (checkResult != null && !checkResult.Succes && checkResult.Errors.Length > 0)
            {
                throw new Exception(string.Format("Не удалось сохранить событие.\n\r{0}", string.Join(".\n\r", checkResult.Errors)));
            }

            if (calendarEvent.Id == 0)
            {
                // Событие новое, нужно присвоить Id и добавить в "хранилище"
                calendarEvent.Id = ++idNumerator;
                calendarEvent.CreateDate = DateTime.Now;

                calendarEvents.Add(calendarEvent.Id, calendarEvent);
            }
            else if (calendarEvents.ContainsKey(calendarEvent.Id))
            {
                calendarEvents[calendarEvent.Id] = calendarEvent;
            }
            else
            {
                throw new Exception("Не удалось найти ранее сохраненное событие.");
            }

            // Создать напоминание
            var reminderKey = string.Format(reminderKeyFormat, calendarEvent.Id);
            if (reminderService.Exists(reminderKey))
            {
                reminderService.Remove(reminderKey);
            }

            if (calendarEvent.RemindeTime.HasValue && 
                calendarEvent.RemindeTime.Value != TimeSpan.Zero && 
                calendarEvent.RemindeTime.HasValue && 
                calendarEvent.CreateDate - calendarEvent.RemindeTime.Value - TimeSpan.FromMinutes(1) > DateTime.Now)
            {
                var timeSpan = calendarEvent.StartDate - calendarEvent.RemindeTime.Value - DateTime.Now;
                reminderService.Add(reminderKey, timeSpan, calendarEvent, CreateReminderNotification);
            }
        }

        /// <summary>
        /// Проверить на валидность
        /// </summary>
        /// <param name="calendarEvent">Событие</param>
        /// <returns>Результат проверки</returns>
        public CheckResult Check(CalendarEvent calendarEvent)
        {
            var errors = new List<string> { };
            var correctDates = true;

            // Идентификтор больше либо равен 0
            if (calendarEvent.Id < 0)
            {
                errors.Add("Некорректный идентификатор события.");
            }

            // Время начала должно быть больше чем сейчас
            if (calendarEvent.StartDate <= DateTime.Now)
            {
                correctDates = false;
                errors.Add("Время начала должно быть больше чем сейчас.");
            }

            // Время окончания должно быть больше чем сейчас
            if (calendarEvent.EndDate <= DateTime.Now)
            {
                correctDates = false;
                errors.Add("Время окончания должно быть больше чем сейчас.");
            }

            // Время окончания должно быть больше чем время начала
            if (calendarEvent.EndDate <= calendarEvent.StartDate)
            {
                correctDates = false;
                errors.Add("Время окончания должно быть больше чем время начала.");
            }

            // Не должно быть пересечений с другими событиями
            if (correctDates)
            {
                var events = Find(calendarEvent.StartDate, calendarEvent.EndDate)
                    .Where(ev => ev.Id != calendarEvent.Id)
                    .ToArray();

                if (events.Length > 0)
                {
                    errors.Add(string.Format("Найдены пересечения: \n\r{0}", string.Join("\n\r", events.Select(evnt => ToString(evnt)))));

                    string ToString(CalendarEvent evnt)
                    {
                        return string.Format("id:{0}. {1} - {2} {3}",evnt.Id, evnt.StartDate.ToString(dateTimeFormat), evnt.EndDate.ToString(dateTimeFormat), evnt.Description);
                    }
                }
            }

            // Описание заполнено
            if (string.IsNullOrWhiteSpace(calendarEvent.Description))
            {
                errors.Add("Описание события не заполнено.");
            }

            return new CheckResult
            {
                Succes = errors.Count == 0,
                Errors = errors.ToArray()
            };
        }

        /// <summary>
        /// Получить событие
        /// </summary>
        /// <param name="id">Идентификатор</param>
        public CalendarEvent GetOrNull(int id)
        {
            if (calendarEvents.ContainsKey(id))
            {
                return calendarEvents[id];
            }
            return null;
        }

        /// <summary>
        /// Удалить
        /// </summary>
        /// <param name="id">Идентификатор</param>
        public void Delete(int id)
        {
            if (calendarEvents.ContainsKey(id))
            {
                calendarEvents.Remove(id);
            }
            else
            {
                throw new Exception("Не удалось найти ранее сохраненное событие.");
            }
        }

        /// <summary>
        /// Найти события
        /// </summary>
        /// <param name="startDate">Начало интервала поиска</param>
        /// <param name="endDate">Конец интервала поиска</param>
        /// <returns>Список событий</returns>
        public CalendarEvent[] Find(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new Exception("Конец интервала поиска не может начинаться раньше начала интервала поиска.");
            }

            return calendarEvents.Values.Where(evnt => CheckDateTime(evnt.StartDate) || CheckDateTime(evnt.EndDate)).ToArray();

            bool CheckDateTime(DateTime dateTime)
            {
                return dateTime >= startDate && dateTime <= endDate;
            }
        }

        /// <summary>
        /// Найти события
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Список событий</returns>
        public CalendarEvent[] Find(DateTime date)
        {
            var startDate = date.Date;
            var endDate = date.Date.AddDays(1).AddTicks(-1);

            return Find(startDate, endDate);
        }

        /// <summary>
        /// Экспортировать события в файл за выбранный день
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string ExportToFileFromDate(DateTime date)
        {
            var calendarEvents = Find(date);
            var random = new Random();
            var fileName = string.Format("export_{0}_{1}", date.ToString("ddmmyyy"), random.Next());
            return ExportToFile(calendarEvents, fileName);
        }

        /// <summary>
        /// Экспортировать события в файл
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string ExportToFile(CalendarEvent[] events, string fileName)
        {
            if (!Directory.Exists(exportDirectoryName))
            {
                Directory.CreateDirectory(exportDirectoryName);
            }
            var fullPath = Path.Combine(exportDirectoryName, fileName);
            if (File.Exists(fullPath))
            {
                throw new Exception("Такой файл уже существует.");
            }

            var json = JsonConvert.SerializeObject(events, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    });

            File.WriteAllText(fullPath, json);

            return Path.GetFullPath(fullPath);
        }

        private void CreateReminderNotification(object obj)
        {
            if (obj is CalendarEvent evnt)
            {
                var notificationText = string.Format("! Напоминание о начале события {0}: {1}", evnt.StartDate.ToString(dateTimeFormat), evnt.Description);
                writeTextAction.Invoke(notificationText);
            }
        }
    }
}
