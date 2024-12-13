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
            shop.Serve(player);
        }
    }

    public class Shop
    {
        private readonly Seller _seller;

        public Shop(Seller seller)
        {
            _seller = seller;
        }

        public void Serve(Player customer)
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
                        Trading(customer);
                        break;

                    case CommandExit:
                        isServing = false;
                        break;
                }
            }
        }

        private void Trading(Player customer)
        {
            Console.Clear();
            _seller.ShowItems();
            customer.ShowMoney();

            Console.WriteLine($"Введите название предмета который хотите купить");
            string itemName = Console.ReadLine();

            if (_seller.TryGetItem(itemName, out Item item))
            {
                if (TryDeal(customer, item))
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

        private bool TryDeal(Player customer, Item item)
        {
            bool canPay = customer.CanPay(item.Price);

            if (canPay)
            {
                _seller.TakeMoney(customer.GiveMoney(item.Price));
                customer.TakeItem(_seller.GiveItem(item));
            }

            return canPay;
        }
    }

    public class Inventory
    {
        private List<Cell> _cells = new();

        public IReadOnlyList<IReadOnlyCell> Cells => _cells;

        public void AddItem(Item item)
        {
            if (TryGetCell(item, out Cell cell) == false)
            {
                cell = new Cell(item);
                _cells.Add(cell);
            }

            cell.Add();
        }

        public void RemoveItem(Item item)
        {
            if (TryGetCell(item, out Cell cell))
            {
                cell.Substract();

                if (cell.Count == 0)
                {
                    _cells.Remove(cell);
                }
            }
            else
            {
                throw new ArgumentException("item was not found", nameof(item));
            }
        }

        private bool TryGetCell(Item item, out Cell returnedCell)
        {
            foreach (var cell in _cells)
            {
                if (cell.Item == item)
                {
                    returnedCell = cell;
                    return true;
                }
            }

            returnedCell = null;
            return false;
        }
    }

    public class Cell : IReadOnlyCell
    {
        public Cell(Item item)
        {
            Item = item;
        }

        public Item Item { get; }

        public int Count { get; private set; }

        public void Add()
        {
            Count++;
        }

        public void Substract()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Count);
            Count--;
        }
    }

    public interface IReadOnlyCell
    {
        public Item Item { get; }

        public int Count { get; }
    }

    public abstract class Character
    {
        public Character(int money)
        {
            Inventory = new();
            TakeMoney(money);
        }

        public int Money { get; protected set; }

        protected Inventory Inventory { get; }

        public void TakeMoney(int money)
        {
            Money += money;
        }

        public void TakeItem(Item item)
        {
            Inventory.AddItem(item);
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
            foreach (var cell in Inventory.Cells)
            {
                Console.WriteLine($"{cell.Item.Name} - {cell.Count} ({cell.Item.Price})");
            }
        }

        public bool TryGetItem(string name, out Item item)
        {
            item = GetItem(name);
            return (object)item != null;
        }

        public Item GiveItem(Item item)
        {
            RemoveItem(item);
            return item;
        }

        private Item GetItem(string name)
        {
            Item getedItem = null;

            foreach (var cell in Inventory.Cells)
            {
                if (cell.Item.Name == name)
                {
                    getedItem = cell.Item;
                    return getedItem;
                }
            }

            return getedItem;
        }

        private void RemoveItem(Item item)
        {
            Inventory.RemoveItem(item);
        }

        private void Initialize(List<Item> items, int count)
        {
            foreach (var item in items)
            {
                for (int i = 0; i < count; i++)
                {
                    TakeItem(item.Clone());
                }
            }
        }
    }

    public class Player : Character
    {
        public Player(int money) : base(money) { }

        public bool CanPay(int price) => Money >= price;

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

        public static bool operator ==(Item item1, Item item2)
        {
            bool equalsName = item1.Name == item2.Name;
            bool equalsPrice = item1.Price == item2.Price;
            return equalsName && equalsPrice;
        }

        public static bool operator !=(Item item1, Item item2)
        {
            return item1 == item2 == false;
        }

        public Item Clone()
        {
            return new Item(Name, Price);
        }
    }
}
