using System;

namespace MeetingControl.Models
{
    /// <summary>
    /// Событие
    /// </summary>
    internal class CalendarEvent
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CalendarEvent()
        {}

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="startDate">Начало</param>
        /// <param name="endDate">Окончание</param>
        /// <param name="description">Описание</param>
        public CalendarEvent(DateTime startDate, DateTime endDate, string description)
        {
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата и время начала
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата и время окончания
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Дата и время создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Напоминание
        /// </summary>
        public TimeSpan? RemindeTime { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
    }
}
