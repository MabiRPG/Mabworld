using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowInventorySplitStack : Window
{
    private WindowInventoryItem itemHover;
    private IntManager quantity = new IntManager(1);
    
    private TMP_InputField quantityInput;
    private NumberRangeValidator rangeValidator;
    private TMP_Text maxQuantityText;

    private Button decreaseButton;
    private Slider slider;
    private Button increaseButton;

    private Button cancelButton;
    private Button confirmButton;
    private WindowInventory windowInventory;

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

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(delegate { SetQuantity(slider.value); });
        quantityInput.onValueChanged.AddListener(delegate {
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

    public void SetItem(WindowInventoryItem itemHover, WindowInventory windowInventory)
    {
        this.itemHover = itemHover;
        this.windowInventory = windowInventory;

        quantity.Value = itemHover.quantity / 2;
        rangeValidator.SetRange(1f, itemHover.quantity);
        maxQuantityText.text = "/ " + itemHover.quantity;
        slider.maxValue = itemHover.quantity;
        slider.minValue = 1;
        slider.wholeNumbers = true;

        gameObject.SetActive(true);
        gameObject.transform.SetAsLastSibling();
    }

    private void CreateItem(int quantity)
    {
        itemHover.SetItem(itemHover.item, itemHover.quantity - quantity);
        //WindowInventoryItem inventoryItem = windowInventory.CreateItem(itemHover.item, quantity);
        //windowInventory.SetHoverItem(inventoryItem);
        CloseWindow();
    }

    private void SetQuantity(float quantity)
    {
        if (1f <= quantity && quantity <= itemHover.quantity)
        {
            this.quantity.Value = (int)quantity;
        }
    }

    private void UpdateText()
    {
        quantityInput.text = quantity.Value.ToString();
    }

    private void UpdateSlider()
    {
        slider.value = quantity.Value;
    }
}