﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EA4S.Db;
using EA4S.Teacher;

namespace EA4S
{
    /// <summary>
    /// Handles logic that represent the Teacher's expert system:
    /// - selects minigames according to a given progression flow
    /// - selects question packs according to the given profression flow
    /// - selects minigame difficulty according to the player's status
    /// </summary>
    public class TeacherAI
    {
        public static TeacherAI I;

        // Temporary configuration
        private static int NUMBER_OF_MINIGAMES_PER_PLAYSESSION = 3;

        // References
        private DatabaseManager dbManager;
        private PlayerProfile playerProfile;

        // Inner engines
        public LogIntelligence logger;

        // Helpers
        public WordHelper wordHelper;
        public JourneyHelper journeyHelper;

        // Selection engines
        MiniGameSelectionAI minigameSelectionAI;
        WordSelectionAI wordSelectionAI;
        DifficultySelectionAI difficultySelectionAI;

        // State
        private List<MiniGameData> currentPlaySessionMiniGames = new List<MiniGameData>();

        #region Setup

        public TeacherAI(DatabaseManager _dbManager, PlayerProfile _playerProfile)
        {
            I = this;
            this.dbManager = _dbManager;
            this.playerProfile = _playerProfile;

            this.wordHelper = new WordHelper(_dbManager, this);
            this.journeyHelper = new JourneyHelper(_dbManager, this);

            this.logger = new Teacher.LogIntelligence(_dbManager);

            this.minigameSelectionAI = new MiniGameSelectionAI(dbManager, playerProfile);
            this.wordSelectionAI = new WordSelectionAI(dbManager, playerProfile, this);
            this.difficultySelectionAI = new DifficultySelectionAI(dbManager, playerProfile, this);
        }

        private void ResetPlaySession()
        {
            this.currentPlaySessionMiniGames.Clear();

            this.minigameSelectionAI.InitialiseNewPlaySession();
            this.wordSelectionAI.InitialiseNewPlaySession();
        }

        #endregion

        #region Interface - PlaySession & MiniGame Selection

        public List<MiniGameData> InitialiseCurrentPlaySession()
        {
            return InitialiseCurrentPlaySession(NUMBER_OF_MINIGAMES_PER_PLAYSESSION);
        }

        private List<MiniGameData> InitialiseCurrentPlaySession(int nMinigamesToSelect)
        {
            ResetPlaySession();
            this.currentPlaySessionMiniGames = SelectMiniGamesForCurrentPlaySession(nMinigamesToSelect);
            return currentPlaySessionMiniGames;
        }

        #endregion

        #region Interface - Current MiniGame Getters

        public List<MiniGameData> CurrentPlaySessionMiniGames
        {
            get
            { 
                return currentPlaySessionMiniGames;
            }
        }

        public MiniGameData CurrentMiniGame
        {
            get
            {
                return currentPlaySessionMiniGames.ElementAt(playerProfile.CurrentMiniGameInPlaySession);
            }
        }

        #endregion

        #region Interface - Difficulty

        public float GetCurrentDifficulty(MiniGameCode miniGameCode)
        {
            return difficultySelectionAI.SelectDifficulty(miniGameCode);
        }

        #endregion

        // HELPER (move to JourneyHelper?)
        public string JourneyPositionToPlaySessionId(JourneyPosition journeyPosition)
        {
            return journeyPosition.Stage + "." + journeyPosition.LearningBlock + "." + journeyPosition.PlaySession;
        }
      
        #region MiniGame Selection queries

        private List<Db.MiniGameData> SelectMiniGamesForCurrentPlaySession(int nMinigamesToSelect)
        {
            var currentPlaySessionId = JourneyPositionToPlaySessionId(this.playerProfile.CurrentJourneyPosition);
            return SelectMiniGamesForPlaySession(currentPlaySessionId, nMinigamesToSelect);
        }

        public List<Db.MiniGameData> SelectMiniGamesForPlaySession(string playSessionId, int numberToSelect)
        {
            return minigameSelectionAI.PerformSelection(playSessionId, numberToSelect);
        }

        #endregion

        #region Letter/Word Selection queries

        // TEST - DEPRECATED
        public List<Db.WordData> SelectWordsForPlaySession(string playSessionId, int numberToSelect)
        {
            return this.wordSelectionAI.PerformSelection(playSessionId, numberToSelect);
        }
        
        #endregion

        #region Score Log queries

        public List<float> GetLatestScoresForMiniGame(MiniGameCode minigameCode, int nLastDays)
        {
            int fromTimestamp = GenericUtilities.GetRelativeTimestampFromNow(-nLastDays);
            string query = string.Format("SELECT * FROM LogPlayData WHERE MiniGame = '{0}' AND Timestamp < {1}",
                (int)minigameCode, fromTimestamp);
            List<LogPlayData> list = dbManager.FindLogPlayDataByQuery(query);
            List<float> scores = list.ConvertAll(x => x.Score);
            return scores;
        }

        public List<ScoreData> GetCurrentScoreForAllPlaySessions()
        {
            string query = string.Format("SELECT * FROM ScoreData WHERE TableName = 'PlaySessions' ORDER BY ElementId ");
            List<ScoreData> list = dbManager.FindScoreDataByQuery(query);
            return list;
        }

        public List<ScoreData> GetCurrentScoreForPlaySessionsOfStage(int stage)
        {
            // First, get all data given a stage
            List<PlaySessionData> eligiblePlaySessionData_list = this.dbManager.FindPlaySessionData(x => x.Stage == stage);
            List<string> eligiblePlaySessionData_id_list = eligiblePlaySessionData_list.ConvertAll(x => x.Id);

            // Then, get all scores
            string query = string.Format("SELECT * FROM ScoreData WHERE TableName = 'PlaySessions'");
            List<ScoreData> all_score_list = dbManager.FindScoreDataByQuery(query);

            // At last, filter by the given stage
            List<ScoreData> filtered_score_list = all_score_list.FindAll(x => eligiblePlaySessionData_id_list.Contains(x.ElementId));
            return filtered_score_list;
        }

        public List<ScoreData> GetCurrentScoreForPlaySessionsOfStageAndLearningBlock(int stage, int learningBlock)
        {
            // First, get all data given a stage
            List<PlaySessionData> eligiblePlaySessionData_list = this.dbManager.FindPlaySessionData(x => x.Stage == stage && x.LearningBlock == learningBlock); // TODO: make this readily available!
            List<string> eligiblePlaySessionData_id_list = eligiblePlaySessionData_list.ConvertAll(x => x.Id);

            // Then, get all scores
            string query = string.Format("SELECT * FROM ScoreData WHERE TableName = 'PlaySessions'");
            List<ScoreData> all_score_list = dbManager.FindScoreDataByQuery(query);

            // At last, filter
            List<ScoreData> filtered_score_list = all_score_list.FindAll(x => eligiblePlaySessionData_id_list.Contains(x.ElementId));
            return filtered_score_list;
        }
        

        public List<ScoreData> GetCurrentScoreForLearningBlocksOfStage(int stage)
        {
            // First, get all data given a stage
            List<LearningBlockData> eligibleLearningBlockData_list = this.dbManager.FindLearningBlockData(x => x.Stage == stage);
            List<string> eligibleLearningBlockData_id_list = eligibleLearningBlockData_list.ConvertAll(x => x.Id);

            // Then, get all scores
            string query = string.Format("SELECT * FROM ScoreData WHERE TableName = 'LearningBlock'");
            List<ScoreData> all_score_list = dbManager.FindScoreDataByQuery(query);

            // At last, filter by the given stage
            List<ScoreData> filtered_score_list = all_score_list.FindAll(x => eligibleLearningBlockData_id_list.Contains(x.ElementId));
            return filtered_score_list;
        }

        #endregion

        #region Assessment Log queries

        public List<LetterData> GetFailedAssessmentLetters(MiniGameCode assessmentCode) // also play session
        {
            // @note: this code shows how to work on the dynamic and static db together
            string query =
                string.Format(
                    "SELECT * FROM LogLearnData WHERE TableName = 'LetterData' AND Score < 0 and MiniGame = {0}",
                    (int)assessmentCode);
            List<LogLearnData> logLearnData_list = dbManager.FindLogLearnDataByQuery(query);
            List<string> letter_ids_list = logLearnData_list.ConvertAll(x => x.ElementId);
            List<LetterData> letters = dbManager.FindLetterData(x => letter_ids_list.Contains(x.Id));
            return letters;
        }

        public List<WordData> GetFailedAssessmentWords(MiniGameCode assessmentCode)
        {
            string query =
                string.Format(
                    "SELECT * FROM LogLearnData WHERE TableName = 'WordData' AND Score < 0 and MiniGame = {0}",
                    (int)assessmentCode);
            List<LogLearnData> logLearnData_list = dbManager.FindLogLearnDataByQuery(query);
            List<string> words_ids_list = logLearnData_list.ConvertAll(x => x.ElementId);
            List<WordData> words = dbManager.FindWordData(x => words_ids_list.Contains(x.Id));
            return words;
        }

        #endregion

        #region Journeymap Log queries

        public List<LogPlayData> GetScoreHistoryForCurrentJourneyPosition()
        {
            // @note: shows how to work with playerprofile as well as the database
            JourneyPosition currentJourneyPosition = playerProfile.CurrentJourneyPosition;
            string query = string.Format("SELECT * FROM LogPlayData WHERE PlayEvent = {0} AND PlaySession = '{1}'",
                (int)PlayEvent.GameFinished, currentJourneyPosition.ToString());
            List<LogPlayData> list = dbManager.FindLogPlayDataByQuery(query);
            return list;
        }

        #endregion

        #region Mood Log queries

        public List<LogMoodData> GetLastMoodData(int number)
        {
            string query = string.Format("SELECT * FROM LogMoodData ORDER BY Timestamp LIMIT {0}", number);
            List<LogMoodData> list = dbManager.FindLogMoodDataByQuery(query);
            return list;
        }

        #endregion


        #region Interface - Fake data for question providers

        private static bool giveWarningOnFake = false;

        public List<LL_LetterData> GetAllTestLetterDataLL()
        {
            List<LL_LetterData> list = new List<LL_LetterData>();
            foreach (var letterData in this.wordHelper.GetAllRealLetters())
                list.Add(BuildLetterData_LL(letterData));
            return list;
        }

        public LL_LetterData GetRandomTestLetterLL()
        {
            if (giveWarningOnFake)
            {
                Debug.LogWarning("You are using fake data for testing. Make sure to test with real data too.");
                giveWarningOnFake = false;
            }

            var data = this.wordHelper.GetAllRealLetters().RandomSelectOne();
            return BuildLetterData_LL(data);
        }

        public LL_WordData GetRandomTestWordDataLL()
        {
            if (giveWarningOnFake)
            {
                Debug.LogWarning("You are using fake data for testing. Make sure to test with real data too.");
                giveWarningOnFake = false;
            }

            var data = this.wordHelper.GetWordsByCategory(WordDataCategory.BodyPart).RandomSelectOne();
            return BuildWordData_LL(data); 
        }

        private LL_LetterData BuildLetterData_LL(LetterData data)
        {
            return new LL_LetterData(data.GetId());
        }

        private List<ILivingLetterData> BuildLetterData_LL_Set(List<LetterData> data_list)
        {
            return data_list.ConvertAll<ILivingLetterData>(x => BuildLetterData_LL(x));
        }

        private LL_WordData BuildWordData_LL(WordData data)
        {
            return new LL_WordData(data.GetId(), data);
        }

        private List<ILivingLetterData> BuildWordData_LL_Set(List<WordData> data_list)
        {
            return data_list.ConvertAll<ILivingLetterData>(x => BuildWordData_LL(x));
        }

        #endregion


    }
}
