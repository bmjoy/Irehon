using System.Collections.Generic;
using UnityEngine;
public interface IAbilityTreeController
{
    RectTransform GetDragger();
    Canvas GetSlotCanvas();
    void OnSelectSkill(int id);
    void UnlockSelectedSkill();
    void SetUnlockedSkillsInfo(List<int> unlockedSkills);
    void SetAbilitys(List<IAbility> abilities);
}