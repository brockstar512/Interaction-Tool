using System.Threading.Tasks;

public interface ILocked
{
    public bool CanOpen(Utilities.KeyTypes keyType);
}
