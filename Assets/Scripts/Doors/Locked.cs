using System.Threading.Tasks;
using UnityEngine;

namespace Doors
{
    public abstract class Locked : MonoBehaviour, ILocked
    {
        [SerializeField] private Utilities.KeyTypes keyType;
        protected bool IsOpen = false;
        protected void UpdateLayerName()
        {
            this.gameObject.layer = LayerMask.NameToLayer(Utilities.LockedLayer);
        }
        protected virtual void OpenAnimation()
        {
            IsOpen = true;
        }
        protected bool CorrectKey(Utilities.KeyTypes key)
        {
            if (keyType == key)
            {
                return true;
            }
            return false;
        }
        public Task<bool> CanOpen(Utilities.KeyTypes key)
        {
            if (IsOpen)
            {
                return Task.FromResult(false);
            }

            if (CorrectKey(key))
            {
                OpenAnimation();
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

   

    }
}
