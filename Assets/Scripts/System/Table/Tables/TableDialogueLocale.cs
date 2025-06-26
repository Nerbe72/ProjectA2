using System;
using System.Collections.Generic;

public enum DialogueType
{
    Talk,
    Choice
}

public class TableDialogueLocale : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public string TalkKo;
        public string ChoiceKo;
        public string TalkEn;
        public string ChoiceEn;
        public string TalkJa;
        public string ChoiceJa;
    }

    public Dictionary<int, Info> Dictionary = new Dictionary<int, Info>();

    public Info Get(int _Idx)
    {
        if (Dictionary.ContainsKey(_Idx))
            return Dictionary[_Idx];

        return null;
    }

    public string Get(int _Idx, DialogueType _type, Locale _locale)
    {
        if (Dictionary.ContainsKey(_Idx))
        {
            Info info = Dictionary[_Idx];
            switch (_locale)
            {
                case Locale.Korean:
                default:
                    switch (_type)
                    {
                        case DialogueType.Talk:
                            return info.TalkKo;
                        case DialogueType.Choice:
                            return info.ChoiceKo;
                    }
                    break;
                case Locale.English:
                    switch (_type)
                    {
                        case DialogueType.Talk:
                            return info.TalkEn;
                        case DialogueType.Choice:
                            return info.ChoiceEn;
                    }
                    break;
                case Locale.Japanese:
                    switch (_type)
                    {
                        case DialogueType.Talk:
                            return info.TalkJa;
                        case DialogueType.Choice:
                            return info.ChoiceJa;
                    }
                    break;
            }
        }
        return string.Empty;
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
        _Reader.get(_Row, ref _Info.TalkKo);
        _Reader.get(_Row, ref _Info.ChoiceKo);
        _Reader.get(_Row, ref _Info.TalkEn);
        _Reader.get(_Row, ref _Info.ChoiceEn);
        _Reader.get(_Row, ref _Info.TalkJa);
        _Reader.get(_Row, ref _Info.ChoiceJa);

        return true;
    }
}
