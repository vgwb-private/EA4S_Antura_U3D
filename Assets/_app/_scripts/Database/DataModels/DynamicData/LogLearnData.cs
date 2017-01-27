﻿using EA4S.Utilities;
using SQLite;

namespace EA4S.Database
{
    /// <summary>
    /// Learning achievements obtained at a given timestamp. Logged at runtime.
    /// Table can be: Letter, Phrase, Word
    /// </summary>
    [System.Serializable]
    public class LogLearnData : IData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Session { get; set; } // DailySession Id
        public int Timestamp { get; set; }

        public string PlaySession { get; set; }
        public MiniGameCode MiniGame { get; set; }
        public string TableName { get; set; } // word, letter, phrases (see DbTables enum)
        public string ElementId { get; set; }
        public float Score { get; set; } // -1.0 (bad)... 1.0 (perfect)

        public LogLearnData()
        {
        }

        public LogLearnData(string _Session, string _PlaySession, MiniGameCode _MiniGame, VocabularyDataType _dataType, string _elementId, float _score)
        {
            Session = _Session;
            PlaySession = _PlaySession;
            MiniGame = _MiniGame;
            TableName = _dataType.ToString();   //  TODO: instead of TableName we should use VocabularyDataType 
            ElementId = _elementId;
            Score = _score;
            Timestamp = GenericUtilities.GetTimestampForNow();
        }

        public string GetId()
        {
            return Id.ToString();
        }

        public override string ToString()
        {
            return string.Format("S{0},T{1},PS{2},MG{3},T{4},E{5},S{6}",
                Session,
                Timestamp,
                PlaySession,
                MiniGame,
                TableName,
                ElementId,
                Score
                );
        }

    }
}