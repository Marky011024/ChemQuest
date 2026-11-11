using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Stage 5 Levels 1, 2 and 3 share this script.
// The source sample stays anchored in the sample bank; a non-blocking drag
// preview follows the pointer outside the bank, so tokens never drift or
// overlap the bank edge and failed drops always leave the source in place.
[RequireComponent(typeof(CanvasGroup))]
public class Stage5SampleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int SampleIndex;
    [HideInInspector] public Stage5ElementCrucibleGame Game;

    private RectTransform rect;
    private RectTransform bank;
    private CanvasGroup group;
    private Canvas canvas;
    private RectTransform preview;
    private Vector2 home;
    private bool dragging;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        bank = transform.parent as RectTransform;
        home = rect.anchoredPosition;
    }

    public void ResetToken()
    {
        if (rect == null) Awake();
        dragging = false;
        ClearPreview();
        rect.anchoredPosition = home;
        group.blocksRaycasts = true;
        group.alpha = 1f;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (Game == null || !Game.CanInteract) return;
        dragging = true;

        var guide = FindFirstObjectByType<ChemQuestFirstTimeTutorial>();
        if (guide != null) guide.NotifyFirstInteraction();

        // Let the destination receive OnDrop, but keep the original sample
        // visually fixed in the source bank.
        group.blocksRaycasts = false;
        rect.anchoredPosition = home;
        CreatePreview();
        UpdatePreview(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!dragging) return;
        // Even if play is interrupted, do not move the original token.
        rect.anchoredPosition = home;
        UpdatePreview(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        ResetToken();
    }

    private void CreatePreview()
    {
        if (canvas == null) return;
        ClearPreview();
        var go = new GameObject("Stage5SampleDragPreview", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(canvas.transform, false);
        preview = go.GetComponent<RectTransform>();
        preview.anchorMin = preview.anchorMax = new Vector2(.5f, .5f);
        preview.pivot = new Vector2(.5f, .5f);
        preview.sizeDelta = rect.rect.size;

        var original = GetComponent<Image>();
        var img = go.GetComponent<Image>();
        if (original != null)
        {
            img.sprite = original.sprite;
            img.type = original.type;
            img.color = original.color;
        }
        else img.color = new Color32(65, 130, 175, 255);

        var cg = go.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = .88f;

        var originalLabel = GetComponentInChildren<TMP_Text>(true);
        if (originalLabel != null)
        {
            var labelGo = new GameObject("Caption", typeof(RectTransform),
                typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = originalLabel.text;
            label.font = originalLabel.font;
            label.fontSize = originalLabel.fontSize;
            label.color = originalLabel.color;
            label.alignment = originalLabel.alignment;
            label.raycastTarget = false;
        }
    }

    private void UpdatePreview(PointerEventData e)
    {
        if (preview == null || canvas == null) return;

        // No preview inside the sample bank: this prevents the visible
        // half-token protruding from the bank while dragging starts.
        var insideBank = bank != null &&
            RectTransformUtility.RectangleContainsScreenPoint(bank, e.position, e.pressEventCamera);
        preview.gameObject.SetActive(!insideBank);
        if (insideBank) return;

        var canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : e.pressEventCamera,
            out Vector2 position))
        {
            preview.anchoredPosition = position;
        }
    }

    private void ClearPreview()
    {
        if (preview == null) return;
        // Destroy safely in Edit Mode too (e.g. rebuilding the scene).
        if (Application.isPlaying) Destroy(preview.gameObject);
        else DestroyImmediate(preview.gameObject);
        preview = null;
    }

    private void OnDisable()
    {
        if (rect == null) return;
        ResetToken();
    }
}
