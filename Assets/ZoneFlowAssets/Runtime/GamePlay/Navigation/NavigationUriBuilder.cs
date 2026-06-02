namespace ZoneFlow
{
    /// <summary>게임플레이 NavigationUri를 생성하는 편의 팩토리 클래스.</summary>
    public static class NavigationUriBuilder
    {
        /// <summary>탐색 모드 URI를 생성한다.</summary>
        public static string Exploration(string zoneId, string id = null, ModeSwitch switchMode = ModeSwitch.Replace)
            => Build("exploration", zoneId, id, switchMode);

        /// <summary>전투 모드 URI를 생성한다.</summary>
        public static string Battle(string zoneId, string id = null, ModeSwitch switchMode = ModeSwitch.Replace)
            => Build("battle", zoneId, id, switchMode);

        /// <summary>스토리 모드 URI를 생성한다.</summary>
        public static string Story(string zoneId, string id = null, ModeSwitch switchMode = ModeSwitch.Replace)
            => Build("story", zoneId, id, switchMode);

        /// <summary>셸 모드 URI를 생성한다.</summary>
        public static string Shell(string id, string zoneId = null, ModeSwitch switchMode = ModeSwitch.Replace)
            => Build("shell", zoneId, id, switchMode);

        /// <summary>이전 모드로 돌아가는 Pop URI를 반환한다.</summary>
        public static string Pop() => "gameplay://pop";

        /// <summary>포털 ID를 이용하는 포털 URI를 생성한다.</summary>
        public static string Portal(string portalId) => $"gameplay://portal?id={portalId}";

        private static string Build(string host, string zoneId, string id, ModeSwitch switchMode)
        {
            var path = string.IsNullOrEmpty(zoneId) ? string.Empty : $"/{zoneId}";
            var queryBuilder = new System.Text.StringBuilder();

            if (switchMode == ModeSwitch.Stack)
                Append(queryBuilder, "switch", "stack");
            else if (switchMode == ModeSwitch.ReplaceAll)
                Append(queryBuilder, "switch", "replaceall");

            if (!string.IsNullOrEmpty(id))
                Append(queryBuilder, "id", id);

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
