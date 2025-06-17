using System;
using System.Linq;
using UnityEngine;

public class NPC : Character, IInteractable
{
    protected Animator animator;

    private Inventory inventory;
    private Player player;

    [SerializeField] public int NPCID;
    public Transform ZoomTarget;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private bool isTalking = false;
    private bool isLookToPlayer = false;
    private bool isRotateToPlayer = false;

    // 대화, 상호작용 등
    public InteractType InteractType => InteractType.NPC;

    public string ShownString { get; private set; }

    public bool IsNowInteractable { get; private set; }

    public event Action OnInteractStart;
    public event Action OnInteractEnd;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();

        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    protected virtual void Start()
    {
        player = Singleton.Player;
        inventory = Singleton.Inventory;
    }

    protected virtual void FixedUpdate()
    {
        if (isTalking)
        {
            if (isLookToPlayer)
            {
                //고개 돌리기
            }

            if (isRotateToPlayer)
            {
                var direction = (Singleton.Player.transform.position - transform.position);
                direction.y = 0;

                Quaternion targetedRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetedRotation, 30f * Time.deltaTime);
            }
            return;
        }

        if (Vector3.SqrMagnitude(transform.position - defaultPosition) >= 0.001)
        {
            transform.position = Vector3.Lerp(transform.position, defaultPosition, Time.fixedDeltaTime * 5f);
        }

        if (Math.Abs(Quaternion.Dot(transform.rotation, defaultRotation)) >= 0.001)
        {
            Quaternion targetedRotation = defaultRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetedRotation, 30f * Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        var interactManager = Singleton.Get<InteractManager>();
        if (interactManager != null)
        {
            interactManager.UnSetInteract(this);
        }
    }

    public virtual void DoAction()
    {
        // npcid를 통한 상태 확인 후
        var npc_selected = Singleton.Get<TableDataManager>().Table.NPC;
        var condition_result = Singleton.Get<TableDataManager>().Table.NPCDialogueCondition.GetConditionResult(NPCID, inventory, player);
        
        if (condition_result == 0)
            condition_result = npc_selected.Get(NPCID).DefaultDialogueID;

        Singleton.Get<DialogueManager>().StartDialogue(condition_result, this);
        Singleton.Player.TalkCount.AddTalkCount(NPCID);

        // 퀘스트 완료 체크
        var quests = Singleton.Player.QuestStateInstance.QuestStates.Values.ToList();
        foreach (var quest in quests)
        {
            var questInfo = Singleton.Get<QuestManager>().GetQuestInfo(quest.QuestID);
            if (questInfo != null && questInfo.ReceiverNPCID == this.NPCID)
            {
                if (player.QuestStateInstance.GetQuestState(quest.QuestID).State != QuestState.Achieved) continue;
                //완료는 한 대화에 하나씩만
                if (Singleton.Get<QuestManager>().CompleteQuest(quest.QuestID)) break;
            }
        }
    }

    public virtual void EndAction()
    {
        
    }

    /// <summary>
    /// 대화에 맞춰 애니메이션 동작 수행 Sync with Dialogue
    /// </summary>
    public virtual void PlayAnimation(string _animaitonName)
    {
        int hash = AnimationHash.GetHash(_animaitonName);

        animator.SetTrigger(hash);
    }

    public void SetTalking(bool _true)
    {
        isTalking = _true;

        if (!_true)
        {
            EndAction();
        }
    }

    protected virtual void LookToPlayer()
    {
        isLookToPlayer = true;
    }

    protected virtual void RotateToPlayer()
    {
        isRotateToPlayer = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        Player player = other.GetComponent<Player>();

        if (player == null) return;

        var table = Singleton.Get<TableDataManager>().Table.NPC.Get(NPCID);

        Singleton.Get<InteractIndicator>().SetShowIndicator(true, table.InteractID);
        Singleton.Get<InteractManager>().SetInteract(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        Player player = other.GetComponent<Player>();

        if (player == null) return;

        Singleton.Get<InteractIndicator>().SetShowIndicator(false);
        Singleton.Get<InteractManager>().UnSetInteract(this);
    }
} 