using UnityEngine;
using UnityEngine.SceneManagement; // Để load lại màn chơi
using Unity.Entities;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject WinPanel;
    public GameObject LosePanel;
    public GameObject HealthUI; 
    private EntityManager _entityManager;
    private bool _isGameEnded = false;

    void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if(WinPanel) WinPanel.SetActive(false);
        if(LosePanel) LosePanel.SetActive(false);
    }

    void Update()
    {
        if (_isGameEnded) return;

        var query = _entityManager.CreateEntityQuery(typeof(GameStateData));
        if (query.CalculateEntityCount() > 0)
        {
            var stateData = query.GetSingleton<GameStateData>();

            if (stateData.CurrentState == GameState.Won)
            {
                GameOver(true);
            }
            else if (stateData.CurrentState == GameState.Lost)
            {
                GameOver(false);
            }
        }
    }

    void GameOver(bool isWin)
    {
        _isGameEnded = true;
        if (isWin)
        {
            if(WinPanel) WinPanel.SetActive(true);
            Debug.Log("YOU WIN!");
        }
        else
        {
            if(LosePanel) LosePanel.SetActive(true);
            Debug.Log("YOU LOSE!");
        }

        if (HealthUI) HealthUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

   public void RestartGame()
    {
        // 1. Phá hủy thế giới cũ (để xóa hết Entity rác)
        World.DisposeAllWorlds(); 
        
        // 2. Tạo thế giới mới sạch sẽ
        DefaultWorldInitialization.Initialize("Default World", false);
        
        // 3. Load lại Scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}