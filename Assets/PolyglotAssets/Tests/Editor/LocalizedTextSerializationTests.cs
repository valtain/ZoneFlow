using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

namespace Polyglot.Editor.Tests
{
    /// <summary>
    /// "CallState가 EditorAndRuntime인 <see cref="LocalizeStringEvent"/> 리스너가 겨냥한
    /// <see cref="TMP_Text"/>의 <c>m_text</c>는 디스크에서 비어 있어야 한다"는 불변식을 검증한다(#119).
    ///
    /// Localization은 driven 등록 여부를 <c>(target.GetType(), methodName)</c> 문자열 쌍의 정확 일치로만
    /// 판단하므로, <see cref="TextDrivenPropertyRegistrar"/>가 <c>(PolyglotText, "SetText")</c> 키를
    /// 등록해 두지 않으면 프리뷰 locale 문자열이 그대로 <c>m_text</c>에 직렬화된다. 이 테스트는 등록자
    /// 도입 이전에는 실패하고 이후에는 통과하는 회귀 가드다.
    /// </summary>
    internal class LocalizedTextSerializationTests
    {
        private Locale _originalLocale;

        [SetUp]
        public void SetUp()
        {
            // 프리뷰가 발화하지 않도록 locale을 비운다 — 메모리 값이 곧 디스크에 저장된 값과 같아진다.
            _originalLocale = LocalizationSettings.SelectedLocale;
            LocalizationSettings.SelectedLocale = null;
        }

        [TearDown]
        public void TearDown()
        {
            LocalizationSettings.SelectedLocale = _originalLocale;
        }

        /// <summary>
        /// 씬을 Additive로 열어 디스크 상태를 검사하고, 저장하지 않은 채 닫는다 — 사용자가 이미 열어 둔
        /// 씬(예: Intro)을 건드리지 않기 위함이다.
        /// </summary>
        [TestCase("Assets/ZoneFlowAssets/Scenes/LocalizationDemo.unity")]
        [TestCase("Assets/ZoneFlowAssets/Scenes/Intro.unity")]
        public void DrivenListenerTargets_HaveEmptyTextOnDisk(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (var stringEvent in root.GetComponentsInChildren<LocalizeStringEvent>(true))
                    {
                        AssertDrivenTargetsAreEmpty(stringEvent);
                    }
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        /// <summary>
        /// <see cref="LocalizeStringEvent.OnUpdateString"/>의 persistent 리스너 중 CallState가
        /// EditorAndRuntime이고 대상이 <see cref="TMP_Text.SetText(string)"/>/<c>set_text</c>인 것만
        /// 검사한다 — <c>Editor_RegisterKnownDrivenProperties</c>가 driven 등록에 쓰는 조건과 동일하다.
        /// </summary>
        private static void AssertDrivenTargetsAreEmpty(LocalizeStringEvent stringEvent)
        {
            UnityEngine.Events.UnityEventBase updateString = stringEvent.OnUpdateString;
            int count = updateString.GetPersistentEventCount();
            for (var i = 0; i < count; i++)
            {
                if (updateString.GetPersistentListenerState(i) != UnityEngine.Events.UnityEventCallState.EditorAndRuntime)
                {
                    continue;
                }

                var target = updateString.GetPersistentTarget(i) as TMP_Text;
                if (target == null)
                {
                    continue;
                }

                string methodName = updateString.GetPersistentMethodName(i);
                if (methodName != "SetText" && methodName != "set_text")
                {
                    continue;
                }

                Assert.IsTrue(string.IsNullOrEmpty(target.text),
                    $"'{target.gameObject.name}'의 m_text가 디스크에 비어 있지 않습니다: '{target.text}' " +
                    $"(리스너: {stringEvent.gameObject.name}, 메서드: {methodName})");
            }
        }
    }
}
