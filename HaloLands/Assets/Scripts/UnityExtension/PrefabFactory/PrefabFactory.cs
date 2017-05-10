using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.ObjectPool;
using System.Reflection;

namespace Game
{
    /// <summary>
    /// Note:   Make sure to set PrefabFactory prefab into the first scene,
    ///         or load ./Assets/Scene/Config.scene together with the first scene
    /// </summary>
    
    // TODO: So far cannot generated different prefabs with same component.

    public class PrefabFactory : Singleton<PrefabFactory> {

        #region Prefab Fields 

        // Example
        public PooledObject PooledObjectPrefab;
        public GameObject ExampleGameObjectPrefab;

        #endregion Prefab Fields

        #region Fields

        /// <summary>
        /// Dictionary of all prefabs
        /// </summary>
        private Dictionary<System.Type, UnityEngine.Object> PrefabDictionary;

        #endregion Fields

        #region MonoBehaviour

        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            // Update everytime increasing PrefabDictionary
            int expectedSize = 1;

            // Build Dictionary
            this.PrefabDictionary = new Dictionary<System.Type, UnityEngine.Object>(expectedSize);

            // Note: Add all prefabs in PrefabDictionary below
            // Example
            this.PrefabDictionary.Add(typeof(PooledObject), this.PooledObjectPrefab);            
        }

        #endregion MonoBehaviour

        #region Instantiate

        public T Instantiate<T>() where T : UnityEngine.MonoBehaviour
        {
            System.Type type = typeof(T);
            UnityEngine.Object value;
            if (!this.PrefabDictionary.TryGetValue(type, out value))
                throw new System.ApplicationException(string.Format("Expected Field {0} has been set on PrefabFactory.", type.ToString()));

            if (value == null)
                throw new System.ApplicationException(string.Format("Expected Prefab {0} has been set on PrefabFactory.", type.ToString()));

            UnityEngine.Object obj = Instantiate(value);

            return (T)obj;
        }

        public GameObject InstantiatePrefab(GameObject gameObj)
        {
            GameObject obj = Instantiate(gameObj) as GameObject;
            return obj;
        }

        #endregion Instantiate
    }
}

