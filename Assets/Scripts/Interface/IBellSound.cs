using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Items.Bell
{
    public interface IBellSound
    {
      public IBellSound Init();
      public void Stop();
    }
}
