using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// Unityエディタでシリアライズ可能なDictionary
    /// </summary>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// キーの配列
        /// </summary>
        [SerializeField]
        private TKey[] keys = new TKey[0];

        /// <summary>
        /// バリューの配列
        /// </summary>
        [SerializeField]
        private TValue[] values = new TValue[0];

        /// <summary>
        /// デシリアライズ後の処理
        /// </summary>
        public void OnAfterDeserialize()
        {
            // 既存のdictionaryをクリア
            this.Clear();

            // 配列が存在し、長さが一致している場合のみ復元
            if (keys != null && values != null && keys.Length == values.Length)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    this[keys[i]] = values[i];   
                }
            }
        }

        /// <summary>
        /// シリアライズ前の処理
        /// DictionaryのデータをシリアライズできるようにKeys配列とValues配列に変換する
        /// </summary>
        public void OnBeforeSerialize()
        {
            // Dictionaryのサイズに合わせて配列を初期化
            keys = new TKey[this.Count];
            values = new TValue[this.Count];

            int i = 0;
            using (var e = this.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    keys[i] = e.Current.Key;
                    values[i] = e.Current.Value;
                    i++;
                }
            }
        }
    }
}
