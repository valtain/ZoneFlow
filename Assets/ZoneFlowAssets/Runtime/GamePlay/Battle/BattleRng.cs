using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 주입 시드 기반 경량 LCG(선형 합동 생성기) 구현.
    /// <para>
    /// 파라미터: a = 1664525, c = 1013904223 (Numerical Recipes in C 제시 값).
    /// 전역 <see cref="UnityEngine.Random"/> · <see cref="System.Random"/> 을 사용하지 않으므로
    /// 같은 시드에서 항상 동일한 수열을 생성한다(결정론).
    /// </para>
    /// </summary>
    public sealed class BattleRng : IBattleRng
    {
        // LCG 파라미터 (모듈러스 = 2^32, uint 오버플로우가 자연스러운 모듈러스 역할)
        private const uint _a = 1664525u;
        private const uint _c = 1013904223u;

        private uint _state;

        /// <summary>지정한 시드로 LCG 상태를 초기화한다.</summary>
        /// <param name="seed">결정론 재현을 위한 시드 값.</param>
        public BattleRng(int seed)
        {
            // 음수 시드도 안전하게 비트 캐스팅
            _state = (uint)seed;
        }

        /// <inheritdoc/>
        public int Next(int minInclusive, int maxExclusive)
        {
            Debug.Assert(maxExclusive > minInclusive,
                $"BattleRng.Next: maxExclusive({maxExclusive}) must be > minInclusive({minInclusive})");

            _state = _state * _a + _c;
            int range = maxExclusive - minInclusive;
            // 부호 없는 상태를 양수 범위로 매핑 (부호 비트 제거 후 모듈러스)
            int offset = (int)(_state >> 1) % range;
            return minInclusive + offset;
        }

        /// <inheritdoc/>
        public int Next(int maxExclusive)
        {
            return Next(0, maxExclusive);
        }
    }
}
