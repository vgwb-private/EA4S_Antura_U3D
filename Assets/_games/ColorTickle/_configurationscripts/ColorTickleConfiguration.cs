﻿namespace EA4S.ColorTickle
{
    public class ColorTickleConfiguration : IGameConfiguration
    {
        // Game configuration
        public IGameContext Context { get; set; }
        public float Difficulty { get; set; }

        /////////////////
        // Singleton Pattern
        static ColorTickleConfiguration instance;
        public static ColorTickleConfiguration Instance
        {
            get
            {
                if (instance == null)
                    instance = new ColorTickleConfiguration();
                return instance;
            }
        }
        /////////////////

        private ColorTickleConfiguration()
        {
            // Default values
            // THESE SETTINGS ARE FOR SAMPLE PURPOSES, THESE VALUES MUST BE SET BY GAME CORE
            Context = new SampleGameContext();
            Difficulty = 0.5f;
        }
    }
}
