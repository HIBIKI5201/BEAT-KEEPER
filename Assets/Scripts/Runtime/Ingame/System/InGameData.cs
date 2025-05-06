using System.Collections.Generic;
using UnityEngine;

namespace BeatKeeper
{
    [CreateAssetMenu(fileName = "InGameData", menuName = "BeatKeeper/System/InGameSData")]
    public class InGameData : ScriptableObject
    {
        [SerializeField] private GameObject _playerPrefab;
        public GameObject PlayerPrefab => _playerPrefab;
        
        [SerializeField] private List<GameObject> _enemyPrefabs;
        public List<GameObject> EnemyPrefabs => _enemyPrefabs;
    }
}
