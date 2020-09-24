using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace BNJMO
{
    [Serializable]
    public class BAnchorCollectionTupple
    {
        public BAnchorCollectionTupple()
        {
        }

        public BAnchorCollectionTupple(string bAnchorID, BAnchor bAnchorPrefab)
        {
            BAnchorID = bAnchorID;
            BAnchorPrefab = bAnchorPrefab;
        }

        public string BAnchorID = "None";
        public BAnchor BAnchorPrefab;
    }

    public class BAnchorsCollection : AbstractSingletonManager<BAnchorsCollection>
    {
        #region Public Events


        #endregion

        #region Public Methods
        public BAnchor GetBAnchorPrefab(string bAnchorID)
        {
            if (bAnchorCollectionMap.ContainsKey(bAnchorID))
            {
                return bAnchorCollectionMap[bAnchorID];
            }

            LogConsoleError("No BAnchor prefab was associated to the given BAnchorID in the BAnchor Collection : " + bAnchorID);

            return null;
        }

        #endregion

        #region Inspector Variables
        [SerializeField]
        [BoxGroup("BAnchors Collection", centerLabel: true)]
        private BAnchorCollectionTupple[] bAnchorCollectionTupples = new BAnchorCollectionTupple[0];

        #endregion

        #region Private Variables
        private Dictionary<string, BAnchor> bAnchorCollectionMap = new Dictionary<string, BAnchor>();

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            // Initialize Banchor Collection Map
            foreach (BAnchorCollectionTupple bAnchorCollectionTupple in bAnchorCollectionTupples)
            {
                string bAnchorID = bAnchorCollectionTupple.BAnchorID;
                BAnchor bAnchorPrefab = bAnchorCollectionTupple.BAnchorPrefab;
                if (IS_KEY_NOT_CONTAINED(bAnchorCollectionMap, bAnchorID)
                    && IS_NOT_NULL(bAnchorPrefab))
                {
                    bAnchorCollectionMap.Add(bAnchorID, bAnchorPrefab);
                }
            }
        }

        #endregion

        #region Events Callbacks


        #endregion

        #region Others


        #endregion
    }
}
