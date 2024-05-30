using Athanasia.Models.Tables;
using Athanasia.Models.Views;
using AutoMapper;

namespace Athanasia.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductoSimpleView, PedidoProducto>();
            CreateMap<PedidoProducto, ProductoSimpleView>();
        }
    }
}
