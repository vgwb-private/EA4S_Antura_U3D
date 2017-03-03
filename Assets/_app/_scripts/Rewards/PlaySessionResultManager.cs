﻿using UnityEngine;
using System.Collections.Generic;
using EA4S.Core;
using EA4S.Database;

namespace EA4S.Rewards
{

    /// <summary>
    /// Manager for the Play Session Result scene.
    /// Accessed a play session is completed.
    /// </summary>
    public class PlaySessionResultManager : MonoBehaviour
    {

        void Start()
        {
            // Calculate items to unlock count
            int itemsToUnlock = AppManager.I.NavigationManager.CalculateUnlockItemCount();

            List<RewardPackUnlockData> oldRewards = AppManager.I.Player.RewardsUnlocked.FindAll(ru => ru.GetJourneyPosition().Equals(AppManager.I.Player.CurrentJourneyPosition));
            int itemAlreadyUnlocked = oldRewards.Count;
            for (int i = 0; i < itemsToUnlock - itemAlreadyUnlocked; i++) {
                // if necessary add one new random reward unlocked
                RewardPackUnlockData newRewardToUnlock = RewardSystemManager.GetNextRewardPack(true)[0];
                oldRewards.Add(newRewardToUnlock);
                AppManager.I.Player.AddRewardUnlocked(newRewardToUnlock);
            }

            // Show UI result and unlock transform parent where show unlocked items
            GameObject[] objs = new GameObject[] { };
            objs = GameResultUI.ShowEndsessionResult(AppManager.I.NavigationManager.UseEndSessionResults(), itemAlreadyUnlocked);

            for (int i = 0; i < objs.Length - oldRewards.Count; i++) {
                // if necessary add one new random reward not to be unlocked!
                oldRewards.Add(RewardSystemManager.GetNextRewardPack(true)[0]);
            }

            LogManager.I.LogPlaySessionScore(AppManager.I.JourneyHelper.GetCurrentPlaySessionData().Id, objs.Length);
            // save max progression (internal check if necessary)
            AppManager.I.Player.AdvanceMaxJourneyPosition();

            // for any rewards mount them model on parent transform object (objs)
            for (int i = 0; i < oldRewards.Count && i < objs.Length; i++) {
                ModelsManager.MountModel(
                    oldRewards[i].ItemId,
                    objs[i].transform,
                    oldRewards[i].GetMaterialPair()
                    );
            }

        }

    }
}