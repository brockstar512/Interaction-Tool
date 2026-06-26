namespace IT.Effects.Flash
{
    public interface IFlashable
    {
        // duration <= 0 => unbounded (stop manually via StopFlash).
        void StartFlash(float duration);
        void StopFlash();
    }
}
