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

            if (!IsVowel(previous) && !IsVowel(next) && current == 'o')
            {
                tmp = tmp.Replace(previous + "o" + next, previous + "u" + next);
            }
            
            if (!IsVowel(previous) && current == 'e' && (char.IsWhiteSpace(next) || next == txt[^1]))
            {
                tmp = tmp.Replace((previous + "e" + next), previous.ToString() + next);
            }
        }

        txt = tmp;
        
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