using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public static class PlayerNetworkEntityConstants
    {
        public const string EntitiesContainer = "Player.EntitiesContainer";

        public static readonly NetworkEntityTypeData Player = new("player");
    }
}
