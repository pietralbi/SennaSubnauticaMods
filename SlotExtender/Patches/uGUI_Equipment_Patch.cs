using UnityEngine;
using HarmonyLib;
using Common;

namespace SlotExtender.Patches
{
    [HarmonyPatch(typeof(uGUI_Equipment), "Awake")]    
    public static class uGUI_Equipment_Awake_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(uGUI_Equipment __instance)
        {
            Transform transform = __instance.gameObject.transform;

            void _setSlotPos(GameObject slot, Vector2 pos)
            {
                uGUI_EquipmentSlot equipmentSlot = slot.GetComponent<uGUI_EquipmentSlot>();

                if (equipmentSlot == null)
                {
                    SNLogger.Warn($"Missing uGUI_EquipmentSlot component on '{slot.name}'.");
                    return;
                }

                equipmentSlot.rectTransform.anchoredPosition = pos;
            }

            void _processSlot(SlotData slotData, GameObject normal, GameObject ArmLeft, GameObject ArmRight)
            {
                switch (slotData.SlotType)
                {
                    case SlotType.CloneChip:
                        _processCloneSlot(slotData, normal);
                        break;
                    case SlotType.OriginalNormal:
                    case SlotType.OriginalArmLeft:
                    case SlotType.OriginalArmRight:
                        _processOriginalSlot(slotData);
                        break;                   

                    case SlotType.CloneNormal:
                        _processCloneSlot(slotData, normal);
                        break;

                    case SlotType.CloneArmLeft:
                        _processCloneSlot(slotData, ArmLeft);
                        break;

                    case SlotType.CloneArmRight:
                        _processCloneSlot(slotData, ArmRight);
                        break;
                }
            }

            void _processOriginalSlot(SlotData slotData)
            {
                Transform originalSlotTransform = transform.Find(slotData.SlotID);

                if (originalSlotTransform == null)
                {
                    SNLogger.Warn($"Original slot '{slotData.SlotID}' was not found on uGUI_Equipment instance.");
                    return;
                }

                GameObject originalSlot = originalSlotTransform.gameObject;

                _setSlotPos(originalSlot, slotData.SlotPos);                
            }

            void _processCloneSlot(SlotData slotData, GameObject prefab)
            {
                if (prefab == null)
                {
                    SNLogger.Warn($"Cannot create slot '{slotData.SlotID}' because its prefab is missing.");
                    return;
                }

                Transform existingSlotTransform = transform.Find(slotData.SlotID);
                GameObject temp_slot = existingSlotTransform != null
                    ? existingSlotTransform.gameObject
                    : Object.Instantiate(prefab, transform, false);

                temp_slot.name = slotData.SlotID;

                _setSlotPos(temp_slot, slotData.SlotPos);

                uGUI_EquipmentSlot equipmentSlot = temp_slot.GetComponent<uGUI_EquipmentSlot>();

                if (equipmentSlot != null)
                {
                    equipmentSlot.slot = slotData.uGui_SlotName;
                }
            }

            // initializing GameObject variables for cloning
            GameObject NormalModuleSlot = transform.Find("SeamothModule2").gameObject;
            GameObject ArmLeftSlot = transform.Find("ExosuitArmLeft").gameObject;
            GameObject ArmRightSlot = transform.Find("ExosuitArmRight").gameObject;
            GameObject ChipSlot = transform.Find("Chip1").gameObject;

            // processing Player chip slots            
            SlotHelper.NewChipSlots.ForEach(slotData => _processSlot(slotData, ChipSlot, null, null));

            // processing Seamoth slots            
            SlotHelper.SessionSeamothSlots.ForEach(slotData => _processSlot(slotData, NormalModuleSlot, ArmLeftSlot, ArmRightSlot));

            // repositioning Seamoth background picture
            transform.Find("SeamothModule1/Seamoth").localPosition = SlotHelper.VehicleImgPos;

            // processing Exosuit slots
            SlotHelper.SessionExosuitSlots.ForEach(slotData => _processSlot(slotData, NormalModuleSlot, ArmLeftSlot, ArmRightSlot));

            // repositioning Exosuit background picture
            transform.Find("ExosuitModule1/Exosuit").localPosition = SlotHelper.VehicleImgPos;

            SNLogger.Log("uGUI_Equipment Slots Patched!");
        }

        
        [HarmonyPostfix]
        public static void Postfix(ref uGUI_Equipment __instance)
        {
            __instance.gameObject.EnsureComponent<uGUI_SlotTextHandler>();
        }
        
    }
}
