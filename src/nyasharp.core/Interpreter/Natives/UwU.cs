using System.Text;
using System.Text.RegularExpressions;

namespace nyasharp.Interpreter.Natives;

// see https://gist.github.com/cgytrus/737d45afdfcabab6b59833373d4b99ff for licensing info
// slightly modified to work with this project
public static class UwU {
    public class Settings {
        public float periodToExclamationChance = 0.2f;
        public float stutterChance = 0.1f;
        public float presuffixChance = 0.1f;
        public float suffixChance = 0.3f;
        public float duplicateCharactersChance = 0.4f;
        public int duplicateCharactersAmount = 3;
    }

    public static Settings settings { get; private set; } = new();
    public static Random random { get; set; } = Random.Shared;

    public static void ResetSettings() => settings = new Settings();

    private static bool GetChance(float chance) => random.NextSingle() < chance;

    private static readonly Regex escapeRegex = new("(?=[~_<>])", RegexOptions.Compiled);
    private static string EscapeString(string text) => escapeRegex.Replace(text, "\\");

    private static bool IsCaps(string text) => text == text.ToUpperInvariant();

    private static readonly List<(Regex, string)> simpleReplacements = new() {
        (new Regex("l", RegexOptions.Compiled), "w"),
        (new Regex("r", RegexOptions.Compiled), "w"),
        (new Regex("na", RegexOptions.Compiled), "nya"),
        (new Regex("ne", RegexOptions.Compiled), "nye"),
        (new Regex("ni", RegexOptions.Compiled), "nyi"),
        (new Regex("no", RegexOptions.Compiled), "nyo"),
        (new Regex("nu", RegexOptions.Compiled), "nyu"),
        (new Regex("pow", RegexOptions.Compiled), "paw"),
        (new Regex("(?<!w)ui", RegexOptions.Compiled), "wi"),
        (new Regex("(?<!w)ue", RegexOptions.Compiled), "we"),
        (new Regex("attempt", RegexOptions.Compiled), "attwempt"),
        (new Regex("config", RegexOptions.Compiled), "cwonfig")
    };

    private static readonly Dictionary<string, string> wordReplacements = new() {
        { "you", "uwu" },
        { "no", "nu" },
        { "oh", "ow" },
        { "attempt", "attwempt" },
        { "config", "cwonfig" }
    };

    private class SuffixChoice {
        private readonly string? _text;
        private readonly SuffixChoice[]? _choices;

        public SuffixChoice(string text) => _text = text;
        public SuffixChoice(SuffixChoice[] choices) => _choices = choices;

        public string Choose() {
            if(_choices != null)
                return _choices[random.Next(0, _choices.Length)].Choose();
            return _text ?? "";
        }
    }

    private static readonly SuffixChoice presuffixes = new(new SuffixChoice[] {
        new("~"),
        new("~~"),
        new(",")
    });

    private static readonly SuffixChoice suffixes = new(new SuffixChoice[] {
        new(":D"),
        new(new SuffixChoice[] {
            new("xD"),
            new("XD")
        }),
        new(":P"),
        new(";3"),
        new("<{^v^}>"),
        new("^-^"),
        new("x3"),
        new(new SuffixChoice[] {
            new("rawr"),
            new("rawr~"),
            new("rawr~~"),
            new("rawr x3"),
            new("rawr~ x3"),
            new("rawr~~ x3")
        }),
        new(new SuffixChoice[] {
            new("owo"),
            new("owo~"),
            new("owo~~")
        }),
        new(new SuffixChoice[] {
            new("uwu"),
            new("uwu~"),
            new("uwu~~")
        }),
        new("-.-"),
        new(">w<"),
        new(":3"),
        new(new SuffixChoice[] {
            new("nya"),
            new("nya~"),
            new("nya~~"),
            new("nyaa"),
            new("nyaa~"),
            new("nyaa~~")
        }),
        new(new SuffixChoice[] {
            new(">_<"),
            new(">-<")
        }),
        new(":flushed:"),
        new("\uD83D\uDC49\uD83D\uDC48"),
        new(new SuffixChoice[] {
           new("^^"),
           new("^^;;")
        }),
        new(new SuffixChoice[] {
            new("w"),
            new("ww")
        }),
        new(",")
    });

    private class Replacement {
        public Regex regex { get; private set; }
        private readonly Func<bool, Func<int, string, bool>, Func<string, int, string, string>> _replacement;

        public Replacement(Regex regex,
            Func<bool, Func<int, string, bool>, Func<string, int, string, string>> replacement) {
            this.regex = regex;
            _replacement = replacement;
        }

        public Func<string, int, string, string> GetReplacement(bool escape, Func<int, string, bool> isIgnoredAt) =>
            _replacement(escape, isIgnoredAt);
    }

    private static readonly Replacement[] replacements = {
        // . to !
        // match a . with a space or string end after it
        new(new Regex("\\.(?= |$)", RegexOptions.Compiled), (_, isIgnoredAt) =>
            (match, offset, text) => {
                if(isIgnoredAt(offset, text)) return match;
                return !GetChance(settings.periodToExclamationChance) ? match : "!";
            }),
        // duplicate characters
        new(new Regex("[,!]", RegexOptions.Compiled), (escape, isIgnoredAt) =>
            (match, offset, text) => {
                if(isIgnoredAt(offset, text)) return match;
                if(!GetChance(settings.duplicateCharactersChance))
                    return match;
                int amount =
                    (int)MathF.Floor((random.NextSingle() + 1f) * (settings.duplicateCharactersAmount - 1));

                StringBuilder matchBuilder = new(match, match.Length + amount);
                for(int i = 0; i < amount; i++)
                    matchBuilder.Append(',');
                match = matchBuilder.ToString();
                return match;
            }),
        // simple and word replacements
        // match a word
        new(new Regex("(?<=\\b)[a-zA-Z']+(?=\\b)", RegexOptions.Compiled), (_, isIgnoredAt) =>
            (match, offset, text) => {
                if(isIgnoredAt(offset, text)) return match;
                bool caps = IsCaps(match);
                match = match.ToLowerInvariant();
                if(wordReplacements.TryGetValue(match, out string? newMatch))
                    match = newMatch; // only replace whole words
                foreach((Regex? regex, string? replacement) in simpleReplacements) {
                    // don't replace whole words
                    bool wholeWord = false;
                    foreach(Match regexMatch in regex.Matches(match)) {
                        if(regexMatch.Value != match)
                            continue;
                        wholeWord = true;
                        break;
                    }
                    if(wholeWord) continue;
                    match = regex.Replace(match, replacement);
                }
                return caps ? match.ToUpperInvariant() : match;
            }),
        // stutter
        // match beginning of a word
        new(new Regex("(?<= |^)[a-zA-Z]", RegexOptions.Compiled), (_, isIgnoredAt) =>
            (match, offset, text) => {
                if(isIgnoredAt(offset, text)) return match;
                return !GetChance(settings.stutterChance) ? match : $"{match}-{match}";
            }),
        // suffixes
        new(new Regex("(?<=[.!?,;\\-])(?= )|(?=$)", RegexOptions.Compiled), (escape, isIgnoredAt) =>
            (match, offset, text) => {
                if(isIgnoredAt(offset, text)) return match;
                string presuffix = "";
                string suffix = "";
                if(GetChance(settings.presuffixChance))
                    presuffix = presuffixes.Choose();
                if(GetChance(settings.suffixChance))
                    suffix = $" {suffixes.Choose()}";
                string finalSuffix = $"{presuffix}{suffix}";
                if(escape) finalSuffix = EscapeString(finalSuffix);
                return finalSuffix;
            }),
    };

    public static string Uwuify(string text, bool escape, Func<int, string, bool> isIgnoredAt) {
        if(string.IsNullOrWhiteSpace(text))
            return text;

        foreach(Replacement replacement in replacements) {
            Func<string, int, string, string> func = replacement.GetReplacement(escape, isIgnoredAt);
            text = replacement.regex.Replace(text, match => func(match.Value, match.Index, text));
        }

        return text;
    }

    public static string Uwuify(string text) => Uwuify(text, false, (_, _) => false);

    public static string uwuify(string text) => Uwuify(text);
}
