using Microsoft.EntityFrameworkCore;
using VetCare.Models;

namespace VetCare.Data;

public class VetCareDbContext(DbContextOptions<VetCareDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Usuarios => Set<AppUser>();
    public DbSet<Tutor> Tutores => Set<Tutor>();
    public DbSet<Animal> Animais => Set<Animal>();
    public DbSet<AnimalTutor> AnimalTutores => Set<AnimalTutor>();
    public DbSet<SolicitacaoVinculoAnimal> SolicitacoesVinculoAnimal => Set<SolicitacaoVinculoAnimal>();
    public DbSet<Veterinario> Veterinarios => Set<Veterinario>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppUser>().ToTable("Usuarios").HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<AppUser>().HasOne(x => x.Tutor).WithMany().HasForeignKey(x => x.TutorId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Tutor>().ToTable("Tutores");
        modelBuilder.Entity<Tutor>().HasIndex(x => x.CPF).IsUnique();
        modelBuilder.Entity<Tutor>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Veterinario>().ToTable("Veterinarios");
        modelBuilder.Entity<Veterinario>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Veterinario>().HasIndex(x => x.CRMV).IsUnique();
        modelBuilder.Entity<Servico>().ToTable("Servicos");
        modelBuilder.Entity<Animal>().ToTable("Animais");
        modelBuilder.Entity<Animal>().HasIndex(x => x.CodigoVinculo).IsUnique();
        modelBuilder.Entity<Animal>().HasOne(x => x.Tutor).WithMany(x => x.Animais).HasForeignKey(x => x.TutorId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AnimalTutor>().ToTable("AnimalTutores").HasKey(x => new { x.AnimalId, x.TutorId });
        modelBuilder.Entity<AnimalTutor>().HasOne(x => x.Animal).WithMany(x => x.TutorVinculos).HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AnimalTutor>().HasOne(x => x.Tutor).WithMany(x => x.AnimalVinculos).HasForeignKey(x => x.TutorId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SolicitacaoVinculoAnimal>().ToTable("SolicitacoesVinculoAnimal");
        modelBuilder.Entity<SolicitacaoVinculoAnimal>().HasOne(x => x.Animal).WithMany(x => x.SolicitacoesVinculo).HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SolicitacaoVinculoAnimal>().HasOne(x => x.Tutor).WithMany(x => x.SolicitacoesVinculo).HasForeignKey(x => x.TutorId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SolicitacaoVinculoAnimal>().HasIndex(x => new { x.AnimalId, x.TutorId, x.Status });
        modelBuilder.Entity<Agendamento>().ToTable("Agendamentos");
        modelBuilder.Entity<Agendamento>().HasOne(x => x.Animal).WithMany(x => x.Agendamentos).HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Agendamento>().HasOne(x => x.Veterinario).WithMany(x => x.Agendamentos).HasForeignKey(x => x.VeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Agendamento>().HasOne(x => x.Servico).WithMany(x => x.Agendamentos).HasForeignKey(x => x.ServicoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().ToTable("Atendimentos");
        modelBuilder.Entity<Atendimento>().HasOne(x => x.Animal).WithMany(x => x.Atendimentos).HasForeignKey(x => x.AnimalId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(x => x.Veterinario).WithMany(x => x.Atendimentos).HasForeignKey(x => x.VeterinarioId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Atendimento>().HasOne(x => x.Agendamento).WithOne(x => x.Atendimento).HasForeignKey<Atendimento>(x => x.AgendamentoId).OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Servico>().Property(x => x.Preco).HasPrecision(10, 2);
    }
}
