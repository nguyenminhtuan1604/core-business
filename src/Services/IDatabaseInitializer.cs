namespace CoreBusinessService.Services;

public interface IDatabaseInitializer
{
    Task EnsureReadyAsync(CancellationToken cancellationToken);
}
