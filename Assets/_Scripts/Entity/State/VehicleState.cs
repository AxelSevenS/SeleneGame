using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeleneGame {
    
    public class VehicleState : State{

        public override int id => 7;
        protected override Vector3 GetCameraPosition() => new Vector3(1f, 1f, -3.5f);
        protected override Vector3 GetEntityUp(){
            if ( Vector3.Dot(entity.groundOrientation * -entity.gravityDown, -entity.gravityDown) > 0.75f)
                return entity.groundOrientation * -entity.gravityDown;
            return -entity.gravityDown;
        }

        protected override bool canJump => (coyoteTimer > 0f && jumpCount > 0 && useGravity) && entity.jumpCooldown == 0;
        protected override bool canEvade => (evadeCount > 0f && entity.evadeTimer == 0f);
        
        protected override bool canTurn => (entity.evadeTimer < entity.data.evadeCooldown);
        protected override bool useGravity => true;

        public override bool shifting => false;

        private float moveAmount = 0f;
        private float turnDirection = 0f;

        private bool landed;
        public float coyoteTimer = 0f;

        private void OnEnable(){
            entity.groundData.startAction += OnEntityLand;
        }
        private void OnDisable(){
            entity.groundData.startAction -= OnEntityLand;
        }

        public override void StateUpdate(){;
        }

        public override void StateFixedUpdate(){

            Gravity(entity.gravityForce, entity.gravityDown);

            coyoteTimer = Mathf.Max( Mathf.MoveTowards( coyoteTimer, 0f, Time.deltaTime ), (System.Convert.ToSingle(entity.onGround) * 0.4f) );
            
            if ( entity.groundData.currentValue ){
                evadeCount = 1;
                if( entity.jumpCooldown == 0 )
                    jumpCount = 1;
            }
            
            // Jump if the Jump key is pressed.
            if (entity.jumpInputData.currentValue){
                Jump();
            }


            // When the Entity is sliding
            if (entity.sliding)
                entity._rb.velocity += entity.groundOrientation * entity.evadeDirection *entity.data.baseSpeed * entity.inertiaMultiplier * Time.deltaTime;



            //  ---------------------------- When the Entity is Focusing
            if ( entity.onGround && entity.focusing)
                entity.gravityDown = Vector3.Lerp( entity.gravityDown, -entity.groundHit.normal, 0.1f );
            
            entity.SetRotation(-entity.gravityDown);

            
            
            // Handling Tank Movement Logic.
            float turningSpeed = (-(Mathf.Max(entity.moveSpeed, .1f)/entity.data.baseSpeed) + 2.5f)/1.25f;
            
            entity.relativeForward = Quaternion.AngleAxis(180f * turningSpeed * turnDirection * Time.deltaTime, Vector3.up) * entity.relativeForward;
            entity.rotationForward = Vector3.Lerp(entity.rotationForward, entity.relativeForward, 0.7f).normalized;

            entity.moveDirection = entity.absoluteForward * moveAmount;
            entity.GroundedMove(entity.moveDirection * Time.deltaTime * entity.moveSpeed);
            
            RotateEntity(entity.rotationForward);

        }

        public override void HandleInput(){
            // Handle Movement Input.
            moveAmount = entity.currentFrameMoveInput.z;
            turnDirection = entity.currentFrameMoveInput.x;
        }

        private void OnEntityLand(float timer){
            entity.StartWalkAnim();
        }
    }
}
