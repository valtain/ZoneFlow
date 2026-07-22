using TMPro;
using UnityEditor;
using UnityEngine;

namespace Polyglot.Editor
{
    /// <summary>
    /// TMP_FontAsset의 동적 글리프 아틀라스 데이터가 저장 시 .asset에 직렬화되어
    /// 오염되는 것을 방지한다. Static 폰트(유일한 글리프 원본)는 절대 건드리지 않는다.
    /// </summary>
    public class FontAtlasGuard : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (string path in paths)
            {
                if (!path.EndsWith(".asset"))
                {
                    continue;
                }

                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font == null)
                {
                    continue;
                }

                if (font.atlasPopulationMode == AtlasPopulationMode.Static)
                {
                    continue;
                }

                var so = new SerializedObject(font);
                var clearOnBuildProp = so.FindProperty("m_ClearDynamicDataOnBuild");
                if (clearOnBuildProp != null && !clearOnBuildProp.boolValue)
                {
                    clearOnBuildProp.boolValue = true;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                font.ClearFontAssetData();
                Debug.Log($"[Polyglot] 폰트 아틀라스 오염 방지: {path}");
            }

            return paths;
        }
    }
}
