namespace EA.Services.RepositoryFactory;

public interface IRepositoryFactory
{
    IRepository<T> GetRepository<T>() where T : class;
}