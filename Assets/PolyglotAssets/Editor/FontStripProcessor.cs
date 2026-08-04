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
        // PolyglotText(ExecuteAlways)가 driven으로 등록하는 필드와 동일한 집합.
        // (FieldInfo, 직렬화 속성 경로) 쌍으로 묶어 리플렉션 기록과 driven 등록/해제에 함께 쓴다.
        static readonly (FieldInfo Field, string PropertyPath)[] s_targetFields =
        {
            (typeof(TMP_Text).GetField("m_fontAsset", BindingFlags.NonPublic | BindingFlags.Instance), "m_fontAsset"),
            (typeof(TMP_Text).GetField("m_sharedMaterial", BindingFlags.NonPublic | BindingFlags.Instance), "m_sharedMaterial"),
            (typeof(TMP_Text).GetField("m_StyleSheet", BindingFlags.NonPublic | BindingFlags.Instance), "m_StyleSheet"),
        };

        static FontStripProcessor()
        {
            PrefabStage.prefabSaving += OnPrefabSaving;
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        /// <summary>
        /// 대상 오브젝트 하위 모든 TMP_Text 컴포넌트의 폰트 참조(m_fontAsset/m_sharedMaterial/m_StyleSheet)를
        /// 제거한다. Style(m_TextStyleHashCode)과 폰트 애셋 자체는 건드리지 않는다.
        ///
        /// PolyglotText(ExecuteAlways)는 OnEnable에서 값을 바꾸기 "전"에 이 필드들을 Localization의
        /// driven으로 등록한다. driven 등록은 등록 시점 값의 스냅샷을 저장해두고, 등록이 해제되기 전까지는
        /// 이후 어떤 변경도 무시한 채 "그 스냅샷"을 직렬화한다 — 즉 리플렉션으로 null을 기록해도, 등록이
        /// 살아있는 한 저장 시 디스크에는 스냅샷(원래 폰트)이 되살아난다(#116).
        ///
        /// TMP_Text.font 세터(및 SerializedObject.ApplyModifiedProperties 경유 기록)는 값 대입 시
        /// LoadFontAsset() 등 부수효과를 동기 호출해 필드를 재할당할 수 있으므로, 세터 대신 리플렉션으로
        /// 필드를 직접 조작해 그 부작용을 우회한다.
        ///
        /// 필드마다 다음 순서로 처리하며, 순서가 결과를 좌우한다:
        ///  1) UnmarkDriven — 먼저 호출해 등록 스냅샷(디스크 원본 값)으로 필드를 되돌린다.
        ///     이 호출보다 먼저 null을 기록하면 이 단계가 그 null을 스냅샷 값으로 덮어써 버린다.
        ///  2) 값이 있으면 리플렉션으로 필드를 null로 기록 — 이제 필드가 진짜 null이 된다.
        ///  3) MarkDriven — null인 상태에서 다시 등록해 스냅샷 자체를 null로 고정한다. 이후 어떤 경로가
        ///     필드를 재대입해도 in-memory에만 남고, 저장 시에는 이 null 스냅샷이 기록된다.
        /// 3단계는 이미 클린한 필드를 포함해 모든 TMP_Text 컴포넌트·모든 대상 필드에 항상 실행한다 —
        /// 그래야 저장 전에 재오염될 여지를 차단한다. 반환값은 실제로 값이 바뀐 컴포넌트 수.
        /// </summary>
        public static int StripFonts(GameObject root)
        {
            int n = 0;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                bool changed = false;
                foreach (var (field, propertyPath) in s_targetFields)
                {
                    TmpFontFacade.UnmarkDriven(t, propertyPath);

                    if (field.GetValue(t) != null)
                    {
                        field.SetValue(t, null);
                        changed = true;
                    }

                    TmpFontFacade.MarkDriven(t, propertyPath);
                }

                if (changed)
                {
                    EditorUtility.SetDirty(t);
                    n++;
                }
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
