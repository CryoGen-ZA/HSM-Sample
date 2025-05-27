using Character;
using HSM.Core;

namespace HSM.PlayerStates
{
    public class IdleState : BaseState
    {
        private readonly PlayerCharacter _ctx;

        public IdleState(StateGenerator generator, PlayerCharacter ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx.PlayerInput.magnitude > 0 || _ctx.MovementComponent.GetCurrentVelocityWithoutGravity().magnitude > 0)
            {
                newState = nameof(RunState);
                return true;
            }
            
            newState = null;
            return false;
        }
    }
}