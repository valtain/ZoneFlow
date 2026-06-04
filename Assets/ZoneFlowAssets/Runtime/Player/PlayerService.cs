using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>Player 인스턴스의 생성·이동·제거를 담당하는 서비스.</summary>
    [DefaultExecutionOrder(-900)]
    public sealed class PlayerService : MonoService<PlayerService>
    {
        [SerializeField] private PlayerController _playerPrefab;

        /// <summary>현재 씬에 존재하는 Player 인스턴스. 없으면 null.</summary>
        public PlayerController Player { get; private set; }

        /// <summary>Player가 없으면 SpawnPoint에 생성하고, 있으면 SpawnPoint로 텔레포트한다.</summary>
        public void SpawnAt(SpawnPoint sp)
        {
            Debug.Assert(sp != null, "[PlayerService] SpawnAt: SpawnPoint가 null입니다.");
            Debug.Assert(_playerPrefab != null, "[PlayerService] _playerPrefab이 할당되지 않았습니다.");

            if (Player == null)
                Player = Instantiate(_playerPrefab, sp.SpawnTransform.position, sp.SpawnTransform.rotation);
            else
                Player.transform.SetPositionAndRotation(sp.SpawnTransform.position, sp.SpawnTransform.rotation);
        }

        /// <summary>현재 Player 인스턴스를 제거한다.</summary>
        public void Despawn()
        {
            if (Player == null) return;
            Destroy(Player.gameObject);
            Player = null;
        }
    }
}
