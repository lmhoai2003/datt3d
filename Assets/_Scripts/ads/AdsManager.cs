using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Header("Cài đặt")]
    public bool EnableInterstitial = true;
    public bool EnableRewarded = true; // [MỚI] Bật QC hồi sinh

    [Header("Thời gian chờ (Giây)")]
    public float MinAdInterval = 180f;
    private float _nextAdTime = 0f;

    // --- TEST ID ---
#if UNITY_ANDROID
    private string _interstitialId = "ca-app-pub-3940256099942544/1033173712";
    private string _rewardedId = "ca-app-pub-3940256099942544/5224354917"; // [MỚI] ID Test cho Reward
#else
    private string _interstitialId = "unused";
    private string _rewardedId = "unused";
#endif

    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd; // [MỚI] Biến chứa QC hồi sinh

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            if (EnableInterstitial) LoadInterstitial();
            if (EnableRewarded) LoadRewardedAd(); // [MỚI] Tải QC hồi sinh
        });
    }

    // --- 1. LOGIC QUẢNG CÁO FULL (GIỮ NGUYÊN) ---
    public void LoadInterstitial()
    {
        if (_interstitialAd != null) { _interstitialAd.Destroy(); _interstitialAd = null; }

        InterstitialAd.Load(_interstitialId, new AdRequest(), (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null) return;
            _interstitialAd = ad;
            _interstitialAd.OnAdFullScreenContentClosed += () => LoadInterstitial();
        });
    }

    public bool ShowInterstitial()
    {
        if (!EnableInterstitial) return false;
        if (Time.time < _nextAdTime) return false;

        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
            _nextAdTime = Time.time + MinAdInterval;
            return true;
        }
        else
        {
            LoadInterstitial();
            return false;
        }
    }

    // --- 2. LOGIC QUẢNG CÁO HỒI SINH (MỚI TINH) ---
    public void LoadRewardedAd()
    {
        if (_rewardedAd != null) { _rewardedAd.Destroy(); _rewardedAd = null; }

        RewardedAd.Load(_rewardedId, new AdRequest(), (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Lỗi tải Reward: " + error);
                return;
            }

            _rewardedAd = ad;
            Debug.Log("Reward Ad đã tải xong!");

            // Khi đóng thì tải cái mới
            _rewardedAd.OnAdFullScreenContentClosed += () => LoadRewardedAd();
        });
    }

    // Hàm gọi QC hồi sinh (Nhận vào một hành động 'onReward' để chạy sau khi xem xong)
    public void ShowRewardedAd(Action onReward)
    {
        if (!EnableRewarded) return;

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                // Người chơi đã xem hết -> Thực hiện hồi sinh
                Debug.Log("Đã xem xong QC! Hồi sinh ngay.");
                onReward.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Reward Ad chưa sẵn sàng.");
            LoadRewardedAd();
        }
    }
}