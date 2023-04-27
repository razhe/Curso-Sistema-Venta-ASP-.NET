using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.DAL.DBContext;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.DAL.Implementacion;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.BLL.Implementacion;

//using SistemaVenta.DAL.Implementancion;
//using SistemaVenta.DAL.Interfaces;
//using SistemaVenta.BLL.Implementancion;
//using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.IOC
{
    public static class Dependencia
    {
       public static void InyectarDependencia(this IServiceCollection services, IConfiguration configuration)
       {
            services.AddDbContext<DbventaContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("conexion"));
            });

            /*
             * averiguar que es trasient (aunque creo que es para no espeficar clases ya que estas pueden variar)
             * Es para trabjar con las clases genericas y las interfaces
             */
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IVentasRepository, VentaRepository>();

            services.AddScoped<ICorreoService, CorreoService>(); // envio de correo
       }
    }
}
