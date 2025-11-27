using UnityEngine;
using UnityEngine.EventSystems; // Thư viện bắt buộc cho cảm ứng

public class MobileJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Cài đặt UI")]
    public RectTransform Background; // Hình nền tròn to
    public RectTransform Handle;     // Hình tròn nhỏ ở giữa
    
    [HideInInspector] 
    public Vector2 InputDirection;   // Kết quả hướng đi (X, Y)

    private Vector2 _initialPos;

    void Start()
    {
        // Đảm bảo Handle nằm giữa lúc đầu
        Handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = Vector2.zero;
        
        // Tính toán vị trí ngón tay tương đối với hình nền
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            Background, 
            eventData.position, 
            eventData.pressEventCamera, 
            out position))
        {
            // Chuẩn hóa kích thước (để tính tỉ lệ từ 0 đến 1)
            position.x = (position.x / Background.sizeDelta.x) * 2;
            position.y = (position.y / Background.sizeDelta.y) * 2;

            // Lưu hướng
            InputDirection = new Vector2(position.x, position.y);
            
            // Giới hạn độ dài vector không quá 1 (để không kéo ra ngoài vòng tròn)
            if (InputDirection.magnitude > 1) 
                InputDirection = InputDirection.normalized;

            // Di chuyển hình ảnh Handle chạy theo ngón tay
            Handle.anchoredPosition = new Vector2(
                InputDirection.x * (Background.sizeDelta.x / 2),
                InputDirection.y * (Background.sizeDelta.y / 2));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // Bấm cái là nhận luôn
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Thả tay ra -> Về 0
        InputDirection = Vector2.zero;
        Handle.anchoredPosition = Vector2.zero;
    }
}