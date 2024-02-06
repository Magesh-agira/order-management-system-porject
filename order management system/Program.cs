using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace order_management_system
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Available_nos { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime DeliveryDate => CalculateDeliveryDate();

        private DateTime CalculateDeliveryDate()
        {
            int daysToAdd = 2;
            DateTime estimatedDeliveryDate = OrderDate.AddDays(daysToAdd);

            // Check if the estimated delivery date falls on Saturday or Sunday
            if (estimatedDeliveryDate.DayOfWeek == DayOfWeek.Saturday)
            {
                estimatedDeliveryDate = estimatedDeliveryDate.AddDays(2); // Move to Monday
            }
            else if (estimatedDeliveryDate.DayOfWeek == DayOfWeek.Sunday)
            {
                estimatedDeliveryDate = estimatedDeliveryDate.AddDays(1); // Move to Monday
            }

            return estimatedDeliveryDate;
        }

        public decimal TotalAmount => OrderItems.Sum(item => item.Product.Price * item.Quantity);
    }


    public class OrderService
    {
        private List<Order> _orders = new List<Order>();
        private List<Product> _products = new List<Product>();
        private int _lastOrderId = 0;

        public void AddProduct(Product product)
        {
            _products.Add(product);
        }

        public void PlaceOrder(Order order)
        {
            order.OrderDate = DateTime.Now;
            foreach (var item in order.OrderItems)
            {
                // Check if the requested quantity is available
                if (item.Quantity > item.Product.Available_nos)
                {
                    Console.WriteLine($"Insufficient quantity available for {item.Product.Name}. Available: {item.Product.Available_nos}");
                    return;
                }

                // Reduce the available quantity
                item.Product.Available_nos -= item.Quantity;
            }

            order.OrderId = ++_lastOrderId;
            _orders.Add(order);
            Console.WriteLine($"Order placed successfully! Order ID: {order.OrderId}, Order Date: {order.OrderDate}, Estimated Delivery Date: {order.DeliveryDate}");
        }

        public List<Order> GetOrders()
        {
            return _orders;
        }

        public List<Product> GetProducts()
        {
            return _products;
        }  //h
    }

    class Program
    {
        static void Main()
        {
            OrderService orderService = new OrderService();

            // Adding sample products with available quantity
            orderService.AddProduct(new Product { ProductId = 1, Name = "Laptop", Price = 999, Available_nos = 5 });
            orderService.AddProduct(new Product { ProductId = 2, Name = "Phone", Price = 499, Available_nos = 10 });

            while (true)
            {
                Console.WriteLine("1. Place Order");
                Console.WriteLine("2. View Orders");
                Console.WriteLine("3. Admin - Add Product");
                Console.WriteLine("4. Exit");
                Console.Write("Choose an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        PlaceOrder(orderService);
                        break;
                    case "2":
                        ViewOrders(orderService);
                        break;
                    case "3":
                        if (IsAdminSection())
                        {
                            AdminAddProduct(orderService);
                        }
                        else
                        {
                            Console.WriteLine("Invalid option. Please try again.");
                        }
                        break;
                    case "4":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        static bool IsAdminSection()
        {
            Console.Write("Enter admin code: "); // You can replace this with any condition for identifying admin access.
            string adminCode = Console.ReadLine();
            return adminCode == "admin123"; // Replace with your actual admin code or condition.
        }

        static void AdminAddProduct(OrderService orderService)
        {
            Console.WriteLine("Admin - Add Product");
            Console.Write("Enter Product Name: ");
            string productName = Console.ReadLine();

            Console.Write("Enter Product Price: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal productPrice))
            {
                Console.Write("Enter Available Quantity: ");
                if (int.TryParse(Console.ReadLine(), out int availableQuantity))
                {
                    Product newProduct = new Product
                    {
                        ProductId = orderService.GetProducts().Count + 1,
                        Name = productName,
                        Price = productPrice,
                        Available_nos = availableQuantity
                    };

                    orderService.AddProduct(newProduct);
                    Console.WriteLine("Product added successfully!");
                }
                else
                {
                    Console.WriteLine("Invalid quantity. Please enter a valid number.");
                }
            }
            else
            {
                Console.WriteLine("Invalid price. Please enter a valid number.");
            }
        }
        static void PlaceOrder(OrderService orderService)
        {
            Console.WriteLine("Available Products:");
            List<Product> products = orderService.GetProducts();
            DisplayProducts(products);

            Order order = new Order();
            bool orderPlaced = false;

            while (!orderPlaced)
            {
                Console.Write("Enter Product ID (or 0 to finish): ");
                if (int.TryParse(Console.ReadLine(), out int productId))
                {
                    if (productId == 0)
                    {
                        break;
                    }

                    Product selectedProduct = products.Find(p => p.ProductId == productId);
                    if (selectedProduct != null)
                    {
                        Console.WriteLine($"Available quantity for {selectedProduct.Name}: {selectedProduct.Available_nos}");
                        Console.Write("Enter Quantity: ");
                        if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0 && quantity <= selectedProduct.Available_nos)
                        {
                            OrderItem orderItem = new OrderItem
                            {
                                Product = selectedProduct,
                                Quantity = quantity
                            };

                            order.OrderItems.Add(orderItem);
                        }
                        else
                        {
                            Console.WriteLine("Invalid quantity. Please enter a positive integer within the available quantity.");

                            // Log error to a text file
                            LogError($"Invalid quantity entered for product {selectedProduct.Name} on {DateTime.Now.ToString("yyyy-MM-dd")}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid product ID. Please enter a valid ID.");

                        // Log error to a text file
                        LogError($"Invalid product ID entered on {DateTime.Now.ToString("yyyy-MM-dd")}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");

                    // Log error to a text file
                    LogError($"Invalid input entered on {DateTime.Now.ToString("yyyy-MM-dd")}");
                }

                // Check if the order is ready to be placed
                if (order.OrderItems.Count > 0)
                {
                    orderService.PlaceOrder(order);
                    Console.WriteLine("Order placed successfully!");
                    orderPlaced = true;
                }
            }
        }

        static void LogError(string error)
        {
            string logFileName = $"ErrorLog_{DateTime.Now.ToString("yyyy-MM-dd")}.txt";
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                string logFilePath = Path.Combine(projectDirectory, logFileName);

                using (StreamWriter sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging: {ex.Message}");
            }
        }




        static void ViewOrders(OrderService orderService)
        {
            List<Order> orders = orderService.GetOrders();
            Console.WriteLine("Orders:");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("| {0, -10} | {1, -20} | {2, -20} | {3, -15} |", "Order ID", "Order Date", "Estimated Delivery", "Total Amount");
            Console.WriteLine(new string('-', 80));

            foreach (var order in orders)
            {
                Console.WriteLine("| {0, -10} | {1, -20} | {2, -20} | {3, -15} |", order.OrderId, order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"), order.DeliveryDate.ToString("yyyy-MM-dd"), order.TotalAmount.ToString("C"));

                Console.WriteLine(new string('-', 80));
                Console.WriteLine("| {0, -30} | {1, -10} | {2, -20} |", "Product", "Quantity", "Subtotal");
                Console.WriteLine(new string('-', 80));

                DisplayOrderItems(order.OrderItems);

                Console.WriteLine(new string('-', 80));
            }
        }


        static void DisplayProducts(List<Product> products)
        {
            foreach (var product in products)
            {
                Console.WriteLine($"Product ID: {product.ProductId}, Name: {product.Name}, Price: {product.Price:C}, Available Quantity: {product.Available_nos}");
            }
        }

        static void DisplayOrderItems(List<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                Console.WriteLine("| {0, -30} | {1, -10} | {2, -20} |", item.Product.Name, item.Quantity, (item.Product.Price * item.Quantity).ToString("C"));
                Console.WriteLine(new string('-', 80));
            }
        }
    }
}