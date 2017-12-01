using System.Collections;
using Antura.Audio;
using Antura.CameraControl;
using Antura.Core;
using Antura.Database;
using Antura.Keeper;
using Antura.Minigames;
using Antura.Profile;
using Antura.Tutorial;
using Antura.UI;
using DG.Tweening;
using UnityEngine;

namespace Antura.Map
{
    /// <summary>
    ///     General manager for the Map scene.
    ///     Handles the different maps for all Stages of the game.
    ///     Allows navigation from one map to the next (between stages).
    /// </summary>
    public class StageMapsManager : MonoBehaviour
    {
        [Header("Options")]
        public bool MovePlayerWithStageChange = true;
        public bool FollowPlayerWhenMoving = false;
        public bool ShowStageButtons = false;
        public bool ShowMovementButtons = false;

        [Header("References")]
        public StageMap[] stageMaps;
        public PlayerPin playerPin;
        public MapCameraController mapCamera;

        [Header("UI")]

        public Camera UICamera;
        public MapStageIndicator mapStageIndicator;
        public GameObject leftStageButton;
        public GameObject rightStageButton;

        public MapPlayInfoPanel playInfoPanel;
        public MapPlayButtonsPanel playButtonsPanel;

        // DEPRECATED
        public GameObject nextPlaySessionButton;
        // DEPRECATED
        public GameObject beforePlaySessionButton;

        public GameObject playButton;

        // Additional UI for navigation
        public GameObject navigationIconsPanel;
        public GameObject learningBookButton;
        public GameObject minigamesBookButton;
        public GameObject profileBookButton;
        public GameObject anturaSpaceButton;

        #region State

        // The stage that is currently shown to the player
        private int shownStage;
        private bool inTransition;

        #endregion

        #region Properties

        private int PreviousPlayerStage
        {
            get { return PreviousJourneyPosition.Stage; }
        }

        private int CurrentPlayerStage    // @note: this may be different than shownStage as you can preview the next stages
        {
            get { return CurrentJourneyPosition.Stage; }
        }

        public static JourneyPosition PreviousJourneyPosition
        {
            get
            {
                return AppManager.I.Player.PreviousJourneyPosition;
            }
        }
        private JourneyPosition targetCurrentJourneyPosition;

        public static JourneyPosition CurrentJourneyPosition
        {
            get
            {
                return AppManager.I.Player.CurrentJourneyPosition;
            }
        }

        private int MaxUnlockedStage
        {
            get
            {
                return AppManager.I.Player.MaxJourneyPosition.Stage;
            }
        }

        private int FinalStage
        {
            get { return AppManager.I.JourneyHelper.GetFinalJourneyPosition().Stage; }
        }

        public StageMap StageMap(int Stage)
        {
            return stageMaps[Stage - 1];
        }

        public bool IsAtFirstStage
        {
            get { return shownStage == 1; }
        }

        private bool IsAtMaxUnlockedStage
        {
            get { return shownStage == MaxUnlockedStage; }
        }

        public bool IsAtFinalStage
        {
            get { return shownStage == FinalStage; }
        }

        public StageMap CurrentShownStageMap
        {
            get { return StageMap(shownStage); }
        }

        private bool IsStagePlayable(int stage)
        {
            return stage <= MaxUnlockedStage;
        }

        #endregion

        private static bool TEST_JOURNEY_POS = false;

        private void Awake()
        {
            // DEBUG
            if (TEST_JOURNEY_POS)
            {
                TEST_JOURNEY_POS = false;

                // TEST: already at a PS, later stage
                AppManager.I.Player.SetMaxJourneyPosition(new JourneyPosition(3, 4, 2), _forced: true);
                AppManager.I.Player.SetCurrentJourneyPosition(new JourneyPosition(3, 4, 2));
                AppManager.I.Player.ForcePreviousJourneyPosition(new JourneyPosition(3, 4, 2));

                // TEST: basic PS
                //AppManager.I.Player.SetMaxJourneyPosition(new JourneyPosition(1, 1, 2), _forced: true);
                //AppManager.I.Player.SetCurrentJourneyPosition(new JourneyPosition(1, 1, 2));
                //AppManager.I.Player.ForcePreviousJourneyPosition(new JourneyPosition(1, 1, 1));

                // TEST: basic assessment
                //AppManager.I.Player.SetMaxJourneyPosition(new JourneyPosition(1, 1, 100), _forced: true);
                //AppManager.I.Player.SetCurrentJourneyPosition(new JourneyPosition(1, 1, 100));
                //AppManager.I.Player.ForcePreviousJourneyPosition(new JourneyPosition(1, 1, 2));

                // TEST: next-stage PS
                //AppManager.I.Player.SetMaxJourneyPosition(new JourneyPosition(2, 1, 1), _forced: true);
                //AppManager.I.Player.SetCurrentJourneyPosition(new JourneyPosition(2, 1, 1));
                //AppManager.I.Player.ForcePreviousJourneyPosition(new JourneyPosition(1, 14, 100));

                Debug.Log("FORCED TEST_JOURNEY_POS");
            }

            shownStage = PreviousPlayerStage;
            targetCurrentJourneyPosition = CurrentJourneyPosition;

            // Setup stage availability
            for (int stage_i = 1; stage_i <= stageMaps.Length; stage_i++)
            {
                bool isStageUnlocked = stage_i <= MaxUnlockedStage;
                bool isWholeStageUnlocked = stage_i < MaxUnlockedStage;
                StageMap(stage_i).Initialise(isStageUnlocked, isWholeStageUnlocked);
                //StageMap(stage_i).Hide();
            }

            // Disable 2D play buttons
            playButtonsPanel.gameObject.SetActive(false);
        }

        private void Start()
        {
            // Show the current stage
            TeleportCameraToShownStage(shownStage);
            UpdateStageIndicatorUI(shownStage);
            UpdateButtonsForStage(shownStage);

            // Position the player
            playerPin.gameObject.SetActive(true);
            //playerPin.onMoveStart += HidePlayPanel;
            //playerPin.onMoveStart += CheckCurrentStageForPlayerReset;
            //playerPin.onMoveEnd += ShowPlayPanel;
            playerPin.ForceToJourneyPosition(PreviousJourneyPosition, justVisuals: true);
            playerPin.LookAtNextPin(false);

            UpdateStageButtonsUI();

            var isGameCompleted = AppManager.I.Player.HasFinalBeenShown();
            if (!isGameCompleted && WillPlayAssessmentNext())
            {
                PlayRandomAssessmentDialog();
            }

            // Coming from the other stage
            StartCoroutine(InitialMovementCO());

            mapCamera.Initialise(this);

            var tutorialManager = gameObject.GetComponentInChildren<MapTutorialManager>();
            tutorialManager.HandleStart();
        }

        private IEnumerator InitialMovementCO()
        {
            //HidePlayPanel();
            StageMap(shownStage).FlushAppear(PreviousJourneyPosition);

            bool needsAnimation = !Equals(targetCurrentJourneyPosition, PreviousJourneyPosition);
            //Debug.Log("TARGET CURRENT: " + targetCurrentJourneyPosition  + " PREV: " + PreviousJourneyPosition);
            TeleportCameraToShownStage(shownStage);
            if (!needsAnimation)
            {
                //Debug.Log("Already at the correct stage " + shownStage);
                SelectPin(StageMap(shownStage).PinForJourneyPosition(targetCurrentJourneyPosition));
                StageMap(shownStage).FlushAppear(AppManager.I.Player.MaxJourneyPosition);
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                StageMap(shownStage).Appear(PreviousJourneyPosition, AppManager.I.Player.MaxJourneyPosition);

                //Debug.Log("Shown stage: " + shownStage + " TargetJourneyPos " + targetCurrentJourneyPosition +   " PreviousJourneyPos " + PreviousJourneyPosition);
                if (shownStage != targetCurrentJourneyPosition.Stage)
                {
                    //Debug.Log("ANIMATING TO STAGE: " + targetCurrentJourneyPosition.Stage + " THEN MOVING TO " + targetCurrentJourneyPosition);
                    yield return StartCoroutine(SwitchFromToStageCO(shownStage, targetCurrentJourneyPosition.Stage, true));
                    mapCamera.SetAutoFollowTransformCurrentMap(playerPin.transform);
                    SelectPin(StageMap(shownStage).PinForJourneyPosition(targetCurrentJourneyPosition));
                    playerPin.MoveToJourneyPosition(targetCurrentJourneyPosition, StageMap(shownStage));
                }
                else
                {
                    //Debug.Log("JUST MOVING TO " + targetCurrentJourneyPosition);
                    yield return new WaitForSeconds(3.0f);
                    mapCamera.SetAutoFollowTransformCurrentMap(playerPin.transform);
                    SelectPin(StageMap(shownStage).PinForJourneyPosition(targetCurrentJourneyPosition));
                    playerPin.MoveToJourneyPosition(targetCurrentJourneyPosition, StageMap(shownStage));
                }
            }

            while (playerPin.IsAnimating)
            {
                yield return null;
            }

            mapCamera.SetManualMovementCurrentMap();
            //ReSelectCurrentPin();
        }

        private bool WillPlayAssessmentNext()
        {
            return AppManager.I.JourneyHelper.IsAssessmentTime(CurrentJourneyPosition)
                && CurrentJourneyPosition.Stage == AppManager.I.Player.MaxJourneyPosition.Stage &&
                CurrentJourneyPosition.LearningBlock == AppManager.I.Player.MaxJourneyPosition.LearningBlock;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #region Dialogs

        private void PlayRandomAssessmentDialog()
        {
            var data = new LocalizationDataId[3];
            data[0] = LocalizationDataId.Assessment_Start_1;
            data[1] = LocalizationDataId.Assessment_Start_2;
            data[2] = LocalizationDataId.Assessment_Start_3;
            var n = Random.Range(0, data.Length);
            KeeperManager.I.PlayDialog(data[n], true, true);
        }

        #endregion

        #region Selection

        private Pin selectedPin = null;

        // Used by the Antura Hint
        public void MoveToPlayerPin()
        {
            // Do not teleport to player while he is animating!
            //if (playerPin.IsAnimating) return;

            var playerStageMap = StageMap(CurrentPlayerStage);
            //Debug.Log("Player current pin is " + playerPin.CurrentPinIndex);
            //Debug.Log("CurrentPlayerStage: " + CurrentPlayerStage);
            //Debug.Log("current player stage map: " + playerStageMap.stageNumber);
            var targetPin = playerStageMap.PinForIndex(playerPin.CurrentPinIndex);
            mapCamera.SetAutoFollowTransformCurrentMap(targetPin.transform);

            if (targetPin != selectedPin)
            {
                //Debug.Log("Selecting pin " + targetPin.journeyPosition);
                SelectPin(targetPin);
            }
        }

        private void ReSelectCurrentPin()
        {
            var playerStageMap = StageMap(CurrentPlayerStage);
            SelectPin(playerStageMap.PinForIndex(playerPin.CurrentPinIndex));

            // Make sure to move the camera too
            mapCamera.SetAutoFollowTransformCurrentMap(playerPin.transform);
        }

        public Pin SelectedPin {  get { return selectedPin;  } }

        public void SelectPin(Pin pin)
        {
            if (!pin.IsInteractionEnabled) return;

            if (selectedPin == pin)
            {
                // Already selected: PLAY directly (if not locked)
                if (selectedPin.isLocked)
                {
                    HandleLockedButton();
                }
                else
                {
                    PlayCurrentPlaySession();
                }
            }
            else
            {
                // New selection
                selectedPin = pin;

                // Check for stage change (can happen at the sides!)
                if (shownStage != pin.journeyPosition.Stage)
                {
                    shownStage = pin.journeyPosition.Stage;
                    ColorCameraToShownStage(shownStage);
                }

                ResetSelections();
                selectedPin.Select(true);

                playInfoPanel.SetPin(pin);
                playButtonsPanel.SetPin(pin);

                // Optionally move Antura there
                if (!pin.isLocked)
                {
                    playerPin.MoveToPin(pin.pinIndex, shownStage);
                }
            }
        }

        void PlayCurrentPlaySession()
        {
            AppManager.I.NavigationManager.GoToNextScene();
        }

        #endregion

        #region Selection

        public void ResetSelections()
        {
            foreach (var stageMap in stageMaps)
            {
                // Deselect all pins
                foreach (var pin in stageMap.Pins)
                {
                    pin.Select(false);
                }

                // Select the current pin
                //var correctPin = stageMap.PinForJourneyPosition(CurrentJourneyPosition);
                //if (correctPin != null) correctPin.Select(true);
            }
        }

        #endregion

        // TODO: check if something called this
        /*private void PlayDialogStages(LocalizationDataId data)
        {
            KeeperManager.I.PlayDialog(data);
        }*/

        #region Stage Navigation

        /// <summary>
        /// Move to the next Stage map
        /// Called by buttons.
        /// </summary>
        public void MoveToNextStageMap()
        {
            if (inTransition) return;
            if (IsAtFinalStage) return;

            int fromStage = shownStage;
            int toStage = shownStage + 1;

            SwitchFromToStage(fromStage, toStage, true);

            //HideTutorial();
        }

        /// <summary>
        /// Move to the previous Stage map
        /// Called by buttons.
        /// </summary>
        public void MoveToPreviousStageMap()
        {
            if (inTransition) return;
            if (IsAtFirstStage) return;

            int fromStage = shownStage;
            int toStage = shownStage - 1;

            SwitchFromToStage(fromStage, toStage, true);

            //if (IsAtFirstStage) {
            //    ShowTutorial();
            //}
        }

        public void MoveToStageMap(int toStage, bool animateCamera = false)
        {
            if (inTransition) return;

            int fromStage = shownStage;
            if (toStage == fromStage) return;

            //Debug.Log("FROM " + fromStage + " TO " + toStage);
            SwitchFromToStage(fromStage, toStage, animateCamera);
        }

        private void UpdateButtonsForStage(int stage)
        {
            UpdateStageButtonsUI();

            bool playable = IsStagePlayable(stage);
            //playButton.SetActive(playable);

            nextPlaySessionButton.SetActive(ShowMovementButtons && playable);
            beforePlaySessionButton.SetActive(ShowMovementButtons && playable);
        }

        /*private void CheckCurrentStageForPlayerReset()
        {
            //Debug.Log("ShownStage: " + shownStage + " Current: " + CurrentPlayerStage);
            if (shownStage != CurrentPlayerStage)
            {
                bool comingFromHigherStage = CurrentPlayerStage > shownStage;
                playerPin.ResetPlayerPositionAfterStageChange(comingFromHigherStage);
            }
        }*/

        private void SwitchFromToStage(int fromStage, int toStage, bool animateCamera = false)
        {
            StartCoroutine(SwitchFromToStageCO(fromStage, toStage, animateCamera));
        }

        private IEnumerator SwitchFromToStageCO(int fromStage, int toStage, bool animateCamera = false)
        {
            shownStage = toStage;

            inTransition = true;
            //Debug.Log("Switch from " + fromStage + " to " + toStage);

            // HidePlayPanel();

            // Change stage reference
            StageMap(toStage).FlushAppear(AppManager.I.Player.MaxJourneyPosition);

            // Update Player Stage too, if needed
            /*if (MovePlayerWithStageChange)
            {
                if (IsStagePlayable(toStage) && toStage != shownStage)
                {
                    bool comingFromHigherStage = fromStage > toStage;
                    playerPin.ResetPlayerPositionAfterStageChange(comingFromHigherStage);
                }
            }*/

            // Animate the switch
            if (animateCamera)
            {
                AnimateCameraToShownStage(toStage);
                yield return new WaitForSeconds(0.8f);
            }
            else
            {
                ColorCameraToShownStage(toStage);
            }

            // Update the stage map for the player too
            if (MovePlayerWithStageChange) SwitchStageMapForPlayer(StageMap(toStage));

            // Show the new stage
            UpdateStageIndicatorUI(toStage);
            UpdateButtonsForStage(toStage);

            /*
            if (MovePlayerWithStageChange) {
                ShowPlayPanel();
            } else {
                if (toStage == CurrentPlayerStage) {
                    ShowPlayPanel();
                }
            }
            */

            // Hide the last stage
            //StageMap(fromStage).Hide();

            // End transition
            inTransition = false;

            //Debug.Log("We are at stage " + shownStage + ". Player current is " + CurrentPlayerStage);

            yield return null;
        }

        #endregion

        #region Camera

        private void TeleportCameraToShownStage(int stage)
        {
            var stageMap = StageMap(stage);
            stageMap.Show();

            // We'll look at the current player position, if possible.
            bool playable = IsStagePlayable(stage);
            if (playable)
            {
                mapCamera.TeleportToLookAtFree(playerPin.transform, stageMap.cameraPivotStart);
            }
            else
            {
                mapCamera.TeleportTo(stageMap.cameraPivotStart);
            }

            Camera.main.backgroundColor = stageMap.color;
            Camera.main.GetComponent<CameraFog>().color = stageMap.color;

            SwitchStageMapForPlayer(stageMap, true);
        }

        private void AnimateCameraToShownStage(int stage)
        {
            //Debug.Log("Animating to stage " + stage);
            var stageMap = StageMap(stage);
            stageMap.Show();
            stageMap.ResetStageOnShow(CurrentPlayerStage == stage);

            // We'll look at the current player position, if possible
            // AND if the player is in that stage
            bool playable = IsStagePlayable(stage);
            if (playable && playerPin.currentStageMap == stageMap)
            {
                mapCamera.SetAutoMoveToLookAtFree(playerPin.transform, stageMap.cameraPivotStart, 0.6f);
            }
            else
            {
                mapCamera.SetAutoMoveToTransformFree(stageMap.cameraPivotStart, 0.6f);
            }

            Camera.main.DOColor(stageMap.color, 1);
            Camera.main.GetComponent<CameraFog>().color = stageMap.color;
        }

        private void ColorCameraToShownStage(int stage)
        {
            var stageMap = StageMap(stage);
            Camera.main.DOColor(stageMap.color, 1);
            Camera.main.GetComponent<CameraFog>().color = stageMap.color;
        }

        private void SwitchStageMapForPlayer(StageMap newStageMap, bool init = false)
        {
            if (playerPin.IsAnimating) playerPin.StopAnimation(stopWhereItIs: false);
            playerPin.currentStageMap = newStageMap;

            // Move the player too, if the stage is unlocked
            if (!init && !newStageMap.FirstPin.isLocked && MovePlayerWithStageChange)
            {
                playerPin.ForceToJourneyPosition(CurrentJourneyPosition);
            }
        }

        #endregion

        #region Stage Indicator UI

        private void UpdateStageIndicatorUI(int stage)
        {
            mapStageIndicator.Init(stage - 1, FinalStage);
        }

        private void UpdateStageButtonsUI()
        {
            if (!ShowStageButtons)
            {
                rightStageButton.SetActive(false);
                leftStageButton.SetActive(false);
            }
            else
            {
                if (IsAtFirstStage)
                {
                    rightStageButton.SetActive(false);
                }
                else if (IsAtFinalStage)
                {
                    leftStageButton.SetActive(false);
                }
                else
                {
                    rightStageButton.SetActive(true);
                    leftStageButton.SetActive(true);
                }
            }
        }

        #endregion

        #region UI Activation

        /*
        private void ShowPlayButton()
        {
            playButton.SetActive(true);
        }

        private void HidePlayButton()
        {
            playButton.SetActive(false);
        }

        private void ShowPlayPanel()
        {
            //playButtonsPanel.gameObject.SetActive(true);
            playInfoPanel.gameObject.SetActive(true);
            playerPin.CheckMovementButtonsEnabling();
        }

        private void HidePlayPanel()
        {
            playButtonsPanel.gameObject.SetActive(false);
            playInfoPanel.gameObject.SetActive(false);
        }
        */

        public void DeactivateAllUI()
        {
            SetStageUIActivation(false);
            SetLearningBookUIActivation(false);
            SetMinigamesBookUIActivation(false);
            SetProfileBookUIActivation(false);
            SetAnturaSpaceUIActivation(false);
            SetGlobalUIActivation(false);
            SetPlayUIActivation(false);
        }

        public void ActivateAllUI()
        {
            SetStageUIActivation(true);
            SetLearningBookUIActivation(true);
            SetMinigamesBookUIActivation(true);
            SetProfileBookUIActivation(true);
            SetAnturaSpaceUIActivation(true);
            SetGlobalUIActivation(true);
            SetPlayUIActivation(true);
        }

        public void SetPlayUIActivation(bool choice)
        {
            if (selectedPin) selectedPin.EnableInteraction(choice);
            playButtonsPanel.gameObject.SetActive(choice);
            playInfoPanel.gameObject.SetActive(choice);
        }

        public void SetStageUIActivation(bool choice)
        {
            mapStageIndicator.gameObject.SetActive(choice);
        }

        public void SetLearningBookUIActivation(bool choice)
        {
            learningBookButton.gameObject.SetActive(choice);
        }

        public void SetProfileBookUIActivation(bool choice)
        {
            profileBookButton.gameObject.SetActive(choice);
        }

        public void SetMinigamesBookUIActivation(bool choice)
        {
            minigamesBookButton.gameObject.SetActive(choice);
        }

        public void SetAnturaSpaceUIActivation(bool choice)
        {
            anturaSpaceButton.gameObject.SetActive(choice);
        }

        public void SetGlobalUIActivation(bool choice)
        {
            GlobalUI.ShowPauseMenu(choice);
        }

        #endregion

        public void HandleLockedButton()
        {
            AudioManager.I.PlaySound(Sfx.KO);
        }

        #region Static Utilities

        public static int GetPosIndexFromJourneyPosition(StageMap stageMap, JourneyPosition journeyPos)
        {
            var st = journeyPos.Stage;

            if (stageMap.stageNumber > st)
                return 0;

            if (stageMap.stageNumber < st)
                return stageMap.MaxUnlockedPinIndex;

            var pin = stageMap.PinForJourneyPosition(journeyPos);
            return pin.pinIndex;
        }

        #endregion

    }
}