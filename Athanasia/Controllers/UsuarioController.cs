using Athanasia.Extension;
using Athanasia.Filters;
using Athanasia.Helpers;
using Athanasia.Models.Tables;
using Athanasia.Models.Util;
using Athanasia.Models.Views;
using Athanasia.Repositories;
using Athanasia.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using static NuGet.Packaging.PackagingConstants;

namespace Athanasia.Controllers
{
    public class UsuarioController : Controller
    {
        private IRepositoryAthanasia repo;
        private HelperMails helperMail;
        private HelperPathProvider helperPathProvider;
        private ServiceStorageBlobs serviceStorageBlobs;

        public UsuarioController(IRepositoryAthanasia repo, HelperMails helperMail, HelperPathProvider helperPathProvider, ServiceStorageBlobs serviceStorageBlobs)
        {
            this.repo = repo;
            this.helperMail = helperMail;
            this.helperPathProvider = helperPathProvider;
            this.serviceStorageBlobs = serviceStorageBlobs;
        }

        public IActionResult Terminos()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Registro(string nombre, string apellido, string email, string password, string password2, bool terminos)
        {
            UsuarioRegistro info = new UsuarioRegistro
            {
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                Password = password,
                Password2 = password2,
                Terminos = terminos
            };
            if (password != password2)
            {
                ViewData["REGISTROUSUARIO"] = info;
                ViewData["ERROR"] = "La contraseña no es la misma";
                return View();
            }
            Usuario usuario = await this.repo.RegistrarUsuarioAsync(nombre, apellido, email, password);
            string serverUrl = this.helperPathProvider.MapUrlServerPath();
            serverUrl = serverUrl + "/Managed/Login?token=" + usuario.Token;
            string mensaje = "<h3>Usuario registrado<h3>";
            mensaje += "<p>Puede activar su cuenta pulsando el siguiente enlace:</p>";
            mensaje += "<a href='" + serverUrl + "'>" + serverUrl + "</a>";
            //try
            //{
            //    await this.helperMail.SendMailAsync(email, "Activación cuenta[NO REPLY]", mensaje);
            //    ViewData["MENSAJE"] = "Usuario registrado correctamente, activa tu cuenta desde tu correo";
            //}
            //catch (Exception)
            //{
            //    ViewData["REGISTROUSUARIO"] = info;
            //    ViewData["ERROR"] = "Error al enviar correo";
            //    //await this.repo.DeleteUsuarioAsync(usuario.IdUsuario);
            //}
            ViewData["ERROR"] = "Correo bloqueado";
            ViewData["URL"] = serverUrl;
            return View();
        }

        public IActionResult _Menus()
        {
            return PartialView("_Menus");
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Perfil()
        {
            return View();
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> Editar(int idusuario)
        {
            Usuario usuario = await this.repo.FindUsuarioByIdAsync(idusuario);
            return View(usuario);
        }
        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> Editar(int idusuario, string nombre, string apellido, string email, IFormFile? fichero)
        {
            string imagen = null;
            if (fichero != null)
            {
                //string path = this.helperPathProvider.MapPath(fichero.FileName, FoldersImages.Usuarios);
                //using (Stream stream = new FileStream(path, FileMode.Create))
                //{
                //    await fichero.CopyToAsync(stream);
                //}
                imagen = fichero.FileName;
                this.serviceStorageBlobs.UploadBlobAsync("usuarios", imagen,fichero.OpenReadStream());
            }
            await this.repo.UpdateUsuarioAsync(idusuario, nombre, apellido, email, imagen);
            return RedirectToAction("Logout", "Managed");
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Compras()
        {

            return View();
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> ResetPass()
        {
            string email = HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Usuario usuario = await this.repo.UpdateUsuarioTokenAsync(idusuario);
            string serverUrl = this.helperPathProvider.MapUrlServerPath();
            serverUrl = serverUrl + "/Usuario/CambioPassword?token=" + usuario.Token;
            string mensaje = "<h3>Cambio de contraseña<h3>";
            mensaje += "<p>Para cambiar de contraseña pulse en el siguiente enlace:</p>";
            mensaje += "<a href='" + serverUrl + "'>" + serverUrl + "</a>";
            //try
            //{
            //    await this.helperMail.SendMailAsync(email, "Cambio contraseña[NO REPLY]", mensaje);
            //    ViewData["MENSAJE"] = "Se te ha enviado un correo";
            //}
            //catch (Exception)
            //{
            //    ViewData["MENSAJE"] = "Error al enviar correo";
            //}
            ViewData["MENSAJE"] = "Error al enviar correo";
            ViewData["URL"] = serverUrl;
            return View("Perfil");
        }


        [AuthorizeUsuarios]
        public async Task<IActionResult> CambioPassword(string token)
        {
            Usuario usuario = await this.repo.GetUsuarioByTokenAsync(token);
            if (usuario != null)
            {
                return View();
            }
            else
            {
                ViewData["MENSAJE"] = "No se ha podido cambiar la contraseña";
                return RedirectToAction("Perfil");
            }
        }
        [HttpPost]
        [AuthorizeUsuarios]
        public async Task<IActionResult> CambioPassword(string password, string password2)
        {
            if (password == password2)
            {
                int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                await this.repo.UpdateUsuarioPasswordAsync(idusuario, password);
                return RedirectToAction("Logout", "Managed");
            }
            else
            {
                ViewData["MENSAJE"] = "La contraseña no es la misma";
                return View();
            }
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> MetodosPago()
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<InformacionCompraView> info = await this.repo.GetAllInformacionCompraViewByIdUsuarioAsync(idusuario);
            return View(info);
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> AddMetodoPago()
        {
            ViewData["METODOSPAGO"] = await this.repo.GetMetodoPagosAsync();
            return View();
        }
        [AuthorizeUsuarios]
        [HttpPost]
        public async Task<IActionResult> AddMetodoPago(InformacionCompra info)
        {
            if (info.Indicaciones == null)
            {
                info.Indicaciones = "";
            }
            await this.repo.InsertInformacionAsync(info.Nombre, info.Direccion, info.Indicaciones, info.IdMetodoPago.Value, info.IdUsuario.Value);
            return RedirectToAction("MetodosPago");
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> EliminarInformacionCompra(int idmetodopago)
        {
            await this.repo.DeleteInformacionCompraByIdAsync(idmetodopago);
            return RedirectToAction("MetodosPago");
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> HistorialCompras()
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<PedidoView> pedidos = await this.repo.GetAllPedidoViewByIdUsuario(idusuario);
            return View(pedidos);
        }
    }
}
