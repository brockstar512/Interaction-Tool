using System.Threading.Tasks;

public interface ILocked
{
    public Task<bool> CanOpen(Utilities.KeyTypes keyType);
}
