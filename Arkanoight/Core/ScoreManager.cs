using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Arkanoight.Models;

namespace Arkanoight.Core
{
    public static class ScoreManager
    {
        private static readonly string ScoresFilePath = "scores.json";
        private static List<ScoreRecord> scores = new List<ScoreRecord>();

        public static void LoadScores()
        {
            try
            {
                if (File.Exists(ScoresFilePath))
                {
                    var json = File.ReadAllText(ScoresFilePath);
                    scores = JsonSerializer.Deserialize<List<ScoreRecord>>(json) ?? new List<ScoreRecord>();
                }
            }
            catch { }
        }

        public static void SaveScores()
        {
            try
            {
                var json = JsonSerializer.Serialize(scores);
                File.WriteAllText(ScoresFilePath, json);
            }
            catch { }
        }

        public static void AddScore(string name, int score, int lives, string type)
        {
            if (type == "Досрочный выход") return;

            scores.Add(new ScoreRecord
            {
                PlayerName = name,
                Score = score,
                Lives = lives,
                Date = DateTime.Now,
                GameEndType = type
            });

            scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();
            SaveScores();
        }
        public static List<ScoreRecord> GetScores() => scores.OrderByDescending(s => s.Score).ToList();
    }
}