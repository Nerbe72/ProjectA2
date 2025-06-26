using System;
using System.Collections.Generic;

public class TableEnemy : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int Type;
        public int Name;
        public float Speed;
        public string Weapon;
        public int Health;
        public int Damage;
        public int Defense;
        public float Sight_Offset;
        public float Sight_Angle;
        public float Sight_Distance;
        public float Sight_Height;
        public float Distance_Movable;
        public float Distance_Attackable;
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
        _Reader.get(_Row, ref _Info.Type);
        _Reader.get(_Row, ref _Info.Name);
        _Reader.get(_Row, ref _Info.Speed);
        _Reader.get(_Row, ref _Info.Weapon);
        _Reader.get(_Row, ref _Info.Health);
        _Reader.get(_Row, ref _Info.Damage);
        _Reader.get(_Row, ref _Info.Defense);
        _Reader.get(_Row, ref _Info.Sight_Offset);
        _Reader.get(_Row, ref _Info.Sight_Angle);
        _Reader.get(_Row, ref _Info.Sight_Distance);
        _Reader.get(_Row, ref _Info.Sight_Height);
        _Reader.get(_Row, ref _Info.Distance_Movable);
        _Reader.get(_Row, ref _Info.Distance_Attackable);

        return true;
    }
}
