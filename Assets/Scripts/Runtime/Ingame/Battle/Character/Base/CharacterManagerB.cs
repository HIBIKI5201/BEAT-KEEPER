using BeatKeeper.Runtime.Ingame.Battle;
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
        public Action<int> OnHitAttack { get; set; }

        public TDataType Data => _data;

        public virtual void HitAttack(AttackData data) { }

        [SerializeField] protected TDataType _data;

        protected virtual void Awake()
        {
            //データがなかったら初期データをアサイン
            if (!_data) _data = ScriptableObject.CreateInstance<TDataType>();
            
            Debug.Log($"{_data.Name} initialized");
        }

    }
}
