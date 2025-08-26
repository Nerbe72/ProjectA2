using System;
using System.Collections.Generic;
using UnityEngine;

public class TableEnhancement : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int TotalEnhancement;
        public int AddGrowthSTR;
        public int AddGrowthDEX;
        public int AddGrowthINT;
    }

    public Dictionary<(int, int), Info> Dictionary = new Dictionary<(int, int), Info>();

    public Info Get(int _Idx, int _EnhanceCount)
    {
        if (Dictionary.ContainsKey((_Idx, _EnhanceCount)))
            return Dictionary[(_Idx, _EnhanceCount)];

        return null;
    }

    public void Init_Binary(string _Name)
    {
        Load_Binary<Dictionary<(int, int), Info>>(_Name, ref Dictionary);
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

            Dictionary.Add((info.ID, info.TotalEnhancement), info);
        }
    }

    protected bool Read(CSVReader _Reader, Info _Info, int _Row, int _Col)
    {
        if (_Reader.reset_row(_Row, _Col) == false)
            return false;

        _Reader.get(_Row, ref _Info.ID);
        _Reader.get(_Row, ref _Info.TotalEnhancement);
        _Reader.get(_Row, ref _Info.AddGrowthSTR);
        _Reader.get(_Row, ref _Info.AddGrowthDEX);
        _Reader.get(_Row, ref _Info.AddGrowthINT);
        
        return true;
    }
}
