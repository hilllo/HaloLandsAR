using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Event
{
    public class ManagerEvent : Event
    {
        [SerializeField]
        private GameMode gameMode;

        public override void StartEvent()
        {
            GameManager.Instance.gameMode = this.gameMode;
            base.StartEvent();
        }
    }
}
