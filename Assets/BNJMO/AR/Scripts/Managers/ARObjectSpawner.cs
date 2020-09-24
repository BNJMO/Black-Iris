using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace BNJMO
{
    public class ARObjectSpawner : BBehaviour
    {
        #region Public Events

        #endregion

        #region Public Methods and Getters

        public void ReplaceRoot()
        {
            WorldRootBAnchor worldRootBAnchor = ARManager.Instance.WorldRootBAnchor;
            if (IS_NOT_NULL(worldRootBAnchor))
            {
                worldRootBAnchor.transform.position = ARCursor.Instance.GetCursorPosition();
                worldRootBAnchor.transform.rotation = ARCursor.Instance.GetCursorRotation();
                worldRootBAnchor.gameObject.SetActive(true);
            }
        }

        public void SetRepObject()
        {
            if (IS_NOT_NULL(ARManager.Instance))
            {
                // Spawn or replace
                if (replicatedObject == null)
                {
                    if (IS_NOT_NULL(replicatedObjectPrefab))
                    {
                        replicatedObject = Instantiate(replicatedObjectPrefab, ARCursor.Instance.GetCursorPosition(), ARCursor.Instance.GetCursorRotation());
                    }
                }
                else
                {
                    replicatedObject.transform.position = ARCursor.Instance.GetCursorPosition();
                    replicatedObject.transform.rotation = ARCursor.Instance.GetCursorRotation();
                }

                BAnchorInformation anchorInformation = replicatedObject.GetBAnchorInformation();

                string serializedBAnchorInformation = BUtils.SerializeObject(anchorInformation);


                // Save position and rotation
                PlayerPrefs.SetString("TEST_Anchor", serializedBAnchorInformation);
            }
        }

        public void LoadRepObject()
        {
            if (replicatedObject == null)
            {
                if (IS_NOT_NULL(replicatedObjectPrefab))
                {
                    replicatedObject = Instantiate(replicatedObjectPrefab);
                }
            }

            string serializedBAnchorInformation = PlayerPrefs.GetString("TEST_Anchor", "Not_Init");
            if (serializedBAnchorInformation == "Not_Init")
            {
                LogConsoleError("Trying to load a BAnchor that was never saved");
                return;
            }

            //BAnchorInformation bAnchorInformation = JsonConvert.DeserializeObject<BAnchorInformation>(serializedBAnchorInformation);
            BAnchorInformation bAnchorInformation = BUtils.DeserializeObject<BAnchorInformation>(serializedBAnchorInformation);
            if (IS_NOT_NULL(bAnchorInformation))
            {
                replicatedObject.SetTransformedPosition(bAnchorInformation.TransformedPosition);
                replicatedObject.SetTransformedRotation(bAnchorInformation.TransformedRotation);
            }    
        }



        #endregion

        #region Serialized Fields
        [SerializeField] private BAnchor replicatedObjectPrefab;

        #endregion

        #region Private Variables
        private BAnchor replicatedObject;


        #endregion

        #region Life Cycle


        #endregion

        #region Events Callbacks

        #endregion

        #region Others

        #endregion
    }
}
