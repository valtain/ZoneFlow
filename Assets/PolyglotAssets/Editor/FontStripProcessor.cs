using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyglot.Editor
{
    /// <summary>
    /// TMP 컴포넌트가 폰트를 직렬화하지 않도록 저장/임포트 시점에 강제로 스트립한다.
    /// 컴포넌트는 TMP_Settings.defaultFontAsset을 상속하고 Style만 지정해야 한다.
    /// 프리팹 임포트, Prefab Mode 저장, 씬 저장 세 경로 모두 이 헬퍼로 수렴한다.
    /// </summary>
    [InitializeOnLoad]
    public static class FontStripProcessor
    {
        static readonly FieldInfo s_fontAssetField =
            typeof(TMP_Text).GetField("m_fontAsset", BindingFlags.NonPublic | BindingFlags.Instance);

        static FontStripProcessor()
        {
            PrefabStage.prefabSaving += OnPrefabSaving;
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        /// <summary>
        /// 대상 오브젝트 하위 모든 TMP_Text 컴포넌트의 폰트 참조(m_fontAsset)를 제거한다.
        /// Style(m_TextStyleHashCode/m_StyleSheet)과 폰트 애셋 자체는 건드리지 않는다.
        /// TMP_Text.font 세터(및 SerializedObject.ApplyModifiedProperties 경유 기록)는
        /// OnValidate를 동기 호출해 m_fontAsset을 TMP_Settings.defaultFontAsset으로
        /// 즉시 재할당하므로, 세터 대신 리플렉션으로 m_fontAsset 필드만 직접 null로 기록해
        /// 이 재할당 부작용을 우회한다.
        /// 반환값은 스트립한 컴포넌트 수.
        /// </summary>
        public static int StripFonts(GameObject root)
        {
            int n = 0;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.font == null)
                {
                    continue;
                }
                s_fontAssetField.SetValue(t, null);
                EditorUtility.SetDirty(t);
                n++;
            }
            return n;
        }

        static void OnPrefabSaving(GameObject prefabContentsRoot)
        {
            int n = StripFonts(prefabContentsRoot);
            if (n > 0)
            {
                EditorUtility.SetDirty(prefabContentsRoot);
                Debug.Log($"[Polyglot] Prefab 저장 시 폰트 스트립: {prefabContentsRoot.name} ({n}개 컴포넌트)");
            }
        }

        static void OnSceneSaving(Scene scene, string path)
        {
            int total = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                int n = StripFonts(root);
                if (n > 0)
                {
                    EditorUtility.SetDirty(root);
                }
                total += n;
            }
            if (total > 0)
            {
                Debug.Log($"[Polyglot] 씬 저장 시 폰트 스트립: {scene.name} ({total}개 컴포넌트)");
            }
        }
    }

    /// <summary>
    /// 프리팹 임포트 시점에 폰트 참조를 스트립한다.
    /// </summary>
    public class FontStripImportProcessor : AssetPostprocessor
    {
        void OnPostprocessPrefab(GameObject root)
        {
            int n = FontStripProcessor.StripFonts(root);
            if (n > 0)
            {
                Debug.Log($"[Polyglot] 프리팹 임포트 시 폰트 스트립: {assetPath} ({n}개 컴포넌트)");
            }
        }
    }
}
