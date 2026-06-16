using IT.Core.Dependency;

namespace IT.Core.WorldState
{
    public interface IWorldFlag : IDependencySource<bool>
    {
        string Id { get; }
        FlagScope Scope { get; }
    }
}
