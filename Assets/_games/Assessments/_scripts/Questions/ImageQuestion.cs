using EA4S.MinigamesAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EA4S.Assessment
{
    public class ImageQuestion : IQuestion
    {
        private StillLetterBox view;
        private ILivingLetterData imageData;

        public ImageQuestion( StillLetterBox wordGO, ILivingLetterData image, AssessmentDialogues dialogues)
        {
            imageData = image;
            view = wordGO;
            placeholdersSet = new List< GameObject>();

            var question = wordGO.gameObject.AddComponent< QuestionBehaviour>();
            question.SetQuestion( this, dialogues);
        }


        public GameObject gameObject
        {
            get
            {
                return view.gameObject;
            }
        }

        public QuestionBehaviour QuestionBehaviour
        {
            get
            {
                return view.GetComponent< QuestionBehaviour>();
            }
        }

        public ILivingLetterData Image()
        {
            return imageData;
        }

        public ILivingLetterData LetterData()
        {
            return view.Data;
        }

        public int PlaceholdersCount()
        {
            return 1;
        }

        private List< GameObject> placeholdersSet;

        public void TrackPlaceholder( GameObject gameObject)
        {
            placeholdersSet.Add( gameObject);
        }

        public IEnumerable< GameObject> GetPlaceholders()
        {
            if (placeholdersSet.Count != 1)
                throw new InvalidOperationException("Something wrong. Check Question placer");

            return placeholdersSet;
        }

        private AnswerSet answerSet;
        public void SetAnswerSet( AnswerSet answerSet)
        {
            this.answerSet = answerSet;
        }

        public AnswerSet GetAnswerSet()
        {
            return answerSet;
        }
    }
}
