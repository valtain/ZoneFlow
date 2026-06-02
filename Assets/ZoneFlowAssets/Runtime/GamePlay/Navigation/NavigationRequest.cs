using System;
using System.Collections.Generic;

namespace ZoneFlow
{
    /// <summary>NavigationUri를 파싱한 결과를 담는 불변 구조체.</summary>
    public readonly struct NavigationRequest
    {
        /// <summary>전환할 내비게이션 스킴 (URI host).</summary>
        public NavigationHost Host { get; }

        /// <summary>모드 전환 방식. Scheme이 Pop/Portal일 때는 무의미.</summary>
        public ModeSwitch Switch { get; }

        /// <summary>대상 존 ID. 없으면 null.</summary>
        public string ZoneId { get; }

        /// <summary>스폰 포인트 ID, 패널 ID, 포털 ID 등 단일 식별자. 없으면 null.</summary>
        public string Id { get; }

        private NavigationRequest(
            NavigationHost scheme,
            ModeSwitch @switch,
            string zoneId,
            string id)
        {
            Host = scheme;
            Switch = @switch;
            ZoneId = zoneId;
            Id = id;
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
                request = new NavigationRequest(NavigationHost.Pop, default, null, null);
                return true;
            }

            if (host == "portal")
            {
                var portalQuery = ParseQuery(parsed.Query);
                portalQuery.TryGetValue("id", out var portalId);
                request = new NavigationRequest(NavigationHost.Portal, default, null, portalId);
                return true;
            }

            if (!TryParseModeScheme(host, out var modeScheme))
                return false;

            var queryParams = ParseQuery(parsed.Query);

            var switchMode = ModeSwitch.Replace;
            if (queryParams.TryGetValue("switch", out var switchValue))
            {
                if (string.Equals(switchValue, "stack", StringComparison.OrdinalIgnoreCase))
                    switchMode = ModeSwitch.Stack;
                else if (string.Equals(switchValue, "replaceall", StringComparison.OrdinalIgnoreCase))
                    switchMode = ModeSwitch.ReplaceAll;
            }

            string zoneId = null;
            var pathSegments = parsed.AbsolutePath.Trim('/');
            if (!string.IsNullOrEmpty(pathSegments))
            {
                var segments = pathSegments.Split('/');
                if (segments.Length > 0 && !string.IsNullOrEmpty(segments[0]))
                    zoneId = segments[0];
            }

            queryParams.TryGetValue("id", out var id);

            request = new NavigationRequest(modeScheme, switchMode, zoneId, id);
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

        private static bool TryParseModeScheme(string host, out NavigationHost modeScheme)
        {
            switch (host)
            {
                case "exploration": modeScheme = NavigationHost.Exploration; return true;
                case "battle":      modeScheme = NavigationHost.Battle;      return true;
                case "story":       modeScheme = NavigationHost.Story;       return true;
                case "shell":       modeScheme = NavigationHost.Shell;       return true;
                default:            modeScheme = default;                      return false;
            }
        }
    }
}
