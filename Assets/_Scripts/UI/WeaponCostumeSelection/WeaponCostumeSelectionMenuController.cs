using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using SeleneGame.Core;

using SevenGame.Utility;

namespace SeleneGame.UI {
    
    public class WeaponCostumeSelectionMenuController : UIMenu<WeaponCostumeSelectionMenuController> {

        public const int WEAPON_COSTUME_CASES_PER_ROW = 5;

        

        [SerializeField] private GameObject weaponSelectionMenu;
        [SerializeField] private GameObject weaponCostumeSelectionContainer;
        [SerializeField] private GameObject weaponCostumeCaseTemplate;
        
        [SerializeField] private List<WeaponCostumeCase> weaponCostumes = new List<WeaponCostumeCase>();

        private Weapon.Instance currentWeapon;

        private Action<WeaponCostume> onWeaponCostumeSelected;



        public override void Enable() {

            base.Enable();

            weaponSelectionMenu.SetActive( true );

            UIController.current.UpdateMenuState();
        }

        public override void Disable() {

            base.Disable();

            weaponSelectionMenu.SetActive( false );

            UIController.current.UpdateMenuState();
        }

        public override void ResetGamePadSelection(){
            EventSystem.current.SetSelectedGameObject(weaponCostumes[0].gameObject);
        }

        public override void OnCancel() {
            WeaponInventoryMenuController.current.Enable();
        }
        

        public void ReplaceWeaponCostume(Weapon.Instance weapon) {
            currentWeapon = weapon;

            onWeaponCostumeSelected = (selectedCostume) => {
                currentWeapon.SetCostume(selectedCostume);
                OnCancel();
            };

            GetEquippableCostumes();

            Enable();
        }


        public void OnSelectWeaponCostume(WeaponCostume weaponCostume) {
            if ( !Enabled ) return;
            onWeaponCostumeSelected?.Invoke(weaponCostume);
        }

        private void GetEquippableCostumes() {

            foreach ( WeaponCostumeCase costume in weaponCostumes ) {
                GameUtility.SafeDestroy(costume.gameObject);
            }
            weaponCostumes = new();
            
            WeaponCostume.GetBaseAsync( currentWeapon.name, (defaultCostume) => {

                    // Get the Default Costume (corresponds to an empty slot, should be in the first space)
                    CreateWeaponCostumeCase(defaultCostume);
                    ResetGamePadSelection();

                    // and then get all the other costumes.
                    WeaponCostume.GetCostumesFor(currentWeapon.weapon, (costume) => {
                            if ( weaponCostumes.Exists( (obj) => { return obj.weaponCostume == costume; }) ) 
                                return;

                            CreateWeaponCostumeCase(costume);
                        }
                    );
                }
            );


        }

        private void CreateWeaponCostumeCase(WeaponCostume costume){
            var caseObject = Instantiate(weaponCostumeCaseTemplate, weaponCostumeSelectionContainer.transform);
            var costumeCase = caseObject.GetComponentInChildren<WeaponCostumeCase>();
            costumeCase.weaponCostume = costume;
            
            weaponCostumes.Add( costumeCase );
            if (weaponCostumes.Count > 1) {
                WeaponCostumeCase previousCase = weaponCostumes[weaponCostumes.Count - 2];
                previousCase.elementRight = costumeCase;
                costumeCase.elementLeft = previousCase;
            }

            if (weaponCostumes.Count > WEAPON_COSTUME_CASES_PER_ROW) {
                WeaponCostumeCase aboveCase = weaponCostumes[weaponCostumes.Count - (WEAPON_COSTUME_CASES_PER_ROW + 1)];
                aboveCase.elementDown = costumeCase;
                costumeCase.elementUp = aboveCase;
            }

        }



        private void Awake() {
            weaponCostumeSelectionContainer.GetComponent<GridLayoutGroup>().constraintCount = WEAPON_COSTUME_CASES_PER_ROW;
        }

    }
}