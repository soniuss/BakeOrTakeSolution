using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


namespace Persistence.ApiRest
{
        public class ApplicationDbContext : DbContext
        {
            // Constructor que EF Core usará cuando se configure por Inyección de Dependencias en Program.cs
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            // Constructor sin parámetros, útil para herramientas de EF Core como las de Migraciones
            public ApplicationDbContext()
            {
            }

            // --- Definición de los DbSet (Colecciones que mapean a Tablas) ---
            // DbSet por cada una de las entidades principales
            public DbSet<Cliente> Clientes { get; set; }
            public DbSet<Empresa> Empresas { get; set; }
            public DbSet<Receta> Recetas { get; set; }
            public DbSet<PedidoOferta> PedidosOfertas { get; set; } // Entidad fusionada Pedido/Oferta
            public DbSet<Favorito> Favoritos { get; set; } // Entidad asociativa para Favoritos

            // --- Configuración del Modelo (Mapeo Claves y Relaciones) ---
            // Con este metodo se configura como las clases se mapean a la base de datos
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                
                base.OnModelCreating(modelBuilder);

                // --- Configuración de Claves Primarias ---
                // Configura las PKs si sus nombres no siguen la convención 'Id' o 'NombreClaseId'
                modelBuilder.Entity<Cliente>().HasKey(c => c.id_cliente);
                modelBuilder.Entity<Empresa>().HasKey(e => e.id_empresa);
                modelBuilder.Entity<Receta>().HasKey(r => r.id_receta);
                modelBuilder.Entity<PedidoOferta>().HasKey(po => po.id_pedido_oferta);

                // Configura la Clave Compuesta para la entidad asociativa FAVORITO
                modelBuilder.Entity<Favorito>().HasKey(f => new { f.id_cliente, f.id_receta });


                // --- Configuración de Relaciones ---

                // Relación 1:N Cliente (creador) - Receta
                // Un Cliente puede crear muchas Recetas; una Receta es creada por un Cliente.
                modelBuilder.Entity<Receta>()
                    .HasOne(r => r.ClienteCreador) // Una Receta tiene UN Cliente Creador (propiedad de navegación en Receta)
                    .WithMany(c => c.RecetasCreadas) // Un Cliente Creador tiene MUCHAS Recetas creadas (propiedad de navegación en Cliente)
                    .HasForeignKey(r => r.id_cliente_creador); // La Clave Foránea está en la entidad 'muchos' (Receta)


                // Relación M:N Cliente-Receta resuelta por la Entidad Asociativa FAVORITO
                // Un Cliente tiene muchos Favoritos; un registro de Favorito es de un Cliente.
                modelBuilder.Entity<Favorito>()
                    .HasOne(f => f.Cliente) // Un Favorito tiene UN Cliente
                    .WithMany(c => c.Favoritos) // Un Cliente tiene MUCHOS Favoritos
                    .HasForeignKey(f => f.id_cliente); // La FK en Favorito apunta a Cliente

                // Una Receta está en muchos Favoritos; un registro de Favorito es de una Receta.
                modelBuilder.Entity<Favorito>()
                    .HasOne(f => f.Receta) // Un Favorito tiene UNA Receta
                    .WithMany(r => r.Favoritos) // Una Receta está en MUCHOS Favoritos
                    .HasForeignKey(f => f.id_receta); // La FK en Favorito apunta a Receta


                // Relación 1:N Empresa - PedidoOferta (Empresa crea/ofrece PedidoOferta)
                // Una Empresa crea/ofrece muchos PedidoOfertas; un PedidoOferta es de una Empresa.
                modelBuilder.Entity<PedidoOferta>()
                    .HasOne(po => po.Empresa) // Un PedidoOferta tiene UNA Empresa
                    .WithMany(e => e.OfertasYPedidos) // Una Empresa tiene MUCHOS PedidoOfertas
                    .HasForeignKey(po => po.id_empresa); // La FK está en PedidoOferta


                // Relación 1:N Receta - PedidoOferta (Receta es base de PedidoOferta)
                // Una Receta es base de muchos PedidoOfertas; un PedidoOferta es de una Receta.
                modelBuilder.Entity<PedidoOferta>()
                   .HasOne(po => po.Receta) // Un PedidoOferta tiene UNA Receta
                   .WithMany(r => r.PedidosYOfertas) // Una Receta es base de MUCHOS PedidoOfertas
                   .HasForeignKey(po => po.id_receta); // La FK está en PedidoOferta


                // Relación 0..1:N Cliente - PedidoOferta (Cliente realiza Pedido - SOLO CUANDO ES PEDIDO)
                // Un Cliente realiza muchos PedidoOfertas (los que son pedidos); un PedidoOferta PUEDE TENER un Cliente.
                // Esta es la relación crucial para distinguir Oferta (id_cliente es NULL) de Pedido (id_cliente NO ES NULL).
                modelBuilder.Entity<PedidoOferta>()
                    .HasOne(po => po.ClienteRealiza) // Un PedidoOferta PUEDE TENER UN Cliente (propiedad de navegación en PedidoOferta)
                    .WithMany(c => c.PedidosRealizados) // Un Cliente tiene MUCHOS PedidoOfertas (los que ha realizado) (propiedad de navegación en Cliente)
                    .HasForeignKey(po => po.id_cliente) // La Clave Foránea es id_cliente en PedidoOferta
                    .IsRequired(false); //la FK no es obligatoria, permitiendo valores NULL.


                // --- Configuración de la Valoración (Embebida) ---
                // Como los campos de Valoracion (puntuacion, comentario, fechaValoracion) son propiedades
                // directamente en PedidoOferta y son nullable (DateTime? int? string?), EF Core los mapeará
                // automáticamente a columnas nullable en la tabla PedidosOfertas. No necesitas configuración extra aquí.


                
            }
        }
    }
