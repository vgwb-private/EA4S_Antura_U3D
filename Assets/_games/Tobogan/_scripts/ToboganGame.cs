﻿using EA4S.MinigamesAPI;
using EA4S.MinigamesCommon;
using System.Collections.Generic;
using UnityEngine;

namespace EA4S.Minigames.Tobogan
{
    public class ToboganGame : MiniGame
    {
        public static readonly Color32 LETTER_MARK_COLOR = new Color32(0x4C, 0xAF, 0x50, 0xFF);
        public static readonly Color32 LETTER_MARK_PIPE_COLOR = new Color32(0xFF, 0xFF, 0xFF, 0xFF);

        public Material textMaterial;
        public Material drawingMaterial;
        public Material markedTextMaterial;
        public Material markedDrawingMaterial;

        public PipesAnswerController pipesAnswerController;
        public GameObject questionLivingLetterPrefab;
        public FixedHeightShadow shadowPrefab;
        public QuestionLivingLettersBox questionLivingLetterBox;
		public Camera tubesCamera;
        public ToboganFeedbackGraphics feedbackGraphics;

        public QuestionsManager questionsManager;

        public int CurrentScore { get; private set; }
        public int CurrentScoreRecord { get; private set; }

        [HideInInspector]
        public bool isTimesUp;

        public const int MAX_ANSWERS_RECORD = 15;

        const int STARS_1_THRESHOLD = 5;
        const int STARS_2_THRESHOLD = 8;
        const int STARS_3_THRESHOLD = 12;

        public IQuestionProvider SunMoonQuestions { get; set; }

        public int CurrentStars
        {
            get
            {
                if (CurrentScoreRecord < STARS_1_THRESHOLD)
                    return 0;
                if (CurrentScoreRecord < STARS_2_THRESHOLD)
                    return 1;
                if (CurrentScoreRecord < STARS_3_THRESHOLD)
                    return 2;
                return 3;
            }
        }

        bool tutorial;
        public bool showTutorial { get { if (tutorial) { tutorial = false; return true; } else return false; } }

        public ToboganQuestionState QuestionState { get; private set; }
        public ToboganPlayState PlayState { get; private set; }
        public ToboganResultGameState ResultState { get; private set; }
        public ToboganTutorialState TutorialState { get; private set; }
        
        public void ResetScore()
        {
            CurrentScoreRecord = 0;
            CurrentScore = 0;
        }

        protected override IGameConfiguration GetConfiguration()
        {
            return ToboganConfiguration.Instance;
        }

        protected override IState GetInitialState()
        {
            return QuestionState;
        }

        protected override void OnInitialize(IGameContext context)
        {
            tutorial = true;

            pipesAnswerController.SetSignHidingProbability(ToboganConfiguration.Instance.Difficulty);
            SunMoonQuestions = new SunMoonTutorialQuestionProvider(ToboganConfiguration.Instance.Questions);

            QuestionState = new ToboganQuestionState(this);
            PlayState = new ToboganPlayState(this);
            ResultState = new ToboganResultGameState(this);
            TutorialState = new ToboganTutorialState(this);

            questionsManager = new QuestionsManager(this);

            feedbackGraphics.Initialize();

            feedbackGraphics.onTowerHeightIncreased += () =>
            {
                Context.GetAudioManager().PlaySound(Sfx.Transition);
            };

            foreach (var w in AppManager.I.DB.StaticDatabase.GetWordTable().GetValuesTyped())
            {
                var analysis = Helpers.ArabicAlphabetHelper.AnalyzeData(w, false, false);
            }



            /*
            List<Database.PhraseData> phrases = new List<Database.PhraseData>(AppManager.I.DB.StaticDatabase.GetPhraseTable().GetValuesTyped());

            int idx;
            foreach (var word in phrases)
            {
                if ((idx = word.Arabic.IndexOf((char)int.Parse("0623", System.Globalization.NumberStyles.HexNumber))) >= 0 &&
                    (idx = word.Arabic.IndexOf((char)int.Parse("0644", System.Globalization.NumberStyles.HexNumber))) >= 0)
                    Debug.Log("FOUND! " + word);
            }

            List<Database.WordData> words = new List<Database.WordData>(AppManager.I.DB.StaticDatabase.GetWordTable().GetValuesTyped());

            foreach (var word in words)
            {
                if ((idx = word.Arabic.IndexOf((char)int.Parse("0623", System.Globalization.NumberStyles.HexNumber))) >= 0 &&
                    (idx = word.Arabic.IndexOf((char)int.Parse("0644", System.Globalization.NumberStyles.HexNumber))) >= 0)
                    Debug.Log("FOUND! " + word);
            }

            //LL_WordData newWordData = new LL_WordData(AppManager.I.DB.GetWordDataById("wolf"));
            */
        }

        public void OnResult(bool result)
        {
            if (result)
                Context.GetAudioManager().PlaySound(Sfx.StampOK);
            else
            {
                Context.GetAudioManager().PlaySound(Sfx.KO);
                Context.GetAudioManager().PlaySound(Sfx.Lose);
            }

            Context.GetCheckmarkWidget().Show(result);
            feedbackGraphics.OnResult(result);

            if (result)
            {
                ++CurrentScore;
                if (CurrentScore > CurrentScoreRecord)
                    CurrentScoreRecord = CurrentScore;
            }
            else
            {
                CurrentScore = 0;
            }

            Context.GetOverlayWidget().SetStarsScore(CurrentScoreRecord);
        }

        public void InitializeOverlayWidget()
        {
            Context.GetOverlayWidget().Initialize(true, true, false);
            Context.GetOverlayWidget().SetStarsThresholds(STARS_1_THRESHOLD, STARS_2_THRESHOLD, STARS_3_THRESHOLD);
        }

        public override Vector3 GetGravity()
        {
            return Vector3.up * (-80);
        }
    }
}