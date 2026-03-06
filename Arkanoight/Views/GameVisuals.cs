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
        private static readonly Color HintBgColor = Color.FromArgb(100, Color.Black);
        private static readonly Color ScoreBoardColor = Color.FromArgb(50, 50, 50);
        private static readonly Color ScoreBoardBorderColor = Color.Gold;

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
            Color.FromArgb(255, 255, 150, 150),
            Color.FromArgb(255, 255, 200, 150),
            Color.FromArgb(255, 255, 255, 150),
            Color.FromArgb(255, 150, 255, 150),
            Color.FromArgb(255, 150, 150, 255)
        };

        /// <summary>
        /// Рисует всю игру
        /// </summary>
        public static void DrawGame(Graphics g, IArkanoightEngine engine, Size clientSize)
        {
            DrawBackground(g, clientSize);
            DrawPlatform(g, engine.Platform);

            foreach (var ball in engine.Balls)
            {
                if (ball.IsActive)
                    DrawBall(g, ball);
            }

            DrawBricks(g, engine.Bricks);
            DrawPowerUps(g, engine.PowerUps);
            DrawUI(g, engine.GameState, clientSize);

            // Если игра на паузе - показываем оверлей
            if (engine.GameState.IsPaused)
            {
                DrawPauseOverlay(g, clientSize);
            }

            // Если игра не запущена (мяч на платформе) - показываем подсказки
            if (!engine.IsBallLaunched && !engine.GameState.IsGameOver && !engine.GameState.IsGameWon && !engine.GameState.IsPaused)
            {
                DrawControlsHint(g, clientSize);
            }

            // Если игра завершена - показываем сообщение и таблицу рекордов
            if (engine.GameState.IsGameOver || engine.GameState.IsGameWon)
            {
                DrawGameOverlay(g, engine, clientSize);
            }
        }

        private static void DrawBackground(Graphics g, Size clientSize)
        {
            using (SolidBrush brush = new SolidBrush(BackgroundColor))
                g.FillRectangle(brush, new Rectangle(0, 0, clientSize.Width, clientSize.Height));
        }

        private static void DrawPlatform(Graphics g, PlatformModel platform)
        {
            using (SolidBrush brush = new SolidBrush(PlatformColor))
                g.FillRectangle(brush, platform.X, platform.Y, platform.Width, platform.Height);
        }

        private static void DrawBall(Graphics g, BallModel ball)
        {
            using (SolidBrush brush = new SolidBrush(BallColor))
                g.FillEllipse(brush, ball.X, ball.Y, ball.Size, ball.Size);
        }

        private static void DrawBricks(Graphics g, IReadOnlyList<BrickModel> bricks)
        {
            foreach (var brick in bricks)
            {
                if (!brick.IsActive) continue;

                Color brickColor = brick.IsHit
                    ? HitBrickColors[brick.Row % HitBrickColors.Length]
                    : BrickColors[brick.Row % BrickColors.Length];

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
        /// Рисует все падающие усиления с символами
        /// </summary>
        private static void DrawPowerUps(Graphics g, List<PowerUpModel> powerUps)
        {
            foreach (var powerUp in powerUps)
            {
                if (!powerUp.IsActive) continue;

                Color powerUpColor;
                string symbol = "";

                switch (powerUp.Type)
                {
                    case PowerUpType.ExtraBall:
                        powerUpColor = Color.Cyan;
                        symbol = "⚽";
                        break;
                    case PowerUpType.DamageBoost:
                        powerUpColor = Color.Red;
                        symbol = "⚡";
                        break;
                    case PowerUpType.WidePaddle:
                        powerUpColor = Color.Green;
                        symbol = "⬌";
                        break;
                    default:
                        powerUpColor = Color.White;
                        symbol = "?";
                        break;
                }

                // Рисуем цветной квадратик
                using (SolidBrush brush = new SolidBrush(powerUpColor))
                {
                    g.FillRectangle(brush, powerUp.X, powerUp.Y, powerUp.Size, powerUp.Size);
                }

                // Рисуем белую рамку
                using (Pen pen = new Pen(Color.White, 1))
                {
                    g.DrawRectangle(pen, powerUp.X, powerUp.Y, powerUp.Size, powerUp.Size);
                }

                // Рисуем символ черным цветом
                using (Font font = new Font("Arial", 12, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    g.DrawString(symbol, font, brush, powerUp.X + 2, powerUp.Y + 2);
                }
            }
        }

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
        /// Рисует подсказки по управлению в центре экрана при запуске
        /// </summary>
        private static void DrawControlsHint(Graphics g, Size clientSize)
        {
            int boxWidth = 600;
            int boxHeight = 450;
            int boxX = (clientSize.Width - boxWidth) / 2;
            int boxY = (clientSize.Height - boxHeight) / 2 - 20;

            // Полупрозрачный фон
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(230, 20, 20, 20)))
            {
                g.FillRectangle(bgBrush, boxX, boxY, boxWidth, boxHeight);
            }

            // Рамка
            using (Pen pen = new Pen(Color.Cyan, 3))
            {
                g.DrawRectangle(pen, boxX, boxY, boxWidth, boxHeight);
            }

            // Заголовок
            using (Font titleFont = new Font("Arial", 26, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.Cyan))
            {
                string title = "АРКАНОИД";
                SizeF titleSize = g.MeasureString(title, titleFont);
                float titleX = boxX + (boxWidth - titleSize.Width) / 2;
                g.DrawString(title, titleFont, titleBrush, titleX, boxY + 25);
            }

            // Разделительная линия 1
            using (Pen pen = new Pen(Color.Cyan, 1))
            {
                g.DrawLine(pen, boxX + 30, boxY + 70, boxX + boxWidth - 30, boxY + 70);
            }

            // Подзаголовок "Управление"
            using (Font subFont = new Font("Arial", 18, FontStyle.Bold))
            using (SolidBrush subBrush = new SolidBrush(Color.White))
            {
                g.DrawString("УПРАВЛЕНИЕ", subFont, subBrush, boxX + 30, boxY + 85);
            }

            // Подсказки по управлению
            using (Font font = new Font("Arial", 12, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                g.DrawString("• ЛКМ - запуск мяча", font, brush, boxX + 50, boxY + 120);
                g.DrawString("• Движение мыши - управление платформой", font, brush, boxX + 50, boxY + 145);
                g.DrawString("• Пробел - пауза / продолжить", font, brush, boxX + 50, boxY + 170);
                g.DrawString("• R - перезапуск (после победы/поражения)", font, brush, boxX + 50, boxY + 195);
            }

            // Разделительная линия 2
            using (Pen pen = new Pen(Color.Cyan, 1))
            {
                g.DrawLine(pen, boxX + 30, boxY + 225, boxX + boxWidth - 30, boxY + 225);
            }

            // Подзаголовок "Усиления"
            using (Font subFont = new Font("Arial", 18, FontStyle.Bold))
            using (SolidBrush subBrush = new SolidBrush(Color.Yellow))
            {
                g.DrawString("УСИЛЕНИЯ", subFont, subBrush, boxX + 30, boxY + 240);
            }

            // Рисуем квадратики усилений с символами (такие же как в игре)
            int squareSize = 25;
            int startY = boxY + 280;

            // Голубой - дополнительный мяч (⚽)
            using (SolidBrush colorBrush = new SolidBrush(Color.Cyan))
            {
                g.FillRectangle(colorBrush, boxX + 40, startY, squareSize, squareSize);
            }
            using (Pen pen = new Pen(Color.White, 1))
            {
                g.DrawRectangle(pen, boxX + 40, startY, squareSize, squareSize);
            }
            using (Font symbolFont = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush symbolBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("⚽", symbolFont, symbolBrush, boxX + 45, startY + 2);
            }

            // Красный - увеличение урона (⚡)
            using (SolidBrush colorBrush = new SolidBrush(Color.Red))
            {
                g.FillRectangle(colorBrush, boxX + 40, startY + 45, squareSize, squareSize);
            }
            using (Pen pen = new Pen(Color.White, 1))
            {
                g.DrawRectangle(pen, boxX + 40, startY + 45, squareSize, squareSize);
            }
            using (Font symbolFont = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush symbolBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("⚡", symbolFont, symbolBrush, boxX + 47, startY + 47);
            }

            // Зеленый - широкая платформа (⬌)
            using (SolidBrush colorBrush = new SolidBrush(Color.Green))
            {
                g.FillRectangle(colorBrush, boxX + 40, startY + 90, squareSize, squareSize);
            }
            using (Pen pen = new Pen(Color.White, 1))
            {
                g.DrawRectangle(pen, boxX + 40, startY + 90, squareSize, squareSize);
            }
            using (Font symbolFont = new Font("Arial", 14, FontStyle.Bold))
            using (SolidBrush symbolBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("⬌", symbolFont, symbolBrush, boxX + 45, startY + 92);
            }

            // Текстовые описания усилений
            using (Font powerFont = new Font("Arial", 11, FontStyle.Regular))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                // Голубой
                g.DrawString("Голубой ⚽ - дополнительный мяч", powerFont, textBrush, boxX + 75, startY + 5);
                g.DrawString("(появляется на платформе)", powerFont, textBrush, boxX + 75, startY + 22);

                // Красный
                g.DrawString("Красный ⚡ - увеличение урона всех мячей на 1", powerFont, textBrush, boxX + 75, startY + 50);
                g.DrawString("(суммируется)", powerFont, textBrush, boxX + 75, startY + 67);

                // Зеленый
                g.DrawString("Зеленый ⬌ - широкая платформа на 3 секунды", powerFont, textBrush, boxX + 75, startY + 95);
                g.DrawString("(временный эффект)", powerFont, textBrush, boxX + 75, startY + 112);
            }
        }

        /// <summary>
        /// Рисует оверлей паузы
        /// </summary>
        private static void DrawPauseOverlay(Graphics g, Size clientSize)
        {
            string pauseText = "ПАУЗА";
            string continueText = "Нажмите ПРОБЕЛ для продолжения";

            // Полупрозрачный затемняющий фон
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
            {
                g.FillRectangle(bgBrush, 0, 0, clientSize.Width, clientSize.Height);
            }

            using (Font font = new Font("Arial", 48, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(230, Color.White)))
            {
                SizeF textSize = g.MeasureString(pauseText, font);
                float x = (clientSize.Width - textSize.Width) / 2;
                float y = (clientSize.Height - textSize.Height) / 2 - 30;
                g.DrawString(pauseText, font, brush, x, y);
            }

            using (Font font = new Font("Arial", 18, FontStyle.Regular))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(230, Color.Yellow)))
            {
                SizeF textSize = g.MeasureString(continueText, font);
                float x = (clientSize.Width - textSize.Width) / 2;
                float y = (clientSize.Height - textSize.Height) / 2 + 30;
                g.DrawString(continueText, font, brush, x, y);
            }
        }

        /// <summary>
        /// Рисует таблицу рекордов
        /// </summary>
        private static void DrawScoreBoard(Graphics g, Size clientSize)
        {
            var scores = ScoreManager.GetScores();

            int boardWidth = 400;
            int boardHeight = 280;
            int boardX = (clientSize.Width - boardWidth) / 2;
            int boardY = (clientSize.Height - boardHeight) / 2 + 30;

            // Фон таблицы
            using (SolidBrush bgBrush = new SolidBrush(ScoreBoardColor))
            {
                g.FillRectangle(bgBrush, boardX, boardY, boardWidth, boardHeight);
            }

            // Рамка
            using (Pen pen = new Pen(ScoreBoardBorderColor, 2))
            {
                g.DrawRectangle(pen, boardX, boardY, boardWidth, boardHeight);
            }

            // Заголовок
            using (Font titleFont = new Font("Arial", 18, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(ScoreBoardBorderColor))
            {
                g.DrawString("ТАБЛИЦА РЕКОРДОВ", titleFont, titleBrush, boardX + 80, boardY + 10);
            }

            // Заголовки колонок
            using (Font headerFont = new Font("Arial", 11, FontStyle.Bold))
            using (SolidBrush headerBrush = new SolidBrush(Color.White))
            {
                g.DrawString("№", headerFont, headerBrush, boardX + 20, boardY + 45);
                g.DrawString("Игрок", headerFont, headerBrush, boardX + 50, boardY + 45);
                g.DrawString("Счет", headerFont, headerBrush, boardX + 200, boardY + 45);
                g.DrawString("Тип", headerFont, headerBrush, boardX + 280, boardY + 45);
            }

            // Разделительная линия
            using (Pen pen = new Pen(Color.Gray))
            {
                g.DrawLine(pen, boardX + 10, boardY + 65, boardX + boardWidth - 10, boardY + 65);
            }

            // Список рекордов
            if (scores.Count == 0)
            {
                using (Font font = new Font("Arial", 14, FontStyle.Italic))
                using (SolidBrush brush = new SolidBrush(Color.Gray))
                {
                    g.DrawString("Пока нет рекордов", font, brush, boardX + 110, boardY + 120);
                    g.DrawString("Сыграйте игру, чтобы появились результаты!", font, brush, boardX + 40, boardY + 150);
                }
            }
            else
            {
                using (Font font = new Font("Arial", 10, FontStyle.Regular))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    for (int i = 0; i < scores.Count; i++)
                    {
                        int yPos = boardY + 80 + i * 20;
                        g.DrawString($"{i + 1}.", font, brush, boardX + 20, yPos);
                        g.DrawString(scores[i].PlayerName, font, brush, boardX + 50, yPos);
                        g.DrawString(scores[i].Score.ToString(), font, brush, boardX + 200, yPos);

                        Color typeColor = scores[i].GameEndType == "Победа" ? Color.Gold : Color.LightCoral;
                        using (SolidBrush typeBrush = new SolidBrush(typeColor))
                        {
                            g.DrawString(scores[i].GameEndType, font, typeBrush, boardX + 280, yPos);
                        }
                    }
                }
            }
        }

        private static void DrawGameOverlay(Graphics g, IArkanoightEngine engine, Size clientSize)
        {
            string message = engine.GameState.IsGameOver ? "ИГРА ОКОНЧЕНА!" : "ВЫ ПОБЕДИЛИ!";

            using (Font mainFont = new Font("Arial", 28, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(engine.GameState.IsGameOver ? Color.Red : Color.Gold))
            {
                SizeF mainSize = g.MeasureString(message, mainFont);
                float x = (clientSize.Width - mainSize.Width) / 2;
                float y = 50;
                g.DrawString(message, mainFont, brush, x, y);
            }

            DrawScoreBoard(g, clientSize);
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