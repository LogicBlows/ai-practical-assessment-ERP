namespace SmeErp.Domain.Entities;

public class SigningKey
{
    public int Id { get; set; }

    public string KeyValue { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; }
}
