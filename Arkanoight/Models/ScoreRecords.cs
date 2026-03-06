namespace Arkanoight.Models
{
    /// <summary>
    /// Модель записи в таблице рекордов
    /// </summary>
    public class ScoreRecord
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Количество очков
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Количество жизней (для информации)
        /// </summary>
        public int Lives { get; set; }

        /// <summary>
        /// Дата и время рекорда
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Тип окончания игры (победа или поражение)
        /// </summary>
        public string GameEndType { get; set; }
    }
}