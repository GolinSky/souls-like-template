using System;
using UnityEngine;

namespace MultiPlayerTemplate
{
    [Serializable]
    public class KeyValue<TKey, TValue>:IKeyValue<TKey, TValue>
    {
        [SerializeField] private TKey key;
        [SerializeField] private TValue value;

        public TKey Key => key;

        public TValue Value => value;


        public void Assign(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
}