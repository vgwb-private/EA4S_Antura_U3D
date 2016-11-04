﻿namespace EA4S.ColorTickle
{
    public class ResultGameState : IGameState
    {
        ColorTickleGame game;

        float timer = 1;
        public ResultGameState(ColorTickleGame game)
        {
            this.game = game;
        }

        public void EnterState()
        {
        }

        public void ExitState()
        {
        }

        public void Update(float delta)
        {
            timer -= delta;

            if (timer < 0)
            {
                game.EndGame(0,0);
            }
        }

        public void UpdatePhysics(float delta)
        {
        }
    }
}
