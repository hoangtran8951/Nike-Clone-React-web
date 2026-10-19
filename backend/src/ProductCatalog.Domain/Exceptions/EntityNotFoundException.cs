namespace ProductCatalog.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public object Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with identifier '({key})' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}