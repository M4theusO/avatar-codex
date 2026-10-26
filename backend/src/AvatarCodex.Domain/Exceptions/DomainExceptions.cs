namespace AvatarCodex.Domain.Exceptions;

/// <summary>Base das exceções de negócio. A mensagem é segura para ser exibida ao usuário.</summary>
public abstract class DomainException(string message) : Exception(message);

/// <summary>Recurso inexistente (HTTP 404).</summary>
public class NotFoundException(string message) : DomainException(message)
{
    public static NotFoundException For(string resource, object id) =>
        new($"{resource} '{id}' não encontrado(a).");
}

/// <summary>Conflito com o estado atual, ex. email já cadastrado (HTTP 409).</summary>
public class ConflictException(string message) : DomainException(message);

/// <summary>Violação de regra de negócio (HTTP 422).</summary>
public class BusinessRuleException(string message) : DomainException(message);
