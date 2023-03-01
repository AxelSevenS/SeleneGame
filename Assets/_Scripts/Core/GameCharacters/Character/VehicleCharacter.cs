using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeleneGame.Core {

    [System.Serializable]
    public abstract class VehicleCharacter : Character {

        public Seat.SittingPose[] sittingPoses;

        public VehicleCharacter(Entity entity, CharacterData data, CharacterCostume costume = null) : base(entity, data, costume) {
        }
    }
}
