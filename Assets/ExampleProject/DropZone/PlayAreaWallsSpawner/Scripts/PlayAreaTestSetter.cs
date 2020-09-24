using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNJMO
{
    public class PlayAreaTestSetter : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods
        public void SetTestPlayArea()
        {
            StartCoroutine(SetTestPlayAreaCoroutine());
        }

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
        private IEnumerator SetTestPlayAreaCoroutine()
        {
            ARManager.Instance.SetupPlayArea();

            yield return new WaitForEndOfFrame();

            ARCursor.Instance.transform.position = new Vector3(0.0f, 0.0f, 1.0f);
            PlayAreaBAnchor playAreaBAnchor = FindObjectOfType<PlayAreaBAnchor>();
            if (IS_NOT_NULL(playAreaBAnchor))
            {
                playAreaBAnchor.EndPlacing();
            }

            yield return new WaitForEndOfFrame();

            ARCursor.Instance.transform.position = new Vector3(3.5f, 0.0f, 8.6f);

        }

        #endregion
    }
}
