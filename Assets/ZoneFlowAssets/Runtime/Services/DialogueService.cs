using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Yarn.Unity;

namespace ZoneFlow
{
    /// <summary>
    /// YarnSpinner 기반 내러티브 진행을 관장하는 서비스. ContentServices 씬에 배치한다.
    /// DialogueRunner와 변수 저장소를 보유하므로, 진행 상태(Yarn 변수)의 수명이 ContentServices 씬의
    /// 로드~언로드 경계에 종속된다 — Zone 전환(Zone 씬 unload/load)에 영향받지 않는다(AQ-2).
    /// </summary>
    public sealed class DialogueService : MonoService<DialogueService>
    {
        [SerializeField] private DialogueRunner _runner;

        /// <summary>대화가 현재 실행 중이면 true.</summary>
        public bool IsDialogueRunning => _runner != null && _runner.IsDialogueRunning;

        /// <summary>이 콘텐츠 세션에서 대화가 한 번이라도 시작되었으면 true. 세션(씬) 수명 동안 유지된다.</summary>
        public bool HasStarted { get; private set; }

        /// <summary>지정한 노드의 대화를 시작한다. 완료까지 await할 수 있다(presenter가 없으면 즉시 진행).</summary>
        public UniTask StartDialogueAsync(string nodeName)
        {
            HasStarted = true;
            return _runner.StartDialogue(nodeName);
        }

        /// <summary>대화를 시작하고 await하지 않는다(인게임 진입 연출용).</summary>
        public void StartDialogue(string nodeName)
        {
            HasStarted = true;
            _runner.StartDialogue(nodeName).Forget();
        }

        /// <summary>현재 실행 중인 대화가 끝날 때까지 대기한다. 실행 중이 아니면 즉시 완료된다.</summary>
        public UniTask AwaitDialogueAsync()
            => _runner.DialogueTask;

        /// <summary>실행 중인 대화를 중단한다.</summary>
        public UniTask StopAsync()
            => _runner.Stop();

        /// <summary>대화 라인을 표출할 presenter들을 DialogueRunner에 연결한다. Zone 전환과 무관한 Overlay 패널이 제공한다.</summary>
        public void BindPresenters(IEnumerable<DialoguePresenterBase> presenters)
            => _runner.DialoguePresenters = presenters;

        /// <summary>연결된 presenter를 모두 해제한다. 패널 파괴 전에 호출해 dangling 참조를 막는다.</summary>
        public void ClearPresenters()
            => _runner.DialoguePresenters = System.Array.Empty<DialoguePresenterBase>();

        // ── Yarn 변수 접근 (테스트·게임 상태 조회용; Yarn 타입을 외부로 노출하지 않는다) ──

        /// <summary>float Yarn 변수를 읽는다. 변수명은 '$'로 시작해야 한다.</summary>
        public bool TryGetFloat(string variableName, out float value)
            => _runner.VariableStorage.TryGetValue(variableName, out value);

        /// <summary>float Yarn 변수를 설정한다. 변수명은 '$'로 시작해야 한다.</summary>
        public void SetFloat(string variableName, float value)
            => _runner.VariableStorage.SetValue(variableName, value);

        /// <summary>string Yarn 변수를 읽는다. 변수명은 '$'로 시작해야 한다.</summary>
        public bool TryGetString(string variableName, out string value)
            => _runner.VariableStorage.TryGetValue(variableName, out value);

        /// <summary>string Yarn 변수를 설정한다. 변수명은 '$'로 시작해야 한다.</summary>
        public void SetString(string variableName, string value)
            => _runner.VariableStorage.SetValue(variableName, value);
    }
}
