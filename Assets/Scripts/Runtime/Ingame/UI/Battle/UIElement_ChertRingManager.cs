using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.Debugger;
using SymphonyFrameWork.System;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     ���ʂ̃����O�\��������}�l�[�W���[
    /// </summary>
    public class UIElement_ChertRingManager : MonoBehaviour
    {
        [SerializeField, Tooltip("�����O�̃f�[�^�Q")] private RingData[] _ringDatas;

        private StageEnemyAdmin _enemies;
        private MusicEngineHelper _musicEngineHelper;

        private EnemyData _targetData;
        private Dictionary<AttackKindEnum, ObjectPool<UIElement_RingIndicator>> _ringPools = new();

        private async void Start()
        {
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();

            SceneLoader.RegisterAfterSceneLoad(SceneListEnum.Battle.ToString(),
                () =>
                {
                    _enemies = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
                    _targetData = _enemies.GetActiveEnemy().Data;
                    _musicEngineHelper.OnJustChangedBeat += OnBeat;
                });

            #region �e�����O�̃I�u�W�F�N�g�v�[����������
            for (int i = 0; i < _ringDatas.Length; i++)
            {
                var index = i;
                var data = _ringDatas[i];
                _ringPools.Add(
                    data.AttackKind,
                    new(
                        createFunc: () =>
                        {
                            var go = Instantiate(_ringDatas[index].RingPrefab);
                            go.transform.SetParent(transform);

                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                manager.Initialize((float)_musicEngineHelper.DurationOfBeat);
                                return manager;
                            }
                            else
                            {
                                SymphonyDebugLog.DirectLog("���̃v���n�u�̓����O�C���W�P�[�^�[���A�^�b�`����Ă��܂���", SymphonyDebugLog.LogKind.Warning);
                                return null;
                            }
                        },
                        actionOnDestroy: go => Destroy(go),
                        defaultCapacity: _ringDatas[i].DefaultCapacity,
                        maxSize: 10));
            }
            #endregion
        }

        private void OnBeat()
        {
            if (_enemies == null) return;

            var timing = _musicEngineHelper.GetBeatsSinceStart();


            foreach (var data in _ringDatas)
            {
                var kind = _targetData.Chart[(timing + data.ApearTiming) % 32];

                if (_ringPools.TryGetValue(kind, out var op)) //�Ή�����v�[�����Ăяo����
                {
                    var ring = op.Get(); //�����O���擾
                    //TODO �ʒu�𕈖ʂ���擾����
                    ring.OnGet(() => _ringPools[kind].Release(ring),
                        new Vector2(UnityEngine.Random.Range(Screen.width / 4, Screen.width * 3 / 4),
                        UnityEngine.Random.Range(Screen.height / 4, Screen.height * 3 / 4)));
                    ring.EffectStart();
                }
            }
        }

        private void OnGUI()
        {
            if (_musicEngineHelper)
            {
                var currentBeat = _musicEngineHelper.GetBeatsSinceStart();

                GUI.Label(new Rect(10, 10, 200, 20), $"Current Beat: {currentBeat}");
            }
        }
    }

    [Serializable]
    public class RingData
    {
        [SerializeField] private AttackKindEnum _attackKind;
        public AttackKindEnum AttackKind => _attackKind;

        [SerializeField, Tooltip("�o���^�C�~���O�̔���")] private int _apearTiming = 5;
        public int ApearTiming => _apearTiming;

        [SerializeField] private GameObject _ringPrefab;
        public GameObject RingPrefab => _ringPrefab;

        [SerializeField, Tooltip("�����O�̎��O�p�Ӑ��i������x�̓����o��������́j")] private int _defaultCapacity;
        public int DefaultCapacity => _defaultCapacity = 3;
    }

}
