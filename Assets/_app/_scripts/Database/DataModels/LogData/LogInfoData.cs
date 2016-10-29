﻿using SQLite;
using System;

namespace EA4S.Db
{

    public enum InfoEvent
    {
        ProfileCreated = 1,

        AppStarted = 20,
        AppClosed = 21,

        Book = 30,
    }


    [System.Serializable]
    public class LogInfoData : IData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Session { get; set; }
        public int Timestamp { get; set; }

        public InfoEvent Event { get; set; }
        public string Parameters { get; set; } // examples: "playerId:0, rewardType:2"

        public LogInfoData()
        {
        }

        public LogInfoData(string _Session, InfoEvent _Event, PlaySkill _PlaySkill, string _Parameters)
        {
            this.Session = _Session;
            this.Event = _Event;
            this.Parameters = _Parameters;
            this.Timestamp = GenericUtilities.GetTimestampForNow();
        }

        public string GetId()
        {
            return Id.ToString();
        }

        public override string ToString()
        {
            return string.Format("T{0},T{1},E{2},PARS{3}",
                Session,
                Timestamp,
                Event,
                Parameters
            );
        }

    }
}