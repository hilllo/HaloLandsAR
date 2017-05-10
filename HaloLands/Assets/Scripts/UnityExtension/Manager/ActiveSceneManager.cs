using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.Manager
{
    public abstract class ActiveSceneManager<T> : Manager<T>
    {

        #region Fields
        [Header("Common SceneManager Settings")]
        /// <summary>
        /// SceneType
        /// TODO: Create a enum for this
        /// </summary>
        //[SerializeField]
        //protected SceneType _sceneType;

        /// <summary>
        /// Control init/deactive(when scene ends) sequence through order here (deactive lastly)
        /// </summary>
        [Tooltip("Control init/deactive(when scene ends) sequence through order here (deactive lastly)")]
        [SerializeField]
        protected List<GameObject> InitActiveObjs;

        /// <summary>
        /// Control deactive(when scene ends) sequence through order here"
        /// </summary>
        [Tooltip("Control deactive(when scene ends) sequence through order here")]
        [SerializeField]
        protected List<GameObject> InitInactiveObjs;

        /// <summary>
        /// Control deactive(when scene ends) sequence through order here (deactive firstly)"
        /// </summary>
        [Tooltip("Control deactive(when scene ends) sequence through order here (deactive firstly)")]
        [SerializeField]
        protected Dictionary<int, GameObject> RuntimeObjs;

        /// <summary>
        /// RuntimeGenerateObjGroup
        /// </summary>
        protected Transform _RuntimeGenerateObjGroup;

        #endregion Fields

        #region Properties

        /// <summary>
        /// SceneType
        /// TODO: Create a enum for this
        /// </summary>
        //public SceneType sceneType
        //{
        //    get
        //    {
        //        return this._sceneType;
        //    }
        //}

        /// <summary>
        /// RuntimeGenerateObjGroup
        /// </summary>
        protected Transform RuntimeGenerateObjGroup
        {
            get
            {
                if (this._RuntimeGenerateObjGroup == null)
                    this._RuntimeGenerateObjGroup = GameObject.FindGameObjectWithTag("RuntimeInstantiateObjsGroup").transform;
                return this._RuntimeGenerateObjGroup;
            }
        }

        #endregion Properties

        #region MonoBehaviour

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            // TODO: Add GameManager
            //if (!GameManager.Instance.Init(this._sceneType) && !GameManager.Instance.IsScene(this._sceneType))
                //throw new System.ApplicationException(string.Format("Expected not to called {0} when GameManager.currentSceneType != {1}", this.name, this._sceneType.ToString()));

            base.Awake();
        }

        /// <summary>
        /// Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            for (int i = 0; i < this.InitActiveObjs.Count; ++i)
            {
                this.InitActiveObjs[i].SetActive(true);
            }
            this.RuntimeObjs = new Dictionary<int, GameObject>();
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        protected override void OnDestroy()
        {
            foreach (KeyValuePair<int, GameObject> kvp in this.RuntimeObjs)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(false);
                else
                    this.RuntimeObjs.Remove(kvp.Key);
            }
            this.RuntimeObjs.Clear();

            for (int i = 0; i < this.InitInactiveObjs.Count; ++i)
            {
                if (this.InitInactiveObjs[i] != null)
                    this.InitInactiveObjs[i].SetActive(false);
            }
            this.InitInactiveObjs.Clear();

            for (int i = 0; i < this.InitActiveObjs.Count; ++i)
            {
                if (this.InitActiveObjs[i] != null)
                    this.InitActiveObjs[i].SetActive(false);
            }
            this.InitActiveObjs.Clear();

            base.OnDestroy();
        }

        #endregion MonoBehaviour
    }

}
