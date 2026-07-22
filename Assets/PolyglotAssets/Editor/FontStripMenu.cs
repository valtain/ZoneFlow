using UnityEditor;
using UnityEngine;

namespace Polyglot.Editor
{
    /// <summary>
    /// 프로젝트 전체 프리팹을 순회해 TMP 컴포넌트의 폰트 참조를 1회 배치로 스트립한다.
    /// 기존에 저장된(on-disk) 프리팹을 대상으로 하며, 이후 저장/임포트는
    /// FontStripProcessor의 지속 훅이 담당한다.
    /// </summary>
    public static class FontStripMenu
    {
        /// <summary>
        /// 프로젝트 내 모든 프리팹을 스캔해 폰트 참조를 스트립하고 결과를 콘솔에 요약한다.
        /// </summary>
        [MenuItem("Tools/Polyglot/Strip Component Fonts")]
        public static void StripAllPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            int prefabCount = 0;
            int strippedPrefabCount = 0;
            int strippedComponentCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // 패키지(Packages/)는 immutable 폴더라 저장할 수 없으므로 프로젝트 프리팹만 대상으로 한다.
                if (!path.StartsWith("Assets/"))
                {
                    continue;
                }
                prefabCount++;

                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int n = FontStripProcessor.StripFonts(contents);
                    if (n > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(contents, path);
                        strippedPrefabCount++;
                        strippedComponentCount += n;
                        Debug.Log($"[Polyglot] 폰트 스트립: {path} ({n}개 컴포넌트)");
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            Debug.Log($"[Polyglot] 배치 완료: 총 프리팹 {prefabCount}개 중 {strippedPrefabCount}개 스트립, {strippedComponentCount}개 컴포넌트");
        }
    }
}
