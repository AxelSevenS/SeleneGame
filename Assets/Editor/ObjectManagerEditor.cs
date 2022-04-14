using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SeleneGame.Core;

namespace SeleneGame.EditorUI {

    [CustomEditor(typeof(ObjectManager))]
    public class ObjectManagerEditor : Editor{
        
        public override void OnInspectorGUI(){
            DrawDefaultInspector();
            ObjectManager objectManager = (ObjectManager)target;

            if (GUILayout.Button( "Disable all Objects" )){
                objectManager.DisableAllObjects();
            }
        }
    }
}