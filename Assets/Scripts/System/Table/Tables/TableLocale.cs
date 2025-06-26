using System;
using System.Collections.Generic;

public class TableLocale : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public string Korean;
        public string English;
        public string Japanese;
    }

    public Dictionary<int, Info> Dictionary = new Dictionary<int, Info>();

    public Info Get(int _Idx)
    {
        if (Dictionary.ContainsKey(_Idx))
            return Dictionary[_Idx];

        return null;
    }

    public string Get(int _Idx, Locale _locale)
    {
        if (Dictionary.ContainsKey(_Idx))
        {
            switch (_locale)
            {
                case Locale.English:
                    return Dictionary[_Idx].English;
                case Locale.Korean:
                    return Dictionary[_Idx].Korean;
                case Locale.Japanese:
                    return Dictionary[_Idx].Japanese;
            }
        }

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
        _Reader.get(_Row, ref _Info.Korean);
        _Reader.get(_Row, ref _Info.English);
        _Reader.get(_Row, ref _Info.Japanese);

        return true;
    }
}
