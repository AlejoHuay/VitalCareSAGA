using MSClientes.API.AdaptadoresDeInterfaz.Gateways;
using MSClientes.API.CasosDeUso.PuertosEntrada;
using MSClientes.API.CasosDeUso.Validadores;
using MSClientes.API.Entidades;

namespace MSClientes.API.CasosDeUso.Interactores
{
    public class ClienteInteractor : IClienteInputPort
    {
        private readonly IClienteRepository clienteRepository;
        private readonly IResult<Cliente> clienteValidacion;

        public ClienteInteractor(
            IClienteRepository clienteRepository,
            IResult<Cliente> clienteValidacion)
        {
            this.clienteRepository = clienteRepository;
            this.clienteValidacion = clienteValidacion;
        }

        public IEnumerable<Cliente> ObtenerTodos(string filtro)
        {
            return clienteRepository.GetAll(filtro);
        }

        public Cliente? ObtenerPorId(int id)
        {
            return clienteRepository.GetById(id);
        }

        public (bool Exito, string Mensaje) Crear(Cliente cliente)
        {
            cliente.IdCliente = 0;
            cliente.Estado = 1;
            cliente.CorreoElectronico ??= string.Empty;
            AplicarConsumidorFinal(cliente);

            Result validacion = ValidarCliente(cliente);
            if (!validacion.IsSuccess)
                return (false, validacion.Error);

            if (clienteRepository.Insert(cliente) <= 0)
                return (false, "No se pudo registrar el cliente.");

            return (true, "Cliente registrado correctamente.");
        }

        public (bool Exito, string Mensaje) Actualizar(int id, Cliente cliente)
        {
            cliente.IdCliente = id;
            cliente.Estado = 1;
            cliente.CorreoElectronico ??= string.Empty;
            AplicarConsumidorFinal(cliente);

            Result validacion = ValidarCliente(cliente);
            if (!validacion.IsSuccess)
                return (false, validacion.Error);

            if (clienteRepository.Update(cliente) <= 0)
                return (false, "No se pudo actualizar el cliente.");

            return (true, "Cliente actualizado correctamente.");
        }

        public (bool Exito, string Mensaje) Eliminar(int id, int idUsuario)
        {
            Cliente cliente = new Cliente
            {
                IdCliente = id,
                IdUsuario = idUsuario
            };

            if (clienteRepository.Delete(cliente) <= 0)
                return (false, "No se pudo eliminar el cliente.");

            return (true, "Cliente eliminado correctamente.");
        }

        private Result ValidarCliente(Cliente cliente)
        {
            Result validacion = clienteValidacion.Validar(cliente);
            if (!validacion.IsSuccess)
                return validacion;

            LimpiarCampos(cliente);
            return ValidarDuplicado(cliente);
        }

        private Result ValidarDuplicado(Cliente cliente)
        {
            if (cliente.Nit.Equals("CF", StringComparison.OrdinalIgnoreCase))
                return Result.Ok();

            IEnumerable<Cliente> clientes = clienteRepository.GetAll(cliente.Nit);

            foreach (Cliente clienteExistente in clientes)
            {
                string nit = QuitarEspacios(clienteExistente.Nit);

                if (nit.Equals(cliente.Nit, StringComparison.OrdinalIgnoreCase) &&
                    clienteExistente.IdCliente != cliente.IdCliente)
                {
                    return Result.Fail("Ya existe un cliente con ese NIT.");
                }
            }

            return Result.Ok();
        }

        private static void AplicarConsumidorFinal(Cliente cliente)
        {
            if (!cliente.EsConsumidorFinal)
                return;

            cliente.Nit = "CF";
            cliente.RazonSocial = "Consumidor Final";
            cliente.CorreoElectronico = string.Empty;
        }

        private static void LimpiarCampos(Cliente cliente)
        {
            cliente.Nit = QuitarEspacios(cliente.Nit);
            cliente.RazonSocial = LimpiarEspacios(cliente.RazonSocial);
            cliente.CorreoElectronico = QuitarEspacios(cliente.CorreoElectronico);
        }

        private static string QuitarEspacios(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? string.Empty
                : valor.Replace(" ", string.Empty).Trim();
        }

        private static string LimpiarEspacios(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return string.Join(" ", valor.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
