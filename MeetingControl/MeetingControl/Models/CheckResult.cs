namespace MeetingControl.Models
{
    /// <summary>
    /// Результат проверки
    /// </summary>
    internal class CheckResult
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public CheckResult()
        {
            Errors = new string[] { };
        }

        /// <summary>
        /// Успешна ли проверка
        /// </summary>
        public bool Succes { get; set; }

        /// <summary>
        /// Ошибки
        /// </summary>
        public string[] Errors { get; set; }
    }
}
