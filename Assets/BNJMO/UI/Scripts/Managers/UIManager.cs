using UnityEngine;
using System;
using System.Collections.Generic;

namespace BNJMO
{
    [Serializable]
    public struct SStartUIState
    {
        public EAppScene AppScene;
        public EUIState UIState;
    }

    public class UIManager : AbstractSingletonManager<UIManager>
    {
        public BFrame CurrentBFraneFocused          { get; private set; }
        public BMenu CurrentBMenuHighlighted        { get; private set; }
        public BButton CurrentBButtonHighlighted    { get; private set; }

        [Header("UIState")]
        [SerializeField] private SStartUIState[] startStates = new SStartUIState[0];

        private Dictionary<EUIState, BButton> highlightButtonsMap = new Dictionary<EUIState, BButton>();
        private bool canPressButton;

        protected override void Start()
        {
            base.Start();

            BEventsCollection.UI_FocusedFrameUpdated.Event += On_UI_FocusedFrameUpdated;
            BEventsCollection.UI_HighlightedBMenuUpdated.Event += On_UI_HighlightedBMenuUpdated;     
            BEventsCollection.UI_ButtonHighlighted += On_UI_ButtonHighlighted;

            BEventsCollection.INPUT_ButtonPressed.Event += On_INPUT_ButtonPressed;
            BEventsCollection.INPUT_ButtonReleased.Event += On_INPUT_ButtonReleased;

            //EventManager.Instance.GAME_GameStarted += On_GAME_GameStarted;
            //EventManager.Instance.GAME_GamePaused += On_GAME_GamePaused;
            //EventManager.Instance.GAME_GameUnPaused += On_GAME_GameUnPaused;
            //EventManager.Instance.GAME_GameEnded += On_GAME_GameEnded;
        }



        //protected override void InitializeStateMachine()
        //{
        //    LogConsole("InitializeStateMachine");

        //    // 1) Initialize start states
        //    foreach (SStartUIState startState in startStates)
        //    {
        //        if (IS_KEY_NOT_CONTAINED(startStatesMap, startState.AppScene))
        //        {
        //            startStatesMap.Add(startState.AppScene, startState.UIState);
        //        }
        //    }

        //    // 2) Define debugStateID
        //    debugStateID = "UIState";
        //}

        //protected override void BindStateMachineEvent()
        //{
        //    LogConsole("BindStateMachineEvent");

        //    // 3) Bind event
        //    StateUpdateEvent += EventManager.Instance.UI_UIStateUpdated.Invoke;
        //}

        #region Events Callbacks
        //private void On_UI_UIStateUpdated(StateBEHandle<EUIState> bEHandle)
        //{
        //    // Update highlighted button
        //    if (highlightButtonsMap.ContainsKey(bEHandle.NewState))
        //    {
        //        CurrentHihghlightedButton = highlightButtonsMap[bEHandle.NewState];
        //    }
        //    else
        //    {
        //        LogConsoleWarning("No button set as highlight for the UIState '" + bEHandle.NewState + "' found in this scene!");
        //    }
        //}

        private void On_UI_FocusedFrameUpdated(BEHandle<BFrame> bEHandle)
        {
            CurrentBFraneFocused = bEHandle.Arg1;
        }

        private void On_UI_HighlightedBMenuUpdated(BEHandle<BMenu> bEHandle)
        {
            CurrentBMenuHighlighted = bEHandle.Arg1;
        }

        private void On_UI_ButtonHighlighted(BEHandle<BButton> bEHandle)
        {
            CurrentBButtonHighlighted = bEHandle.Arg1;
        }

        private void On_INPUT_ButtonPressed(BEHandle<EControllerID, EInputButton> eventHandle)
        {
            EInputButton inputButton = eventHandle.Arg2;

            if (CurrentBButtonHighlighted)
            {
                BButton nextButton = null;
                switch (inputButton)
                {
                    case EInputButton.CONFIRM:
                        CurrentBButtonHighlighted.OnPressed();
                        canPressButton = true;
                        break;

                    case EInputButton.LEFT:
                        nextButton = CurrentBButtonHighlighted.GetNextButton(EButtonDirection.LEFT);
                        break;

                    case EInputButton.RIGHT:
                        nextButton = CurrentBButtonHighlighted.GetNextButton(EButtonDirection.RIGHT);
                        break;

                    case EInputButton.UP:
                        nextButton = CurrentBButtonHighlighted.GetNextButton(EButtonDirection.UP);
                        break;

                    case EInputButton.DOWN:
                        nextButton = CurrentBButtonHighlighted.GetNextButton(EButtonDirection.DOWN);
                        break;
                }

                // Update highlighted button
                if (nextButton != null)
                {
                    CurrentBButtonHighlighted.OnUnhighlighted();
                    CurrentBButtonHighlighted = nextButton;
                    nextButton.OnHighlighted();
                    canPressButton = false;
                }
            }
        }

        private void On_INPUT_ButtonReleased(BEHandle<EControllerID, EInputButton> eventHandle)
        {
            EInputButton inputButton = eventHandle.Arg2;

            if (CurrentBButtonHighlighted)
            {
                switch (inputButton)
                {
                    case EInputButton.CONFIRM:
                        if (canPressButton == true)
                        {
                            CurrentBButtonHighlighted.OnReleased(true);
                        }
                        break;
                }
            }
        }

        //private void On_GAME_GameEnded(AbstractGameMode gameMode, bool wasAborted)
        //{
        //    UpdateState(EUIState.IN_GAME_OVER);
        //}
        //private void On_GAME_GameUnPaused(AbstractGameMode gameMode)
        //{
        //    UpdateState(EUIState.IN_GAME_IN_RUNNING);
        //}
        //private void On_GAME_GamePaused(AbstractGameMode gameMode)
        //{
        //    UpdateState(EUIState.IN_GAME_IN_PAUSED);
        //}

        //private void On_GAME_GameStarted(AbstractGameMode gameMode)
        //{
        //    UpdateState(EUIState.IN_GAME_IN_RUNNING);
        //}
        #endregion



    }
}
