namespace AvatarCodex.Domain.Entities;

/// <summary>
/// "Follower segue Followed" (Fase 24). A PK composta impede seguir duas vezes; o CHECK do banco,
/// seguir a si mesmo. Excluir qualquer uma das contas apaga a linha (CASCADE).
/// </summary>
public class UserFollow
{
    public Guid FollowerId { get; set; }
    public User? Follower { get; set; }

    public Guid FollowedId { get; set; }
    public User? Followed { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
