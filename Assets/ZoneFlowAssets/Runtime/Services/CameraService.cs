using Unity.Cinemachine;
using UnityEngine;

namespace ZoneFlow
{
    public sealed class CameraService : MonoService<CameraService>
    {
        [field: SerializeField] public CinemachineBrain CinemachineBrain { get; private set; }
        [field: SerializeField] public Camera MainCamera { get; private set; }
    }
}
