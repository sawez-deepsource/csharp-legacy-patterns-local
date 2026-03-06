using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ============================================================
// Mixed old + modern C# — DeepSource analyzer regression testing
// ============================================================

// --- Classic class with multiple anti-patterns ---
public class UserManager
{
    // CS-R1137: could be readonly
    private string connectionString = "Server=localhost;Database=test";
    private int timeout = 30;

    private Dictionary<int, User> _cache = new Dictionary<int, User>();

    // CS-R1048: use brace initializer
    public User CreateUser(string name, string email)
    {
        var user = new User();
        user.Name = name;
        user.Email = email;
        user.CreatedAt = DateTime.Now;
        user.IsActive = true;
        return user;
    }

    // CS-R1123: dict add can be simplified
    public void PopulateCache()
    {
        _cache.Add(1, CreateUser("Alice", "alice@test.com"));
        _cache.Add(2, CreateUser("Bob", "bob@test.com"));
        _cache.Add(3, CreateUser("Charlie", "charlie@test.com"));
    }

    // CS-R1037: use range index
    public string GetDomain(string email)
    {
        int atIndex = email.IndexOf('@');
        return email.Substring(atIndex + 1);
    }

    // CS-W1100: unused variable
    public void ProcessUsers()
    {
        var unused = DateTime.Now;
        var unusedList = new List<string>();
        foreach (var kvp in _cache)
        {
            Console.WriteLine($"User: {kvp.Value.Name}");
        }
    }

    // CS-W1030: variable shadows field
    public void UpdateConnection(string connectionString)
    {
        Console.WriteLine($"Updating to: {connectionString}");
        Console.WriteLine($"Timeout: {timeout}");
    }
}

public class User
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}

// --- Multiple public classes in one file: CS-R1035 ---
public class OrderService
{
    private List<OrderItem> _items = new List<OrderItem>();

    // CS-R1123: dict add
    public Dictionary<string, decimal> GetPriceLookup()
    {
        var lookup = new Dictionary<string, decimal>();
        lookup.Add("Widget", 9.99m);
        lookup.Add("Gadget", 19.99m);
        lookup.Add("Doohickey", 4.99m);
        lookup.Add("Thingamajig", 14.99m);
        return lookup;
    }

    // CS-R1048: brace initializer
    public OrderItem CreateItem(string name, int qty)
    {
        var item = new OrderItem();
        item.Name = name;
        item.Quantity = qty;
        item.UnitPrice = 10.0m;
        return item;
    }

    public void AddItem(string name, int qty)
    {
        _items.Add(CreateItem(name, qty));
    }

    // CS-R1037: range index
    public string GetLastFour(string cardNumber)
    {
        return cardNumber.Substring(cardNumber.Length - 4);
    }

    // Clean method — no issues
    public decimal GetTotal() => _items.Sum(i => i.Total);
}

public class OrderItem
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}

// ============================================================
// CS-W1094: Lock on local variable tests
// ============================================================
public class LockPatterns
{
    private readonly object _syncRoot = new object();
    private int _counter;

    // GOOD: lock on field — should NOT trigger CS-W1094
    public void LockOnField()
    {
        lock (_syncRoot)
        {
            _counter++;
            Console.WriteLine($"Counter: {_counter}");
        }
    }

    // BAD: lock on local object — SHOULD trigger CS-W1094
    public void LockOnLocalObject()
    {
        var localLock = new object();
        lock (localLock)
        {
            Console.WriteLine("Locked on local variable — bad!");
        }
    }

    // BAD: lock on another local — SHOULD trigger CS-W1094
    public void LockOnAnotherLocal()
    {
        object temp = new object();
        lock (temp)
        {
            Console.WriteLine("Also locked on local — bad!");
        }
    }

    // BAD: lock on local assigned from parameter
    public void LockOnParam()
    {
        object lockObj = new object();
        lock (lockObj)
        {
            Console.WriteLine("Local from new — bad!");
        }
    }

    // GOOD: lock on System.Threading.Lock (C# 13+) — should NOT trigger
    public void LockOnThreadingLock()
    {
        Lock myLock = new Lock();
        lock (myLock)
        {
            Console.WriteLine("System.Threading.Lock — should be skipped");
        }
    }

    // GOOD: lock on field Lock
    private readonly Lock _fieldLock = new Lock();
    public void LockOnFieldLock()
    {
        lock (_fieldLock)
        {
            Console.WriteLine("Field Lock — correct");
        }
    }
}

// --- Async patterns ---
public class DataFetcher
{
    // CS-W1100: unused variable
    public async Task<List<string>> FetchDataAsync()
    {
        var startTime = DateTime.Now;
        var unused = "placeholder";

        await Task.Delay(100);
        return new List<string> { "data1", "data2", "data3" };
    }

    // Clean async method
    public async Task<int> CountItemsAsync()
    {
        var data = await FetchDataAsync();
        return data.Count;
    }
}

// --- String patterns ---
public class StringUtils
{
    // CS-R1037: substring vs range
    public string TrimPrefix(string input, int prefixLen)
    {
        return input.Substring(prefixLen);
    }

    public string TrimSuffix(string input, int suffixLen)
    {
        return input.Substring(0, input.Length - suffixLen);
    }

    // Clean method
    public string SafeTrim(string? input) => input?.Trim() ?? string.Empty;
}

// --- Collection patterns ---
public class CollectionPatterns
{
    // CS-R1123: dict.Add
    public Dictionary<string, List<int>> GroupNumbers()
    {
        var groups = new Dictionary<string, List<int>>();
        groups.Add("evens", new List<int> { 2, 4, 6, 8 });
        groups.Add("odds", new List<int> { 1, 3, 5, 7 });
        groups.Add("primes", new List<int> { 2, 3, 5, 7 });
        return groups;
    }

    // CS-R1048: brace initializer
    public User CreateDefaultUser()
    {
        var user = new User();
        user.Name = "Default";
        user.Email = "default@test.com";
        user.IsActive = false;
        user.Roles = new List<string> { "viewer" };
        return user;
    }

    // Clean: collection expression
    public List<int> GetFibonacci() => [1, 1, 2, 3, 5, 8, 13, 21];
}

// --- Nullable patterns ---
public class NullablePatterns
{
    private User? _currentUser;

    // CS-W1030: shadows field
    public void SetUser(User? _currentUser)
    {
        if (_currentUser != null)
        {
            Console.WriteLine($"Setting user: {_currentUser.Name}");
        }
    }

    // Clean
    public string GetUserName() => _currentUser?.Name ?? "Anonymous";

    public bool IsUserActive() => _currentUser?.IsActive ?? false;
}

// --- Exception patterns ---
public class ExceptionPatterns
{
    // CS-W1100: unused
    public void HandleError()
    {
        try
        {
            var result = int.Parse("not a number");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error: {ex.Message}";
            throw;
        }
    }
}

// --- LINQ (clean code) ---
public class Analytics
{
    public record SalesRecord(string Product, decimal Amount, DateTime Date);

    public IEnumerable<(string Product, decimal Total)> GetTopProducts(
        IEnumerable<SalesRecord> sales, int top = 5)
    {
        return sales
            .GroupBy(s => s.Product)
            .Select(g => (Product: g.Key, Total: g.Sum(s => s.Amount)))
            .OrderByDescending(x => x.Total)
            .Take(top);
    }

    public Dictionary<int, decimal> MonthlySummary(IEnumerable<SalesRecord> sales)
    {
        return sales
            .GroupBy(s => s.Date.Month)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));
    }
}

// --- Pattern matching (clean) ---
public class Classifier
{
    public string Classify(object obj) => obj switch
    {
        int i when i > 0 => "positive",
        int i when i < 0 => "negative",
        int => "zero",
        string { Length: > 10 } => "long string",
        string => "short string",
        null => "null",
        _ => "unknown"
    };
}

// --- Main ---
class Program
{
    static async Task Main()
    {
        // User management
        var manager = new UserManager();
        manager.PopulateCache();
        manager.ProcessUsers();
        manager.UpdateConnection("new-connection");
        Console.WriteLine($"Domain: {manager.GetDomain("test@example.com")}");

        // Orders
        var orderService = new OrderService();
        orderService.AddItem("Widget", 3);
        orderService.AddItem("Gadget", 2);
        Console.WriteLine($"Order total: {orderService.GetTotal()}");
        Console.WriteLine($"Last four: {orderService.GetLastFour("4111111111111234")}");

        // Lock patterns (CS-W1094 tests)
        var locks = new LockPatterns();
        locks.LockOnField();
        locks.LockOnLocalObject();
        locks.LockOnAnotherLocal();
        locks.LockOnParam();
        locks.LockOnThreadingLock();
        locks.LockOnFieldLock();

        // Async
        var fetcher = new DataFetcher();
        var count = await fetcher.CountItemsAsync();
        Console.WriteLine($"Items: {count}");

        // Strings
        var strUtils = new StringUtils();
        Console.WriteLine(strUtils.TrimPrefix("HelloWorld", 5));
        Console.WriteLine(strUtils.SafeTrim("  spaces  "));

        // Collections
        var collections = new CollectionPatterns();
        collections.GroupNumbers();
        Console.WriteLine($"Fib: {string.Join(", ", collections.GetFibonacci())}");

        // Nullable
        var nullable = new NullablePatterns();
        nullable.SetUser(null);
        Console.WriteLine($"User: {nullable.GetUserName()}");

        // Exceptions
        var exceptions = new ExceptionPatterns();
        try { exceptions.HandleError(); } catch { }

        // Clean analytics
        var analytics = new Analytics();
        var classifier = new Classifier();
        Console.WriteLine(classifier.Classify(42));
        Console.WriteLine(classifier.Classify("hello"));
    }
}
