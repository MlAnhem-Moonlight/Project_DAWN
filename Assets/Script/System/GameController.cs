using System.Collections;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool isFightState = false;

    public GameObject night, day;
    public GameObject allyContainer;
    public GameObject player;
    public GameObject trasition;

    public GameObject losePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");


    }

    // Update is called once per frame
    void Update()
    {
        if(player.GetComponent<Stats>().currentHP <= 0)
        {
            Losing();
        }
    }

    [ContextMenu("Switch game state")]
    public void GameStateController()
    {
        isFightState = !isFightState;
        //Quản lý 2 trạng thái combat và collection
        //Chuyển đổi giữa 2 trạng thái dựa trên quyết định của người chơi và các sự kiện trong trò chơi
        //(chuyển sang combat khi tương tác với Hall (trời sáng), chuyển về collection khi hết quái (trời tối))
        StartCoroutine(wait(1.5f));

        if (isFightState)
        {
            trasition.SetActive(true);
            trasition.GetComponent<Animator>().SetInteger("SwitchState", 0); // layer 0
            StartCoroutine(wait(1f));
            //Chuyển sang trạng thái combat
            BattleManager battleManager = FindAnyObjectByType<BattleManager>();
            battleManager.StartCombatSession();
            SwitchAllyState();
            day.SetActive(false);
            night.SetActive(true);


        }
        else
        {
            //Chuyển về trạng thái collection
            trasition.SetActive(true);
            trasition.SetActive(true);
            trasition.GetComponent<Animator>().SetInteger("SwitchState", 1); // layer 0
            day.SetActive(true);
            night.SetActive(false);

        }
    }

    IEnumerator wait(float time)
    {
        yield return new WaitForSeconds(time);
    }

    private void SwitchAllyState()
    {
        // Lặp qua toàn bộ child
        foreach (Transform child in allyContainer.transform)
        {
            // Kiểm tra có component SpearBehavior hoặc ArcherBehavior
            SpearBehavior spear = child.GetComponent<SpearBehavior>();
            ArcherBehavior archer = child.GetComponent<ArcherBehavior>();

            if (spear != null)
            {
                spear.spearState = AllyState.Defensive;
            }

            if (archer != null)
            {
                archer.spearState = AllyState.Defensive;
            }
        }
    }


    public void Losing()
    {
        //Xử lý khi người chơi thua
        losePanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game

    }

    public void Winning()
    {
        //Xử lý khi người chơi thắng
    }
}
