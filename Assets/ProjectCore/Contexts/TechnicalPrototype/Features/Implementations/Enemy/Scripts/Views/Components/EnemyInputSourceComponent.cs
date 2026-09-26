using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyInputSourceComponent : BaseEntityInputSourceComponent<EnemyInputFrameData>
    {
        private EnemyInputFrameData _currentInput;
        private bool _hasInput;

        public override bool TryGetInput(out EnemyInputFrameData input)
        {
            input = _currentInput;
            return _hasInput;
        }

        public void SetInput(EnemyInputData input)
        {
            _currentInput = new EnemyInputFrameData(input.Direction);
            _hasInput = true;
        }

        public void ClearInput()
        {
            _currentInput = default;
            _hasInput = false;
        }

        public override void Init()
        {
            ClearInput();
        }
    }
}
