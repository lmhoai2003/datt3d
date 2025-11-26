using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject StartPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    public GameObject HealthUI;

    private EntityManager _entityManager;
    private bool _isGameEnded = false;
    private bool _isGameStarted = false; 

    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null) _entityManager = world.EntityManager;
        
        if(WinPanel) WinPanel.SetActive(false);
        if(LosePanel) LosePanel.SetActive(false);
        if(StartPanel) StartPanel.SetActive(true);
    }

    void Update()
    {
        if (_isGameEnded || _entityManager == default) return;
        if (!_entityManager.World.IsCreated) return;

        var query = _entityManager.CreateEntityQuery(typeof(GameStateData));
        if (query.CalculateEntityCount() > 0)
        {
            var stateData = query.GetSingleton<GameStateData>();

            // Tự động tắt bảng Start khi trạng thái chuyển sang Playing
            if (!_isGameStarted && stateData.CurrentState == GameState.Playing)
            {
                _isGameStarted = true;
                if(StartPanel) StartPanel.SetActive(false);
                if(HealthUI) HealthUI.SetActive(true); 
            }

            if (stateData.CurrentState == GameState.Won) GameOver(true);
            else if (stateData.CurrentState == GameState.Lost) GameOver(false);
        }
    }

    // --- [MỚI] HÀM GẮN VÀO NÚT START ---
    public void OnStartButtonClicked()
    {
        if (!_entityManager.World.IsCreated) return;

        // Tìm component GameStateData và đổi nó thành Playing
        var query = _entityManager.CreateEntityQuery(typeof(GameStateData));
        if (query.CalculateEntityCount() > 0)
        {
            var entity = query.GetSingletonEntity();
            var data = query.GetSingleton<GameStateData>();
            
            data.CurrentState = GameState.Playing;
            
            _entityManager.SetComponentData(entity, data);
            
            Debug.Log("BUTTON CLICKED: Game Started!");
        }
    }
    // -----------------------------------

    void GameOver(bool isWin)
    {
        _isGameEnded = true;
        if (isWin) { if(WinPanel) WinPanel.SetActive(true); }
        else { if(LosePanel) LosePanel.SetActive(true); }

        if (HealthUI) HealthUI.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld != null)
        {
            defaultWorld.EntityManager.CompleteAllTrackedJobs();
            defaultWorld.Dispose();
        }
        DefaultWorldInitialization.Initialize("Default World", false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}