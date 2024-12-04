namespace Shop
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Item> items = new() { new("Яблоко", 10), new("Меч", 200), new("Щит", 150) };
            Seller seller = new(items, 15, 500);
            Player player = new(1000);
            seller.Serv(player);
        }
    }

    public abstract class Character
    {
        private List<Item> _items;
        private int _money;

        public Character(int money)
        {
            _items = new();
            TakeMoney(money);
        }

        public int Money { get => _money; }

        protected IReadOnlyList<Item> Items => _items;

        public void TakeMoney(int money)
        {
            _money += money;
        }

        public void TakeItem(Item item)
        {
            _items.Add(item);
        }

        protected bool TryGiveItem(string name, out Item givedItem)
        {
            foreach (var item in _items)
            {
                if (item.Name == name)
                {
                    givedItem = item;
                    _items.Remove(item);
                    return true;
                }
            }

            givedItem = null;
            return false;
        }

        protected int GiveMoney(int money)
        {
            _money -= money;
            return money;
        }
    }

    public class Seller : Character
    {
        private const string CommandBuy = "1";
        private const string CommandExit = "2";

        private Player _customer;
        private bool _isServing;

        public Seller(List<Item> items, int count, int money) : base(money)
        {
            foreach (var item in items)
            {
                for (int i = 0; i < count; i++)
                {
                    TakeItem(item.Clone() as Item);
                }
            }
        }

        public void Serv(Player customer)
        {
            _customer = customer;
            _isServing = true;

            while (_isServing)
            {
                Console.Clear();
                _customer.ShowMoney();
                Console.WriteLine($"{CommandBuy}) Купить предмет\n" +
                                  $"{CommandExit}) выйти из меню торговца");
                string command = Console.ReadLine();

                switch (command)
                {
                    case CommandBuy:
                        Deal();
                        break;

                    case CommandExit:
                        _isServing = false;
                        break;
                }
            }

            _isServing = false;
        }

        private void Deal()
        {
            Console.Clear();
            ShowItems();
            _customer.ShowMoney();

            Console.WriteLine($"Введите название предмета который хотите купить");
            string name = Console.ReadLine();

            if (TryGiveItem(name, out Item item))
            {
                if (_customer.TryPay(item.Price, out int money))
                {
                    TakeMoney(money);
                    _customer.TakeItem(item);
                    Console.WriteLine("Покупка успешна");
                }
                else
                {
                    TakeItem(item);
                }
            }
            else
            {
                Console.WriteLine($"Предмета \"{name}\" нет в наличии");
            }

            Console.ReadKey();
        }

        private void ShowItems()
        {
            Dictionary<string, List<Item>> itemsCounts = SortItems();

            foreach (var itemCount in itemsCounts)
            {
                Console.WriteLine($"{itemCount.Key} - {itemCount.Value.Count} ({itemCount.Value[0].Price})");
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

        public bool TryPay(int price, out int givedMovey)
        {
            if (Money >= price)
            {
                givedMovey = GiveMoney(price);
                return true;
            }
            else
            {
                Console.WriteLine("Недостаточно монет");
                givedMovey = 0;
                return false;
            }
        }

        public void ShowMoney()
        {
            Console.WriteLine($"Монеты: {Money}");
        }
    }

    public class Item : ICloneable
    {
        public Item(string name, int price)
        {
            Name = name;
            Price = price;
        }

        public string Name { get; private set; }

        public int Price { get; private set; }

        public object Clone()
        {
            return new Item(Name, Price);
        }
    }
}
