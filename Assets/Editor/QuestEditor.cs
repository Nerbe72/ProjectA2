using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class QuestEditor : EditorWindow
{
    private Dictionary<int, QuestInfo> quests = new Dictionary<int, QuestInfo>();
    private string dataPath;

    // 추가된 필드들
    private Vector2 scrollPosition;
    private Vector2 questEditScrollPosition;
    private QuestInfo selectedQuest = null;
    private bool showAddQuestForm = false;
    private QuestInfo newQuest = new QuestInfo();

    // 후속 퀘스트 관리용
    private int newNextQuestID = 0;

    [MenuItem("Window/Quest Editor")]
    public static void ShowWindow()
    {
        GetWindow<QuestEditor>("Quest Editor");
    }

    private void OnGUI()
    {
        // 전체 상단 공백
        EditorGUILayout.Space(10);

        dataPath = Application.persistentDataPath;
        
        // 전체 좌우 공백을 위한 외부 Horizontal 그룹
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(10); // 왼쪽 공백

        EditorGUILayout.BeginVertical(); // 실제 내용을 담을 내부 Vertical 그룹

        // 기존의 메인 Horizontal 그룹 (QuestListSide와 QuestEditSide를 나누는)
        EditorGUILayout.BeginHorizontal(); 
        
        QuestListSide();
        
        EditorGUILayout.Space(10); // 목록과 편집기 사이 공백 (이미 있음)
        
        QuestEditSide();
        
        EditorGUILayout.EndHorizontal(); // 메인 Horizontal 그룹 끝

        EditorGUILayout.EndVertical(); // 실제 내용을 담는 내부 Vertical 그룹 끝

        EditorGUILayout.Space(10); // 오른쪽 공백
        EditorGUILayout.EndHorizontal(); // 전체 좌우 공백을 위한 외부 Horizontal 그룹 끝

        // 전체 하단 공백
        EditorGUILayout.Space(10);
    }

    private void QuestListSide()
    {
        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(400), GUILayout.ExpandHeight(true));
        GUILayout.Label("Quest List", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("불러오기"))
        {
            ImportJSON();
        }
        if (GUILayout.Button("저장"))
        {
            ExportJSON();
        }
        if (GUILayout.Button("새 퀘스트"))
        {
            ShowAddQuestForm();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // 퀘스트 목록 스크롤
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        
        foreach (var questPair in quests)
        {
            QuestInfo quest = questPair.Value;
            
            EditorGUILayout.BeginHorizontal();
            
            // 퀘스트 선택 버튼
            if (GUILayout.Button($"[{quest.QuestID}] Quest {quest.QuestID}", GUILayout.Width(200)))
            {
                SelectQuest(quest);
            }
            
            // 삭제 버튼
            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                DeleteQuest(quest.QuestID);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();

        // 새 퀘스트 추가 폼
        if (showAddQuestForm)
        {
            ShowNewQuestForm();
        }

        EditorGUILayout.EndVertical();
    }

    private void QuestEditSide()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        if (selectedQuest != null)
        {
            GUILayout.Label("Quest Editor", EditorStyles.boldLabel);
            ShowQuestEditor(selectedQuest);
        }
        else
        {
            GUILayout.Label("퀘스트를 선택하거나 새로 만드세요.", EditorStyles.helpBox);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void ShowAddQuestForm()
    {
        showAddQuestForm = true;
        newQuest = new QuestInfo();
        newQuest.Objectives = new List<ObjectiveInfo>();
        newQuest.Reward = new RewardInfo();
        newQuest.NextQuestIDs = new List<int>();
        
        // 새 퀘스트의 기본 ID 설정
        int newId = GetNextQuestID();
        newQuest.QuestID = newId;
    }

    private void ShowNewQuestForm()
    {
        EditorGUILayout.Space(10);
        GUILayout.Label("새 퀘스트 추가", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        newQuest.QuestID = EditorGUILayout.IntField("퀘스트 ID", newQuest.QuestID);
        newQuest.NameID = EditorGUILayout.IntField("이름 ID", newQuest.NameID);
        newQuest.DescriptionID = EditorGUILayout.IntField("설명 ID", newQuest.DescriptionID);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("추가"))
        {
            AddNewQuest();
        }
        
        if (GUILayout.Button("취소"))
        {
            showAddQuestForm = false;
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void ShowQuestEditor(QuestInfo _quest)
    {
        questEditScrollPosition = EditorGUILayout.BeginScrollView(questEditScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        // 기본 정보
        GUILayout.Label("기본 정보", EditorStyles.boldLabel);
        _quest.QuestID = EditorGUILayout.IntField("퀘스트 ID", _quest.QuestID);
        _quest.NameID = EditorGUILayout.IntField("이름 ID", _quest.NameID);
        _quest.DescriptionID = EditorGUILayout.IntField("설명 ID", _quest.DescriptionID);
        
        EditorGUILayout.Space(10);
        
        // 연결 정보
        GUILayout.Label("연결 정보", EditorStyles.boldLabel);
        _quest.PrerequisiteQuestID = EditorGUILayout.IntField("선행 퀘스트 ID", _quest.PrerequisiteQuestID);
        _quest.ReceiverNPCID = EditorGUILayout.IntField("NPC ID", _quest.ReceiverNPCID);
        
        // 후속 퀘스트들 (여러개)
        EditorGUILayout.Space(5);
        GUILayout.Label("후속 퀘스트들", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        for (int i = 0; i < _quest.NextQuestIDs.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _quest.NextQuestIDs[i] = EditorGUILayout.IntField($"후속 퀘스트 {i + 1}", _quest.NextQuestIDs[i]);
            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                _quest.NextQuestIDs.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                GUI.changed = true;
                return;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.BeginHorizontal();
        newNextQuestID = EditorGUILayout.IntField("새 후속 퀘스트 ID", newNextQuestID);
        if (GUILayout.Button("추가", GUILayout.Width(50)))
        {
            if (!_quest.NextQuestIDs.Contains(newNextQuestID))
            {
                _quest.NextQuestIDs.Add(newNextQuestID);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 기타 설정
        GUILayout.Label("기타 설정", EditorStyles.boldLabel);
        _quest.Repeatable = EditorGUILayout.Toggle("반복 가능", _quest.Repeatable);
        
        EditorGUILayout.Space(10);
        
        // 목표 편집 (GUI)
        ShowObjectivesEditor(_quest);
        
        EditorGUILayout.Space(10);
        
        // 보상 편집 (GUI)
        ShowRewardsEditor(_quest);
        
        EditorGUILayout.Space(10);
        
        // 적용 버튼
        if (GUILayout.Button("변경사항 적용"))
        {
            ApplyQuestChanges(_quest);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void ShowObjectivesEditor(QuestInfo _quest)
    {
        GUILayout.Label("퀘스트 목표", EditorStyles.boldLabel);
        
        if (_quest.Objectives == null)
        {
            _quest.Objectives = new List<ObjectiveInfo>();
        }

        for (int i = 0; i < _quest.Objectives.Count; i++)
        {
            ObjectiveInfo obj = _quest.Objectives[i];
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"목표 {i + 1}", EditorStyles.miniBoldLabel);

            // 목표 타입을 ObjectiveType enum 드롭다운으로 선택
            ObjectiveType selectedObjectiveTypeEnum = (ObjectiveType)EditorGUILayout.EnumPopup("목표 타입", obj.ObjectiveType);
            if (selectedObjectiveTypeEnum != obj.ObjectiveType)
            {
                obj.ObjectiveType = selectedObjectiveTypeEnum;
            }

            obj.TargetID = EditorGUILayout.IntField("타겟 ID", obj.TargetID);

            // 선택된 목표 타입에 따라 다른 UI 표시
            switch (selectedObjectiveTypeEnum)
            {
                case ObjectiveType.Kill:
                case ObjectiveType.Collect:
                    obj.Required = EditorGUILayout.IntField("요구량", obj.Required);
                    obj.Interacted = false; // Kill, Collect 타입일 때는 interacted 사용 안 함 (데이터 일관성)
                    break;
                case ObjectiveType.Interact:
                    obj.Interacted = EditorGUILayout.Toggle("상호작용 완료 여부", obj.Interacted);
                    obj.Required = 0; // Interact 타입일 때는 required 사용 안 함 (데이터 일관성)
                    break;
            }

            if (GUILayout.Button("이 목표 삭제", GUILayout.Width(100)))
            {
                _quest.Objectives.RemoveAt(i);
                EditorGUILayout.EndVertical();
                GUI.changed = true;
                return;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("처치 목표 추가"))
        {
            _quest.Objectives.Add(new ObjectiveInfo { ObjectiveType = ObjectiveType.Kill, Interacted = false });
        }
        if (GUILayout.Button("수집 목표 추가"))
        {
            _quest.Objectives.Add(new ObjectiveInfo { ObjectiveType = ObjectiveType.Collect, Interacted = false });
        }
        if (GUILayout.Button("상호작용 목표 추가"))
        {
            _quest.Objectives.Add(new ObjectiveInfo { ObjectiveType = ObjectiveType.Interact, Required = 0 });
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ShowRewardsEditor(QuestInfo _quest)
    {
        GUILayout.Label("퀘스트 보상", EditorStyles.boldLabel);
        
        if (_quest.Reward == null)
        {
            _quest.Reward = new RewardInfo();
        }
        if (_quest.Reward.itemIds == null)
        {
            _quest.Reward.itemIds = new List<int>();
        }

        _quest.Reward.Currency = EditorGUILayout.IntField("통화 보상", _quest.Reward.Currency);

        GUILayout.Label("아이템 보상 ID 목록", EditorStyles.miniBoldLabel);
        for (int i = 0; i < _quest.Reward.itemIds.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _quest.Reward.itemIds[i] = EditorGUILayout.IntField($"아이템 ID {i + 1}", _quest.Reward.itemIds[i]);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                _quest.Reward.itemIds.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                GUI.changed = true;
                return;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("보상 아이템 ID 추가"))
        {
            _quest.Reward.itemIds.Add(0);
        }
    }

    private void SelectQuest(QuestInfo _quest)
    {
        selectedQuest = _quest;
        showAddQuestForm = false;
    }

    private void AddNewQuest()
    {
        if (newQuest != null && newQuest.QuestID > 0 && !quests.ContainsKey(newQuest.QuestID))
        {
            quests.Add(newQuest.QuestID, newQuest);
            selectedQuest = newQuest;
            showAddQuestForm = false;
        }
        else
        {
            EditorUtility.DisplayDialog("오류", "유효한 새 퀘스트 ID를 입력하거나 이미 존재하지 않는 ID여야 합니다.", "확인");
        }
    }

    private void DeleteQuest(int _questID)
    {
        if (EditorUtility.DisplayDialog("퀘스트 삭제", $"퀘스트 {_questID}를 정말 삭제하시겠습니까?", "삭제", "취소"))
        {
            quests.Remove(_questID);
            
            if (selectedQuest != null && selectedQuest.QuestID == _questID)
            {
                selectedQuest = null;
            }
            
            Debug.Log($"퀘스트 {_questID} 삭제됨");
        }
    }

    private void ApplyQuestChanges(QuestInfo _quest)
    {
        if (quests.ContainsKey(_quest.QuestID))
        {
            quests[_quest.QuestID] = _quest;
            EditorUtility.DisplayDialog("성공", "퀘스트 변경사항이 내부적으로 적용되었습니다. 저장 버튼을 눌러 파일로 저장하세요.", "확인");
        }
    }

    private int GetNextQuestID()
    {
        int maxId = 0;
        foreach (var questPair in quests)
        {
            if (questPair.Key > maxId)
                maxId = questPair.Key;
        }
        return maxId + 1;
    }

    private void ImportJSON()
    {
        string path = EditorUtility.OpenFilePanel("JSON 파일 가져오기", dataPath, "json");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                QuestDataContainer dataContainer = JsonUtility.FromJson<QuestDataContainer>(json);

                quests.Clear();
                if (dataContainer != null && dataContainer.questList != null)
                {
                    foreach (var questInfo in dataContainer.questList)
                    {
                        if (questInfo != null && !quests.ContainsKey(questInfo.QuestID))
                        {
                            quests.Add(questInfo.QuestID, questInfo);
                        }
                    }
                }
                Debug.Log("JSON에서 퀘스트를 성공적으로 불러왔습니다.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JSON 불러오기 실패: {ex.Message}");
                EditorUtility.DisplayDialog("오류", $"JSON 파일 불러오기 실패: {ex.Message}", "확인");
            }
        }
    }

    private void ExportJSON()
    {
        string path = EditorUtility.SaveFilePanel("JSON 파일로 저장", dataPath, "quests.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                QuestDataContainer dataContainer = new QuestDataContainer();
                dataContainer.questList = new List<QuestInfo>(quests.Values);
                
                string json = JsonUtility.ToJson(dataContainer, true);
                File.WriteAllText(path, json);
                Debug.Log($"퀘스트를 JSON 파일로 성공적으로 저장했습니다: {path}");
                EditorUtility.DisplayDialog("성공", "퀘스트 데이터가 JSON 파일로 성공적으로 저장되었습니다.", "확인");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"JSON 저장 실패: {ex.Message}");
                EditorUtility.DisplayDialog("오류", $"JSON 파일 저장 실패: {ex.Message}", "확인");
            }
        }
    }
}

// QuestType enum이 Quest.cs에 정의되어 있지 않으므로, 필요시 여기에 추가하거나 int로 계속 사용합니다.
// 예시:
// public enum QuestType 
// { 
//     Main, 
//     Sub, 
//     Repeatable 
// }

/* // BasicItemInstance는 Quest.cs 나 QuestEditor와 직접적인 관련이 없어보이므로 일단 주석처리 합니다.
[System.Serializable]
public class BasicItemInstance : ItemInstance
{
    public BasicItemInstance()
    {
    }
}
*/
