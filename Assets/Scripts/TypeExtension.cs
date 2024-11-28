using System;
using System.Collections.Generic;
using UnityEngine;

namespace TypeExtension
{
    public static class MyExtensions
    {
        public static string DisplayWithSuffix(this int num)
        {
            string number = num.ToString();
            if (number.EndsWith("11"))
                return number + "th";
            if (number.EndsWith("12"))
                return number + "th";
            if (number.EndsWith("13"))
                return number + "th";
            if (number.EndsWith("1"))
                return number + "st";
            if (number.EndsWith("2"))
                return number + "nd";
            if (number.EndsWith("3"))
                return number + "rd";
            return number + "th";
        }
    }

    [Serializable]
    public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<K> m_Keys = new List<K>();

        [SerializeField]
        private List<V> m_Values = new List<V>();

        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            m_Values.Clear();
            using Enumerator enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<K, V> current = enumerator.Current;
                m_Keys.Add(current.Key);
                m_Values.Add(current.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < m_Keys.Count; i++)
            {
                Add(m_Keys[i], m_Values[i]);
            }

            m_Keys.Clear();
            m_Values.Clear();
        }
    }
}
