using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BalloonPop.Save;

namespace BalloonPop.UI
{
    [System.Serializable]
    public class ShopItem
    {
        public string Key;
        public string Title;
        public int Price;
        public Button BuyButton;
        public TMP_Text PriceText;
    }

    public class CoinShopPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private ShopItem hammerItem;
        [SerializeField] private ShopItem shuffleItem;
        [SerializeField] private ShopItem moveItem;

        private void OnEnable()
        {
            Wire(hammerItem, () => Buy("hammer", hammerItem.Price));
            Wire(shuffleItem, () => Buy("shuffle", shuffleItem.Price));
            Wire(moveItem, () => Buy("move", moveItem.Price));
            Refresh();
        }

        private void Wire(ShopItem item, System.Action action)
        {
            if (item.BuyButton != null)
            {
                item.BuyButton.onClick.RemoveAllListeners();
                item.BuyButton.onClick.AddListener(() => action());
            }
            if (item.PriceText != null) item.PriceText.text = item.Price.ToString();
        }

        private void Buy(string key, int price)
        {
            if (!SaveSystem.TrySpendCoins(price)) return;
            switch (key)
            {
                case "hammer":  SaveSystem.Data.Hammers++; break;
                case "shuffle": SaveSystem.Data.Shuffles++; break;
                case "move":    SaveSystem.Data.MovePacks++; break;
            }
            SaveSystem.Save();
            Refresh();
        }

        private void Refresh()
        {
            if (coinText != null) coinText.text = SaveSystem.Data.Coins.ToString("N0");
        }
    }
}
