using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IT.Items.Candle
{
    public class CandleLight : MonoBehaviour, ICandleLight
    {
        public void On()
        {
            this.gameObject.SetActive(true);
        }

        public void Off()
        {
            this.gameObject.SetActive(false);
        }
    }
}
