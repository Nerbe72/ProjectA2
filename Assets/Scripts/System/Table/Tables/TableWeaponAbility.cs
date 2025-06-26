using System;
using System.Collections.Generic;

public class TableWeaponAbility : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int Name;
        public float Cooldown;
        public int Description;
        public int AttackType;
        public int ContinuouseTime;
        public int Damage;
        public int KnockbackForce;
        public int Projectile_Amount;
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

        _Reader.get(_Row, ref _Info.ID);
        _Reader.get(_Row, ref _Info.Name);
        _Reader.get(_Row, ref _Info.Cooldown);
        _Reader.get(_Row, ref _Info.Description);
        _Reader.get(_Row, ref _Info.AttackType);
        _Reader.get(_Row, ref _Info.ContinuouseTime);
        _Reader.get(_Row, ref _Info.Damage);
        _Reader.get(_Row, ref _Info.KnockbackForce);
        _Reader.get(_Row, ref _Info.Projectile_Amount);
        _Reader.get(_Row, ref _Info.ProjectileID);

        return true;
    }
}
