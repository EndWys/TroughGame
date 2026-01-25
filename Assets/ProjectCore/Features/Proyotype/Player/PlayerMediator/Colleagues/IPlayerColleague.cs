using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Prototype.Player.PlayerMediator;

namespace ProjectCore.Features.Prototype.Player
{
    public interface IPlayerColleague : IColleague<IPlayerColleague, EPlayerEventType>  
    {
        
    }
}