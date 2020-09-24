using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public abstract class AbstractNetPlayer : AbstractPlayer
    {
        #region Public Events


        #endregion

        #region Public Methods
        public abstract void SetOwner(ENetworkID networkID);

        public abstract ENetworkID GetOwner();

        #endregion

        #region Inspector Variables


        #endregion

        #region Private Variables


        #endregion

        #region Life Cycle


        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
