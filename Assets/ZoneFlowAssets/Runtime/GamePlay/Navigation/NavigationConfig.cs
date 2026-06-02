using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>Inspector에서 NavigationRequest를 편리하게 지정하기 위한 설정 클래스.</summary>
    [Serializable]
    public class NavigationConfig
    {
        [field: SerializeField] public NavigationHost Host   { get; private set; } = NavigationHost.Exploration;
        [field: SerializeField] public string         ZoneId { get; private set; }
        [field: SerializeField] public string         Id     { get; private set; }
        [field: SerializeField] public ModeSwitch     Switch { get; private set; } = ModeSwitch.Replace;

        public string BuildUri() => Host switch
        {
            NavigationHost.Exploration => NavigationUriBuilder.Exploration(ZoneId, Id, Switch),
            NavigationHost.Battle      => NavigationUriBuilder.Battle(ZoneId, Id, Switch),
            NavigationHost.Story       => NavigationUriBuilder.Story(ZoneId, Id, Switch),
            NavigationHost.Shell       => NavigationUriBuilder.Shell(Id, ZoneId, Switch),
            NavigationHost.Pop         => NavigationUriBuilder.Pop(),
            NavigationHost.Portal      => NavigationUriBuilder.Portal(Id),
            _                          => throw new ArgumentOutOfRangeException(nameof(Host), Host, null),
        };
    }
}
