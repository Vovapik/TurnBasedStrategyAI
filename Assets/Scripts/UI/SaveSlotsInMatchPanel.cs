using UnityEngine;
using UnityEngine.UI;

public class SaveSlotsInMatchPanel : MonoBehaviour
{
    public Button slot1Button;
    public Button slot2Button;
    public Button slot3Button;
    public Button cancelButton;

    private GameController controller;

    private void Start()
    {
        controller = FindObjectOfType<GameController>();

        slot1Button.onClick.AddListener(() => SaveToSlot(1));
        slot2Button.onClick.AddListener(() => SaveToSlot(2));
        slot3Button.onClick.AddListener(() => SaveToSlot(3));
        cancelButton.onClick.AddListener(ClosePanel);
    }

    private void SaveToSlot(int slot)
    {
        if (controller == null)
            return;

        controller.SaveCurrentGame(slot);

        ClosePanel();
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}