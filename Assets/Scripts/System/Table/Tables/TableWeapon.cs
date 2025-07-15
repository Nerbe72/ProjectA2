using System;
using System.Collections.Generic;

public class TableWeapon : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int WeaponType;
        public int AttackType;
        public int Damage_Min;
        public int Damage_Max;
        public int Defense_Min;
        public int Defense_Max;
        public int Require_STR;
        public int Require_DEX;
        public int Require_INT;
        public int DamageGrowth_STR;
        public int DamageGrowth_DEX;
        public int DamageGrowth_INT;
        public float CastingTime;
        public int[] Abilities;
        public int ProjectileID;
    }

    public Dictionary<int, Info> Dictionary = new Dictionary<int, Info>();

    public Info Get(int _Idx)
    {
        if (Dictionary.ContainsKey(_Idx))
            return Dictionary[_Idx];

        return null;
    }

    public void Init_Binary(string _Name)
    {
        Load_Binary<Dictionary<int, Info>>(_Name, ref Dictionary);
    }

    public void Save_Binary(string _Name)
    {
        Save_Binary(_Name, Dictionary);
    }

    public void Init_CSV(string _Name, int _Row, int _Col)
    {
        CSVReader reader = GetCSVReader(_Name);

        for (int row = _Row; row < reader.row; ++row)
        {
            Info info = new Info();

            if (Read(reader, info, row, _Col) == false)
                break;

            Dictionary.Add(info.ID, info);
        }
    }

    protected bool Read(CSVReader _Reader, Info _Info, int _Row, int _Col)
    {
        if (_Reader.reset_row(_Row, _Col) == false)
            return false;

        _Info.Abilities = new int[3];

        _Reader.get(_Row, ref _Info.ID);
        _Reader.get(_Row, ref _Info.WeaponType);
        _Reader.get(_Row, ref _Info.AttackType);
        _Reader.get(_Row, ref _Info.Damage_Min);
        _Reader.get(_Row, ref _Info.Damage_Max);
        _Reader.get(_Row, ref _Info.Defense_Min);
        _Reader.get(_Row, ref _Info.Defense_Max);
        _Reader.get(_Row, ref _Info.Require_STR);
        _Reader.get(_Row, ref _Info.Require_DEX);
        _Reader.get(_Row, ref _Info.Require_INT);
        _Reader.get(_Row, ref _Info.DamageGrowth_STR);
        _Reader.get(_Row, ref _Info.DamageGrowth_DEX);
        _Reader.get(_Row, ref _Info.DamageGrowth_INT);
        _Reader.get(_Row, ref _Info.CastingTime);
        _Reader.get(_Row, ref _Info.Abilities, 3);
        _Reader.get(_Row, ref _Info.ProjectileID);

        return true;
    }
}
