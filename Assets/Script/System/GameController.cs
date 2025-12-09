using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool isFightState = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameStateController()
    {
        //Quản lý 2 trạng thái combat và collection
        //Chuyển đổi giữa 2 trạng thái dựa trên quyết định của người chơi và các sự kiện trong trò chơi
        //(chuyển sang combat khi tương tác với Hall (trời sáng), chuyển về collection khi hết quái (trời tối))
    }

    public void Losing()
    {
        //Xử lý khi người chơi thua
    }

    public void Winning()
    {
        //Xử lý khi người chơi thắng
    }
}
