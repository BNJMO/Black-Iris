using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNJMO;

namespace BNJMO
{
    //[RequireComponent(typeof(AudioSource))]
    public class BallDroneAudio : BBehaviour
    {
        #region Public Events


        #endregion

        #region Public Methods


        #endregion

        #region Inspector Variables
        [SerializeField]
        private AudioClip wallCollisionSound;

        [SerializeField]
        private AudioClip floorCollisionSound;

        [SerializeField]
        private AudioClip ballCaughtSound;

        [SerializeField]
        private AudioClip ballReleasedSound;



        #endregion

        #region Private Variables
        //private AudioSource myAudioSource;

        #endregion

        #region Life Cycle
        protected override void Awake()
        {
            base.Awake();

            //myAudioSource = GetComponent<AudioSource>();

            BEventsCollection.DZ_BallDroneCaught += On_DZ_BallDroneCaught;
            BEventsCollection.DZ_BallDroneReleased.Event += On_DZ_BallDroneReleased;
        }
        #endregion

        #region Events Callbacks
        private void OnCollisionEnter(Collision collision)
        {
            PlayAreaPlane playAreaPlane = collision.gameObject.GetComponent<PlayAreaPlane>();
            if (playAreaPlane)
            {
                switch (playAreaPlane.PlayAreaWallType)
                {
                    case EPlayAreaPlaneType.FLOOR:
                        AudioManager.Instance.SpawnSoundObject(transform.position, floorCollisionSound);
                        break;

                    case EPlayAreaPlaneType.WALL:
                        AudioManager.Instance.SpawnSoundObject(transform.position, wallCollisionSound);
                        break;
                }
            }
        }

        private void On_DZ_BallDroneCaught(BEHandle<EPlayerID> handle)
        {
            AudioManager.Instance.SpawnSoundObject(ballCaughtSound);
        }

        private void On_DZ_BallDroneReleased(BEHandle<EPlayerID> handle)
        {
            AudioManager.Instance.SpawnSoundObject(ballReleasedSound);
        }

        #endregion

        #region Others


        #endregion
    }
}
