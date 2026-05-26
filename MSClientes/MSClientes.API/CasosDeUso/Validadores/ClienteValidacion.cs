using System.Text.RegularExpressions;
using MSClientes.API.Entidades;

namespace MSClientes.API.CasosDeUso.Validadores
{
    public class ClienteValidacion : IResult<Cliente>
    {
        public Result Validar(Cliente cliente)
        {
            if (EsConsumidorFinal(cliente))
                return Result.Ok();

            return ValidarNit(cliente.Nit)
                ?? ValidarRazonSocial(cliente.RazonSocial)
                ?? ValidarCorreoElectronico(cliente.CorreoElectronico)
                ?? Result.Ok();
        }

        private static Result? ValidarNit(string nit)
        {
            Result? resultadoBasico = ValidarNitObligatorioYEspacios(nit);
            if (resultadoBasico != null)
                return resultadoBasico;

            Result? resultadoFormato = ValidarNitFormato(nit);
            if (resultadoFormato != null)
                return resultadoFormato;

            return ValidarNitLongitudYContenido(nit);
        }

        private static Result? ValidarNitObligatorioYEspacios(string nit)
        {
            if (nit == null || nit.Length == 0)
                return Result.Fail("El NIT no puede estar vacío.");

            if (string.IsNullOrWhiteSpace(nit))
                return Result.Fail("El NIT no puede estar vacío.");

            if (nit.Any(c => c == '\t' || c == '\n' || c == '\r'))
                return Result.Fail("El NIT no debe contener tabulaciones ni saltos de línea.");

            if (!nit.Equals(nit.Trim(), StringComparison.Ordinal))
                return Result.Fail("El NIT no debe contener espacios al inicio o al final.");

            if (nit.Contains(' '))
                return Result.Fail("El NIT no debe contener espacios internos.");

            return null;
        }

        private static Result? ValidarNitFormato(string nit)
        {
            if (nit.StartsWith("+") || nit.StartsWith("-"))
                return Result.Fail("El NIT no debe contener signos positivos ni negativos.");

            if (Regex.IsMatch(nit, @"^\d+\.\d+$"))
                return Result.Fail("El NIT no debe contener números decimales.");

            bool contieneLetras = nit.Any(char.IsLetter);
            bool contieneSimbolos = nit.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

            if (contieneLetras && contieneSimbolos)
                return Result.Fail("El NIT solo acepta números.");

            if (contieneLetras && nit.Any(char.IsWhiteSpace))
                return Result.Fail("El NIT solo acepta números.");

            if (contieneLetras)
                return Result.Fail("El NIT solo acepta números.");

            if (contieneSimbolos)
                return Result.Fail("El NIT solo acepta números.");

            return null;
        }

        private static Result? ValidarNitLongitudYContenido(string nit)
        {
            if (nit.Length < 5)
                return Result.Fail("El NIT debe contener entre 5 y 12 dígitos; faltan dígitos.");

            if (nit.Length > 12)
                return Result.Fail("El NIT debe contener entre 5 y 12 dígitos; sobran dígitos.");

            if (!Regex.IsMatch(nit, @"^\d{5,12}$"))
                return Result.Fail("El NIT debe contener entre 5 y 12 dígitos numéricos.");

            if (nit.All(c => c == '0'))
                return Result.Fail("El NIT no puede estar compuesto solo por ceros.");

            return null;
        }

        private static Result? ValidarRazonSocial(string razonSocial)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                return Result.Fail("La razón social no puede estar vacía.");

            if (!razonSocial.Equals(razonSocial.Trim(), StringComparison.Ordinal))
                return Result.Fail("La razón social no debe contener espacios al inicio o al final.");

            if (razonSocial.Any(c => c == '\t' || c == '\n' || c == '\r'))
                return Result.Fail("La razón social no debe contener tabulaciones ni saltos de línea.");

            if (razonSocial.Length < 3 || razonSocial.Length > 45)
                return Result.Fail("La razón social debe tener entre 3 y 45 caracteres.");

            if (!Regex.IsMatch(razonSocial, @"^[\p{L}\s]+$"))
                return Result.Fail("La razón social solo debe contener letras y espacios.");

            if (!razonSocial.Any(char.IsLetter))
                return Result.Fail("La razón social debe contener al menos una letra.");

            return null;
        }

        private static Result? ValidarCorreoElectronico(string? correoElectronico)
        {
            if (string.IsNullOrWhiteSpace(correoElectronico))
                return null;

            if (correoElectronico.Length > 45)
                return Result.Fail("El correo electrónico no puede superar los 45 caracteres.");

            if (!Regex.IsMatch(correoElectronico, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Result.Fail("El correo electrónico no tiene un formato válido.");

            return null;
        }

        private static bool EsConsumidorFinal(Cliente cliente)
        {
            return cliente.Nit.Equals("CF", StringComparison.OrdinalIgnoreCase) &&
                   cliente.RazonSocial.Equals("Consumidor Final", StringComparison.OrdinalIgnoreCase);
        }
    }
}
