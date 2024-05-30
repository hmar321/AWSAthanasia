using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Athanasia.Repositories;
using Athanasia.Models.Tables;
using Athanasia.Extension;
using Microsoft.Extensions.Caching.Memory;
using Athanasia.Services;
using Athanasia.Models.Views;

namespace Athanasia.Controllers
{
    public class ManagedController : Controller
    {
        private IRepositoryAthanasia repo;
        private IMemoryCache memoryCache;
        private ServiceCacheRedis serviceRedis;

        public ManagedController(IRepositoryAthanasia repo, IMemoryCache memoryCache, ServiceCacheRedis serviceRedis)
        {
            this.repo = repo;
            this.memoryCache = memoryCache;
            this.serviceRedis = serviceRedis;
        }

        public async Task<IActionResult> Login(string? token)
        {
            if (token != null)
            {
                await this.repo.ActivarUsuarioAsync(token);
                ViewData["MENSAJE"] = "Usuario activado";
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            //Usuario usuario = await this.repo.LogInUserAsync(email, password);
            string token = await this.repo.AuthTokenAsync(email, password);
            if (token != null)
            {
                Usuario usuario = await this.repo.AuthGetUsuarioAsync(token);
                List<ProductoSimpleView> productosCarrito = memoryCache.Get<List<ProductoSimpleView>>("CARRITO");
                if (productosCarrito!=null)
                {
                    await this.serviceRedis.AddMultipleFavoritos(usuario.IdUsuario + "", productosCarrito);
                }
                memoryCache.Remove("CARRITO");
                ClaimsIdentity identity = new ClaimsIdentity(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role);
                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario+""),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Surname, usuario.Apellido),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.IdRol+""),
                    new Claim(ClaimTypes.UserData, usuario.Imagen),
                    new Claim("TOKENJWT",token)
                };
                identity.AddClaims(claims);
                ClaimsPrincipal usePrincipal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    usePrincipal);
                string controller = "Libro";
                string action = "Index";
                if (TempData["controller"] != null && TempData["action"] != null)
                {
                    controller = TempData["controller"].ToString();
                    action = TempData["action"].ToString();
                }
                return RedirectToAction(action, controller);
            }
            else
            {
                ViewData["ERROR"] = "Credenciales incorrectas";
                return View();
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Libro");
        }
    }
}
