﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private Dictionary<int, int> ReadKillMonsters(string killMonsters)
        {
            Dictionary<int, int> result = new Dictionary<int, int>();
            string[] splitSets = killMonsters.Split(';');
            foreach (string set in splitSets)
            {
                if (string.IsNullOrEmpty(set))
                    continue;
                string[] splitData = set.Split(':');
                if (splitData.Length != 2)
                    continue;
                result[int.Parse(splitData[0])] = int.Parse(splitData[1]);
            }
            return result;
        }

        private string WriteKillMonsters(Dictionary<int, int> killMonsters)
        {
            string result = "";
            foreach (KeyValuePair<int, int> keyValue in killMonsters)
            {
                result += keyValue.Key + ":" + keyValue.Value + ";";
            }
            return result;
        }

        private bool ReadCharacterQuest(SQLiteRowsReader reader, out CharacterQuest result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterQuest();
                result.dataId = reader.GetInt32("dataId");
                result.isComplete = reader.GetBoolean("isComplete");
                result.killedMonsters = ReadKillMonsters(reader.GetString("killedMonsters"));
                return true;
            }
            result = CharacterQuest.Empty;
            return false;
        }

        public void CreateCharacterQuest(int idx, string characterId, CharacterQuest characterQuest)
        {
            ExecuteNonQuery("INSERT INTO characterquest (id, idx, characterId, dataId, isComplete, killedMonsters) VALUES (@id, @idx, @characterId, @dataId, @isComplete, @killedMonsters)",
                new SqliteParameter("@id", characterId + "_" + idx),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@characterId", characterId),
                new SqliteParameter("@dataId", characterQuest.dataId),
                new SqliteParameter("@isComplete", characterQuest.isComplete),
                new SqliteParameter("@killedMonsters", WriteKillMonsters(characterQuest.killedMonsters)));
        }

        public List<CharacterQuest> ReadCharacterQuests(string characterId)
        {
            List<CharacterQuest> result = new List<CharacterQuest>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM characterquest WHERE characterId=@characterId ORDER BY idx ASC",
                new SqliteParameter("@characterId", characterId));
            CharacterQuest tempQuest;
            while (ReadCharacterQuest(reader, out tempQuest, false))
            {
                result.Add(tempQuest);
            }
            return result;
        }

        public void DeleteCharacterQuests(string characterId)
        {
            ExecuteNonQuery("DELETE FROM characterquest WHERE characterId=@characterId", new SqliteParameter("@characterId", characterId));
        }
    }
}
