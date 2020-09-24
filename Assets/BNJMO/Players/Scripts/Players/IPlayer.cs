namespace BNJMO
{
    public interface IPlayer
    {
        EPlayerID PlayerID { get; }

        ETeamID TeamID { get; }

        bool IsDead { get; }
    }
}
