namespace MSVentas.Dominio.Puertos.PuertoSalida
{
    public interface IEventPublisher
    {
        void Publish<T>(string routingKey, T evento);
    }
}