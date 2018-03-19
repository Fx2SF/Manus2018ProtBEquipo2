namespace WindowsFormsManus
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=manus;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
        {
        }

        public virtual DbSet<Cheques> Cheques { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cheques>()
                .Property(e => e.imagePath)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.monto)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.cliente)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.montoEscrito)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.serie)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.numero)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.tipoCheque)
                .IsUnicode(false);

            modelBuilder.Entity<Cheques>()
                .Property(e => e.fecha)
                .IsUnicode(false);
        }
    }
}
