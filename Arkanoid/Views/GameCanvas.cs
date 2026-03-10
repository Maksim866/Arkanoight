using Arkanoid.Core.Interfaces;

namespace Arkanoid.Views
{
    /// <summary>
    /// Контрол для отображения игры
    /// </summary>
    public class GameCanvas : Control
    {
        private readonly IArkanoidEngine engine;

        /// <summary>
        /// Инициализирует новый экземпляр игрового холста
        /// </summary>
        public GameCanvas(IArkanoidEngine gameEngine)
        {
            engine = gameEngine ?? throw new ArgumentNullException(nameof(gameEngine));
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        /// Обрабатывает событие отрисовки контрола
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

                GameVisuals.Initialize(this, ClientSize.Width, ClientSize.Height);
                GameVisuals.DrawToBuffer(engine, ClientSize);
                GameVisuals.RefreshDisplay();
        }

        /// <summary>
        /// Освобождает ресурсы, используемые контролом
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameVisuals.Cleanup();
            }
            base.Dispose(disposing);
        }
    }
}