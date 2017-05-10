using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PATH;

namespace Game.Event
{
    public class WaveEvent : Event
    {
        public override void StartEvent()
        {
            GhostManager.Instance.currentWaveEvent = this;
            SpellManager.Instance.stage++;
            SpellManager.Instance.canGenerate = true;
            a_star.SpeedUp();
            base.StartEvent();            
        }
        public override void EndEvent()
        {
            GhostManager.Instance.currentWaveEvent = null;
            SpellManager.Instance.canGenerate = false;                        
            base.EndEvent();
        }

    }
}

