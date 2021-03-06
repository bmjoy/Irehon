using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SkillType { Bow}

[Serializable]
public struct Skill
{
    public string Name => Type.ToString();
    public int Lvl;
    public SkillType Type;
    public int Xp;

    private int xpToNextLvl;

    public Skill(SkillType type, int lvl, int xp)
    {
        Type = type;
        Lvl = lvl;
        Xp = xp;
        xpToNextLvl = lvl * 4;
    }

    public void GetXp(int xp)
    {
        this.Xp += xp;
        while (this.Xp >= xpToNextLvl)
        {
            this.Xp -= xpToNextLvl;
            Lvl++;
            xpToNextLvl = Lvl * 4;
        }
    }
}
