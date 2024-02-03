using System;
using System.Collections.Generic;
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
        }
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

            while (true)
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
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid product ID. Please enter a valid ID.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            }

            orderService.PlaceOrder(order);
            Console.WriteLine("Order placed successfully!");
        }

        static void ViewOrders(OrderService orderService)
        {
            List<Order> orders = orderService.GetOrders();
            Console.WriteLine("Orders:");

            foreach (var order in orders)
            {
                Console.WriteLine($"Order ID: {order.OrderId}, Total Amount: {order.TotalAmount:C}");
                Console.WriteLine(new string('-', 30));
                DisplayOrderItems(order.OrderItems);

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
                Console.WriteLine($"   {item.Product.Name} x {item.Quantity} - {item.Product.Price * item.Quantity:C}");
            }
        }
    }
}