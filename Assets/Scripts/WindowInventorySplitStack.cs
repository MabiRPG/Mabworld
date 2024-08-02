using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Handles the window for splitting items in the inventory window.
/// </summary>
public class WindowInventorySplitStack : Window
{
    // Item to be split
    private WindowInventoryItem itemHover;
    // Value of the input field
    private IntManager quantity = new IntManager(1);
    
    private TMP_InputField quantityInput;
    private NumberRangeValidator rangeValidator;
    private TMP_Text maxQuantityText;

    private Button decreaseButton;
    private Slider slider;
    private Button increaseButton;

    private Button cancelButton;
    private Button confirmButton;
    private Action<int> splitItemAction;

    /// <summary>
    ///     Initializes the object.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        quantityInput = body.transform.Find("Quantity Parent").Find("Quantity Input").GetComponent<TMP_InputField>();
        rangeValidator = (NumberRangeValidator)quantityInput.inputValidator;
        maxQuantityText = body.transform.Find("Quantity Parent").Find("Max Quantity Text").GetComponent<TMP_Text>();
        decreaseButton = body.transform.Find("Slider").Find("Decrease Button").GetComponent<Button>();
        slider = body.transform.Find("Slider").GetComponent<Slider>();
        increaseButton = body.transform.Find("Slider").Find("Increase Button").GetComponent<Button>();
        cancelButton = body.transform.Find("Button Parent").Find("Cancel Button").GetComponent<Button>();
        confirmButton = body.transform.Find("Button Parent").Find("Confirm Button").GetComponent<Button>();

        gameObject.SetActive(false);
    }

    /// <summary>
    ///     Called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        // If the slider changes, change all other quantity fields
        slider.onValueChanged.AddListener(delegate { SetQuantity(slider.value); });
        // If the input field changes, change all other quantity fields
        quantityInput.onValueChanged.AddListener(delegate {
            // Parse the input and check if it is float, otherwise assign it to 1.
            if (float.TryParse(quantityInput.text, out float value))
            {
                SetQuantity(value);
            }
            else
            {
                quantityInput.text = "1";
                quantityInput.stringPosition = 1;
                quantityInput.caretPosition = 1;
                SetQuantity(1);
            }
        });
        decreaseButton.onClick.AddListener(delegate { SetQuantity(quantity.Value - 1); });
        increaseButton.onClick.AddListener(delegate { SetQuantity(quantity.Value + 1); });

        quantity.OnChange += UpdateSlider;
        quantity.OnChange += UpdateText;

        cancelButton.onClick.AddListener(CloseWindow);
        confirmButton.onClick.AddListener(delegate { CreateItem(quantity.Value); });
    }

    /// <summary>
    ///     Called when the object becomes disabled and inactive.
    /// </summary>
    private void OnDisable()
    {
        slider.onValueChanged.RemoveAllListeners();
        quantityInput.onValueChanged.RemoveAllListeners();
        decreaseButton.onClick.RemoveAllListeners();
        increaseButton.onClick.RemoveAllListeners();

        quantity.Clear();

        cancelButton.onClick.RemoveAllListeners();
        confirmButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    ///     Sets the item to be split
    /// </summary>
    /// <param name="itemHover">Item to be split.</param>
    /// <param name="splitItemAction">Action to be called when confirmed.</param>
    public void SetItem(WindowInventoryItem itemHover, Action<int> splitItemAction)
    {
        this.itemHover = itemHover;
        this.splitItemAction = splitItemAction;

        // Re-enable the object first, so the quantity event triggers properly.
        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();

        // Setting quantity here updates all fields
        quantity.Value = 1;
        rangeValidator.SetRange(1f, itemHover.quantity);
        maxQuantityText.text = "/ " + itemHover.quantity;
        slider.maxValue = itemHover.quantity;
        slider.minValue = 1;
        slider.wholeNumbers = true;
    }

    /// <summary>
    ///     Creates a new split item based on the input.
    /// </summary>
    /// <param name="quantity"></param>
    private void CreateItem(int quantity)
    {
        splitItemAction(quantity);
        CloseWindow();
    }

    /// <summary>
    ///     Sets the quantity input and reassigns all quantity fields.
    /// </summary>
    /// <param name="quantity"></param>
    private void SetQuantity(float quantity)
    {
        if (1f <= quantity && quantity <= itemHover.quantity)
        {
            this.quantity.Value = (int)quantity;
        }
    }

    /// <summary>
    ///     Updates the text fields.
    /// </summary>
    private void UpdateText()
    {
        quantityInput.text = quantity.Value.ToString();
    }

    /// <summary>
    ///     Updates the slider bars.
    /// </summary>
    private void UpdateSlider()
    {
        slider.value = quantity.Value;
    }
}