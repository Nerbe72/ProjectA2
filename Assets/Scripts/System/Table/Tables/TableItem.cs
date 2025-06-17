using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class TableItem : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int Name;
        public string Icon;
        public int ItemType;
        public int Rarity;
        public string Prefab;
        public int WeaponID;
        public int Description;
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
        _Reader.get(_Row, ref _Info.Icon);
        _Reader.get(_Row, ref _Info.ItemType);
        _Reader.get(_Row, ref _Info.Rarity);
        _Reader.get(_Row, ref _Info.Prefab);
        _Reader.get(_Row, ref _Info.WeaponID);
        _Reader.get(_Row, ref _Info.Description);

        return true;
    }
}
