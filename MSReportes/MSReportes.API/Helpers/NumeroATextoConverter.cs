namespace MSReportes.API.Helpers
{
    public static class NumeroATextoConverter
    {
        private static readonly string[] Unidades =
        {
            string.Empty, "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
            "diez", "once", "doce", "trece", "catorce", "quince", "dieciseis", "diecisiete",
            "dieciocho", "diecinueve"
        };

        private static readonly string[] Decenas =
        {
            string.Empty, string.Empty, "veinte", "treinta", "cuarenta", "cincuenta",
            "sesenta", "setenta", "ochenta", "noventa"
        };

        private static readonly string[] Centenas =
        {
            string.Empty, "ciento", "doscientos", "trescientos", "cuatrocientos",
            "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos"
        };

        public static string ConvertirDecimalATexto(decimal valor)
        {
            long entero = (long)Math.Truncate(valor);

            if (entero == 0)
                return "cero";

            return ConvertirEntero(entero).Trim();
        }

        private static string ConvertirEntero(long numero)
        {
            if (numero < 20)
                return Unidades[numero];

            if (numero < 30)
                return numero == 20 ? "veinte" : $"veinti{Unidades[numero - 20]}";

            if (numero < 100)
            {
                long unidad = numero % 10;
                return unidad == 0
                    ? Decenas[numero / 10]
                    : $"{Decenas[numero / 10]} y {Unidades[unidad]}";
            }

            if (numero == 100)
                return "cien";

            if (numero < 1000)
            {
                long resto = numero % 100;
                return resto == 0
                    ? Centenas[numero / 100]
                    : $"{Centenas[numero / 100]} {ConvertirEntero(resto)}";
            }

            if (numero < 1000000)
            {
                long miles = numero / 1000;
                long resto = numero % 1000;
                string textoMiles = miles == 1 ? "mil" : $"{ConvertirEntero(miles)} mil";

                return resto == 0
                    ? textoMiles
                    : $"{textoMiles} {ConvertirEntero(resto)}";
            }

            long millones = numero / 1000000;
            long restoMillones = numero % 1000000;
            string textoMillones = millones == 1
                ? "un millon"
                : $"{ConvertirEntero(millones)} millones";

            return restoMillones == 0
                ? textoMillones
                : $"{textoMillones} {ConvertirEntero(restoMillones)}";
        }
    }
}
