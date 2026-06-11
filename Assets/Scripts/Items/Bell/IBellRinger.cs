using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Items.Bell
{
    public interface IBellRinger
    {
      public IBellRinger Init();
      public void Stop();
    }
}
