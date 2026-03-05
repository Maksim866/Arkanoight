using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Arkanoight.Core;
using Arkanoight.Models;

namespace Arkanoight.Views
{
    /// <summary>
    /// Статический класс для визуализации игры
    /// </summary>
    public static class GameVisuals
    {
        // Цвета для визуализации
        private static readonly Color BackgroundColor = Color.Black;
        private static readonly Color PlatformColor = Color.Cyan;
        private static readonly Color BallColor = Color.Yellow;
        private static readonly Color BrickBorderColor = Color.White;
        private static readonly Color TextColor = Color.White;
        private static readonly Color HintColor = Color.FromArgb(200, Color.Yellow);
        private static readonly Color HintBgColor = Color.FromArgb(100, Color.Black);

        private static readonly Color[] BrickColors = new Color[]
        {
            Color.Red,      // 0 ряд - красный (5 жизней)
            Color.Orange,   // 1 ряд - оранжевый (4 жизни)
            Color.Yellow,   // 2 ряд - желтый (3 жизни)
            Color.Green,    // 3 ряд - зеленый (2 жизни)
            Color.Blue      // 4 ряд - синий (1 жизнь)
        };

        private static readonly Color[] HitBrickColors = new Color[]
{
            Color.FromArgb(255, 255, 150, 150), // Светло-красный
            Color.FromArgb(255, 255, 200, 150), // Светло-оранжевый
            Color.FromArgb(255, 255, 255, 150), // Светло-желтый
            Color.FromArgb(255, 150, 255, 150), // Светло-зеленый
            Color.FromArgb(255, 150, 150, 255)  // Светло-синий
};

        /// <summary>
        /// Рисует всю игру
        /// </summary>
        public static void DrawGame(Graphics g, IArkanoightEngine engine, Size clientSize)
        {
            DrawBackground(g, clientSize);
            DrawPlatform(g, engine.Platform);
            DrawBall(g, engine.Ball);
            DrawBricks(g, engine.Bricks);
            DrawUI(g, engine.GameState, clientSize);
            DrawMessages(g, engine, clientSize);
        }

        /// <summary>
        /// Рисует фон
        /// </summary>
        private static void DrawBackground(Graphics g, Size clientSize)
        {
            using (SolidBrush brush = new SolidBrush(BackgroundColor))
                g.FillRectangle(brush, new Rectangle(0, 0, clientSize.Width, clientSize.Height));
        }

        /// <summary>
        /// Рисует платформу
        /// </summary>
        private static void DrawPlatform(Graphics g, PlatformModel platform)
        {
            using (SolidBrush brush = new SolidBrush(PlatformColor))
                g.FillRectangle(brush, platform.X, platform.Y, platform.Width, platform.Height);
        }

        /// <summary>
        /// Рисует мяч
        /// </summary>
        private static void DrawBall(Graphics g, BallModel ball)
        {
            using (SolidBrush brush = new SolidBrush(BallColor))
                g.FillEllipse(brush, ball.X, ball.Y, ball.Size, ball.Size);
        }

        /// <summary>
        /// Рисует все кирпичи с визуальными эффектами при ударе
        /// </summary>
        private static void DrawBricks(Graphics g, IReadOnlyList<BrickModel> bricks)
        {
            foreach (var brick in bricks)
            {
                if (!brick.IsActive) continue;

                Color brickColor;
                if (brick.IsHit)
                {
                    brickColor = HitBrickColors[brick.Row % HitBrickColors.Length];
                }
                else
                {
                    brickColor = BrickColors[brick.Row % BrickColors.Length];
                }

                using (SolidBrush brush = new SolidBrush(brickColor))
                    g.FillRectangle(brush, brick.X, brick.Y, brick.Width, brick.Height);

                using (Pen pen = new Pen(BrickBorderColor, 1))
                    g.DrawRectangle(pen, brick.X, brick.Y, brick.Width, brick.Height);

                if (brick.Health < brick.MaxHealth && brick.Health > 0)
                {
                    DrawDamageIndicator(g, brick);
                }
            }
        }

        /// <summary>
        /// Рисует индикатор повреждения на кирпиче (трещины)
        /// </summary>
        private static void DrawDamageIndicator(Graphics g, BrickModel brick)
        {
            int damageLevel = brick.MaxHealth - brick.Health;

            using (Pen damagePen = new Pen(Color.FromArgb(150, Color.Black), 2))
            {
                if (damageLevel >= 1)
                {
                    g.DrawLine(damagePen, brick.X + 10, brick.Y + 5,
                              brick.X + brick.Width - 10, brick.Y + brick.Height - 5);
                }

                if (damageLevel >= 2)
                {
                    g.DrawLine(damagePen, brick.X + brick.Width - 10, brick.Y + 5,
                              brick.X + 10, brick.Y + brick.Height - 5);
                }

                if (damageLevel >= 3)
                {
                    g.DrawLine(damagePen, brick.X + 15, brick.Y + brick.Height / 2,
                              brick.X + brick.Width - 15, brick.Y + brick.Height / 2);
                }

                if (damageLevel >= 4)
                {
                    g.DrawLine(damagePen, brick.X + brick.Width / 2, brick.Y + 10,
                              brick.X + brick.Width / 2, brick.Y + brick.Height - 10);
                }

                if (damageLevel >= 5)
                {
                    g.DrawLine(damagePen, brick.X + 20, brick.Y + 15,
                              brick.X + 30, brick.Y + 25);
                    g.DrawLine(damagePen, brick.X + brick.Width - 20, brick.Y + 15,
                              brick.X + brick.Width - 30, brick.Y + 25);
                }
            }
        }

        /// <summary>
        /// Рисует интерфейс (счет и жизни)
        /// </summary>
        private static void DrawUI(Graphics g, GameStateModel gameState, Size clientSize)
        {
            using (Font font = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(TextColor))
            {
                g.DrawString($"Счет: {gameState.Score}", font, brush, 10, 10);
                g.DrawString($"Жизни: {gameState.Lives}", font, brush, clientSize.Width - 100, 10);
            }
        }

        /// <summary>
        /// Рисует сообщения (победа/поражение/подсказки)
        /// </summary>
        private static void DrawMessages(Graphics g, IArkanoightEngine engine, Size clientSize)
        {
            if (engine.GameState.IsGameOver)
                DrawCenteredMessage(g, "ИГРА ОКОНЧЕНА!", "Нажмите R для перезапуска", clientSize);
            else if (engine.GameState.IsGameWon)
                DrawCenteredMessage(g, "ВЫ ПОБЕДИЛИ!", "Нажмите R для новой игры", clientSize);
            else if (!engine.IsBallLaunched)
                DrawLaunchHint(g, clientSize);
        }

        /// <summary>
        /// Рисует центрированное сообщение
        /// </summary>
        private static void DrawCenteredMessage(Graphics g, string main, string sub, Size size)
        {
            using (Font mainFont = new Font("Arial", 24, FontStyle.Bold))
            using (Font subFont = new Font("Arial", 16, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(TextColor))
            {
                SizeF mainSize = g.MeasureString(main, mainFont);
                SizeF subSize = g.MeasureString(sub, subFont);

                float x = (size.Width - mainSize.Width) / 2;
                float y = (size.Height - mainSize.Height - subSize.Height) / 2;

                g.DrawString(main, mainFont, brush, x, y);
                g.DrawString(sub, subFont, brush,
                    (size.Width - subSize.Width) / 2,
                    y + mainSize.Height + 10);
            }
        }

        /// <summary>
        /// Рисует подсказку для запуска мяча
        /// </summary>
        private static void DrawLaunchHint(Graphics g, Size size)
        {
            string hint = "⚫ ЛКМ - запуск мяча | R - перезапуск";
            using (Font font = new Font("Arial", 14, FontStyle.Italic | FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(HintColor))
            using (SolidBrush bgBrush = new SolidBrush(HintBgColor))
            {
                SizeF textSize = g.MeasureString(hint, font);
                float x = (size.Width - textSize.Width) / 2;
                float y = size.Height / 2 - 50;

                g.FillRectangle(bgBrush, x - 10, y - 5, textSize.Width + 20, textSize.Height + 10);
                g.DrawString(hint, font, brush, x, y);
            }
        }
    }

    /// <summary>
    /// Контрол для отображения игры
    /// </summary>
    public class GameCanvas : Control
    {
        private IArkanoightEngine gameEngine;

        /// <summary>
        /// Получает или задает игровой движок
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IArkanoightEngine GameEngine
        {
            get => gameEngine;
            set
            {
                gameEngine = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр игрового холста
        /// </summary>
        public GameCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = Color.Black;
        }

        /// <summary>
        /// Обрабатывает событие отрисовки
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (gameEngine == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            GameVisuals.DrawGame(e.Graphics, gameEngine, ClientSize);
        }

        /// <summary>
        /// Принудительно обновляет отображение
        /// </summary>
        public void ForceRefresh()
        {
            Invalidate();
            Update();
        }
    }
}