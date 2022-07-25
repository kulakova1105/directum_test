using MeetingControl.Managers;
using MeetingControl.Models;
using System;

namespace MeetingControl
{
    class Program
    {
        private static readonly CalendarEventManager calendarEventManager = new CalendarEventManager(WriteNotification);

        #region commands

        private const string HelpCommand = "help";
        private const string AddCommand = "add";
        private const string EditCommand = "edit";
        private const string DeleteCommand = "del";
        private const string FindCommand = "find";
        private const string ExportCommand = "export";
        private const string ExitCommand = "exit";

        #endregion

        static void Main(string[] args)
        {
            WriteHelp();
            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write("Введите команду: ");

                    var command = Console.ReadLine().Trim().ToLower();
                    var exit = false;

                    switch (command)
                    {
                        case HelpCommand:
                            {
                                WriteHelp();
                                break;
                            }

                        case AddCommand:
                            {
                                AddCommandFunc();
                                break;
                            }

                        case EditCommand:
                            {
                                EditCommandFunc();
                                break;
                            }

                        case DeleteCommand:
                            {
                                DeleteCommandFunc();
                                break;
                            }

                        case FindCommand:
                            {
                                FindCommandFunc();
                                break;
                            }

                        case ExportCommand:
                            {
                                ExportCommandFunc();
                                break;
                            }

                        case ExitCommand:
                            {
                                exit = true;
                                break;
                            }

                        default:
                            {
                                WriteError("Неправильно введена команда. Попробуйте еще.");
                                break;
                            }
                    }

                    if (exit)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void ExportCommandFunc()
        {
            var date = ReadDate();
            var evnts = calendarEventManager.Find(date);
            if (evnts == null || evnts.Length == 0)
            {
                Console.WriteLine("Не найдено встреч за выбранный день.");
            }
            else
            {
                Console.Write("Найдено {0} встреч(и) за выбранный день. Экспортировать? Да/нет: ", evnts.Length);
                if (IsYes(Console.ReadLine()))
                {
                    var exportFilePath = calendarEventManager.ExportToFileFromDate(date);
                    Console.WriteLine("Встречи экспортированы в файл {0}", exportFilePath);
                }
                else
                {
                    Console.WriteLine("Экспорт отменен.");
                }
            }
        }

        private static void FindCommandFunc()
        {
            var date = ReadDate();
            var events = calendarEventManager.Find(date);
            if (events == null || events.Length == 0)
            {
                Console.WriteLine("Не найдено встреч за выбранный день.");
            }

            Console.Write("За {0} найдено встреч {1}", date.ToString("dd.MM.yyyy"), events.Length);
            foreach (var evnt in events)
            {
                Console.WriteLine();
                WriteEventInfo(evnt);
            }
        }

        private static void DeleteCommandFunc()
        {
            var id = ReadEventId();
            var evnt = calendarEventManager.GetOrNull(id);
            if (evnt == null)
            {
                WriteError(string.Format("Не удалось найти встречу с идентификатором {0}", id));
                return;
            }

            Console.WriteLine("Информация о встрече:");
            WriteEventInfo(evnt);
            Console.Write("Удалить встречу? Да/нет: ");
            if (IsYes(Console.ReadLine()))
            {
                calendarEventManager.Delete(id);
                Console.WriteLine("Встреча удалена");
            }
            else
            {
                Console.WriteLine("Встреча не удалена");
            }
        }

        private static void WriteHelp()
        {
            Console.WriteLine("Список доступных команд:");
            WriteComandString(HelpCommand, "Открыть список доступных команд");
            WriteComandString(AddCommand, "Добавить встречу");
            WriteComandString(EditCommand, "Изменить встречу");
            WriteComandString(DeleteCommand, "Удалить встречу");
            WriteComandString(FindCommand, "Посмотреть расписание встреч за выбранный день");
            WriteComandString(ExportCommand, "Экспортировать расписание встреч за выбранный день");
            WriteComandString(ExitCommand, "Выйти из приложения");

            void WriteComandString(string command, string description)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\t{0}\t", command);
                Console.ForegroundColor = oldColor;

                Console.Write(" - {0}", description);
                Console.WriteLine();
            }
        }

        private static void ChangeEvent(int id) 
        {
            if (id > 0)
            {
                var evnt = calendarEventManager.GetOrNull(id);
                if (evnt == null)
                {
                    WriteError(string.Format("Не удалось найти встречу с идентификатором {0}", id));
                    return;
                }

                Console.WriteLine("Информация о встрече:");
                WriteEventInfo(evnt);
                Console.Write("Изменить данные о встрече? Да/нет: ");
                if (!IsYes(Console.ReadLine()))
                {
                    Console.WriteLine("Встреча не была изменена.");
                    return;
                }
                Console.WriteLine("Изменение данных встречи.");
            }

            Console.WriteLine("Введите данные о встрече в формате {время начала} | {время окончания} | {описание}. Пример: 26.07.2022 10:00 | 26.07.2022 10:30 | Встреча с кандидатом");
            var eventString = Console.ReadLine();
            var eventParts = eventString.Split("|");

            if (eventParts.Length != 3)
            {
                WriteError("Проверьте правильность введенных данных и попробуйте еще раз.");
                return;
            }

            DateTime startDate;
            if (!DateTime.TryParse(eventParts[0].Trim(), out startDate))
            {
                WriteError("Неверно указано начало встречи.");
                return;
            }

            DateTime endDate;
            if (!DateTime.TryParse(eventParts[1].Trim(), out endDate))
            {
                WriteError("Неверно указано окончание встречи.");
                return;
            }

            var description = eventParts[2].Trim();

            var calendarEvent = new CalendarEvent(startDate, endDate, description);
            calendarEvent.Id = id;

            Console.Write("Создать напоминание? Да/нет: ");
            var needNotification = Console.ReadLine();
            if (IsYes(needNotification))
            {
                Console.Write("Укажите интервал в формате hh:mm. Например, 00:30  ");
                var timeStampParts = Console.ReadLine().Split(":");
                int hours;
                int minutes;

                if (timeStampParts.Length != 2 || 
                    !int.TryParse(timeStampParts[0], out hours) || 
                    !int.TryParse(timeStampParts[1], out minutes) || 
                    minutes >= 60)
                {
                    WriteError("Неверно указано напоминание о встрече.");
                    return;
                }

                calendarEvent.RemindeTime = new TimeSpan(hours, minutes, 0);
            }

            var checkResult = calendarEventManager.Check(calendarEvent);
            if (!checkResult.Succes)
            {
                WriteError("Некорректно заданы параметры встречи:");
                WriteError(string.Join("\n\r", checkResult.Errors));
                return;
            }


            Console.WriteLine("Проверьте правильность введенных данных.");
            WriteEventInfo(calendarEvent);
            Console.Write("Сохранить встречу? Да/нет: ");
            var needSave = IsYes(Console.ReadLine());
            if (needSave)
            {
                calendarEventManager.Save(calendarEvent);
                Console.WriteLine("Встреча сохранена.");
            }
            else
            {
                Console.WriteLine("Встреча не сохранена.");
            }
        }
        private static void AddCommandFunc()
        {
            ChangeEvent(0);
        }

        private static void EditCommandFunc()
        {
            var id = ReadEventId();
            ChangeEvent(id);
        }

        private static int ReadEventId()
        {
            Console.Write("Введите идентификатор встречи: ");
            var idString = Console.ReadLine();
            int id;
            if (!int.TryParse(idString, out id) || id <= 0)
            {
                throw new Exception("Неверно указан идентификатор встречи");
            }

            return id;
        }

        private static DateTime ReadDate()
        {
            Console.Write("Введите дату. Например: 26.07.2022: ");
            var dateString = Console.ReadLine();
            DateTime date;
            if (!DateTime.TryParse(dateString, out date))
            {
                throw new Exception("Некорректно указана дата");
            }
            return date.Date;
        }

        private static bool IsYes(string text)
        {
            text = text.Trim().ToLower();
            return text == "да" || text == "yes" || text == "+";
        }

        private static void WriteEventInfo(CalendarEvent calendarEvent)
        {
            if (calendarEvent == null)
            {
                throw new ArgumentNullException("calendarEvent");
            }

            if (calendarEvent.Id > 0)
            {
                WriteString("Идентификатор", calendarEvent.Id);
            }
            WriteString("Начало", calendarEvent.StartDate);
            WriteString("Окончание", calendarEvent.EndDate);
            WriteString("Описание", calendarEvent.Description);

            if (calendarEvent.RemindeTime.HasValue && calendarEvent.RemindeTime.Value != TimeSpan.Zero)
            {
                WriteString("Напоминание", string.Format("{0} час(а) {1} минут(ы)", calendarEvent.RemindeTime.Value.Hours, calendarEvent.RemindeTime.Value.Minutes));
            }

            void WriteString(string name, object value)
            {
                Console.WriteLine("{0,-20} {1,5}", name, value);
            }
        }

        private static void WriteError(string text)
        {
            WriteColorText(text, ConsoleColor.Red);
        }

        private static void WriteNotification(string text)
        {
            WriteColorText(text, ConsoleColor.Green);
        }

        private static void WriteColorText(string text, ConsoleColor color)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            Console.WriteLine(text);

            Console.ForegroundColor = oldColor;
        }
    }
}
