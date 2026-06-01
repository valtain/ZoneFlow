namespace ZoneFlow
{
    /// <summary>게임플레이 NavigationUri를 생성하는 편의 팩토리 클래스.</summary>
    public static class NavigationUriBuilder
    {
        /// <summary>탐색 모드 URI를 생성한다.</summary>
        public static string Exploration(string zoneId, string spawnPointId = null, ModeSwitchType switchType = ModeSwitchType.Replace)
            => Build("exploration", zoneId, spawnPointId, panelId: null, switchType);

        /// <summary>전투 모드 URI를 생성한다.</summary>
        public static string Battle(string zoneId, string spawnPointId = null, ModeSwitchType switchType = ModeSwitchType.Replace)
            => Build("battle", zoneId, spawnPointId, panelId: null, switchType);

        /// <summary>스토리 모드 URI를 생성한다.</summary>
        public static string Story(string zoneId, string spawnPointId = null, ModeSwitchType switchType = ModeSwitchType.Replace)
            => Build("story", zoneId, spawnPointId, panelId: null, switchType);

        /// <summary>셸 모드 URI를 생성한다.</summary>
        public static string Shell(string panelId, string zoneId = null, ModeSwitchType switchType = ModeSwitchType.Replace)
            => Build("shell", zoneId, spawnPointId: null, panelId: panelId, switchType);

        /// <summary>이전 모드로 돌아가는 Pop URI를 반환한다.</summary>
        public static string Pop() => "gameplay://pop";

        /// <summary>포털 ID를 이용하는 포털 URI를 생성한다.</summary>
        public static string Portal(string portalId) => $"gameplay://portal?id={portalId}";

        private static string Build(string host, string zoneId, string spawnPointId, string panelId, ModeSwitchType switchType)
        {
            var path = string.IsNullOrEmpty(zoneId) ? string.Empty : $"/{zoneId}";
            var queryBuilder = new System.Text.StringBuilder();

            if (switchType == ModeSwitchType.Stack)
                Append(queryBuilder, "switch", "stack");
            else if (switchType == ModeSwitchType.Pop)
                Append(queryBuilder, "switch", "pop");

            if (!string.IsNullOrEmpty(spawnPointId))
                Append(queryBuilder, "spawn", spawnPointId);

            if (!string.IsNullOrEmpty(panelId))
                Append(queryBuilder, "panel", panelId);

            var query = queryBuilder.Length > 0 ? $"?{queryBuilder}" : string.Empty;
            return $"gameplay://{host}{path}{query}";
        }

        private static void Append(System.Text.StringBuilder sb, string key, string value)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(key);
            sb.Append('=');
            sb.Append(value);
        }
    }
}
