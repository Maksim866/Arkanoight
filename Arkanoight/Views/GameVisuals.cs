using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Arkanoight.Core;
using Arkanoight.Models;

namespace Arkanoight.Views
{
    /// <summary>
    /// Класс для визуализации игры с кэшированием ресурсов
    /// </summary>
    public static class GameVisuals
    {
        // Кэшированные цвета
        private static readonly Color[] BrickColors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
        private static readonly Color[] HitColors = {
            Color.FromArgb(255, 255, 150, 150),
            Color.FromArgb(255, 255, 200, 150),
            Color.FromArgb(255, 255, 255, 150),
            Color.FromArgb(255, 150, 255, 150),
            Color.FromArgb(255, 150, 150, 255)
        };

        // Кэшированные кисти и перья
        private static readonly SolidBrush platformBrush = new SolidBrush(Color.Cyan);
        private static readonly SolidBrush ballBrush = new SolidBrush(Color.Yellow);
        private static readonly Pen borderPen = new Pen(Color.White, 1);
        private static readonly Pen damagePen = new Pen(Color.FromArgb(150, Color.Black), 2);
        private static readonly SolidBrush whiteBrush = new SolidBrush(Color.White);
        private static readonly SolidBrush blackBrush = new SolidBrush(Color.Black);
        private static readonly SolidBrush pauseBgBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
        private static readonly SolidBrush hintBgBrush = new SolidBrush(Color.FromArgb(230, 20, 20, 20));
        private static readonly SolidBrush cyanBrush = new SolidBrush(Color.Cyan);
        private static readonly SolidBrush goldBrush = new SolidBrush(Color.Gold);
        private static readonly SolidBrush grayBrush = new SolidBrush(Color.Gray);
        private static readonly SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
        private static readonly SolidBrush redBrush = new SolidBrush(Color.Red);
        private static readonly SolidBrush greenBrush = new SolidBrush(Color.Green);
        private static readonly Pen cyanPen3 = new Pen(Color.Cyan, 3);
        private static readonly Pen goldPen2 = new Pen(Color.Gold, 2);
        private static readonly Pen whitePen1 = new Pen(Color.White, 1);

        // Кэшированные кисти для кирпичей
        private static readonly SolidBrush[] brickBrushes;
        private static readonly SolidBrush[] hitBrushes;

        // Кэшированные шрифты
        private static readonly Font scoreFont = new Font("Arial", 14, FontStyle.Bold);
        private static readonly Font pauseFont = new Font("Arial", 48, FontStyle.Bold);
        private static readonly Font continueFont = new Font("Arial", 18, FontStyle.Regular);
        private static readonly Font titleFont = new Font("Arial", 26, FontStyle.Bold);
        private static readonly Font controlFont = new Font("Arial", 12, FontStyle.Regular);
        private static readonly Font powerFont = new Font("Arial", 11, FontStyle.Regular);
        private static readonly Font symbolFont = new Font("Arial", 14, FontStyle.Bold);
        private static readonly Font gameOverFont = new Font("Arial", 28, FontStyle.Bold);
        private static readonly Font scoreboardTitleFont = new Font("Arial", 18, FontStyle.Bold);
        private static readonly Font scoreboardFont = new Font("Arial", 10, FontStyle.Regular);
        private static readonly Font italicFont = new Font("Arial", 14, FontStyle.Italic);

        // Буфер
        private static Bitmap buffer;
        private static int lastWidth, lastHeight;
        private static bool bufferDirty = true;
        private static Control targetControl;

        // Статический конструктор для инициализации массивов кистей
        static GameVisuals()
        {
            brickBrushes = new SolidBrush[BrickColors.Length];
            for (int i = 0; i < BrickColors.Length; i++)
                brickBrushes[i] = new SolidBrush(BrickColors[i]);

            hitBrushes = new SolidBrush[HitColors.Length];
            for (int i = 0; i < HitColors.Length; i++)
                hitBrushes[i] = new SolidBrush(HitColors[i]);
        }

        /// <summary>Инициализирует буфер и привязывает к контролу</summary>
        public static void Initialize(Control control, int width, int height)
        {
            targetControl = control;

            if (buffer != null && lastWidth == width && lastHeight == height)
                return;

            buffer?.Dispose();
            buffer = new Bitmap(width, height);
            lastWidth = width;
            lastHeight = height;
            bufferDirty = true;
        }

        /// <summary>Рисует игру в буфер</summary>
        public static void DrawToBuffer(IArkanoightEngine e, Size cs)
        {
            if (buffer == null) return;

            using (var g = Graphics.FromImage(buffer))
            {
                g.Clear(Color.Black);

                // Платформа
                g.FillRectangle(platformBrush, e.Platform.X, e.Platform.Y, e.Platform.Width, e.Platform.Height);

                // Мячи
                foreach (var ball in e.Balls.Where(b => b.IsActive))
                    g.FillEllipse(ballBrush, ball.X, ball.Y, ball.Size, ball.Size);

                // Кирпичи
                foreach (var br in e.Bricks.Where(b => b.IsActive))
                {
                    var brush = br.IsHit ? hitBrushes[br.Row % hitBrushes.Length] : brickBrushes[br.Row % brickBrushes.Length];
                    g.FillRectangle(brush, br.X, br.Y, br.Width, br.Height);
                    g.DrawRectangle(borderPen, br.X, br.Y, br.Width, br.Height);

                    if (br.Health < br.MaxHealth)
                        for (int i = 0; i < br.MaxHealth - br.Health; i++)
                            g.DrawLine(damagePen, br.X + 10 + i * 10, br.Y + 5,
                                br.X + br.Width - 10 - i * 10, br.Y + br.Height - 5);
                }

                // Усиления - с центрированными значками
                foreach (var u in e.PowerUps.Where(u => u.IsActive))
                {
                    SolidBrush colorBrush;
                    string sym;

                    if (u.Type == PowerUpType.ExtraBall)
                    {
                        colorBrush = cyanBrush;
                        sym = "⚽";
                    }
                    else if (u.Type == PowerUpType.DamageBoost)
                    {
                        colorBrush = redBrush;
                        sym = "⚡";
                    }
                    else
                    {
                        colorBrush = greenBrush;
                        sym = "⬌";
                    }

                    // Рисуем квадратик
                    g.FillRectangle(colorBrush, u.X, u.Y, u.Size, u.Size);
                    g.DrawRectangle(whitePen1, u.X, u.Y, u.Size, u.Size);

                    // Центрируем символ
                    SizeF textSize = g.MeasureString(sym, symbolFont);
                    float textX = u.X + (u.Size - textSize.Width) / 2;
                    float textY = u.Y + (u.Size - textSize.Height) / 2;
                    g.DrawString(sym, symbolFont, blackBrush, textX, textY);
                }

                // UI
                g.DrawString($"Счет: {e.GameState.Score}", scoreFont, whiteBrush, 10, 10);
                g.DrawString($"Жизни: {e.GameState.Lives}", scoreFont, whiteBrush, cs.Width - 100, 10);

                // Специальные экраны
                if (e.GameState.IsPaused)
                    DrawPauseScreen(g, cs);
                else if (!e.IsBallLaunched && !e.GameState.IsGameOver && !e.GameState.IsGameWon)
                    DrawStartScreen(g, cs);
                else if (e.GameState.IsGameOver || e.GameState.IsGameWon)
                    DrawGameOverScreen(g, e, cs);
            }

            bufferDirty = false;
        }

        private static void DrawPauseScreen(Graphics g, Size cs)
        {
            g.FillRectangle(pauseBgBrush, 0, 0, cs.Width, cs.Height);
            g.DrawString("ПАУЗА", pauseFont, whiteBrush, (cs.Width - g.MeasureString("ПАУЗА", pauseFont).Width) / 2, cs.Height / 2 - 30);
            g.DrawString("Нажмите ПРОБЕЛ для продолжения", continueFont, yellowBrush,
                (cs.Width - g.MeasureString("Нажмите ПРОБЕЛ для продолжения", continueFont).Width) / 2, cs.Height / 2 + 30);
        }

        private static void DrawStartScreen(Graphics g, Size cs)
        {
            int w = 600, h = 450, x = (cs.Width - w) / 2, y = (cs.Height - h) / 2 - 20;
            g.FillRectangle(hintBgBrush, x, y, w, h);
            g.DrawRectangle(cyanPen3, x, y, w, h);
            g.DrawString("АРКАНОИД", titleFont, cyanBrush, x + 150, y + 25);

            g.DrawString("• ЛКМ - запуск мяча", controlFont, whiteBrush, x + 50, y + 120);
            g.DrawString("• Движение мыши - управление платформой", controlFont, whiteBrush, x + 50, y + 145);
            g.DrawString("• Пробел - пауза / продолжить", controlFont, whiteBrush, x + 50, y + 170);
            g.DrawString("• R - перезапуск (после победы/поражения)", controlFont, whiteBrush, x + 50, y + 195);

            int sy = y + 280;
            var types = new[] {
                (cyanBrush, "⚽", "Голубой - дополнительный мяч"),
                (redBrush, "⚡", "Красный - увеличение урона всех мячей на 1"),
                (greenBrush, "⬌", "Зеленый - широкая платформа на 3 секунды")
            };

            for (int i = 0; i < 3; i++)
            {
                int squareX = x + 40;
                int squareY = sy + i * 45;
                int squareSize = 25;

                // Рисуем квадратик
                g.FillRectangle(types[i].Item1, squareX, squareY, squareSize, squareSize);
                g.DrawRectangle(whitePen1, squareX, squareY, squareSize, squareSize);

                // Центрируем символ в квадратике
                SizeF textSize = g.MeasureString(types[i].Item2, symbolFont);
                float textX = squareX + (squareSize - textSize.Width) / 2;
                float textY = squareY + (squareSize - textSize.Height) / 2;
                g.DrawString(types[i].Item2, symbolFont, blackBrush, textX, textY);

                // Текст описания
                g.DrawString(types[i].Item3, powerFont, whiteBrush, x + 75, squareY + 5);
                g.DrawString(i == 0 ? "(появляется на платформе)" :
                           i == 1 ? "(суммируется)" : "(временный эффект)", powerFont, whiteBrush, x + 75, squareY + 22);
            }
        }

        private static void DrawGameOverScreen(Graphics g, IArkanoightEngine e, Size cs)
        {
            var scores = ScoreManager.GetScores();
            string msg = e.GameState.IsGameOver ? "ИГРА ОКОНЧЕНА!" : "ВЫ ПОБЕДИЛИ!";
            var msgBrush = e.GameState.IsGameOver ? redBrush : goldBrush;
            g.DrawString(msg, gameOverFont, msgBrush, (cs.Width - g.MeasureString(msg, gameOverFont).Width) / 2, 50);

            int bw = 400, bh = 280, bx = (cs.Width - bw) / 2, by = (cs.Height - bh) / 2 + 30;
            g.FillRectangle(grayBrush, bx, by, bw, bh);
            g.DrawRectangle(goldPen2, bx, by, bw, bh);
            g.DrawString("ТАБЛИЦА РЕКОРДОВ", scoreboardTitleFont, goldBrush, bx + 80, by + 10);

            if (scores.Count == 0)
            {
                g.DrawString("Пока нет рекордов", italicFont, whiteBrush, bx + 110, by + 120);
            }
            else for (int i = 0; i < scores.Count; i++)
            {
                g.DrawString($"{i + 1}. {scores[i].PlayerName}", scoreboardFont, whiteBrush, bx + 20, by + 80 + i * 20);
                g.DrawString(scores[i].Score.ToString(), scoreboardFont, whiteBrush, bx + 200, by + 80 + i * 20);

                var typeBrush = scores[i].GameEndType == "Победа" ? goldBrush : redBrush;
                g.DrawString(scores[i].GameEndType, scoreboardFont, typeBrush, bx + 280, by + 80 + i * 20);
            }
        }

        /// <summary>Помечает буфер как устаревший</summary>
        public static void MarkDirty()
        {
            bufferDirty = true;
        }

        /// <summary>Принудительно обновляет отображение из буфера</summary>
        public static void RefreshDisplay()
        {
            if (targetControl != null && buffer != null && !targetControl.IsDisposed)
            {
                using (var g = targetControl.CreateGraphics())
                {
                    g.DrawImage(buffer, 0, 0);
                }
            }
        }

        /// <summary>Полностью перерисовывает и отображает</summary>
        public static void Render(IArkanoightEngine e, Size cs)
        {
            if (buffer == null) return;

            DrawToBuffer(e, cs);
            RefreshDisplay();
        }

        /// <summary>Очищает ресурсы</summary>
        public static void Cleanup()
        {
            buffer?.Dispose();
            buffer = null;
            targetControl = null;

            // Освобождаем кэшированные ресурсы
            platformBrush.Dispose();
            ballBrush.Dispose();
            borderPen.Dispose();
            damagePen.Dispose();
            whiteBrush.Dispose();
            blackBrush.Dispose();
            pauseBgBrush.Dispose();
            hintBgBrush.Dispose();
            cyanBrush.Dispose();
            goldBrush.Dispose();
            grayBrush.Dispose();
            yellowBrush.Dispose();
            redBrush.Dispose();
            greenBrush.Dispose();
            cyanPen3.Dispose();
            goldPen2.Dispose();
            whitePen1.Dispose();

            scoreFont.Dispose();
            pauseFont.Dispose();
            continueFont.Dispose();
            titleFont.Dispose();
            controlFont.Dispose();
            powerFont.Dispose();
            symbolFont.Dispose();
            gameOverFont.Dispose();
            scoreboardTitleFont.Dispose();
            scoreboardFont.Dispose();
            italicFont.Dispose();

            foreach (var brush in brickBrushes) brush.Dispose();
            foreach (var brush in hitBrushes) brush.Dispose();
        }
    }

    /// <summary>
    /// Контрол для отображения игры
    /// </summary>
    public class GameCanvas : Control
    {
        private IArkanoightEngine engine;

        /// <summary>Игровой движок</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IArkanoightEngine GameEngine
        {
            get => engine;
            set { engine = value; }
        }

        /// <summary>Конструктор</summary>
        public GameCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>Отрисовка контрола</summary>
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

        /// <summary>Освобождение ресурсов</summary>
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