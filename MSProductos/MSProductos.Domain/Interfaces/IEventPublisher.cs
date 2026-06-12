namespace MSProductos.Dominio.Interfaces
{
    public interface IEventPublisher
    {
        void Publish<T>(string routingKey, T evento);
    }
}