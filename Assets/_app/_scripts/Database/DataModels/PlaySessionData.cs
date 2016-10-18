﻿using System;
using System.Collections.Generic;

namespace EA4S.Db
{
    [Serializable]
    public class PlaySessionData : IData
    {
        public int Stage;
        public int LearningBlock;
        public int PlaySession;
        public string Description;
        public DidacticalFocus Focus; 
        public string[] Letters;   
        public string[] Words;     
        public string[] Words_previous;    
        public string[] Phrases;     
        public string[] Phrases_previous;   
        public AssessmentType AssessmentType; 
        public string AssessmentData;
        public List<MiniGameInPlaysession> Minigames;

        public string GetId()
        {
            return Stage + "." + LearningBlock + "." + PlaySession;
        }

        public override string ToString()
        {
            string output = "";
            output += string.Format("[PlaySession: S={0}, LB={1}, PS={2}, description={3}]", Stage, LearningBlock, PlaySession, Description);
            output += "\n MiniGames:";
            foreach(var minigame in Minigames)
            {
                if (minigame.Weight == 0) continue;
                output += "\n      " + minigame.Code + ": \t" + minigame.Weight;
            }
            return output;
        }
        
    }

    [Serializable]
    public struct MiniGameInPlaysession
    {
        public MiniGameCode Code;
        public int Weight;
    }

    public enum DidacticalFocus
    {
        Letters = 1,
        Shapes = 2,
        Words = 3
    }

}