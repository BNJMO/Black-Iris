using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class PlayerAvatarBAnchor : BAnchor
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private GameObject avatarAppearance;

        #endregion

        #region Private Variables


        #endregion

        #region Life Cycle
        protected override void OnEnable()
        {
            base.OnEnable();

            //BEventsCollection.playerjoin
        }

        protected override void Awake()
        {
            base.Awake();

            HideAvatar();
        }

        protected override void LateStart()
        {
            base.LateStart();

            if (Owner == BEventManager.Instance.LocalNetworkID)
            {
                HideAvatar();
            }
            else
            {
                ShowAvatar();
            }
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others
        protected virtual void HideAvatar()
        {
            LogConsoleRed("Hide Avatar");
            if (IS_NOT_NULL(avatarAppearance))
            {
                avatarAppearance.SetActive(false);
            }
        }

        protected virtual void ShowAvatar()
        {
            LogConsoleRed("Show Avatar");
            if (IS_NOT_NULL(avatarAppearance))
            {
                avatarAppearance.SetActive(true);
            }
        }



        #endregion
    }
}
