using FPTBook.Data;
using FPTBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Newtonsoft.Json;
using static NuGet.Packaging.PackagingConstants;
using System.Globalization;

namespace FPTBook.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this.webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin, User")]
        public ActionResult Index(string search)
        {
            var books = from l in _context.Book
                        select l;

            if (!String.IsNullOrEmpty(search))
            {
                books = books.Where(s => s.Title.Contains(search));
            }

            return View(books);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize(Roles = "User")]
        public ActionResult IndexCart()
        {
            var _book = getAllBook();
            ViewBag.book = _book;
            return View();
        }
        [Authorize(Roles = "User")]
        public List<Book> getAllBook()
        {
            return _context.Book.ToList();
        }

        [Authorize(Roles = "User")]
        public Book getDetailBook(int id)
        {
            var book = _context.Book.Find(id);
            return book;
        }
        [Authorize(Roles = "User")]
        public IActionResult addCart(int id)
        {
            var cart = HttpContext.Session.GetString("cart");//get key cart
            if (cart == null)
            {
                var book = getDetailBook(id);
                List<Cart> listCart = new List<Cart>()
               {
                   new Cart
                   {
                       Book = book,
                       Quantity = 1
                   }
               };
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(listCart));

            }
            else
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                bool check = true;
                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].Book.Id == id)
                    {
                        dataCart[i].Quantity++;
                        check = false;
                    }
                }
                if (check)
                {
                    dataCart.Add(new Cart
                    {
                        Book = getDetailBook(id),
                        Quantity = 1
                    });
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "User")]
        public IActionResult ListCart()
        {
            var cart = HttpContext.Session.GetString("cart");//get key cart
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                if (dataCart.Count > 0)
                {
                    ViewBag.carts = dataCart;
                    return View();
                }
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public IActionResult updateCart(int id, int quantity)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);
                if (quantity > 0)
                {
                    for (int i = 0; i < dataCart.Count; i++)
                    {
                        if (dataCart[i].Book.Id == id)
                        {
                            dataCart[i].Quantity = quantity;
                        }
                    }
                    HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
                }
                var cart2 = HttpContext.Session.GetString("cart");
                return Ok(quantity);
            }
            return BadRequest();

        }
        [Authorize(Roles = "User")]
        public IActionResult deleteCart(int id)
        {
            var cart = HttpContext.Session.GetString("cart");
            if (cart != null)
            {
                List<Cart> dataCart = JsonConvert.DeserializeObject<List<Cart>>(cart);

                for (int i = 0; i < dataCart.Count; i++)
                {
                    if (dataCart[i].Book.Id == id)
                    {
                        dataCart.RemoveAt(i);
                    }
                }
                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(dataCart));
                return RedirectToAction(nameof(ListCart));
            }
            return RedirectToAction(nameof(Index));
        }
        public List<Cart> GetCartItems()
        {

            var session = HttpContext.Session;
            string jsoncart = session.GetString("cart");
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<Cart>>(jsoncart);
            }
            return new List<Cart>();
        }
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CheckOut()
        {
            var cart = GetCartItems();
            // tạo cấu trúc db lưu lại đơn hàng và xóa cart khỏi session
            for (int i = 0; i < cart.Count; i++)
            {
                Order order = new Order
                {
                    Name = User.FindFirstValue(ClaimTypes.Email),
                    Quantity = cart[i].Quantity,
                    Price = cart[i].Book.Price,
                    OrderDate = DateTime.Now,
                    Total = cart[i].Book.Price * cart[i].Quantity,
                    Title_Book = cart[i].Book.Title
                };
                _context.Order.Add(order);
                await _context.SaveChangesAsync();
            }
            HttpContext.Session.Remove("cart");
            return RedirectToAction("Index");
        }
    }
}
