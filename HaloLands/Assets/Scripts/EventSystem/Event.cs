using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Event
{
    public class Event : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private UnityEvent _startList;

        [SerializeField]
        private UnityEvent _endList;

        #endregion Fields

        #region Event

        public virtual void StartEvent()
        {
            Debug.Log(string.Format("Start Event: {0}", this.gameObject.name));
            this._startList.Invoke();            
        }

        public virtual void EndEvent()
        {
            Debug.Log(string.Format("End Event: {0}", this.gameObject.name));
            this._endList.Invoke();
        }

        #endregion Event
    }
}

