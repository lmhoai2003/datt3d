using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;

public class GameUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject StartPanel;
    public GameObject WinPanel;
    public GameObject LosePanel;
    
    // Nút xem quảng cáo để hồi sinh (nằm trong LosePanel)
    public GameObject AdReviveButton; 
    
    [Header("Game UI")]
    public GameObject HealthUI;
    public GameObject ShootButton; 
    public GameObject MoveButton;  
    public GameObject SliderHp;  

    private EntityManager _entityManager;
    private bool _isGameEnded = false;
    private bool _isGameStarted = false;
    
    // [ĐÃ XÓA] private bool _hasRevived; -> Không cần biến này nữa vì ta cho hồi sinh thoải mái

    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null) _entityManager = world.EntityManager;
        
        if(WinPanel) WinPanel.SetActive(false);
        if(LosePanel) LosePanel.SetActive(false);
        if(StartPanel) StartPanel.SetActive(true);
        
        PlayQC(); 
    }

    void Update()
    {
        if (_isGameEnded || _entityManager == default) return;
        if (!_entityManager.World.IsCreated) return;

        var query = _entityManager.CreateEntityQuery(typeof(GameStateData));
        if (query.CalculateEntityCount() > 0)
        {
            var stateData = query.GetSingleton<GameStateData>();
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

    public bool PlayQC()
    {
        if (AdsManager.Instance != null) return AdsManager.Instance.ShowInterstitial();
        return false;
    }

    public void OnStartButtonClicked()
    {
        if (PlayQC()) return; 

        if (!_entityManager.World.IsCreated) return;
        var query = _entityManager.CreateEntityQuery(typeof(GameStateData));
        if (query.CalculateEntityCount() > 0)
        {
            var entity = query.GetSingletonEntity();
            var data = query.GetSingleton<GameStateData>();
            data.CurrentState = GameState.Playing;
            _entityManager.SetComponentData(entity, data);
            
            MoveButton.SetActive(true);
            ShootButton.SetActive(true);
            SliderHp.SetActive(true);
        }
    }
    
    void GameOver(bool isWin)
    {
        _isGameEnded = true; 

        ShootButton.SetActive(false);
        MoveButton.SetActive(false);
        if (HealthUI) HealthUI.SetActive(false);

        if (isWin)
        {
            if(WinPanel) WinPanel.SetActive(true);
        }
        else
        {
            // Hiện bảng thua
            if(LosePanel) LosePanel.SetActive(true);

            // --- [SỬA ĐỔI: LUÔN HIỆN NÚT HỒI SINH] ---
            if (AdReviveButton != null)
            {
                // Luôn bật nút này lên, bất kể đã chết bao nhiêu lần
                AdReviveButton.SetActive(true);
            }
        }
    }

    // --- HÀM CHO NÚT "XEM QC" ---
    public void OnWatchAdReviveClicked()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowRewardedAd(() => 
            {
                PerformRevive();
            });
        }
    }

    void PerformRevive()
    {
        _isGameEnded = false;

        // Tắt bảng LosePanel để quay lại game
        if(LosePanel) LosePanel.SetActive(false);
        
        MoveButton.SetActive(true);
        ShootButton.SetActive(true);
        if (HealthUI) HealthUI.SetActive(true);

        // Reset trạng thái ECS về Playing
        var query = _entityManager.CreateEntityQuery(typeof(GameStateData));
        if (query.CalculateEntityCount() > 0)
        {
            var entity = query.GetSingletonEntity();
            var data = query.GetSingleton<GameStateData>();
            data.CurrentState = GameState.Playing;
            _entityManager.SetComponentData(entity, data);
        }

        // Reset Player
        var playerSync = FindFirstObjectByType<PlayerTransformSync>();
        if (playerSync != null) playerSync.Revive();
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