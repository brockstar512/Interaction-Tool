public interface IDependent<T>
{
    // amount: 0 = lever at origin, 1 = lever fully pulled
    void UpdateDependentValue(T amount);
}
