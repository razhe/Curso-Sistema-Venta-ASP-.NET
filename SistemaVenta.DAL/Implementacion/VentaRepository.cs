using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentasRepository
    {
        private readonly DbventaContext _dbContext;

        public VentaRepository(DbventaContext dbContext) : base(dbContext)
        // Ya que se está heredando de GenericRepository así que necesitamos especificar que este contexto lo enviaremos a generic repository
        {
            _dbContext = dbContext;
        }

        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada = new();

            //Transaccion por si ocurre algun error en algún insert de una tabla, nosotros debemos restablecer todas las operaciones como estaban al inicio.
            using (var transaccion = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in entidad.DetalleVenta)
                    {
                        Producto producto = _dbContext.Productos.Where(p => p.IdProducto == item.IdProducto).First();
                        producto.Stock = producto.Stock - item.Cantidad;

                        _dbContext.Productos.Update(producto);
                    }
                    await _dbContext.SaveChangesAsync();

                    NumeroCorrelativo correlativo = _dbContext.NumeroCorrelativos.Where(nc => nc.Gestion == "venta").First();

                    correlativo.UltimoNumero = correlativo.UltimoNumero++;
                    correlativo.FechaActualizacion = DateTime.Now;

                    _dbContext.Update(correlativo);
                    await _dbContext.SaveChangesAsync();

                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos!.Value));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();

                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);
                    entidad.NumeroVenta = numeroVenta;

                    await _dbContext.Venta.AddAsync(entidad);
                    await _dbContext.SaveChangesAsync();

                    ventaGenerada = entidad;
                    transaccion.Commit();
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    throw new Exception(ex.Message);
                }

                return ventaGenerada;
            }
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime fechaInicio, DateTime fechaFin)
        {
            List<DetalleVenta> listaResumen = await _dbContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u!.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv!.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation!.FechaRegistro!.Value.Date >= fechaInicio.Date && 
                    dv.IdVentaNavigation.FechaRegistro.Value.Date <= fechaFin.Date)
                .ToListAsync();

            return listaResumen;
        }
    }
}
