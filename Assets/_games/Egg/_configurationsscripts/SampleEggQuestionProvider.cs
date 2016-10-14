﻿namespace EA4S.Egg
{
    public class SampleEggQuestionProvider : ILivingLetterDataProvider
    {
        float difficulty;
        bool onlyLetter = true;

        ILivingLetterDataProvider letterProvider;
        ILivingLetterDataProvider wordProvider;

        public SampleEggQuestionProvider(float difficulty)
        {
            this.difficulty = difficulty;

            letterProvider = new SampleLetterProvider();
            wordProvider = new SampleWordProvider();
        }

        public ILivingLetterData GetNextData()
        {
            if(onlyLetter)
            {
                return letterProvider.GetNextData();
            }
            else
            {
                return wordProvider.GetNextData();
            }
        }
    }
}
