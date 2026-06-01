using System;
using System.Collections.Generic;

namespace ZoneFlow
{
    /// <summary>NavigationUri를 파싱한 결과를 담는 불변 구조체.</summary>
    public readonly struct NavigationRequest
    {
        /// <summary>모드 전환 방식.</summary>
        public ModeSwitchType SwitchType { get; }

        /// <summary>전환할 모드 타입.</summary>
        public ModeType ModeType { get; }

        /// <summary>대상 존 ID. 없으면 null.</summary>
        public string ZoneId { get; }

        /// <summary>스폰 포인트 ID. 없으면 null.</summary>
        public string SpawnPointId { get; }

        /// <summary>패널 ID (Shell 모드 전용). 없으면 null.</summary>
        public string PanelId { get; }

        /// <summary>포털 ID. 있으면 포털 리다이렉트 처리가 수행된다.</summary>
        public string PortalId { get; }

        private NavigationRequest(
            ModeSwitchType switchType,
            ModeType modeType,
            string zoneId,
            string spawnPointId,
            string panelId,
            string portalId)
        {
            SwitchType = switchType;
            ModeType = modeType;
            ZoneId = zoneId;
            SpawnPointId = spawnPointId;
            PanelId = panelId;
            PortalId = portalId;
        }

        /// <summary>URI 문자열을 파싱하여 NavigationRequest를 반환한다.</summary>
        public static NavigationRequest Parse(string uri)
        {
            TryParse(uri, out var request);
            return request;
        }

        /// <summary>URI 문자열을 파싱하여 NavigationRequest를 반환한다. 파싱 실패 시 false를 반환한다.</summary>
        public static bool TryParse(string uri, out NavigationRequest request)
        {
            request = default;

            if (string.IsNullOrEmpty(uri))
                return false;

            Uri parsed;
            try
            {
                parsed = new Uri(uri);
            }
            catch
            {
                return false;
            }

            if (!string.Equals(parsed.Scheme, "gameplay", StringComparison.OrdinalIgnoreCase))
                return false;

            var host = parsed.Host.ToLowerInvariant();

            if (host == "pop")
            {
                request = new NavigationRequest(ModeSwitchType.Pop, default, null, null, null, null);
                return true;
            }

            if (host == "portal")
            {
                var portalQuery = ParseQuery(parsed.Query);
                portalQuery.TryGetValue("id", out var portalId);
                request = new NavigationRequest(ModeSwitchType.Replace, default, null, null, null, portalId);
                return true;
            }

            if (!TryParseModeType(host, out var modeType))
                return false;

            var queryParams = ParseQuery(parsed.Query);

            var switchType = ModeSwitchType.Replace;
            if (queryParams.TryGetValue("switch", out var switchValue) &&
                string.Equals(switchValue, "stack", StringComparison.OrdinalIgnoreCase))
                switchType = ModeSwitchType.Stack;

            string zoneId = null;
            var pathSegments = parsed.AbsolutePath.Trim('/');
            if (!string.IsNullOrEmpty(pathSegments))
            {
                var segments = pathSegments.Split('/');
                if (segments.Length > 0 && !string.IsNullOrEmpty(segments[0]))
                    zoneId = segments[0];
            }

            queryParams.TryGetValue("spawn", out var spawnPointId);
            queryParams.TryGetValue("panel", out var panelId);

            request = new NavigationRequest(switchType, modeType, zoneId, spawnPointId, panelId, null);
            return true;
        }

        private static Dictionary<string, string> ParseQuery(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(query))
                return result;

            var q = query.TrimStart('?');
            foreach (var pair in q.Split('&'))
            {
                var idx = pair.IndexOf('=');
                if (idx < 0)
                {
                    result[Uri.UnescapeDataString(pair)] = string.Empty;
                }
                else
                {
                    var key = Uri.UnescapeDataString(pair.Substring(0, idx));
                    var val = Uri.UnescapeDataString(pair.Substring(idx + 1));
                    result[key] = val;
                }
            }
            return result;
        }

        private static bool TryParseModeType(string host, out ModeType modeType)
        {
            switch (host)
            {
                case "exploration": modeType = ModeType.Exploration; return true;
                case "battle":      modeType = ModeType.Battle;      return true;
                case "story":       modeType = ModeType.Story;       return true;
                case "shell":       modeType = ModeType.Shell;       return true;
                default:            modeType = default;              return false;
            }
        }
    }
}
