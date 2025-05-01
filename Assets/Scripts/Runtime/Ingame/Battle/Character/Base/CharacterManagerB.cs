using UnityEngine;
using UnityEngine.Serialization;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクター管理クラスのベース
    /// </summary>
    /// <typeparam name="TDataType">キャラクターのデータ</typeparam>
    public abstract class CharacterManagerB<TDataType> : MonoBehaviour, IAttackable
        where TDataType : CharacterData
    {
        [SerializeField] protected TDataType _data;

        protected virtual void Awake()
        {
            Debug.Log($"{_data.Name} initialized");
        }

        public virtual void HitAttack(float damage)
        {
             
        }
    }
}
