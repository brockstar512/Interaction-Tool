public interface IPullDependent//idependent<T>
{
    // amount: 0 = lever at origin, 1 = lever fully pulled
    void OnPullChanged(float amount);
}
