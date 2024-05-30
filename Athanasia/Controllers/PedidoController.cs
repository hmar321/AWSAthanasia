using Athanasia.Extension;
using Athanasia.Filters;
using Athanasia.Models.Tables;
using Athanasia.Models.Views;
using Athanasia.Repositories;
using Athanasia.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Security.Claims;

namespace Athanasia.Controllers
{
    public class PedidoController : Controller
    {
        private IRepositoryAthanasia repo;
        private IMapper mapper;
        private ServiceCacheRedis serviceRedis;

        public PedidoController(IRepositoryAthanasia repo, IMapper mapper, ServiceCacheRedis serviceRedis)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.serviceRedis = serviceRedis;
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> Comprar()
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<InformacionCompraView> infoUsuario = await this.repo.GetAllInformacionCompraViewByIdUsuarioAsync(idusuario);
            if (infoUsuario.Count == 0)
            {
                return RedirectToAction("MetodosPago", "Usuario");
            }
            ViewData["INFOCOMPRAUSUARIO"] = infoUsuario;
            return View();

        }

        [AuthorizeUsuarios]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Comprar(int idinfocompra)
        {
            int idusuario = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<ProductoSimpleView> productosView = await this.serviceRedis.GetProductosFavoritosAsync(idusuario + "");
            if (productosView != null)
            {
                List<PedidoProducto> productos = this.mapper.Map<List<PedidoProducto>>(productosView);
                await this.repo.InsertListPedidoProductosAsync(idusuario, productos);
                this.serviceRedis.RemoveCache(idusuario + "");
            }
            return RedirectToAction("HistorialCompras", "Usuario");
        }

        [AuthorizeUsuarios]
        public async Task<IActionResult> CancelarPedido(int idpedido)
        {
            await this.repo.UpdatePedidoEstadoCancelarAsync(idpedido);
            return RedirectToAction("HistorialCompras", "Usuario");
        }
        [AuthorizeUsuarios]
        public async Task<IActionResult> Detalles(int idpedido)
        {
            List<PedidoProductoView> prodPedido = await this.repo.GetPedidoProductoViewsByIdPedidoAsync(idpedido);
            return View(prodPedido);
        }
    }
}
