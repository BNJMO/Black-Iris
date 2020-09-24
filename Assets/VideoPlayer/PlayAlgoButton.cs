using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNJMO;

public class PlayAlgoButton : BBehaviour
{
    #region Public Events


    #endregion

    #region Public Methods


    #endregion

    #region Inspector Variables
    [SerializeField]
    private BButton bButton;

    #endregion

    #region Private Variables
    private VideoController videoController;

    #endregion

    #region Life Cycle
    protected override void OnValidate()
    {
        base.OnValidate();

        if (CanValidate())
        {
            if (bButton == null)
            {
                bButton = GetComponent<BButton>();
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (IS_NOT_NULL(bButton))
        {
            bButton.ButtonReleased += On_BButton_ButtonReleased;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        bButton.ButtonReleased -= On_BButton_ButtonReleased;
    }

    protected override void Awake()
    {
        base.Awake();

        videoController = FindObjectOfType<VideoController>();
    }

    #endregion

    #region Events Callbacks
    private void On_BButton_ButtonReleased(BButton arg1, bool arg2)
    {
        if (IS_NOT_NULL(videoController))
        {
            if (videoController.UseSynchronization == true)
            {
                videoController.UseSynchronization = false;
                bButton.SetButtonText("Synch Off");
            }
            else
            {
                videoController.UseSynchronization = true;
                bButton.SetButtonText("Synch True");
            }
        }
    }

    #endregion

    #region Others


    #endregion
}
