namespace Arkanoight.Models
{
    /// <summary>
    /// Модель кирпича
    /// </summary>
    public class BrickModel
    {
        /// <summary>
        /// Получает или задает координату X левого верхнего угла кирпича
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Получает или задает координату Y левого верхнего угла кирпича
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Получает или задает ширину кирпича в пикселях
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Получает или задает высоту кирпича в пикселях
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Получает или задает активность кирпича (true - существует, false - разрушен)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Получает или задает номер ряда кирпича (для определения цвета)
        /// </summary>
        public int Row { get; set; }
    }
}