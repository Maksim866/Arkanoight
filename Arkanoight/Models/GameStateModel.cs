namespace Arkanoight.Models
{
    /// <summary>
    /// Состояние игры
    /// </summary>
    public class GameStateModel
    {
        /// <summary>
        /// Получает или задает текущий счет игрока
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Получает или задает количество оставшихся жизней
        /// </summary>
        public int Lives { get; set; }

        /// <summary>
        /// Получает или задает флаг окончания игры (проигрыш)
        /// </summary>
        public bool IsGameOver { get; set; }

        /// <summary>
        /// Получает или задает флаг победы (все кирпичи разрушены)
        /// </summary>
        public bool IsGameWon { get; set; }

        /// <summary>
        /// Получает или задает ширину игрового поля в пикселях
        /// </summary>
        public int GameWidth { get; set; }

        /// <summary>
        /// Получает или задает высоту игрового поля в пикселях
        /// </summary>
        public int GameHeight { get; set; }

        /// <summary>
        /// Получает или задает флаг запуска мяча (true - мяч в движении, false - на платформе)
        /// </summary>
        public bool IsBallLaunched { get; set; }
    }
}