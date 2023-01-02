using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    public class PlayerInventory : MonoBehaviour
    {
        private WeaponSlotManager weaponSlotManager;
        public WeaponItem rigtWeapon;
        public WeaponItem leftWeapon;
        void Awake()
        {
            weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
        }

        private void Start()
        {
            weaponSlotManager.LoadWeaponOnSlot(rigtWeapon,false);
            weaponSlotManager.LoadWeaponOnSlot(leftWeapon,true);
        }

        void Update()
        {
        
        }
    }
}

