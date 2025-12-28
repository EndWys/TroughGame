using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Proyotype.Player.PlayerMediator;

namespace ProjectCore.Features.Proyotype.Player
{
    public interface IPlayerColleague : IColleague<IPlayerColleague, EPlayerEventType>  
    {
        
    }
}