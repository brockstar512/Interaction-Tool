using Interface;
using System.Threading.Tasks;

public abstract class AnimationStateAsync
{
    public abstract Task Play<T>(T stateMachine) where T : IStateMachine;
}
