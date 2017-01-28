﻿using EA4S.MinigamesCommon;

namespace EA4S.Minigames.FastCrowd
{
    public class FastCrowdResultState : IState
    {
        FastCrowdGame game;

        public FastCrowdResultState(FastCrowdGame game)
        {
            this.game = game;
        }

        public void EnterState()
        {
            if (game.CurrentChallenge != null)
            {
                if (!game.ShowChallengePopupWidget(true, OnPopupCloseRequested))
                    game.SetCurrentState(game.QuestionState);
            }
        }


        void OnPopupCloseRequested()
        {
            game.SetCurrentState(game.QuestionState);
        }

        public void ExitState()
        {
            game.Context.GetPopupWidget().Hide();
        }

        public void Update(float delta)
        {
        }

        public void UpdatePhysics(float delta)
        {
        }
    }
}