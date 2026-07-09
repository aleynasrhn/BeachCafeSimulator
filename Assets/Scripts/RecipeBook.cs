using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeBook : MonoBehaviour
{
    [Header("UI")]
    public GameObject recipeBookPanel;

    public TMP_Text coffeeTitle;
    public Image coffeeImage;
    public TMP_Text recipeSteps;
    public TMP_Text priceText;

    [Header("Buttons")]
    public Button nextButton;
    public Button prevButton;

    [Header("Recipes")]
    public Recipe[] recipes;

    private int currentRecipe = 0;

    [Header("Player")]
    public PlayerMovement playerMovement;

    private bool bookOpen = false;

    void Start()
    {
        recipeBookPanel.SetActive(false);

        ShowRecipe();

        nextButton.onClick.AddListener(NextRecipe);
        prevButton.onClick.AddListener(PreviousRecipe);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBook();
        }
    }

    void ToggleBook()
    {
        bookOpen = !bookOpen;

        recipeBookPanel.SetActive(bookOpen);

        playerMovement.canMove = !bookOpen;

        Cursor.visible = bookOpen;

        Cursor.lockState = bookOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    void ShowRecipe()
    {
        if (recipes.Length == 0)
            return;

        coffeeTitle.text = recipes[currentRecipe].coffeeName;
        coffeeImage.sprite = recipes[currentRecipe].coffeeImage;
        recipeSteps.text = recipes[currentRecipe].recipeSteps;
        priceText.text = "Fiyat : " + recipes[currentRecipe].price + " TL";
    }

    public void NextRecipe()
    {
        currentRecipe++;

        if (currentRecipe >= recipes.Length)
        {
            currentRecipe = 0;
        }

        ShowRecipe();
    }

    public void PreviousRecipe()
    {
        currentRecipe--;

        if (currentRecipe < 0)
        {
            currentRecipe = recipes.Length - 1;
        }

        ShowRecipe();
    }
}