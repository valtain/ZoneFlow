using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyglot.Editor
{
    /// <summary>
    /// <see cref="TieredGroupResolver"/> 필드의 라벨에 실제 구체 타입명을 노출하는 드로어.
    /// </summary>
    /// <remarks>
    /// <c>AddressableGroupRules.m_AssetResolver</c>는 <c>[SerializeReference]</c> 다형성 필드라 기본 UI로는
    /// 어떤 resolver가 꽂혀 있는지 보이지 않는다 — 설치 툴이 교체에 성공했는지 눈으로 확인할 수 없다는 뜻이다.
    /// Localization 패키지는 <c>GroupResolver</c>에 이미 드로어를 등록해 두었고 그 클래스는 internal이라 상속·재사용이
    /// 불가능하므로, 같은 타입에 중복 등록해 패키지 UI를 비결정적으로 밀어내는 대신 파생 타입에만 등록하고
    /// base 필드는 기본 <see cref="PropertyField"/>로 직접 구성한다.
    /// </remarks>
    [CustomPropertyDrawer(typeof(TieredGroupResolver))]
    public sealed class TieredGroupResolverDrawer : PropertyDrawer
    {
        /// <summary>base <c>GroupResolver</c>가 직렬화하는 필드 이름들. 선언 순서대로 그린다.</summary>
        private static readonly string[] FieldNames =
        {
            "m_SharedGroupName",
            "m_SharedGroup",
            "m_LocaleGroupNamePattern",
            "m_LocaleGroups",
            "m_MarkEntriesReadOnly"
        };

        /// <summary>타입명을 덧붙인 라벨의 foldout 안에 resolver 필드를 배치한다.</summary>
        /// <param name="property">그릴 managed reference 프로퍼티.</param>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new Foldout
            {
                value = property.isExpanded,
                text = $"{property.displayName} ({GetReferenceTypeName(property)})"
            };
            root.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);

            foreach (string fieldName in FieldNames)
            {
                SerializedProperty field = property.FindPropertyRelative(fieldName);
                if (field != null)
                {
                    root.Add(new PropertyField(field));
                }
            }

            return root;
        }

        /// <summary>
        /// managed reference에 실제로 담긴 타입의 짧은 이름을 뽑는다.
        /// <c>managedReferenceFullTypename</c>은 <c>"어셈블리 네임스페이스.클래스"</c> 형식이다.
        /// </summary>
        /// <param name="property">대상 managed reference 프로퍼티.</param>
        private static string GetReferenceTypeName(SerializedProperty property)
        {
            string fullTypename = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypename))
            {
                return nameof(TieredGroupResolver);
            }

            int lastDot = fullTypename.LastIndexOf('.');
            return lastDot < 0 ? fullTypename : fullTypename.Substring(lastDot + 1);
        }
    }
}
