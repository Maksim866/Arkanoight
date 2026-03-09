using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Arkanoid.Core.Interfaces;

namespace Arkanoid.Views
{
    /// <summary>
    /// Контрол для отображения игры
    /// </summary>
    public class GameCanvas : Control
    {
        private IArkanoidEngine engine;

        /// <summary>
        /// Получает или задает игровой движок
        /// </summary>
        public IArkanoidEngine GameEngine
        {
            get => engine;
            set { engine = value; }
        }

        /// <summary>
        /// Инициализирует новый экземпляр игрового холста
        /// </summary>
        public GameCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>
        /// Обрабатывает событие отрисовки контрола
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (engine != null)
            {
                GameVisuals.Initialize(this, ClientSize.Width, ClientSize.Height);
                GameVisuals.DrawToBuffer(engine, ClientSize);
                GameVisuals.RefreshDisplay();
            }
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