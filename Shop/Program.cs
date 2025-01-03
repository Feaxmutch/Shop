﻿using System.Transactions;

namespace Shop
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<Item, int> itemsCounts = new()
            {
                { new("Яблоко", 10), 20 },
                { new("Меч", 200), 7 },
                { new("Щит", 150), 10 },
            };

            Inventory inventory = new();

            foreach (var itemCount in itemsCounts)
            {
                for (int i = 0; i < itemCount.Value; i++)
                {
                    inventory.AddItem(itemCount.Key);
                }
            }

            Seller seller = new(inventory, 500);
            Player player = new(150);
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

        public void Serve(Player player)
        {
            const string CommandBuy = "1";
            const string CommandExit = "2";

            bool isServe = true;

            while (isServe)
            {
                Console.Clear();
                player.ShowMoney();
                Console.WriteLine($"{CommandBuy}) Купить предмет\n" +
                                  $"{CommandExit}) выйти из меню торговца");
                string command = Console.ReadLine();

                switch (command)
                {
                    case CommandBuy:
                        Trade(player);
                        break;

                    case CommandExit:
                        isServe = false;
                        break;
                }
            }
        }

        private void Trade(Player player)
        {
            Console.Clear();
            player.ShowMoney();
            Console.WriteLine("\nПредметы продавца:");
            _seller.ShowItems();
            Console.WriteLine("\nВаши предметы:");
            player.ShowItems();

            Console.WriteLine($"Введите название предмета который хотите купить");
            string itemName = Console.ReadLine();

            if (_seller.TryGetItem(itemName, out Item item))
            {
                if (TryDeal(player, itemName))
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

        private bool TryDeal(Player player, string itemName)
        {
            bool canPay = default;

            if (_seller.TryGetItem(itemName, out Item item))
            {
                canPay = player.CanPay(item.Price);

                if (canPay && _seller.TryGiveItem(itemName, out item))
                {
                    if (player.TryBuy(item, out int money))
                    {
                        _seller.TakeMoney(money);
                        return true;
                    }
                    else
                    {
                        _seller.TakeItem(item);
                    }
                }
            }

            return false;
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

        public Character(int money, Inventory inventory)
        {
            Inventory = inventory;
            TakeMoney(money);
        }

        public int Money { get; protected set; }

        protected Inventory Inventory { get; }

        public void ShowItems()
        {
            foreach (var cell in Inventory.Cells)
            {
                Console.WriteLine($"{cell.Item.Name} - {cell.Count} ({cell.Item.Price})");
            }
        }

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
        public Seller(Inventory inventory, int money) : base(money, inventory) { }

        public bool TryGetItem(string name, out Item item)
        {
            item = null;

            foreach (var cell in Inventory.Cells)
            {
                if (cell.Item.Name == name)
                {
                    item = cell.Item;
                    return true;
                }
            }

            return false;
        }

        public bool TryGiveItem(string name, out Item item)
        {
            if (TryGetItem(name, out item))
            {
                RemoveItem(item);
                return true;
            }

            return false;
        }

        private void RemoveItem(Item item)
        {
            Inventory.RemoveItem(item);
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

       

        public bool TryBuy(Item item, out int money)
        {
            money = 0;
            bool canPay = CanPay(item.Price);

            if (canPay)
            {
                money = item.Price;
                Money -= item.Price;
                TakeItem(item);
            }

            return canPay;
        }
    }

    public class Item : IItemData
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

    public interface IItemData
    {
        public string Name { get; }

        public int Price { get; }
    }
}
