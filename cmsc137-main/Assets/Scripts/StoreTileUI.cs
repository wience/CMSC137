using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoreTileUI : MonoBehaviour,IPointerClickHandler
{
    public event Action<StoreTileUI> Clicked;
    
    [SerializeField] private Text _valueTxt;
    [SerializeField] private Text _priceTxt;
    private HintProduct _hintProduct;

    public HintProduct HintProduct
    {
        get { return _hintProduct; }
        set
        {
            _valueTxt.text = (value.isUnlimited ? "Unlimited" : value.value.ToString()) + " Hints";
            
            _hintProduct = value;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke(this);
    }
}