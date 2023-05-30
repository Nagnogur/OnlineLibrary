namespace ProcessingService.Specification
{
    public interface ISpecification<in T>
    {
        bool IsSatisfied(T obj);
    }
}
