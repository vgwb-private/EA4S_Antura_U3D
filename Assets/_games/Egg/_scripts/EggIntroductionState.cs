﻿using EA4S.MinigamesCommon;

namespace EA4S.Minigames.Egg
{
    public class EggIntroductionState : IState
    {
        EggGame game;

        float timer = 1f;
        public EggIntroductionState(EggGame game) { this.game = game; }

        public void EnterState()
        {
            game.antura.ResetAnturaIn(EggGame.numberOfStage, 2);
        }

        public void ExitState()
        {
            game.Context.GetAudioManager().PlayMusic(Music.Theme8);
        }

        public void Update(float delta)
        {
            timer -= delta;

            if (timer <= 0f) {
                game.SetCurrentState(game.QuestionState);
                return;
            }
        }

        public void UpdatePhysics(float delta) { }
    }
}