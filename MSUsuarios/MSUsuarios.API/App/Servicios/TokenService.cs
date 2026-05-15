using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ServicioUsuarios.App.DTOs;
using ServicioUsuarios.App.Interfaces;
using ServicioUsuarios.Dominio.Modelos;
using ServicioUsuarios.Dominio.Puertos.PuertoSalida;
using ServicioUsuarios.Dominio.Validadores;
using ServicioUsuarios.Infraestructura.Ayudadores;

namespace ServicioUsuarios.App.Servicios
{
    public class TokenService : ITokenService
    {
        private readonly IUsuarioTokenRepository _usuarioTokenRepository;

        public TokenService(IUsuarioTokenRepository usuarioTokenRepository)
        {
            _usuarioTokenRepository = usuarioTokenRepository;
        }

        public (Result, string) GenerarToken(UsuarioTokenGeneracionDto dto, out string tokenPlano)
        {
            tokenPlano = string.Empty;

            Result validacion = ValidarGeneracionToken(dto);
            if (!validacion.IsSuccess)
                return (validacion, string.Empty);

            _usuarioTokenRepository.EliminarTokensObsoletos(7);
            _usuarioTokenRepository.RevocarTokensActivos(dto.IdUsuario, dto.TipoToken);

            DateTime fechaExpiracion = TokenHelper.GenerarFechaExpiracion(dto.MinutosExpiracion);

            if (dto.TipoToken == TipoTokenConstantes.ActivacionCuenta
                || dto.TipoToken == TipoTokenConstantes.ResetPassword
                || dto.TipoToken == TipoTokenConstantes.ConfirmacionEmail)
            {
                tokenPlano = TokenHelper.GenerarTokenPlano();
                string tokenHashPlano = TokenHelper.GenerarTokenHash(tokenPlano);

                UsuarioToken tokenPlanoEntity = new UsuarioToken(dto.IdUsuario, tokenHashPlano, dto.TipoToken, fechaExpiracion);
                int insertPlano = _usuarioTokenRepository.Insert(tokenPlanoEntity);

                return insertPlano > 0
                    ? (Result.Ok(), tokenPlano)
                    : (Result.Fail("No se pudo registrar el token."), string.Empty);
            }

            string jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? string.Empty;
            string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? string.Empty;
            string jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(jwtKey) || string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
                return (Result.Fail("No se encontro la configuracion JWT requerida."), string.Empty);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, dto.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, dto.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, dto.Role ?? string.Empty)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenJwt = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: fechaExpiracion,
                signingCredentials: creds
            );

            string jwtGenerado = new JwtSecurityTokenHandler().WriteToken(tokenJwt);
            tokenPlano = jwtGenerado;
            string tokenHash = TokenHelper.GenerarTokenHash(jwtGenerado);

            UsuarioToken token = new UsuarioToken(dto.IdUsuario, tokenHash, dto.TipoToken, fechaExpiracion);
            int insert = _usuarioTokenRepository.Insert(token);

            return insert > 0
                ? (Result.Ok(), jwtGenerado)
                : (Result.Fail("No se pudo registrar el token."), string.Empty);
        }

        public UsuarioToken? ValidarToken(string tokenPlano, string tipoToken)
        {
            tokenPlano = NormalizarTokenPlano(tokenPlano);
            tipoToken = tipoToken?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tokenPlano) || string.IsNullOrWhiteSpace(tipoToken))
                return null;

            string tokenHash = TokenHelper.GenerarTokenHash(tokenPlano);
            UsuarioToken? token = _usuarioTokenRepository.GetTokenActivo(tokenHash, tipoToken);

            if (token == null || token.FechaExpiracion <= DateTime.UtcNow)
                return null;

            return token;
        }

        public Result MarcarComoUsado(int idUsuarioToken)
        {
            if (idUsuarioToken <= 0)
                return Result.Fail("El id del token debe ser mayor a cero.");

            int filasAfectadas = _usuarioTokenRepository.MarcarComoUsado(idUsuarioToken);

            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo marcar el token como usado.");
        }

        public Result RevocarTokensActivos(int idUsuario, string tipoToken)
        {
            if (idUsuario <= 0)
                return Result.Fail("El id del usuario debe ser mayor a cero.");

            tipoToken = tipoToken?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tipoToken))
                return Result.Fail("El tipo de token es obligatorio.");

            _usuarioTokenRepository.RevocarTokensActivos(idUsuario, tipoToken);
            return Result.Ok();
        }

        public Result EliminarTokensObsoletos(int dias)
        {
            if (dias <= 0)
                return Result.Fail("La cantidad de dias debe ser mayor a cero.");

            _usuarioTokenRepository.EliminarTokensObsoletos(dias);
            return Result.Ok();
        }

        private static Result ValidarGeneracionToken(UsuarioTokenGeneracionDto dto)
        {
            if (dto == null)
                return Result.Fail("Los datos del token no pueden ser nulos.");

            if (dto.IdUsuario <= 0)
                return Result.Fail("El id del usuario debe ser mayor a cero.");

            string tipoToken = dto.TipoToken?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tipoToken))
                return Result.Fail("El tipo de token es obligatorio.");

            if (tipoToken.Length > 50)
                return Result.Fail("El tipo de token no puede tener mas de 50 caracteres.");

            if (dto.MinutosExpiracion <= 0)
                return Result.Fail("Los minutos de expiracion deben ser mayores a cero.");

            return Result.Ok();
        }

        private static string NormalizarTokenPlano(string? tokenPlano)
        {
            string tokenNormalizado = tokenPlano?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tokenNormalizado))
                return string.Empty;

            for (int i = 0; i < 2; i++)
            {
                if (!ContieneSecuenciaUrlEncoded(tokenNormalizado))
                    break;

                try
                {
                    string tokenDecodificado = Uri.UnescapeDataString(tokenNormalizado);
                    if (tokenDecodificado == tokenNormalizado)
                        break;

                    tokenNormalizado = tokenDecodificado.Trim();
                }
                catch (UriFormatException)
                {
                    break;
                }
            }

            return tokenNormalizado;
        }

        private static bool ContieneSecuenciaUrlEncoded(string valor)
        {
            for (int i = 0; i < valor.Length - 2; i++)
            {
                if (valor[i] == '%' && EsHexadecimal(valor[i + 1]) && EsHexadecimal(valor[i + 2]))
                    return true;
            }

            return false;
        }

        private static bool EsHexadecimal(char caracter)
        {
            return (caracter >= '0' && caracter <= '9')
                || (caracter >= 'A' && caracter <= 'F')
                || (caracter >= 'a' && caracter <= 'f');
        }
    }
}
