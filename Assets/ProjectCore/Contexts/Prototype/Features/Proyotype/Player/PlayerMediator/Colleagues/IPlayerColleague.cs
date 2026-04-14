using Domain;
using ProjectCore.Features.Prototype.Player.PlayerMediator;

namespace ProjectCore.Features.Prototype.Player
{
    public interface IPlayerColleague : IColleague<IPlayerColleague, EPlayerEventType>  
    {
        
    }
}