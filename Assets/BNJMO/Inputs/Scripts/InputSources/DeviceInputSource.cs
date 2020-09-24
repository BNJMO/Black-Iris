using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BNJMO
{
    public class DeviceInputSource : AbstractInputSource
    {
        private Dictionary<EControllerID, PlayerInputListener> connectedDeviceControllers = new Dictionary<EControllerID, PlayerInputListener>();

        protected override void InitializeEventsCallbacks()
        {
            base.InitializeEventsCallbacks();

            BEventsCollection.APP_AppStateUpdated.Event += On_APP_AppStateUpdated;
        }

        // TODO: disconnect player
        private void On_APP_AppStateUpdated(StateBEHandle<EAppState> eventHandle)
        {
            //switch(eventHandle.NewState)
            //{
            //    case EAppState.IN_MENU_IN_CONNECTING_CONTROLLERS:
            //        PlayerInputManager.instance.EnableJoining();
            //        break;

            //    default:
            //        PlayerInputManager.instance.DisableJoining();
            //        break;
            //}
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            foreach (EControllerID controllerID in connectedDeviceControllers.Keys)
            {
                PlayerInputListener playerInputListener = connectedDeviceControllers[controllerID];
                On_PlayerInputListener_AxisUpdated(controllerID, EInputAxis.MOVEMENT, playerInputListener.MoveAxis);
                On_PlayerInputListener_AxisUpdated(controllerID, EInputAxis.ROTATION, playerInputListener.RotateAxis);
                On_PlayerInputListener_AxisUpdated(controllerID, EInputAxis.TRIGGER_AXIS_L, playerInputListener.RotateAxis);
                On_PlayerInputListener_AxisUpdated(controllerID, EInputAxis.TRIGGER_AXIS_R, playerInputListener.RotateAxis);
            }
        }

        /// <summary>
        /// Called from a PlayerInputListener when spawned to connect to the next available ControllerID
        /// </summary>
        /// <param name="playerInput"> New player input listener </param>
        /// <returns> Assigned ControllerID </returns>
        public EControllerID OnNewDeviceJoined(PlayerInputListener playerInputListener)
        {
            IS_NOT_NULL(playerInputListener);
            // Assign a ControllerID
            EControllerID controllerID = GetNextFreeDeviceControllerID();

            if (controllerID != EControllerID.NONE)
            {
                // Connect controller on Input Manager
                if (InputManager.Instance.ConnectController(controllerID) == true)
                {
                    connectedDeviceControllers.Add(controllerID, playerInputListener);

                    // Bind Input events
                    playerInputListener.ButtonPressed += On_PlayerInputListener_ButtonPressed;
                    playerInputListener.ButtonReleased += On_PlayerInputListener_ButtonReleased;
                }
                else
                {
                    return EControllerID.NONE;
                }
            }
            else
            {
                LogConsoleWarning("No free Controller ID found for new connected device : " + playerInputListener.DeviceName);
            }

            return controllerID;
        }

        // TODO
        public void OnDeviceHasLeft(EControllerID controllerID, PlayerInputListener playerInputListener)
        {
            //IS_VALID(playerInputListener);

            //if ((connectedControllers.ContainsKey(controllerID)
            //    && (connectedControllers[controllerID] == playerInputListener))
            //{

            //}
            //else
            //{
            //    LogConsoleWarning("An invalid device has left : " + playerInputListener.DeviceName);
            //}
        }


        private void On_PlayerInputListener_ButtonPressed(EControllerID controllerID, EInputButton inputButton)
        {
            InvokeButtonPressed(controllerID, inputButton);
        }

        private void On_PlayerInputListener_ButtonReleased(EControllerID controllerID, EInputButton inputButton)
        {
            InvokeButtonReleased(controllerID, inputButton);
        }

        private void On_PlayerInputListener_AxisUpdated(EControllerID controllerID, EInputAxis inputAxis, Vector2 axisValues)
        {
            InvokeAxisUpdated(controllerID, inputAxis, axisValues.x, axisValues.y);
        }

        private EControllerID GetNextFreeDeviceControllerID()
        {
            EControllerID controllerID = EControllerID.NONE;
            foreach (EControllerID controllerIDitr in BConsts.DEVICE_CONTROLLERS)
            {
                if (connectedDeviceControllers.ContainsKey(controllerIDitr) == false)
                {
                    controllerID = controllerIDitr;
                    break;
                }
            }
            return controllerID;
        }
    }
}