using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    /// <summary>
    /// Менеджер для управления таблицей рекордов
    /// </summary>
    public static class ScoreManager
    {
        private static readonly string ScoresFilePath = "scores.json";
        private static List<ScoreRecord> scores = new List<ScoreRecord>();

        /// <summary>
        /// Загружает рекорды из файла
        /// </summary>
        public static void LoadScores()
        {
            try
            {
                if (File.Exists(ScoresFilePath))
                {
                    string json = File.ReadAllText(ScoresFilePath);
                    scores = JsonSerializer.Deserialize<List<ScoreRecord>>(json) ?? new List<ScoreRecord>();
                }
            }
            catch
            {
                scores = new List<ScoreRecord>();
            }
        }

        /// <summary>
        /// Сохраняет рекорды в файл
        /// </summary>
        public static void SaveScores()
        {
            try
            {
                string json = JsonSerializer.Serialize(scores);
                File.WriteAllText(ScoresFilePath, json);
            }
            catch { }
        }

        /// <summary>
        /// Добавляет новый рекорд (только для побед или поражений, не для досрочного выхода)
        /// </summary>
        public static void AddScore(string playerName, int score, int lives, string gameEndType)
        {
            // Не добавляем досрочные выходы
            if (gameEndType == "Досрочный выход")
                return;

            scores.Add(new ScoreRecord
            {
                PlayerName = playerName,
                Score = score,
                Lives = lives,
                Date = DateTime.Now,
                GameEndType = gameEndType
            });

            // Сортируем по убыванию очков
            scores = scores.OrderByDescending(s => s.Score).ToList();

            // Оставляем только топ-10
            if (scores.Count > 10)
                scores = scores.Take(10).ToList();

            SaveScores();
        }

        /// <summary>
        /// Получает все рекорды (отсортированные по убыванию)
        /// </summary>
        public static List<ScoreRecord> GetScores()
        {
            return scores.OrderByDescending(s => s.Score).ToList();
        }

        /// <summary>
        /// Очищает все рекорды
        /// </summary>
        public static void ClearScores()
        {
            scores.Clear();
            SaveScores();
        }
    }
}