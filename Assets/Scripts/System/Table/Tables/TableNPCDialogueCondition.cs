using System;
using System.Collections.Generic;

using GameStuff;

public class TableNPCDialogueCondition : TableBase
{
    [Serializable]
    public class Info
    {
        public int ID;
        public int Priority;
        public int ConditionType;
        public string ConditionValue;
        public int DialogueID;
    }

    [Serializable]
    public class ConditionEntry
    {
        public List<Info> Conditions = new List<Info>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_conditionType"></param>
        /// <returns>조건에 맞는 컨디션 리스트(priority 오름차순 정렬)</returns>
        public List<Info> Get(QuestConditionType _conditionType)
        {
            List<Info> result = new List<Info>();
            foreach (var info in Conditions)
            {
                if ((QuestConditionType)info.ConditionType == _conditionType)
                {
                    result.Add(info);
                }
            }

            result.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return result;
        }
    }

    public Dictionary<int, ConditionEntry> Dictionary = new Dictionary<int, ConditionEntry>();

    public ConditionEntry Get(int _ID)
    {
        if (Dictionary.ContainsKey(_ID))
            return Dictionary[_ID];

        return null;
    }

    public int GetConditionResult(int _npcID, Inventory _inventory, Player _player)
    {
        var info = Get(_npcID);

        if (info == null || info.Conditions.Count == 0)
            return 0;

        info.Conditions.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        for (int i = 0; i < info.Conditions.Count; i++)
        {
            //우선순위 순으로 만족하는 컨디션을 찾음
            if (CheckCondition(info.Conditions[i], _inventory, _player))
            {
                return info.Conditions[i].DialogueID;
            }
        }
        return 0;
    }

    public bool CheckCondition(Info _info, Inventory _inventory, Player _player)
    {
        var type = (QuestConditionType)_info.ConditionType;
        switch (type)
        {
            case QuestConditionType.None:
                return true;
            case QuestConditionType.Quest:
                {
                    var questParts = _info.ConditionValue.Split('|');
                    if (questParts.Length == 2)
                    {
                        int questId = int.Parse(questParts[0]);
                        int state = int.Parse(questParts[1]);
                        QuestInstance instance = _player.QuestStateInstance.GetQuestState(questId);
                        if (instance == null) return false;

                        return instance.State == (QuestState)state;
                    }

                    return false;
                }
            case QuestConditionType.Item:
                {
                    if (int.TryParse(_info.ConditionValue, out int itemId))
                        return _inventory.HasItem(itemId);
                    return false;
                }
            case QuestConditionType.Level:
                if (int.TryParse(_info.ConditionValue, out int level))
                    return _player.GetCurrentLevel(LevelType.Total) >= level;
                return false;

            case QuestConditionType.Time:
                {
                    var timeParts = _info.ConditionValue.Split('|');
                    if (timeParts.Length == 2)
                    {
                        int daylight = int.Parse(timeParts[0]);
                        int time = int.Parse(timeParts[1]);

                        switch ((Daylight)daylight)
                        {
                            case Daylight.Morning:
                                return GameTime.CurrentDaylight == Daylight.Morning && GameTime.CurrentTime >= time;
                            default:
                            case Daylight.Day:
                                return GameTime.CurrentDaylight == Daylight.Day && GameTime.CurrentTime >= time;
                            case Daylight.Evening:
                                return GameTime.CurrentDaylight == Daylight.Evening && GameTime.CurrentTime >= time;
                            case Daylight.Night:
                                return GameTime.CurrentDaylight == Daylight.Night && GameTime.CurrentTime >= time;
                        }
                    }
                    break;
                }
            case QuestConditionType.TalkCount:
                {
                    if (int.TryParse(_info.ConditionValue, out int required))
                    {
                        int count = _player.TalkCount.GetTalkCount(_info.ID);
                        return count >= required;
                    }
                    return false;
                }
            default:
                return false;
        }
        return false;
    }

    public void Init_Binary(string _Name)
    {
        Load_Binary<Dictionary<int, ConditionEntry>>(_Name, ref Dictionary);
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

            if (!Dictionary.ContainsKey(info.ID))
            {
                Dictionary.Add(info.ID, new ConditionEntry());
            }

            Dictionary[info.ID].Conditions.Add(info);
        }
    }

    protected bool Read(CSVReader _Reader, Info _Info, int _Row, int _Col)
    {
        if (_Reader.reset_row(_Row, _Col) == false)
            return false;

        _Reader.get(_Row, ref _Info.ID);
        _Reader.get(_Row, ref _Info.Priority);
        _Reader.get(_Row, ref _Info.ConditionType);
        _Reader.get(_Row, ref _Info.ConditionValue);
        _Reader.get(_Row, ref _Info.DialogueID);

        return true;
    }
}
