using TMPro;
using UnityEngine;

namespace InventorySystem.Scripts.UI
{
    public sealed class WeightView : MonoBehaviour
    {
        [SerializeField] private InventoryController controller;
        [SerializeField] private TMP_Text weightText;
        [SerializeField] private string prefix = "Weight: ";
        [SerializeField] private string suffix = "";
        [SerializeField] private int decimals = 1;

        private void OnEnable()
        {
            controller.Model.InventoryChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            controller.Model.InventoryChanged -= Refresh;
        }

        private void OnValidate()
        {
            Debug.Assert(controller != null, $"[{nameof(WeightView)}] '{nameof(controller)}' is not assigned.", this);
            Debug.Assert(weightText != null, $"[{nameof(WeightView)}] '{nameof(weightText)}' is not assigned.", this);
            decimals = Mathf.Clamp(decimals, 0, 6);
        }

        private void Refresh()
        {
            float total = controller.Model.GetTotalWeight();
            decimals = Mathf.Clamp(decimals, 0, 6);
            string formatted = total.ToString($"F{decimals}");
            weightText.text = $"{prefix}{formatted}{suffix}";
        }
    }
}