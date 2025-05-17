using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクター管理クラスのベース
    /// </summary>
    /// <typeparam name="TDataType">キャラクターのデータ</typeparam>
    public abstract class CharacterManagerB<TDataType> : MonoBehaviour, IHitable
        where TDataType : CharacterData
    {
        [SerializeField] protected TDataType _data;
        public TDataType Data => _data;

        public Action OnHitAttack { get; set; }

        protected virtual void Awake()
        {
            //データがなかったら初期データをアサイン
            if (!_data) _data = ScriptableObject.CreateInstance<TDataType>();
            
            Debug.Log($"{_data.Name} initialized");
        }

        public virtual void HitAttack(float damage)
        {
             
        }
    }
}
