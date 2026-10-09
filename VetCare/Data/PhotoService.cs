namespace VetCare.Data;

public class PhotoService(IWebHostEnvironment environment)
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public async Task<string?> SaveAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("A imagem deve ter no máximo 5 MB.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) throw new InvalidOperationException("Formato inválido. Use JPG, PNG ou WEBP.");
        var folder = Path.Combine(environment.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{ext}";
        await using var stream = File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream);
        return $"/uploads/{name}";
    }
}
