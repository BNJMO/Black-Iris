using UnityEngine;

namespace BNJMO
{
    public abstract class AbstractPlayer : BBehaviour, IPlayer, IPawn
    {
        public virtual EPlayerID PlayerID { get { return playerID; } set { playerID = value; } }

        public ETeamID TeamID { get; set; }

        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }

        public bool IsDead { get; set; }

        public void DestroyPawn()
        {
            Destroy(gameObject);
        }

        [SerializeField]
        private EPlayerID playerID = EPlayerID.NONE;
    }
}