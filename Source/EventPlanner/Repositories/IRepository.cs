namespace EventPlanner.Repositories;

public interface IRepository<T>
{
    List<T> GetAll();
    T? GetById(int id);
    int Add(T item);
    void Update(T item);
    void Delete(int id);
}
