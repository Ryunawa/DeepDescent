using _2Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Button weaponButton;
    [SerializeField] private Button armorButton;
    [SerializeField] private Button potionButton;
    [SerializeField] private Button parchmentButton;

    [SerializeField] private GameObject weaponSection;
    [SerializeField] private GameObject armorSection;
    [SerializeField] private GameObject potionSection;
    [SerializeField] private GameObject parchmentSection;

    [SerializeField] private ScrollRect scrollRect;

    private void Start()
    {
        weaponButton.onClick.AddListener(() => ShowSection(weaponSection));
        armorButton.onClick.AddListener(() => ShowSection(armorSection));
        potionButton.onClick.AddListener(() => ShowSection(potionSection));
        parchmentButton.onClick.AddListener(() => ShowSection(parchmentSection));

        // start with one section visible
        ShowSection(weaponSection);
    }

    private void ShowSection(GameObject sectionToShow)
    {
        weaponSection.SetActive(sectionToShow == weaponSection);
        armorSection.SetActive(sectionToShow == armorSection);
        potionSection.SetActive(sectionToShow == potionSection);
        parchmentSection.SetActive(sectionToShow == parchmentSection);
        scrollRect.content = sectionToShow.GetComponent<RectTransform>();
    }
}
