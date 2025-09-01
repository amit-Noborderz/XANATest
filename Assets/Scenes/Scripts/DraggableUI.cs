using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private Button dragButton; // Assign the drag button in inspector
    public bool constrainToScreen = true;
    public bool smoothDrag = false;
    public float smoothSpeed = 10f;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private bool isDragging = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        // If no CanvasGroup exists, add one for transparency control during drag
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Store original position
        originalPosition = rectTransform.anchoredPosition;
        targetPosition = originalPosition;

        // If drag button is not assigned, try to find it by name
        if (dragButton == null)
        {
            Transform dragButtonTransform = transform.Find("DragButton");
            if (dragButtonTransform != null)
            {
                dragButton = dragButtonTransform.GetComponent<Button>();
            }
        }

        // Add the draggable component to the drag button if it exists
        if (dragButton != null)
        {
            AddDragToButton();
        }
        else
        {
            Debug.LogWarning("Drag button not assigned! Please assign a button in the inspector or create a child button named 'DragButton'");
        }
    }

    private void AddDragToButton()
    {
        // Add or get DragHandler component on the drag button
        DragHandler dragHandler = dragButton.GetComponent<DragHandler>();
        if (dragHandler == null)
        {
            dragHandler = dragButton.gameObject.AddComponent<DragHandler>();
        }

        // Set reference to this draggable UI
        dragHandler.draggableUI = this;
    }

    void Update()
    {
        // Smooth dragging movement
        if (smoothDrag && isDragging && Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.1f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // Make panel slightly transparent while dragging
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.8f;
        }

        // Disable raycast blocking while dragging (optional)
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;

        // Convert screen point to canvas position
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out position))
        {
            if (smoothDrag)
            {
                targetPosition = position;
            }
            else
            {
                rectTransform.anchoredPosition = position;
            }

            // Constrain to screen boundaries if enabled
            if (constrainToScreen)
            {
                ConstrainToScreen();
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Restore original transparency
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // Re-enable raycast blocking
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        // Final constraint check
        if (constrainToScreen)
        {
            ConstrainToScreen();
        }
    }

    private void ConstrainToScreen()
    {
        Vector3[] canvasCorners = new Vector3[4];
        canvas.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);

        Vector3[] panelCorners = new Vector3[4];
        rectTransform.GetWorldCorners(panelCorners);

        Vector2 currentPos = smoothDrag ? targetPosition : rectTransform.anchoredPosition;

        // Get canvas bounds
        float canvasWidth = canvasCorners[2].x - canvasCorners[0].x;
        float canvasHeight = canvasCorners[2].y - canvasCorners[0].y;

        // Get panel size
        float panelWidth = rectTransform.rect.width * canvas.scaleFactor;
        float panelHeight = rectTransform.rect.height * canvas.scaleFactor;

        // Calculate constraints
        float maxX = (canvasWidth - panelWidth) / (2 * canvas.scaleFactor);
        float minX = -maxX;
        float maxY = (canvasHeight - panelHeight) / (2 * canvas.scaleFactor);
        float minY = -maxY;

        // Apply constraints
        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
        currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

        if (smoothDrag)
        {
            targetPosition = currentPos;
        }
        else
        {
            rectTransform.anchoredPosition = currentPos;
        }
    }

    // Public method to reset panel to original position
    public void ResetPosition()
    {
        if (smoothDrag)
        {
            targetPosition = originalPosition;
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    // Public method to set panel position programmatically
    public void SetPosition(Vector2 newPosition)
    {
        if (smoothDrag)
        {
            targetPosition = newPosition;
        }
        else
        {
            rectTransform.anchoredPosition = newPosition;
        }

        if (constrainToScreen)
        {
            ConstrainToScreen();
        }
    }
}

// Separate component that handles drag events for the button
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public DraggableUI draggableUI;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (draggableUI != null)
        {
            draggableUI.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggableUI != null)
        {
            draggableUI.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggableUI != null)
        {
            draggableUI.OnEndDrag(eventData);
        }
    }
}