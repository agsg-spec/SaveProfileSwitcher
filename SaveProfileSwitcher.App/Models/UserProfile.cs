namespace SaveProfileSwitcher.App.Models;

public sealed class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarPath { get; set; }
    public string StorageRootPath { get; set; } = string.Empty;
}
