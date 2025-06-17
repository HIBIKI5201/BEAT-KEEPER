using System;
using System.Threading;
using UnityEditor.XR;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class SkillSystem
    {
        public SkillSystem(PlayerData data)
        {
            _data = data;
        }

        public bool IsActive => _isActive;

        public event Action OnStartSkill;
        public event Action OnEndSkill;

        private readonly PlayerData _data;

        private bool _isActive;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        ///     スキルを開始する
        /// </summary>
        public async void StartSkill()
        {
            _cancellationTokenSource?.Cancel(); //すでに実行中なら停止
            _cancellationTokenSource = new CancellationTokenSource();

            _isActive = true;
            OnStartSkill?.Invoke();

            //新しいタスクに停止されたら終わる
            try
            {
                await Awaitable.WaitForSecondsAsync(
                    _data.SkillDuration,
                    _cancellationTokenSource.Token);
            }
            finally
            {
                _isActive = false; //キャンセルされても非アクティブにする
            }

            //キャンセルされなかった場合の処理
            OnEndSkill?.Invoke();
        }
    }
}
