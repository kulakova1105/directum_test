using System;
using System.Collections.Generic;
using System.Threading;

namespace MeetingControl.Services
{
    /// <summary>
    /// Сервис для создания напоминаний
    /// </summary>
    internal class ReminderService
    {
        private IDictionary<string, Timer> timerDictionary;

        /// <summary>
        /// Ctor
        /// </summary>
        public ReminderService()
        {
            timerDictionary = new Dictionary<string, Timer>();
        }

        /// <summary>
        /// Добавить
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="remindeTime">Интервал</param>
        /// <param name="obj">Объект</param>
        /// <param name="callback">Коллбек</param>
        public void Add(string key, TimeSpan remindeTime, object obj, Action<object> callback)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            if (remindeTime == TimeSpan.Zero)
            {
                throw new Exception("Не задан интервал для напоминания");
            }

            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (Exists(key))
            {
                throw new Exception("Напоминание с таким ключом уже было добавлено");
            }

            var timerCallback = new TimerCallback(callback);
            var timer = new Timer(timerCallback, obj, remindeTime, Timeout.InfiniteTimeSpan);
            timerDictionary.Add(key, timer);
        }

        /// <summary>
        /// Существует ли таймер с таким 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            return timerDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Удалить
        /// </summary>
        public void Remove(string key)
        {
            if (!Exists(key))
            {
                throw new Exception("Таймер с таким ключем не найден");
            }
            var timer = timerDictionary[key];
            timer.Change(Timeout.Infinite, Timeout.Infinite);

            timerDictionary.Remove(key);
        }
    }
}
