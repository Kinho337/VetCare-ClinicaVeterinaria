using Microsoft.EntityFrameworkCore;
using VetCare.Models;

namespace VetCare.Data;

public class VetCareDbContext(DbContextOptions<VetCareDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Usuarios => Set<AppUser>();
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Animal> Animais => Set<Animal>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Tutor>().HasIndex(x => x.CPF).IsUnique();
        modelBuilder.Entity<Tutor>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Veterinario>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Veterinario>().HasIndex(x => x.CRmv).IsUnique();
        modelBuilder.Entity<Animal>().HasOne(x => x.Tutor).WithMany(x => x.Animais).HasForeignKey(x => x.TutorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Agendamento>().HasOne(x => x.Animal).WithMany(x => x.Agendamentos).HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Agendamento>().HasOne(x => x.Veterinario).WithMany(x => x.Agendamentos).HasForeignKey(x => x.VeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Agendamento>().HasOne(x => x.Servico).WithMany(x => x.Agendamentos).HasForeignKey(x => x.ServicoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(x => x.Animal).WithMany(x => x.Atendimentos).HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(x => x.Veterinario).WithMany(x => x.Atendimentos).HasForeignKey(x => x.VeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(x => x.Agendamento).WithOne(x => x.Atendimento).HasForeignKey<Atendimento>(x => x.AgendamentoId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Servico>().Property(x => x.Preco).HasPrecision(10, 2);
    }
}
