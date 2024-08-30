using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InfoPanelController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform sideMenuRectTransform;
    private float screenWidth;
    private float startPositionX;
    private float startingAnchoredPositionX;

    public enum Side { left, right }
    public Side side;

    private bool isMenuOpen = false; // Track the menu state

    // Start is called before the first frame update
    void Start()
    {
        screenWidth = Screen.width;

        // Initially place the panel off-screen based on the side
        if (side == Side.right)
            sideMenuRectTransform.anchoredPosition = new Vector2(screenWidth, sideMenuRectTransform.anchoredPosition.y); // Off-screen to the right
        else
            sideMenuRectTransform.anchoredPosition = new Vector2(-sideMenuRectTransform.rect.width, sideMenuRectTransform.anchoredPosition.y); // Off-screen to the left

        // Log the initial position
        Debug.Log($"Start: Initial panel position: {sideMenuRectTransform.anchoredPosition}");

        // Set the panel to inactive initially so it's not visible
        sideMenuRectTransform.gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMenuOpen) // Only allow dragging if the menu is open
        {
            sideMenuRectTransform.anchoredPosition = new Vector2(
                Mathf.Clamp(startingAnchoredPositionX - (startPositionX - eventData.position.x), GetMinPosition(), GetMaxPosition()),
                sideMenuRectTransform.anchoredPosition.y);

            // Log the position during dragging
            Debug.Log($"OnDrag: Panel position during drag: {sideMenuRectTransform.anchoredPosition}");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isMenuOpen) // Only respond to touch if the menu is open
        {
            StopAllCoroutines();
            startPositionX = eventData.position.x;
            startingAnchoredPositionX = sideMenuRectTransform.anchoredPosition.x;

            // Log the position when dragging starts
            Debug.Log($"OnPointerDown: Starting drag. Panel position: {sideMenuRectTransform.anchoredPosition}");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isMenuOpen) // Only respond to touch if the menu is open
        {
            // Determine whether to keep the panel open or close it
            bool shouldOpen = isAfterHalfPoint();
            StartCoroutine(HandleMenuSlide(.25f, sideMenuRectTransform.anchoredPosition.x, shouldOpen ? GetMinPosition() : GetMaxPosition()));

            // Log the decision on whether to open or close the panel
            Debug.Log($"OnPointerUp: Should open panel: {shouldOpen}. Panel position before slide: {sideMenuRectTransform.anchoredPosition}");

            isMenuOpen = shouldOpen; // Update the menu state based on drag position
        }
    }

    private bool isAfterHalfPoint()
    {
        // Adjust the logic here to determine if the panel should stay open or close based on its position
        if (side == Side.right)
        {
            // If the panel's position is more than halfway off-screen to the right, it should close
            Debug.Log($"In isAfterHalf {sideMenuRectTransform.anchoredPosition.x < screenWidth - sideMenuRectTransform.rect.width}");

            return sideMenuRectTransform.anchoredPosition.x < screenWidth - sideMenuRectTransform.rect.width;
        }
        else
        {
            // If the panel's position is more than halfway off-screen to the left, it should close
            return sideMenuRectTransform.anchoredPosition.x > -sideMenuRectTransform.rect.width;
        }
    }

    private float GetMinPosition()
    {
        if (side == Side.right)
        {
            return (screenWidth - sideMenuRectTransform.rect.width) / 70; // Position to fully show the panel on the right
        }
        return 0; // Fully visible position for the left side
    }

    private float GetMaxPosition()
    {
        if (side == Side.right)
        {
            Debug.Log($"MaxPosition is {screenWidth}");

            return screenWidth; // Fully off-screen to the right
        }
        return -sideMenuRectTransform.rect.width; // Fully off-screen to the left
    }

    private IEnumerator HandleMenuSlide(float slideTime, float startingX, float targetX, System.Action onComplete = null)
    {
        for (float i = 0; i <= slideTime; i += .025f)
        {
            sideMenuRectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startingX, targetX, i / slideTime), sideMenuRectTransform.anchoredPosition.y);
            yield return new WaitForSecondsRealtime(.025f);
        }

        // Ensure the panel is set to the target position at the end
        sideMenuRectTransform.anchoredPosition = new Vector2(targetX, sideMenuRectTransform.anchoredPosition.y);

        // Log the final position after the slide
        Debug.Log($"HandleMenuSlide: Final panel position: {sideMenuRectTransform.anchoredPosition}");

        // Invoke the onComplete callback if provided
        onComplete?.Invoke();
    }

    // Method to be called by the InfoButton
    public void ToggleMenu()
    {
        // Activate the panel only when opening
        if (!isMenuOpen)
        {
            sideMenuRectTransform.gameObject.SetActive(true);
        }

        if (isMenuOpen)
        {
            StartCoroutine(HandleMenuSlide(.25f, sideMenuRectTransform.anchoredPosition.x, GetMaxPosition(), () =>
            {
                // Deactivate the panel when it's fully closed
                sideMenuRectTransform.gameObject.SetActive(false);

                // Log when the panel is closed and deactivated
                Debug.Log($"ToggleMenu: Panel closed and deactivated. Final position: {sideMenuRectTransform.anchoredPosition}");
            }));
        }
        else
        {
            StartCoroutine(HandleMenuSlide(.25f, sideMenuRectTransform.anchoredPosition.x, GetMinPosition(), () =>
            {
                // Log when the panel is fully opened
                Debug.Log($"ToggleMenu: Panel opened. Final position: {sideMenuRectTransform.anchoredPosition}");
            }));
        }
        isMenuOpen = !isMenuOpen;

        // Log the toggle state
        Debug.Log($"ToggleMenu: isMenuOpen is now: {isMenuOpen}");
    }
}
