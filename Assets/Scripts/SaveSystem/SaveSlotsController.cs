using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSlotsSceneController : MonoBehaviour
{
    [Header("Slot Buttons")]
    public Button slot1Button;
    public Button slot2Button;
    public Button slot3Button;

    [Header("Slot Labels (TMP)")]
    public TMP_Text slot1Label;
    public TMP_Text slot2Label;
    public TMP_Text slot3Label;

    [Header("Action Panel")]
    public GameObject actionPanel;
    public Button loadButton;
    public Button deleteButton;
    public Button cancelButton;

    [Header("Back Button")]
    public Button backToMenuButton;

    private int selectedSlot = -1;

    private void Start()
    {
        actionPanel.SetActive(false);

        RefreshSlotLabels();

        slot1Button.onClick.AddListener(() => SelectSlot(1));
        slot2Button.onClick.AddListener(() => SelectSlot(2));
        slot3Button.onClick.AddListener(() => SelectSlot(3));

        loadButton.onClick.AddListener(LoadSelected);
        deleteButton.onClick.AddListener(DeleteSelected);
        cancelButton.onClick.AddListener(CancelSelection);

        backToMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("StartMenuScene");
        });
    }
    
    private void SelectSlot(int slot)
    {
        selectedSlot = slot;

        bool hasSave = SaveLoadSystem.HasSave(slot);

        loadButton.interactable = hasSave;
        deleteButton.interactable = hasSave;

        actionPanel.SetActive(true);
    }

    private void CancelSelection()
    {
        selectedSlot = -1;
        actionPanel.SetActive(false);
    }


    private void LoadSelected()
    {
        if (selectedSlot == -1)
            return;

        SaveSession.isLoadRequested = true;
        SaveSession.loadSlot = selectedSlot;

        SceneManager.LoadScene("MatchScene");
    }
    
    private void DeleteSelected()
    {
        if (selectedSlot == -1)
            return;

        SaveLoadSystem.DeleteSlot(selectedSlot);

        RefreshSlotLabels();
        CancelSelection();
    }
    
    private void RefreshSlotLabels()
    {
        slot1Label.text = SaveLoadSystem.HasSave(1) ? "Save Slot 1" : "Empty";
        slot2Label.text = SaveLoadSystem.HasSave(2) ? "Save Slot 2" : "Empty";
        slot3Label.text = SaveLoadSystem.HasSave(3) ? "Save Slot 3" : "Empty";
    }
}
