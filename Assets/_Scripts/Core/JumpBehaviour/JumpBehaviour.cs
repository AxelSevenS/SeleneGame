using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Animancer;

using SevenGame.Utility;

namespace SeleneGame.Core {

    [System.Serializable]
    public class JumpBehaviour : BehaviourStrategy {

        public TimeUntil cooldownTimer;

        
        public virtual bool canJump => cooldownTimer.isDone;


        private AnimationClip _jumpAnimation;

        public AnimationClip jumpAnimation {
            get {
                if ( _jumpAnimation == null )
                    _jumpAnimation = entity.character?.GetAnimation("Jump");
                return _jumpAnimation;
            }
        }



        protected internal override void HandleInput(PlayerEntityController contoller) {;}

        protected internal virtual void Jump(Vector3 direction) {

            Debug.Log(entity.character.jumpHeight * entity.jumpMultiplier);

            AnimancerState jumpState = entity.animancer.Layers[0].Play(jumpAnimation, 0.3f);
            jumpState.Events.OnEnd = () => {
                jumpState.Stop();
            };

            Vector3 newVelocity = entity.rigidbody.velocity.NullifyInDirection( -direction );
            newVelocity += entity.character.jumpHeight * entity.jumpMultiplier * direction;
            entity.rigidbody.velocity = newVelocity;

            cooldownTimer.SetDuration(0.4f);

        }
    }

}
