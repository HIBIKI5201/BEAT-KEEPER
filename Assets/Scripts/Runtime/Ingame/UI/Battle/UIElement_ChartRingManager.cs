using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using Cysharp.Threading.Tasks;
using R3;
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
    public class UIElement_ChartRingManager : MonoBehaviour
    {
        [SerializeField, Tooltip("�����O�̃f�[�^�Q")] private RingData[] _ringDatas;

        private StageEnemyAdmin _enemies;
        private MusicEngineHelper _musicEngineHelper;

        private EnemyData _targetData;
        private Dictionary<ChartKindEnum, ObjectPool<UIElement_RingIndicator>> _ringPools = new();

        private Action _onBeat;
        private void Start()
        {
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            phaseManager.CurrentPhaseProp.Subscribe(OnChangePhase).AddTo(destroyCancellationToken);


            #region �e�����O�̃I�u�W�F�N�g�v�[����������
            for (int i = 0; i < _ringDatas.Length; i++)
            {
                var index = i;
                var data = _ringDatas[i];

                if (!data.RingPrefab)
                    return;

                data.ApearTiming = data.RingPrefab.GetComponent<UIElement_RingIndicator>()?.EffectLength ?? 5;

                _ringPools.Add(
                    data.AttackKind,
                    new(
                        createFunc: () =>
                        {
                            var go = Instantiate(data.RingPrefab);
                            go.transform.SetParent(transform);

                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                manager.gameObject.SetActive(false);
                                return manager;
                            }
                            else
                            {
                                SymphonyDebugLog.DirectLog("���̃v���n�u�̓����O�C���W�P�[�^�[���A�^�b�`����Ă��܂���", SymphonyDebugLog.LogKind.Warning);
                                return null;
                            }
                        },
                        actionOnGet: go =>
                        {
                            go.gameObject.SetActive(true);
                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                _onBeat += manager.AddCount;
                            }
                        },
                        actionOnRelease: go =>
                        {
                            if (go.TryGetComponent<UIElement_RingIndicator>(out var manager))
                            {
                                _onBeat -= manager.AddCount;
                            }
                        },
                        actionOnDestroy: go => Destroy(go),
                        defaultCapacity: data.DefaultCapacity,
                        maxSize: 10));
            }
            #endregion
        }

        private void OnChangePhase(PhaseEnum phase)
        {
            if (phase == PhaseEnum.Battle)
            {
                _enemies = ServiceLocator.GetInstance<BattleSceneManager>()?.EnemyAdmin;
                _targetData = _enemies.GetActiveEnemy().Data;
                _musicEngineHelper.OnJustChangedBeat += OnBeat;
            }
        }

        private void OnBeat()
        {
            if (_enemies == null) return;

            var timing = MusicEngineHelper.GetBeatsSinceStart();

            _onBeat?.Invoke(); //�����O�̃J�E���g���X�V 

            //�V���������O���Ď�
            foreach (var data in _ringDatas)
            {
                var element = _targetData.ChartData.Chart[(timing + data.ApearTiming) % 32];

                if (element.AttackKind != data.AttackKind)
                    continue;

                //�����O�𐶐�
                if (_ringPools.TryGetValue(data.AttackKind, out var op))
                {
                    var ring = op.Get(); //�����O���擾
                    ring.OnGet(() => op.Release(ring), //�I�����̃C�x���g��ݒ�
                        element.Position);
                }
            }
        }

        private void OnGUI()
        {
            if (_musicEngineHelper)
            {
                var currentBeat = MusicEngineHelper.GetBeatsSinceStart();

                GUI.Label(new Rect(10, 10, 200, 20), $"Current Beat: {currentBeat}");
            }
        }
    }

    [Serializable]
    public class RingData
    {
        [HideInInspector] public int ApearTiming;

        [SerializeField] private ChartKindEnum _attackKind;
        public ChartKindEnum AttackKind => _attackKind;

        [SerializeField] private GameObject _ringPrefab;
        public GameObject RingPrefab => _ringPrefab;

        [SerializeField, Tooltip("�����O�̎��O�p�Ӑ��i������x�̓����o��������́j")] private int _defaultCapacity = 3;
        public int DefaultCapacity => _defaultCapacity;
    }
}
