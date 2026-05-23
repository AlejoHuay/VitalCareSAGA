namespace FrontendVCare.Helpers
{
    public static class CiFormatoHelper
    {
        public static string ConstruirCi(string ciBase, string? ciComplemento)
        {
            string ciNormalizado = (ciBase ?? string.Empty).Trim();
            string complemento = (ciComplemento ?? string.Empty).Trim().ToUpperInvariant();

            return string.IsNullOrWhiteSpace(complemento)
                ? ciNormalizado
                : $"{ciNormalizado}-{complemento}";
        }

        public static (string CiBase, string CiComplemento) SepararCi(string? ciCompleto)
        {
            string ci = (ciCompleto ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(ci))
                return (string.Empty, string.Empty);

            string[] partes = ci.Split('-', 2, StringSplitOptions.TrimEntries);
            return partes.Length == 2
                ? (partes[0], partes[1])
                : (ci, string.Empty);
        }
    }
}
