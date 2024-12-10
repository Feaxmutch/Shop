namespace Shop
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Item> items = new() { new("Яблоко", 10), new("Меч", 200), new("Щит", 150) };
            Seller seller = new(items, 15, 500);
            Player player = new(1000);
            Shop shop = new(seller);
            shop.Serv(player);
        }
    }

    public class Shop
    {
        private readonly Seller _seller;

        public Shop(Seller seller)
        {
            _seller = seller;
        }

        public void Serv(Player customer)
        {
            const string CommandBuy = "1";
            const string CommandExit = "2";

            bool isServing = true;

            while (isServing)
            {
                Console.Clear();
                customer.ShowMoney();
                Console.WriteLine($"{CommandBuy}) Купить предмет\n" +
                                  $"{CommandExit}) выйти из меню торговца");
                string command = Console.ReadLine();

                switch (command)
                {
                    case CommandBuy:
                        OpenBuyMenu(customer);
                        break;

                    case CommandExit:
                        isServing = false;
                        break;
                }
            }

        }

        private void OpenBuyMenu(Player customer)
        {
            Console.Clear();
            _seller.ShowItems();
            customer.ShowMoney();

            Console.WriteLine($"Введите название предмета который хотите купить");
            string itemName = Console.ReadLine();

            if (_seller.IsHaveItem(itemName))
            {
                if (TryDeal(customer, itemName))
                {
                    Console.WriteLine("Покупка успешна");
                }
                else
                {
                    Console.WriteLine("Недостаточно монет");
                }
            }
            else
            {
                Console.WriteLine($"Предмета \"{itemName}\" нет в наличии");
            }

            Console.ReadKey();
        }

        private bool TryDeal(Player customer, string itemName)
        {
            int price = _seller.GetItemPrice(itemName);
            bool canPay = customer.CanPay(price);

            if (canPay)
            {
                _seller.TakeMoney(customer.GiveMoney(price));
                customer.TakeItem(_seller.GiveItem(itemName));
            }

            return canPay;
        }
    }

    public abstract class Character
    {
        private List<Item> _items;

        public Character(int money)
        {
            _items = new();
            TakeMoney(money);
        }

        public int Money { get; protected set; }

        protected IReadOnlyList<Item> Items => _items;

        public void TakeMoney(int money)
        {
            Money += money;
        }

        public void TakeItem(Item item)
        {
            _items.Add(item);
        }

        protected void RemoveItem(Item item)
        {
            _items.Remove(item);
        }
    }

    public class Seller : Character
    {
        public Seller(List<Item> items, int count, int money) : base(money)
        {
            Initialize(items, count);
        }

        public void ShowItems()
        {
            Dictionary<string, List<Item>> itemsCounts = SortItems();

            foreach (var itemCount in itemsCounts)
            {
                Console.WriteLine($"{itemCount.Key} - {itemCount.Value.Count} ({itemCount.Value[0].Price})");
            }
        }

        public bool IsHaveItem(string name)
        {
            return GetItem(name) != null;
        }

        public int GetItemPrice(string name)
        {
            return GetItem(name).Price;
        }

        public Item GiveItem(string name)
        {
            Item givedItem = GetItem(name);
            RemoveItem(givedItem);
            return givedItem;
        }

        protected Item GetItem(string name)
        {
            Item getedItem = null;

            foreach (var item in Items)
            {
                if (item.Name == name)
                {
                    getedItem = item;
                    return getedItem;
                }
            }

            return getedItem;
        }

        private void Initialize(List<Item> items, int count)
        {
            foreach (var item in items)
            {
                for (int i = 0; i < count; i++)
                {
                    TakeItem(item.Clone);
                }
            }
        }

        private Dictionary<string, List<Item>> SortItems()
        {
            Dictionary<string, List<Item>> itemsCounts = new();

            foreach (var item in Items)
            {
                if (itemsCounts.ContainsKey(item.Name))
                {
                    itemsCounts[item.Name].Add(item);
                }
                else
                {
                    itemsCounts.Add(item.Name, new List<Item>());
                }
            }

            return itemsCounts;
        }
    }

    public class Player : Character
    {
        public Player(int money) : base(money)
        {

        }

        public bool CanPay(int price)
        {
            return Money >= price;
        }

        public int GiveMoney(int money)
        {
            Money -= money;
            return money;
        }

        public void ShowMoney()
        {
            Console.WriteLine($"Монеты: {Money}");
        }
    }

    public class Item
    {
        public Item(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; private set; }

        public int Price { get; private set; }

        public Item Clone => new Item(Name, Price);
    }
}
