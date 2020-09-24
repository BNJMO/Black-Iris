using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class DropZonePlayer : AbstractNetPlayer
    {
        #region Public Events


        #endregion

        #region Public Methods
        public override void SetOwner(ENetworkID networkID)
        {
            if (IS_NOT_NULL(playerAvatarBAnchor)
                && IS_NOT_NONE(networkID))
            {
                playerAvatarBAnchor.Owner = networkID;
            }
        }
                
        public override ENetworkID GetOwner()
        {
            if (IS_NOT_NULL(playerAvatarBAnchor))
            {
                return playerAvatarBAnchor.Owner;
            }
            return ENetworkID.NONE;
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        private PlayerAvatarBAnchor playerAvatarBAnchor;

        #endregion

        #region Private Variables

        #endregion

        #region Life Cycle
        protected override void OnValidate()
        {
            base.OnValidate();

            if (CanValidate())
            {
                if (playerAvatarBAnchor == null)
                {
                    playerAvatarBAnchor = GetComponent<PlayerAvatarBAnchor>();
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            // Move to camera if locally owned
            if (IS_NOT_NULL(playerAvatarBAnchor)
                && playerAvatarBAnchor.Owner == BEventManager.Instance.LocalNetworkID
                && IS_NOT_NULL(Camera.main))
            {
                transform.parent = Camera.main.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
