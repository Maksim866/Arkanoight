using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Arkanoight.Core;
using Arkanoight.Models;

namespace Arkanoight.Views
{
    /// <summary>
    /// Класс для визуализации игры
    /// </summary>
    public static class GameVisuals
    {
        private static readonly Color[] BrickColors = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue };
        private static readonly Color[] HitColors = {
            Color.FromArgb(255, 255, 150, 150),
            Color.FromArgb(255, 255, 200, 150),
            Color.FromArgb(255, 255, 255, 150),
            Color.FromArgb(255, 150, 255, 150),
            Color.FromArgb(255, 150, 150, 255)
        };

        private static Bitmap buffer;
        private static int lastWidth, lastHeight;
        private static bool bufferDirty = true;
        private static Control targetControl;

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

        /// <summary>Рисует игру в буфер (только если изменилось)</summary>
        public static void DrawToBuffer(IArkanoightEngine e, Size cs)
        {
            if (buffer == null) return;

            using (var g = Graphics.FromImage(buffer))
            {
                g.Clear(Color.Black);

                // Платформа
                using (var b = new SolidBrush(Color.Cyan))
                    g.FillRectangle(b, e.Platform.X, e.Platform.Y, e.Platform.Width, e.Platform.Height);

                // Мячи
                using (var b = new SolidBrush(Color.Yellow))
                    foreach (var ball in e.Balls.Where(b => b.IsActive))
                        g.FillEllipse(b, ball.X, ball.Y, ball.Size, ball.Size);

                // Кирпичи
                using (var p = new Pen(Color.White, 1))
                {
                    foreach (var br in e.Bricks.Where(b => b.IsActive))
                    {
                        using (var b = new SolidBrush(br.IsHit ? HitColors[br.Row] : BrickColors[br.Row]))
                            g.FillRectangle(b, br.X, br.Y, br.Width, br.Height);
                        g.DrawRectangle(p, br.X, br.Y, br.Width, br.Height);

                        if (br.Health < br.MaxHealth)
                            using (var dp = new Pen(Color.FromArgb(150, Color.Black), 2))
                                for (int i = 0; i < br.MaxHealth - br.Health; i++)
                                    g.DrawLine(dp, br.X + 10 + i * 10, br.Y + 5,
                                        br.X + br.Width - 10 - i * 10, br.Y + br.Height - 5);
                    }
                }

                // Усиления
                foreach (var u in e.PowerUps.Where(u => u.IsActive))
                {
                    var col = u.Type == PowerUpType.ExtraBall ? Color.Cyan :
                             u.Type == PowerUpType.DamageBoost ? Color.Red : Color.Green;
                    var sym = u.Type == PowerUpType.ExtraBall ? "⚽" :
                             u.Type == PowerUpType.DamageBoost ? "⚡" : "⬌";
                    using (var b = new SolidBrush(col)) g.FillRectangle(b, u.X, u.Y, u.Size, u.Size);
                    using (var p = new Pen(Color.White, 1)) g.DrawRectangle(p, u.X, u.Y, u.Size, u.Size);
                    using (var f = new Font("Arial", 12, FontStyle.Bold))
                    using (var b = new SolidBrush(Color.Black)) g.DrawString(sym, f, b, u.X + 2, u.Y + 2);
                }

                // UI
                using (var f = new Font("Arial", 14, FontStyle.Bold))
                using (var b = new SolidBrush(Color.White))
                {
                    g.DrawString($"Счет: {e.GameState.Score}", f, b, 10, 10);
                    g.DrawString($"Жизни: {e.GameState.Lives}", f, b, cs.Width - 100, 10);
                }

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
            using (var b = new SolidBrush(Color.FromArgb(150, 0, 0, 0)))
                g.FillRectangle(b, 0, 0, cs.Width, cs.Height);
            using (var f = new Font("Arial", 48, FontStyle.Bold))
            using (var b = new SolidBrush(Color.White))
                g.DrawString("ПАУЗА", f, b, (cs.Width - g.MeasureString("ПАУЗА", f).Width) / 2, cs.Height / 2 - 30);
            using (var f = new Font("Arial", 18, FontStyle.Regular))
            using (var b = new SolidBrush(Color.Yellow))
                g.DrawString("Нажмите ПРОБЕЛ для продолжения", f, b,
                    (cs.Width - g.MeasureString("Нажмите ПРОБЕЛ для продолжения", f).Width) / 2, cs.Height / 2 + 30);
        }

        private static void DrawStartScreen(Graphics g, Size cs)
        {
            int w = 600, h = 450, x = (cs.Width - w) / 2, y = (cs.Height - h) / 2 - 20;
            using (var b = new SolidBrush(Color.FromArgb(230, 20, 20, 20))) g.FillRectangle(b, x, y, w, h);
            using (var p = new Pen(Color.Cyan, 3)) g.DrawRectangle(p, x, y, w, h);
            using (var f = new Font("Arial", 26, FontStyle.Bold))
            using (var b = new SolidBrush(Color.Cyan))
                g.DrawString("АРКАНОИД", f, b, x + 150, y + 25);

            using (var f = new Font("Arial", 12, FontStyle.Regular))
            using (var b = new SolidBrush(Color.White))
            {
                g.DrawString("• ЛКМ - запуск мяча", f, b, x + 50, y + 120);
                g.DrawString("• Движение мыши - управление платформой", f, b, x + 50, y + 145);
                g.DrawString("• Пробел - пауза / продолжить", f, b, x + 50, y + 170);
                g.DrawString("• R - перезапуск (после победы/поражения)", f, b, x + 50, y + 195);
            }

            int sy = y + 280;
            var types = new[] {
                (Color.Cyan, "⚽", "Голубой - дополнительный мяч"),
                (Color.Red, "⚡", "Красный - увеличение урона всех мячей на 1"),
                (Color.Green, "⬌", "Зеленый - широкая платформа на 3 секунды")
            };

            for (int i = 0; i < 3; i++)
            {
                using (var b = new SolidBrush(types[i].Item1)) g.FillRectangle(b, x + 40, sy + i * 45, 25, 25);
                using (var p = new Pen(Color.White, 1)) g.DrawRectangle(p, x + 40, sy + i * 45, 25, 25);
                using (var f = new Font("Arial", 14, FontStyle.Bold))
                using (var b = new SolidBrush(Color.Black))
                    g.DrawString(types[i].Item2, f, b, x + 45, sy + i * 45 + 2);
                using (var f = new Font("Arial", 11, FontStyle.Regular))
                using (var b = new SolidBrush(Color.White))
                {
                    g.DrawString(types[i].Item3, f, b, x + 75, sy + i * 45 + 5);
                    g.DrawString(i == 0 ? "(появляется на платформе)" :
                               i == 1 ? "(суммируется)" : "(временный эффект)", f, b, x + 75, sy + i * 45 + 22);
                }
            }
        }

        private static void DrawGameOverScreen(Graphics g, IArkanoightEngine e, Size cs)
        {
            var scores = ScoreManager.GetScores();
            string msg = e.GameState.IsGameOver ? "ИГРА ОКОНЧЕНА!" : "ВЫ ПОБЕДИЛИ!";
            using (var f = new Font("Arial", 28, FontStyle.Bold))
            using (var b = new SolidBrush(e.GameState.IsGameOver ? Color.Red : Color.Gold))
                g.DrawString(msg, f, b, (cs.Width - g.MeasureString(msg, f).Width) / 2, 50);

            int bw = 400, bh = 280, bx = (cs.Width - bw) / 2, by = (cs.Height - bh) / 2 + 30;
            using (var b = new SolidBrush(Color.FromArgb(50, 50, 50))) g.FillRectangle(b, bx, by, bw, bh);
            using (var p = new Pen(Color.Gold, 2)) g.DrawRectangle(p, bx, by, bw, bh);
            using (var f = new Font("Arial", 18, FontStyle.Bold))
            using (var b = new SolidBrush(Color.Gold))
                g.DrawString("ТАБЛИЦА РЕКОРДОВ", f, b, bx + 80, by + 10);

            if (scores.Count == 0)
            {
                using (var f = new Font("Arial", 14, FontStyle.Italic))
                using (var b = new SolidBrush(Color.Gray))
                    g.DrawString("Пока нет рекордов", f, b, bx + 110, by + 120);
            }
            else for (int i = 0; i < scores.Count; i++)
            {
                using (var f = new Font("Arial", 10, FontStyle.Regular))
                using (var b = new SolidBrush(Color.White))
                {
                    g.DrawString($"{i + 1}. {scores[i].PlayerName}", f, b, bx + 20, by + 80 + i * 20);
                    g.DrawString(scores[i].Score.ToString(), f, b, bx + 200, by + 80 + i * 20);
                    using (var tb = new SolidBrush(scores[i].GameEndType == "Победа" ? Color.Gold : Color.LightCoral))
                        g.DrawString(scores[i].GameEndType, f, tb, bx + 280, by + 80 + i * 20);
                }
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

        /// <summary>Очищает буфер</summary>
        public static void Cleanup()
        {
            buffer?.Dispose();
            buffer = null;
            targetControl = null;
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