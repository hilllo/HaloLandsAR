using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Event
{
    public class TimerEvent : Event
    {
        [SerializeField]
        private float _waitForSec;

        public override void StartEvent()
        {
            base.StartEvent();
            StartCoroutine(this.CountDown());
        }

        private IEnumerator CountDown()
        {
            yield return new WaitForSeconds(this._waitForSec);
            this.EndEvent();
        }

    }
}

