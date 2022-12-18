using System.Net.Mime;
using System.Text.RegularExpressions;

namespace nyasharp.Interpreter.Natives;

public class UwU
{
    public static string uwuify(string text)
    {
        var txt = text;
        var tmp = txt;
        for (int i = 1; i < txt.Length - 1; i++)
        {
            char previous = txt[i - 1];
            char current = txt[i];
            char next = txt[i + 1];

            if (!IsVowel(previous) && !IsVowel(next) && char.ToLower(current) == 'o' && !(char.IsWhiteSpace(next) || next == txt[^1]) && !char.IsWhiteSpace(previous))
            {
                var uChar = current == 'o' ? "u" : "U";
                tmp = tmp.Replace(previous.ToString() + current + next, previous + uChar + next);
            }
        }

        txt = tmp;
        var tmp1 = txt.Split(" ");
        foreach (var str in tmp1)
        {
            if (str.ToLower().EndsWith('e') && str.Length > 3) txt = txt.Replace(str, str.Substring(0, str.Length - 1));
        }
        
        
        string[,] table = { { "r", "w" }, { "l", "w" }, { "you", "chu" } };
        for (var i = 0; i < table.Length / 2; i++)
        {
            txt = txt.Replace(table[i, 0], table[i, 1]);
        }

        return txt;
    }

    private static bool IsVowel(char? c)
    {
        if (c == null) return false;
        return "aeiouAEIOU".Contains((char)c);
    }
}