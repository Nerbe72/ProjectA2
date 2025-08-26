using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using GameStuff;

public class Warp : InteractableObject
{
    [SerializeField] private MapConnection connection;
    [SerializeField] private Map nextMap;
    [SerializeField] private Transform destination;

    [SerializeField] private TMP_Text Name;

    private void Awake()
    {
        if (connection == SceneLoadManager.SelectedConnection)
        {
            var player = Singleton.Player;

            var playerRigidbody = player.GetComponent<Rigidbody>();
            playerRigidbody.position = destination.position;
            playerRigidbody.rotation = destination.rotation;

            player.UnlockMovementAfterWarp();
            StartCoroutine(SavePlayerDataDelayed());
        }
    }

    private IEnumerator SavePlayerDataDelayed()
    {
        yield return new WaitForSeconds(0.2f);
        Singleton.Player.SavePlayerData();
    }

    public override void DoAction()
    {
        Singleton.Get<InteractIndicatorUI>()?.SetShowIndicator(false);
        Singleton.Get<EnemyHealthIndicator>()?.gameObject.SetActive(false);

        Singleton.Player.IsMovementLocked = true;

        SceneLoadManager.SelectedConnection = connection;
        SceneLoadManager.NextScene = nextMap;
        
        InteractManager.Clear();
        SceneManager.LoadScene(1);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        ShowNameIndicator();
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        HideNameIndicator();
    }

    private void ShowNameIndicator()
    {
        if (Name == null) return;

        var localeTable = Singleton.Get<TableDataManager>().Table.Locale;
        Name.text = localeTable.Get((int)nextMap, GameManager.CurrentLocale);
    }

    private void HideNameIndicator()
    {
        if (Name == null) return;

        Name.text = string.Empty;
    }
}
