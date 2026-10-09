using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetCare.Models;

namespace VetCare.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<VetCareDbContext>();
        if (!await db.Usuarios.AnyAsync())
        {
            var hasher = new PasswordHasher<AppUser>();
            var admin = new AppUser { Nome = "Administrador", Email = "admin@vetcare.local", Perfil = "Administrador" };
            admin.PasswordHash = hasher.HashPassword(admin, "VetCare@123");
            var vetUser = new AppUser { Nome = "Dra. Ana Oliveira", Email = "vet@vetcare.local", Perfil = "Veterinario" };
            vetUser.PasswordHash = hasher.HashPassword(vetUser, "VetCare@123");
            db.Usuarios.AddRange(admin, vetUser);
        }
        if (!await db.Servicos.AnyAsync())
        {
            db.Servicos.AddRange(
                new Servico { Nome = "Consulta", Descricao = "Consulta veterinária geral", Preco = 120 },
                new Servico { Nome = "Vacinação", Descricao = "Aplicação de vacina conforme avaliação veterinária", Preco = 85 },
                new Servico { Nome = "Retorno", Descricao = "Retorno de consulta", Preco = 0 }
            );
        }
        if (!await db.Veterinarios.AnyAsync())
            db.Veterinarios.Add(new Veterinario { Nome = "Dra. Ana Oliveira", Email = "ana@vetcare.local", CRmv = "CRMV-SP 12345", Telefone = "(11) 90000-0000" });
        await db.SaveChangesAsync();
    }
}
