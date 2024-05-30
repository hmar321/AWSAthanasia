using Athanasia.Models.Tables;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Athanasia.Services
{
    public class ServiceAWSCache
    {
        private readonly IDistributedCache _cache;

        public ServiceAWSCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<List<Libro>> GetLibrosFavoritosAsync()
        {
            var jsonLibros = await _cache.GetStringAsync("librosfavoritos");
            if (jsonLibros == null)
            {
                return null;
            }
            else
            {
                List<Libro> libros = JsonConvert.DeserializeObject<List<Libro>>(jsonLibros);
                return libros;
            }
        }

        public async Task AddLibrofavoritoAsync(Libro libro)
        {
            var libros = await GetLibrosFavoritosAsync();
            if (libros == null)
            {
                libros = new List<Libro>();
            }
            libros.Add(libro);

            var jsonLibros = JsonConvert.SerializeObject(libros);
            await _cache.SetStringAsync("librosfavoritos", jsonLibros, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
        }

        public async Task DeleteLibroFavoritoAsync(int idlibro)
        {
            var libros = await GetLibrosFavoritosAsync();
            if (libros != null)
            {
                var libroEliminar = libros.FirstOrDefault(z => z.IdLibro == idlibro);
                if (libroEliminar != null)
                {
                    libros.Remove(libroEliminar);

                    if (libros.Count == 0)
                    {
                        await _cache.RemoveAsync("librosfavoritos");
                    }
                    else
                    {
                        var jsonLibros = JsonConvert.SerializeObject(libros);
                        await _cache.SetStringAsync("librosfavoritos", jsonLibros, new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                        });
                    }
                }
            }
        }
    }
}
