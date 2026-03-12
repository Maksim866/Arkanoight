namespace ArkanoidWinForms.Views
{
    /// <summary>
    /// Централизованное хранилище всех шрифтов, используемых в игре
    /// </summary>
    public static class FontResources
    {
        /// <summary>Шрифт для отображения счета и жизней</summary>
        public static readonly Font ScoreFont = new Font("Arial", 14, FontStyle.Bold);

        /// <summary>Шрифт для надписи ПАУЗА</summary>
        public static readonly Font PauseFont = new Font("Arial", 48, FontStyle.Bold);

        /// <summary>Шрифт для подсказки продолжения на паузе</summary>
        public static readonly Font ContinueFont = new Font("Arial", 18, FontStyle.Regular);

        /// <summary>Шрифт для заголовка АРКАНОИД</summary>
        public static readonly Font TitleFont = new Font("Arial", 26, FontStyle.Bold);

        /// <summary>Шрифт для подсказок управления</summary>
        public static readonly Font ControlFont = new Font("Arial", 12, FontStyle.Regular);

        /// <summary>Шрифт для описания усилений</summary>
        public static readonly Font PowerFont = new Font("Arial", 11, FontStyle.Regular);

        /// <summary>Шрифт для символов в квадратиках усилений</summary>
        public static readonly Font SymbolFont = new Font("Arial", 14, FontStyle.Bold);

        /// <summary>Шрифт для сообщения об окончании игры</summary>
        public static readonly Font GameOverFont = new Font("Arial", 28, FontStyle.Bold);

        /// <summary>Шрифт для заголовка таблицы рекордов</summary>
        public static readonly Font ScoreboardTitleFont = new Font("Arial", 18, FontStyle.Bold);

        /// <summary>Шрифт для записей в таблице рекордов</summary>
        public static readonly Font ScoreboardFont = new Font("Arial", 10, FontStyle.Regular);

        /// <summary>Шрифт для курсивных надписей (например, "Пока нет рекордов")</summary>
        public static readonly Font ItalicFont = new Font("Arial", 14, FontStyle.Italic);
    }
}