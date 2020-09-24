using System.Collections.Generic;


namespace BNJMO
{
    public class NetControllerInputSource : AbstractInputSource
    {
        private Dictionary<EControllerID, MirrorPlayerNCListener> connectedNetworkControllers = new Dictionary<EControllerID, MirrorPlayerNCListener>();

        private float debugX;
        private float debugY;

        //protected override void OnGUI()
        //{
        //    base.OnGUI();

        //    string ipAddress = BUtils.GetLocalIPAddress();
        //    GUI.Box(new Rect(10, Screen.height - 50, 100, 30), ipAddress);
        //}

        protected override void Update()
        {
            base.Update();

            foreach (var pair in connectedNetworkControllers)
            {
                LogCanvas("NetControllerInputSource", pair.Value + " : " + pair.Key);
            }

            LogCanvas(BConsts.DEBUGTEXT_NetControllerInputSource, "X : " + debugX + " - Y : " + debugY);
        }

        /// <summary>
        /// Called from a NCPlayerListener when spawned to connect to the next available ControllerID
        /// </summary>
        /// <param name="playerInput"> New NCPlayerListener </param>
        /// <returns> Assigned ControllerID </returns>
        public EControllerID OnNewNCJoined(MirrorPlayerNCListener playerNCListener)
        {
            IS_NOT_NULL(playerNCListener);

            // Assign a ControllerID
            EControllerID controllerID = GetNextFreeNetworkControllerID();
            if (controllerID != EControllerID.NONE)
            {
                // Connect controller on Input Manager
                if (InputManager.Instance.ConnectController(controllerID) == true)
                {
                    LogConsole("Adding new Player NC Listener : " + playerNCListener.gameObject.name);
                    connectedNetworkControllers.Add(controllerID, playerNCListener);

                    // Bind Input events
                    playerNCListener.ButtonPressed += PlayerNCListener_OnButtonPressed;
                    playerNCListener.ButtonReleased += PlayerNCListener_OnButtonReleased;
                    playerNCListener.JoystickMoved += PlayerNCListener_OnJoystickMoved;
                }
                else
                {
                    return EControllerID.NONE;
                }
            }
            else
            {
                LogConsoleWarning("No free Controller ID found for new connected Net Controller : " + playerNCListener.IpAdress);
            }
            return controllerID;
        }

        public void PlayerNCListener_OnJoystickMoved(EControllerID controllerID, EInputAxis inputAxis, float x, float y)
        {
            if (IS_KEY_CONTAINED(connectedNetworkControllers, controllerID))
            {
                if (IS_NOT_NONE(inputAxis))
                {
                    InvokeAxisUpdated(controllerID, inputAxis, x, y);
                }

                // Debug
                debugX = x;
                debugY = y;
            }
        }

        public void PlayerNCListener_OnButtonPressed(EControllerID controllerID, EInputButton inputButton)
        {
            LogConsole("ButtonPressed");

            if (IS_KEY_CONTAINED(connectedNetworkControllers, controllerID))
            {
                if (IS_NOT_NONE(inputButton))
                {
                    InvokeButtonPressed(controllerID, inputButton);
                }
            }
        }

        public void PlayerNCListener_OnButtonReleased(EControllerID controllerID, EInputButton inputButton)
        {
            LogConsole("ButtonReleased");


            if (IS_KEY_CONTAINED(connectedNetworkControllers, controllerID))
            {
                if (IS_NOT_NONE(inputButton))
                {
                    InvokeButtonReleased(controllerID, inputButton);
                }
            }
        }

        private EControllerID GetNextFreeNetworkControllerID()
        {
            LogConsoleError("Depricated code!!");
            EControllerID controllerID = EControllerID.NONE;
            //foreach (EControllerID controllerIDitr in BConsts.NETWORK_CONTROLLERS)
            //{
            //    if (connectedNetworkControllers.ContainsKey(controllerIDitr) == false)
            //    {
            //        controllerID = controllerIDitr;
            //        break;
            //    }
            //}
            return controllerID;
        }
    }
}
