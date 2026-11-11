using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using AvatarCodex.UnitTests.Fakes;
using FluentValidation;

namespace AvatarCodex.UnitTests.Auth;

public class AuthServiceTests
{
    private static readonly RegisterRequest ValidRegistration = new("matheus", "matheus@example.com", "Senha123", "Senha123");

    private readonly FakeUserRepository _users = new();
    private readonly FixedTimeProvider _time = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));
    private readonly FakeProgressionRepository _progression = new();
    private readonly FakeAchievementRepository _achievements;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _achievements = new FakeAchievementRepository(_progression);
        _service = new AuthService(
            _users, new FakePasswordHasher(), new FakeTokenService(),
            TestProgression.Create(_progression, _achievements, _time), _time,
            new RegisterRequestValidator(), new LoginRequestValidator());
    }

    [Fact]
    public async Task Register_CreatesUserWithHashedPasswordAndStartsSession()
    {
        var result = await _service.RegisterAsync(ValidRegistration);

        var user = Assert.Single(_users.Users);
        Assert.Equal("hashed:Senha123", user.PasswordHash);
        Assert.Equal("access-for-matheus", result.Response.AccessToken);
        Assert.Equal(1, result.Response.User.Level);
        Assert.Equal(0, result.Response.User.ExperiencePoints);

        var token = Assert.Single(_users.Tokens);
        Assert.Equal($"hash:{result.RefreshToken}", token.TokenHash);
        Assert.Equal(_time.GetUtcNow().AddDays(7), result.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task Register_UnlocksFirstStepAndReturnsTheReward()
    {
        _achievements.Catalog.Add(new Achievement
        {
            Id = 1, Code = "FIRST_STEP", Name = "Primeiro passo", Description = "Crie sua conta.", Icon = "footprints",
            RequirementType = AchievementRequirementType.AccountCreated, ExperienceReward = 10
        });

        var result = await _service.RegisterAsync(ValidRegistration);

        var reward = result.Response.Reward;
        Assert.NotNull(reward);
        Assert.Equal((10, 10), (reward.XpGained, reward.TotalXp));
        Assert.Equal("Primeiro passo", Assert.Single(reward.AchievementsUnlocked).Name);
        Assert.Equal(10, result.Response.User.ExperiencePoints);
        Assert.Equal(_time.GetUtcNow(), Assert.Single(_progression.Records.OfType<UserAchievement>()).UnlockedAt);
    }

    [Fact]
    public async Task Login_DoesNotReturnReward()
    {
        await _service.RegisterAsync(ValidRegistration);

        var result = await _service.LoginAsync(new LoginRequest(ValidRegistration.Email, ValidRegistration.Password));

        Assert.Null(result.Response.Reward);
    }

    [Fact]
    public async Task Register_NormalizesEmailAndTrimsUsername()
    {
        await _service.RegisterAsync(ValidRegistration with { Username = "  matheus ", Email = "  Matheus@Example.COM " });

        var user = Assert.Single(_users.Users);
        Assert.Equal("matheus", user.Username);
        Assert.Equal("matheus@example.com", user.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsConflict()
    {
        await _service.RegisterAsync(ValidRegistration);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(ValidRegistration with { Username = "outro", Email = "MATHEUS@example.com" }));
        Assert.Equal("Este email já está cadastrado.", ex.Message);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ThrowsConflict()
    {
        await _service.RegisterAsync(ValidRegistration);

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(ValidRegistration with { Username = "MATHEUS", Email = "outro@example.com" }));
        Assert.Equal("Este nome de usuário já está em uso.", ex.Message);
    }

    [Fact]
    public async Task Register_InvalidData_ThrowsValidationWithoutSaving()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.RegisterAsync(ValidRegistration with { Password = "semnumero", ConfirmPassword = "semnumero" }));

        Assert.Empty(_users.Users);
    }

    [Fact]
    public async Task Login_ValidCredentials_StartsNewSession()
    {
        await _service.RegisterAsync(ValidRegistration);

        var result = await _service.LoginAsync(new LoginRequest(" MATHEUS@example.com ", "Senha123"));

        Assert.Equal("matheus", result.Response.User.Username);
        Assert.Equal(2, _users.Tokens.Count);
    }

    [Theory]
    [InlineData("matheus@example.com", "SenhaErrada1")]
    [InlineData("ninguem@example.com", "Senha123")]
    public async Task Login_InvalidCredentials_UsesSameMessage(string email, string password)
    {
        await _service.RegisterAsync(ValidRegistration);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest(email, password)));
        Assert.Equal(AuthService.InvalidCredentialsMessage, ex.Message);
    }

    [Fact]
    public async Task Refresh_RotatesToken()
    {
        var session = await _service.RegisterAsync(ValidRegistration);

        var refreshed = await _service.RefreshAsync(session.RefreshToken);

        Assert.NotEqual(session.RefreshToken, refreshed.RefreshToken);
        var old = _users.Tokens.Single(t => t.TokenHash == $"hash:{session.RefreshToken}");
        Assert.Equal(_time.GetUtcNow(), old.RevokedAt);
        Assert.Null(_users.Tokens.Single(t => t.TokenHash == $"hash:{refreshed.RefreshToken}").RevokedAt);
    }

    [Fact]
    public async Task Refresh_ReusingRotatedToken_ThrowsUnauthorized()
    {
        var session = await _service.RegisterAsync(ValidRegistration);
        await _service.RefreshAsync(session.RefreshToken);

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(session.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ThrowsUnauthorized()
    {
        var session = await _service.RegisterAsync(ValidRegistration);
        _time.Advance(TimeSpan.FromDays(7));

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(session.RefreshToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("token-inexistente")]
    public async Task Refresh_MissingOrUnknownToken_ThrowsUnauthorized(string? token)
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(token));
    }

    [Fact]
    public async Task Logout_RevokesToken()
    {
        var session = await _service.RegisterAsync(ValidRegistration);

        await _service.LogoutAsync(session.RefreshToken);

        Assert.NotNull(Assert.Single(_users.Tokens).RevokedAt);
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshAsync(session.RefreshToken));
    }

    [Fact]
    public async Task Logout_UnknownToken_DoesNothing()
    {
        await _service.LogoutAsync("token-inexistente");
        await _service.LogoutAsync(null);
    }

    [Fact]
    public async Task GetCurrentUser_DeletedUser_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.GetCurrentUserAsync(Guid.NewGuid()));
    }

    private AccountService Account() =>
        new(_users, new FakePasswordHasher(), new DeleteAccountRequestValidator(), new UpdateProfileRequestValidator(),
            new UpdatePrivacyRequestValidator());

    [Fact]
    public async Task UpdatePrivacy_TogglesAndReturnsTheUser()
    {
        var registered = await _service.RegisterAsync(ValidRegistration);
        Assert.False(registered.Response.User.IsProfilePrivate);

        var hidden = await Account().UpdatePrivacyAsync(registered.Response.User.Id, new UpdatePrivacyRequest(true));
        Assert.True(hidden.IsProfilePrivate);
        Assert.True(Assert.Single(_users.Users).IsProfilePrivate);

        // Repetir o estado atual não é erro.
        Assert.True((await Account().UpdatePrivacyAsync(registered.Response.User.Id, new UpdatePrivacyRequest(true))).IsProfilePrivate);

        var shown = await Account().UpdatePrivacyAsync(registered.Response.User.Id, new UpdatePrivacyRequest(false));
        Assert.False(shown.IsProfilePrivate);
    }

    [Fact]
    public async Task UpdatePrivacy_MissingValue_IsAValidationError()
    {
        var registered = await _service.RegisterAsync(ValidRegistration);

        await Assert.ThrowsAsync<ValidationException>(() =>
            Account().UpdatePrivacyAsync(registered.Response.User.Id, new UpdatePrivacyRequest(null)));
    }

    [Fact]
    public async Task UpdatePrivacy_DemoAccount_StaysPublic()
    {
        var registered = await _service.RegisterAsync(
            new RegisterRequest(DemoAccount.Username, DemoAccount.Email, DemoAccount.Password, DemoAccount.Password));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Account().UpdatePrivacyAsync(registered.Response.User.Id, new UpdatePrivacyRequest(true)));

        Assert.Equal(AccountService.DemoPrivacyMessage, error.Message);
        Assert.False(Assert.Single(_users.Users).IsProfilePrivate);
    }

    [Theory]
    [InlineData(false, false, true)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    public void ProfileRules_CanView_PrivateOnlyForTheOwner(bool isPrivate, bool isOwner, bool expected)
    {
        var owner = new User { Id = Guid.NewGuid(), Username = "aang", Email = "a@example.com", PasswordHash = "x", IsProfilePrivate = isPrivate };

        Assert.Equal(expected, ProfileRules.CanView(owner, isOwner ? owner.Id : Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateProfile_NewUsername_IsSavedTrimmedAndReturned()
    {
        var registered = await _service.RegisterAsync(ValidRegistration);

        var user = await Account().UpdateProfileAsync(registered.Response.User.Id, new UpdateProfileRequest("  aang_2026 "));

        Assert.Equal("aang_2026", user.Username);
        Assert.Equal("aang_2026", Assert.Single(_users.Users).Username);
    }

    [Fact]
    public async Task UpdateProfile_OnlyChangingCase_IsAllowed()
    {
        var registered = await _service.RegisterAsync(ValidRegistration);

        var user = await Account().UpdateProfileAsync(registered.Response.User.Id, new UpdateProfileRequest("Matheus"));

        Assert.Equal("Matheus", user.Username);
    }

    [Fact]
    public async Task UpdateProfile_UsernameOfSomeoneElse_IsAConflict()
    {
        await _service.RegisterAsync(ValidRegistration with { Username = "katara", Email = "katara@example.com" });
        var registered = await _service.RegisterAsync(ValidRegistration);

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            Account().UpdateProfileAsync(registered.Response.User.Id, new UpdateProfileRequest("KATARA")));

        Assert.Equal(AccountService.UsernameTakenMessage, error.Message);
        Assert.Equal("matheus", _users.Users.Single(u => u.Id == registered.Response.User.Id).Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("nome com espaço")]
    public async Task UpdateProfile_InvalidUsername_IsAValidationError(string username)
    {
        var registered = await _service.RegisterAsync(ValidRegistration);

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Account().UpdateProfileAsync(registered.Response.User.Id, new UpdateProfileRequest(username)));

        Assert.Equal("Username", Assert.Single(error.Errors).PropertyName);
    }

    [Fact]
    public async Task UpdateProfile_DemoAccount_IsRefused()
    {
        var registered = await _service.RegisterAsync(
            new RegisterRequest(DemoAccount.Username, DemoAccount.Email, DemoAccount.Password, DemoAccount.Password));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Account().UpdateProfileAsync(registered.Response.User.Id, new UpdateProfileRequest("outro_nome")));

        Assert.Equal(AccountService.DemoProfileMessage, error.Message);
        Assert.Equal(DemoAccount.Username, Assert.Single(_users.Users).Username);
    }

    [Fact]
    public async Task DeleteAccount_RightPassword_RemovesUserAndSessions()
    {
        var registered = await _service.RegisterAsync(ValidRegistration);

        await Account().DeleteAsync(registered.Response.User.Id, new DeleteAccountRequest("Senha123"));

        Assert.Empty(_users.Users);
        Assert.Empty(_users.Tokens);
    }

    [Theory]
    [InlineData("", "Informe sua senha para confirmar.")]
    [InlineData("Errada123", AccountService.WrongPasswordMessage)]
    public async Task DeleteAccount_MissingOrWrongPassword_IsAValidationErrorAndKeepsTheUser(
        string password, string message)
    {
        var registered = await _service.RegisterAsync(ValidRegistration);

        var error = await Assert.ThrowsAsync<ValidationException>(() =>
            Account().DeleteAsync(registered.Response.User.Id, new DeleteAccountRequest(password)));

        var failure = Assert.Single(error.Errors);
        Assert.Equal("Password", failure.PropertyName);
        Assert.Equal(message, failure.ErrorMessage);
        Assert.Single(_users.Users);
    }

    [Fact]
    public async Task DeleteAccount_DemoAccount_IsRefused()
    {
        var registered = await _service.RegisterAsync(
            new RegisterRequest(DemoAccount.Username, DemoAccount.Email, DemoAccount.Password, DemoAccount.Password));

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            Account().DeleteAsync(registered.Response.User.Id, new DeleteAccountRequest(DemoAccount.Password)));

        Assert.Equal(AccountService.DemoAccountMessage, error.Message);
        Assert.Single(_users.Users);
    }

    [Fact]
    public async Task DeleteAccount_UserAlreadyGone_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            Account().DeleteAsync(Guid.NewGuid(), new DeleteAccountRequest("Senha123")));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];
        public List<RefreshToken> Tokens { get; } = [];

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken) =>
            Task.FromResult(Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));

        public Task<User?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) => GetByIdAsync(id, cancellationToken);

        public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken) =>
            Task.FromResult(Tokens.FirstOrDefault(t => t.TokenHash == tokenHash));

        public void Add(User user) => Users.Add(user);

        public void AddRefreshToken(RefreshToken token)
        {
            token.User = Users.Single(u => u.Id == token.UserId);
            Tokens.Add(token);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            Users.RemoveAll(u => u.Id == id);
            Tokens.RemoveAll(t => t.UserId == id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";
        public bool Verify(string passwordHash, string password) => passwordHash == Hash(password);
    }

    private sealed class FakeTokenService : ITokenService
    {
        private int _counter;
        public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(7);
        public AccessToken CreateAccessToken(User user) => new($"access-for-{user.Username}", 900);
        public string CreateRefreshToken() => $"refresh-{++_counter}";
        public string HashRefreshToken(string refreshToken) => $"hash:{refreshToken}";
    }

}
