using System.Collections.Generic;
using System;
using UnityEngine;

public class TableQuest : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int NameID;
        public int DescriptionID;
        /// <summary>
        /// Quest Type
        /// </summary>
        public int Type;
        public int PrerequisiteID;
        public List<int> NextIDs;
        public int ReceiverNPC;
        public string Objectives;
        public string Reward;
        public bool Repeatable;
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
        _Reader.get(_Row, ref _Info.NameID);
        _Reader.get(_Row, ref _Info.DescriptionID);
        _Reader.get(_Row, ref _Info.Type);
        _Reader.get(_Row, ref _Info.PrerequisiteID);
        _Reader.get(_Row, ref _Info.NextIDs);
        _Reader.get(_Row, ref _Info.ReceiverNPC);
        _Reader.get(_Row, ref _Info.Objectives);
        _Reader.get(_Row, ref _Info.Reward);
        _Reader.get(_Row, ref _Info.Repeatable);

        return true;
    }
}
