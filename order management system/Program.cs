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
        public decimal TotalAmount => OrderItems.Sum(item => item.Product.Price * item.Quantity);
    }

    public class OrderService
    {
        private List<Order> _orders = new List<Order>();
        private List<Product> _products = new List<Product>();

        public void AddProduct(Product product)
        {
            _products.Add(product);
        }

        public void PlaceOrder(Order order)
        {
            _orders.Add(order);
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

            // Adding sample products
            orderService.AddProduct(new Product { ProductId = 1, Name = "Laptop", Price = 999 });
            orderService.AddProduct(new Product { ProductId = 2, Name = "Phone", Price = 499 });

            while (true)
            {
                Console.WriteLine("1. Place Order");
                Console.WriteLine("2. View Orders");
                Console.WriteLine("3. Exit");
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
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
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
                        Console.Write("Enter Quantity: ");
                        if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
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
                            Console.WriteLine("Invalid quantity. Please enter a positive integer.");
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
                DisplayOrderItems(order.OrderItems);
                Console.WriteLine(new string('-', 30));
            }
        }

        static void DisplayProducts(List<Product> products)
        {
            foreach (var product in products)
            {
                Console.WriteLine($"Product ID: {product.ProductId}, Name: {product.Name}, Price: {product.Price:C}");
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
