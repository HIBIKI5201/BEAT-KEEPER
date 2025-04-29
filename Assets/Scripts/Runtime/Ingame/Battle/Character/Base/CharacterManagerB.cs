using UnityEngine;
using UnityEngine.Serialization;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     キャラクター管理クラスのベース
    /// </summary>
    /// <typeparam name="TDataType">キャラクターのデータ</typeparam>
    public abstract class CharacterManagerB<TDataType> : MonoBehaviour where TDataType : CharacterData
    {
        [FormerlySerializedAs("data")] [SerializeField]
        protected TDataType _data;
        protected CharacterHealthSystem _healthSystem;

        protected virtual void Awake()
        {
            _healthSystem = new CharacterHealthSystem(_data);
        }
    }
}
