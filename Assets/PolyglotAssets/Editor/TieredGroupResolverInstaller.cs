using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization;
using UnityEditor.Localization.Addressables;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace Polyglot.Editor
{
    /// <summary>
    /// Localization의 asset resolver를 <see cref="TieredGroupResolver"/>로 교체하고, 이미 배치된 boot 티어
    /// 자산을 새 그룹으로 옮기는 설치 툴. 규칙만 바꾸면 <b>기존 엔트리는 움직이지 않으므로</b> 이동까지 함께 수행한다.
    /// </summary>
    public static class TieredGroupResolverInstaller
    {
        /// <summary>asset resolver를 <see cref="TieredGroupResolver"/>로 교체하고 기존 boot 자산을 재배치한다(멱등).</summary>
        [MenuItem("Tools/Polyglot/Install Tiered Addressables Group Resolver")]
        public static void Install()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[TieredGroupResolverInstaller] Addressables 설정을 찾을 수 없습니다.");
                return;
            }

            AddressableGroupRules rules = AddressableGroupRules.Instance;
            if (rules.AssetResolver is not TieredGroupResolver)
            {
                rules.AssetResolver = CloneAsTiered(rules.AssetResolver);
                Debug.Log($"[TieredGroupResolverInstaller] asset resolver를 {nameof(TieredGroupResolver)}로 교체");
            }

            (int visited, int moved) = MoveBootAssetsToTieredGroups(rules, settings);

            EditorUtility.SetDirty(rules);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"[TieredGroupResolverInstaller] 설치 완료 — boot 자산 {visited}개 중 {moved}개 재배치");
        }

        /// <summary>기존 resolver의 그룹 이름 규칙을 그대로 물려받은 <see cref="TieredGroupResolver"/>를 만든다.</summary>
        /// <param name="source">복제할 원본 resolver.</param>
        private static TieredGroupResolver CloneAsTiered(GroupResolver source)
        {
            return new TieredGroupResolver(source.LocaleGroupNamePattern, source.SharedGroupName)
            {
                SharedGroup = source.SharedGroup,
                MarkEntriesReadOnly = source.MarkEntriesReadOnly
            };
        }

        /// <summary>
        /// 모든 Asset Table의 엔트리를 훑어 boot 티어 자산을 resolver가 지정하는 그룹으로 다시 배치한다.
        /// 이미 기대 그룹에 있는 자산은 <c>AddToGroup</c>이 no-op이므로 건너뛰어, 반환값이 실제 이동 수가 되게 한다.
        /// </summary>
        /// <param name="rules">사용할 그룹 규칙.</param>
        /// <param name="settings">대상 Addressables 설정.</param>
        /// <returns>훑은 boot 자산 수와 그중 실제로 옮긴 수.</returns>
        private static (int visited, int moved) MoveBootAssetsToTieredGroups(AddressableGroupRules rules, AddressableAssetSettings settings)
        {
            var visited = 0;
            var moved = 0;
            foreach ((Object asset, LocaleIdentifier locale) in EnumerateBootAssets())
            {
                visited++;
                var locales = new[] { locale };

                string expected = rules.AssetResolver.GetExpectedGroupName(locales, asset, settings);
                string actual = FindCurrentGroupName(asset, settings);
                if (actual == expected)
                {
                    continue;
                }

                rules.AssetResolver.AddToGroup(asset, locales, settings, false);
                moved++;
                Debug.Log($"[TieredGroupResolverInstaller] {asset.name}: {actual ?? "(미등록)"} → {expected}");
            }

            return (visited, moved);
        }

        /// <summary>
        /// 모든 Asset Table을 훑어 boot 티어 자산과 그 자산이 속한 locale을 열거한다.
        /// 티어 판별은 <see cref="TieredGroupResolver.IsBootTierAsset"/>에 위임해 resolver와 기준을 공유한다.
        /// </summary>
        private static IEnumerable<(Object asset, LocaleIdentifier locale)> EnumerateBootAssets()
        {
            foreach (AssetTableCollection collection in LocalizationEditorSettings.GetAssetTableCollections())
            {
                foreach (AssetTable table in collection.AssetTables)
                {
                    if (table == null)
                    {
                        continue;
                    }

                    foreach (AssetTableEntry entry in table.Values)
                    {
                        Object asset = LoadEntryAsset(entry);
                        if (TieredGroupResolver.IsBootTierAsset(asset))
                        {
                            yield return (asset, table.LocaleIdentifier);
                        }
                    }
                }
            }
        }

        /// <summary>자산이 현재 속한 Addressables 그룹 이름. Addressables에 등록되지 않았으면 null.</summary>
        /// <param name="asset">조회할 자산.</param>
        /// <param name="settings">대상 Addressables 설정.</param>
        private static string FindCurrentGroupName(Object asset, AddressableAssetSettings settings)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long _))
            {
                return null;
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            AddressableAssetGroup group = entry?.parentGroup;
            return group == null ? null : group.Name;
        }

        /// <summary>Asset Table 엔트리가 가리키는 자산을 로드한다. GUID가 비었거나 자산이 없으면 null.</summary>
        /// <param name="entry">대상 엔트리.</param>
        private static Object LoadEntryAsset(AssetTableEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.Guid))
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(entry.Guid);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Object>(path);
        }

        [MenuItem("Tools/Polyglot/Reveal Active AddressableGroupRules")]
        public static void RevealActiveRules()
        {
            AddressableGroupRules rules = AddressableGroupRules.Instance;
            LogRulesLocation(rules);

            // Project 창에서 해당 에셋을 실제로 선택/하이라이트
            EditorGUIUtility.PingObject(rules);
            Selection.activeObject = rules;
        }

        /// <summary>
        /// 활성 rules와 asset resolver의 현재 상태를 콘솔에 보고한다. 자산을 옮기거나 그룹을 만들지 않는 읽기 전용
        /// 점검이며, boot 자산이 기대 그룹에 있는지까지 dry-run으로 대조한다.
        /// </summary>
        [MenuItem("Tools/Polyglot/Verify Asset Resolver")]
        public static void VerifyAssetResolver()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[TieredGroupResolverInstaller] Addressables 설정을 찾을 수 없습니다.");
                return;
            }

            AddressableGroupRules rules = AddressableGroupRules.Instance;
            LogRulesLocation(rules);

            GroupResolver resolver = rules.AssetResolver;
            if (resolver == null)
            {
                Debug.LogError("[TieredGroupResolverInstaller] AssetResolver가 비어 있습니다 — SerializeReference는 어셈블리·네임스페이스·클래스 이름으로 바인딩되므로 TieredGroupResolver의 이름이 바뀌었는지 확인하세요.");
                return;
            }

            Type resolverType = resolver.GetType();
            Debug.Log($"[TieredGroupResolverInstaller] AssetResolver: {resolverType.FullName} (asm {resolverType.Assembly.GetName().Name}) — isTiered={resolver is TieredGroupResolver}");

            VerifyBootAssetGroups(resolver, settings);
        }

        /// <summary>
        /// boot 자산이 실제로 resolver가 지정하는 그룹에 있는지 대조해 자산별로 로그를 남긴다.
        /// <see cref="GroupResolver.GetExpectedGroupName"/>은 그룹을 만들지 않으므로 이 대조는 부수효과가 없다.
        /// </summary>
        /// <param name="resolver">기대 그룹을 계산할 resolver.</param>
        /// <param name="settings">대상 Addressables 설정.</param>
        private static void VerifyBootAssetGroups(GroupResolver resolver, AddressableAssetSettings settings)
        {
            var visited = 0;
            var mismatched = 0;
            foreach ((Object asset, LocaleIdentifier locale) in EnumerateBootAssets())
            {
                visited++;
                string expected = resolver.GetExpectedGroupName(new[] { locale }, asset, settings);
                string actual = FindCurrentGroupName(asset, settings);
                bool ok = actual == expected;
                if (!ok)
                {
                    mismatched++;
                }

                Debug.Log($"[TieredGroupResolverInstaller] {asset.name} ({locale.Code}) expected={expected} actual={actual ?? "(미등록)"} {(ok ? "OK" : "MISMATCH")}");
            }

            string summary = $"[TieredGroupResolverInstaller] boot 자산 {visited}개 중 {mismatched}개 불일치";
            if (mismatched > 0)
            {
                Debug.LogWarning($"{summary} — Tools/Polyglot/Install Tiered Addressables Group Resolver를 실행하세요.");
            }
            else
            {
                Debug.Log(summary);
            }
        }

        /// <summary>
        /// 활성 rules의 에셋 경로를 로그로 남긴다. <see cref="AddressableGroupRules.Instance"/>는 config object가
        /// 없으면 null이 아니라 <b>비영속 임시 인스턴스</b>를 돌려주므로, 그 경우를 오류로 구분해 보고한다.
        /// </summary>
        /// <param name="rules">점검할 그룹 규칙.</param>
        private static void LogRulesLocation(AddressableGroupRules rules)
        {
            if (!EditorUtility.IsPersistent(rules))
            {
                Debug.LogError("[TieredGroupResolverInstaller] 활성 AddressableGroupRules가 디스크에 없는 임시 인스턴스입니다 — EditorBuildSettings에 config object가 등록되지 않아 변경이 저장되지 않습니다.");
                return;
            }

            Debug.Log($"[TieredGroupResolverInstaller] 활성 AddressableGroupRules 경로: {AssetDatabase.GetAssetPath(rules)}");
        }
    }
}
