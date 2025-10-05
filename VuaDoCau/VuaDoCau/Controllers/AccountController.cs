using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VuaDoCau.Models;

namespace VuaDoCau.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        public AccountController(UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signInMgr)
        {
            _userMgr = userMgr;
            _signInMgr = signInMgr;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                PhoneNumber = vm.Phone,
                Address = vm.Address,
                EmailConfirmed = true
            };

            var rs = await _userMgr.CreateAsync(user, vm.Password);
            if (rs.Succeeded)
            {
                await _userMgr.AddToRoleAsync(user, "User");
                await _signInMgr.SignInAsync(user, isPersistent: true);
                TempData["Success"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            foreach (var e in rs.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            TempData["Error"] = "Đăng ký thất bại. Vui lòng kiểm tra thông tin.";
            return View(vm);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginVm { ReturnUrl = returnUrl });

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var rs = await _signInMgr.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
            if (rs.Succeeded)
            {
                TempData["Success"] = "Đăng nhập thành công!";
                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Sai email hoặc mật khẩu.");
            TempData["Error"] = "Đăng nhập thất bại. Vui lòng thử lại.";
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInMgr.SignOutAsync();
            TempData["Success"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => Content("Bạn không có quyền truy cập.");
    }

    public record RegisterVm
    {
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }

    public record LoginVm
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}