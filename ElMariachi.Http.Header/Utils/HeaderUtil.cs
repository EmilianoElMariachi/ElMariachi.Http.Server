namespace ElMariachi.Http.Header.Utils
{
    public class HeaderUtil
    {
        public const string SEPARATORS = "()<>@,;:\\\"/[]?={} \t";

        public static bool IsToken(string? str)
        {
            if (str == null)
                return false;

            /*

             token          = 1*<any CHAR except CTLs or separators>
             separators     = "(" | ")" | "<" | ">" | "@" | "," | ";" | ":" | "\" | <"> | "/" | "[" | "]" | "?" | "=" | "{" | "}" | SP | HT
             CTL            = <any US-ASCII control character (octets 0 - 31) and DEL (127)>

             */

            if (str.Length <= 0)
                return false;

            foreach (var c in str)
            {
                if ((c >= 0 && c <= 31) || c == 127)
                    return false;
                if (SEPARATORS.Contains(c))
                    return false;
            }

            return true;
        }

    }
}
