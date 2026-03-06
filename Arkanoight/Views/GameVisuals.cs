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
        // Цвета
        private static readonly Color[] BrickColors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
        private static readonly Color[] HitColors = {
            Color.FromArgb(255, 255, 150, 150),
            Color.FromArgb(255, 255, 200, 150),
            Color.FromArgb(255, 255, 255, 150),
            Color.FromArgb(255, 150, 255, 150),
            Color.FromArgb(255, 150, 150, 255)
        };

        // Кисти
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

        // Кисти для кирпичей
        private static readonly SolidBrush[] brickBrushes;
        private static readonly SolidBrush[] hitBrushes;

        // Шрифты
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

        /// <summary>Статический конструктор для инициализации массивов кистей</summary>
        static GameVisuals()
        {
            brickBrushes = new SolidBrush[BrickColors.Length];
            for (var i = 0; i < BrickColors.Length; i++)
                brickBrushes[i] = new SolidBrush(BrickColors[i]);

            hitBrushes = new SolidBrush[HitColors.Length];
            for (var i = 0; i < HitColors.Length; i++)
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
        public static void DrawToBuffer(IArkanoightEngine engine, Size clientSize)
        {
            if (buffer == null) return;

            using (var graphics = Graphics.FromImage(buffer))
            {
                graphics.Clear(Color.Black);

                // Платформа
                graphics.FillRectangle(platformBrush, engine.Platform.X, engine.Platform.Y, engine.Platform.Width, engine.Platform.Height);

                // Мячи
                foreach (var ball in engine.Balls.Where(b => b.IsActive))
                    graphics.FillEllipse(ballBrush, ball.X, ball.Y, ball.Size, ball.Size);

                // Кирпичи
                foreach (var brick in engine.Bricks.Where(b => b.IsActive))
                {
                    var brush = brick.IsHit ? hitBrushes[brick.Row % hitBrushes.Length] : brickBrushes[brick.Row % brickBrushes.Length];
                    graphics.FillRectangle(brush, brick.X, brick.Y, brick.Width, brick.Height);
                    graphics.DrawRectangle(borderPen, brick.X, brick.Y, brick.Width, brick.Height);

                    if (brick.Health < brick.MaxHealth)
                        for (var i = 0; i < brick.MaxHealth - brick.Health; i++)
                            graphics.DrawLine(damagePen, brick.X + 10 + i * 10, brick.Y + 5,
                                brick.X + brick.Width - 10 - i * 10, brick.Y + brick.Height - 5);
                }

                // Усиления
                foreach (var powerUp in engine.PowerUps.Where(p => p.IsActive))
                {
                    SolidBrush colorBrush;
                    string symbol;

                    if (powerUp.Type == PowerUpType.ExtraBall)
                    {
                        colorBrush = cyanBrush;
                        symbol = "⚽";
                    }
                    else if (powerUp.Type == PowerUpType.DamageBoost)
                    {
                        colorBrush = redBrush;
                        symbol = "⚡";
                    }
                    else
                    {
                        colorBrush = greenBrush;
                        symbol = "⬌";
                    }

                    graphics.FillRectangle(colorBrush, powerUp.X, powerUp.Y, powerUp.Size, powerUp.Size);
                    graphics.DrawRectangle(whitePen1, powerUp.X, powerUp.Y, powerUp.Size, powerUp.Size);

                    var textSize = graphics.MeasureString(symbol, symbolFont);
                    var textX = powerUp.X + (powerUp.Size - textSize.Width) / 2;
                    var textY = powerUp.Y + (powerUp.Size - textSize.Height) / 2;
                    graphics.DrawString(symbol, symbolFont, blackBrush, textX, textY);
                }

                // UI
                graphics.DrawString($"Счет: {engine.GameState.Score}", scoreFont, whiteBrush, 10, 10);
                graphics.DrawString($"Жизни: {engine.GameState.Lives}", scoreFont, whiteBrush, clientSize.Width - 100, 10);

                // Специальные экраны
                if (engine.GameState.IsPaused)
                    DrawPauseScreen(graphics, clientSize);
                else if (!engine.IsBallLaunched && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon)
                    DrawStartScreen(graphics, clientSize);
                else if (engine.GameState.IsGameOver || engine.GameState.IsGameWon)
                    DrawGameOverScreen(graphics, engine, clientSize);
            }

            bufferDirty = false;
        }

        /// <summary>Рисует экран паузы</summary>
        private static void DrawPauseScreen(Graphics graphics, Size clientSize)
        {
            graphics.FillRectangle(pauseBgBrush, 0, 0, clientSize.Width, clientSize.Height);
            graphics.DrawString("ПАУЗА", pauseFont, whiteBrush,
                (clientSize.Width - graphics.MeasureString("ПАУЗА", pauseFont).Width) / 2,
                clientSize.Height / 2 - 30);
            graphics.DrawString("Нажмите ПРОБЕЛ для продолжения", continueFont, yellowBrush,
                (clientSize.Width - graphics.MeasureString("Нажмите ПРОБЕЛ для продолжения", continueFont).Width) / 2,
                clientSize.Height / 2 + 30);
        }

        /// <summary>Рисует стартовый экран с подсказками</summary>
        private static void DrawStartScreen(Graphics graphics, Size clientSize)
        {
            var boxWidth = 600;
            var boxHeight = 450;
            var boxX = (clientSize.Width - boxWidth) / 2;
            var boxY = (clientSize.Height - boxHeight) / 2 - 20;

            graphics.FillRectangle(hintBgBrush, boxX, boxY, boxWidth, boxHeight);
            graphics.DrawRectangle(cyanPen3, boxX, boxY, boxWidth, boxHeight);
            graphics.DrawString("АРКАНОИД", titleFont, cyanBrush, boxX + 150, boxY + 25);

            graphics.DrawString("• ЛКМ - запуск мяча", controlFont, whiteBrush, boxX + 50, boxY + 120);
            graphics.DrawString("• Движение мыши - управление платформой", controlFont, whiteBrush, boxX + 50, boxY + 145);
            graphics.DrawString("• Пробел - пауза / продолжить", controlFont, whiteBrush, boxX + 50, boxY + 170);
            graphics.DrawString("• R - перезапуск (после победы/поражения)", controlFont, whiteBrush, boxX + 50, boxY + 195);

            var startY = boxY + 280;
            var types = new[] {
                (cyanBrush, "⚽", "Голубой - дополнительный мяч"),
                (redBrush, "⚡", "Красный - увеличение урона всех мячей на 1"),
                (greenBrush, "⬌", "Зеленый - широкая платформа на 3 секунды")
            };

            for (var i = 0; i < types.Length; i++)
            {
                var squareX = boxX + 40;
                var squareY = startY + i * 45;
                var squareSize = 25;

                graphics.FillRectangle(types[i].Item1, squareX, squareY, squareSize, squareSize);
                graphics.DrawRectangle(whitePen1, squareX, squareY, squareSize, squareSize);

                var textSize = graphics.MeasureString(types[i].Item2, symbolFont);
                var textX = squareX + (squareSize - textSize.Width) / 2;
                var textY = squareY + (squareSize - textSize.Height) / 2;
                graphics.DrawString(types[i].Item2, symbolFont, blackBrush, textX, textY);

                graphics.DrawString(types[i].Item3, powerFont, whiteBrush, boxX + 75, squareY + 5);
                graphics.DrawString(i == 0 ? "(появляется на платформе)" :
                                   i == 1 ? "(суммируется)" : "(временный эффект)",
                                   powerFont, whiteBrush, boxX + 75, squareY + 22);
            }
        }

        /// <summary>Рисует экран окончания игры с таблицей рекордов</summary>
        private static void DrawGameOverScreen(Graphics graphics, IArkanoightEngine engine, Size clientSize)
        {
            var scores = ScoreManager.GetScores();
            var message = engine.GameState.IsGameOver ? "ИГРА ОКОНЧЕНА!" : "ВЫ ПОБЕДИЛИ!";
            var messageBrush = engine.GameState.IsGameOver ? redBrush : goldBrush;

            graphics.DrawString(message, gameOverFont, messageBrush,
                (clientSize.Width - graphics.MeasureString(message, gameOverFont).Width) / 2, 50);

            var boardWidth = 400;
            var boardHeight = 280;
            var boardX = (clientSize.Width - boardWidth) / 2;
            var boardY = (clientSize.Height - boardHeight) / 2 + 30;

            graphics.FillRectangle(grayBrush, boardX, boardY, boardWidth, boardHeight);
            graphics.DrawRectangle(goldPen2, boardX, boardY, boardWidth, boardHeight);
            graphics.DrawString("ТАБЛИЦА РЕКОРДОВ", scoreboardTitleFont, goldBrush, boardX + 80, boardY + 10);

            if (scores.Count == 0)
            {
                graphics.DrawString("Пока нет рекордов", italicFont, whiteBrush, boardX + 110, boardY + 120);
            }
            else for (var i = 0; i < scores.Count; i++)
            {
                var yPos = boardY + 80 + i * 20;
                graphics.DrawString($"{i + 1}. {scores[i].PlayerName}", scoreboardFont, whiteBrush, boardX + 20, yPos);
                graphics.DrawString(scores[i].Score.ToString(), scoreboardFont, whiteBrush, boardX + 200, yPos);

                var typeBrush = scores[i].GameEndType == "Победа" ? goldBrush : redBrush;
                graphics.DrawString(scores[i].GameEndType, scoreboardFont, typeBrush, boardX + 280, yPos);
            }
        }

        /// <summary>Помечает буфер как устаревший (требующий перерисовки)</summary>
        public static void MarkDirty() => bufferDirty = true;

        /// <summary>Принудительно обновляет отображение из буфера</summary>
        public static void RefreshDisplay()
        {
            if (targetControl != null && buffer != null && !targetControl.IsDisposed)
            {
                using (var graphics = targetControl.CreateGraphics())
                {
                    graphics.DrawImage(buffer, 0, 0);
                }
            }
        }

        /// <summary>Полностью перерисовывает и отображает игру</summary>
        public static void Render(IArkanoightEngine engine, Size clientSize)
        {
            if (buffer == null) return;

            DrawToBuffer(engine, clientSize);
            RefreshDisplay();
        }

        /// <summary>Очищает все кэшированные ресурсы</summary>
        public static void Cleanup()
        {
            buffer?.Dispose();
            buffer = null;
            targetControl = null;

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

        /// <summary>Получает или задает игровой движок</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IArkanoightEngine GameEngine
        {
            get => engine;
            set { engine = value; }
        }

        /// <summary>Инициализирует новый экземпляр игрового холста</summary>
        public GameCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        /// <summary>Обрабатывает событие отрисовки контрола</summary>
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

        /// <summary>Освобождает ресурсы, используемые контролом</summary>
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