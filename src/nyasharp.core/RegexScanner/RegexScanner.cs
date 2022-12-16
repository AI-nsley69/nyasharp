using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nyasharp.Scanner
{
    public class RegexScanner : IScanner
    {
        public List<Token> ScanTokens(string source)
        {
            List<Token> tokens = new();
            MatchCollection matches = pattern.Matches(source); //parse all tokens to matches
            foreach (Match match in matches) 
            {
                //find which alternative was found and translate it to Token object
                if (AddIf(tokens, match, "comment",     TokenType.Null          )) { continue; }
                if (AddIf(tokens, match, "asign",       TokenType.Assign        )) { continue; }
                if (AddIf(tokens, match, "const",       TokenType.Const         )) { continue; }
                if (AddIf(tokens, match, "var",         TokenType.Var           )) { continue; }
                if (AddIf(tokens, match, "equal",       TokenType.Equal         )) { continue; }
                if (AddIf(tokens, match, "equless",     TokenType.LessEqual     )) { continue; }
                if (AddIf(tokens, match, "equmore",     TokenType.GreaterEqual  )) { continue; }
                if (AddIf(tokens, match, "less",        TokenType.Less          )) { continue; }
                if (AddIf(tokens, match, "more",        TokenType.Greater       )) { continue; }
                if (AddIf(tokens, match, "notequal",    TokenType.NotEqual      )) { continue; }
                if (AddIf(tokens, match, "add",         TokenType.Add           )) { continue; }
                if (AddIf(tokens, match, "sub",         TokenType.Sub           )) { continue; }
                if (AddIf(tokens, match, "mul",         TokenType.Mult          )) { continue; }
                if (AddIf(tokens, match, "div",         TokenType.Div           )) { continue; }
                if (AddIf(tokens, match, "mod",         TokenType.Mod           )) { continue; }
                if (AddIf(tokens, match, "or",          TokenType.Or            )) { continue; }
                if (AddIf(tokens, match, "and",         TokenType.And           )) { continue; }
                if (AddIf(tokens, match, "not",         TokenType.Not           )) { continue; }
                if (AddIf(tokens, match, "if",          TokenType.If            )) { continue; }
                if (AddIf(tokens, match, "else",        TokenType.Else          )) { continue; }
                if (AddIf(tokens, match, "for",         TokenType.For           )) { continue; }
                if (AddIf(tokens, match, "while",       TokenType.While         )) { continue; }
                if (AddIf(tokens, match, "fun",         TokenType.Func          )) { continue; }
                if (AddIf(tokens, match, "rtn",         TokenType.Return        )) { continue; }
                if (AddIf(tokens, match, "openblock",   TokenType.BlockStart    )) { continue; }
                if (AddIf(tokens, match, "closblock",   TokenType.BlockEnd      )) { continue; }
                if (AddIf(tokens, match, "openpar",     TokenType.LeftParen     )) { continue; }
                if (AddIf(tokens, match, "clospar",     TokenType.RightParen    )) { continue; }
                if (AddIf(tokens, match, "eol",         TokenType.EOF           )) { continue; }
                if (AddIf(tokens, match, "num",         TokenType.Number        )) { continue; }
                if (AddIf(tokens, match, "str",         TokenType.String        )) { continue; }
                if (AddIf(tokens, match, "true",        TokenType.True          )) { continue; }
                if (AddIf(tokens, match, "false",       TokenType.False         )) { continue; }
                if (AddIf(tokens, match, "print",       TokenType.Print         )) { continue; }
                if (AddIf(tokens, match, "iden",        TokenType.Identifier    )) { continue; }
                if (AddIf(tokens, match, "invalid",     TokenType.Null          )) { continue; }
            }
            return tokens;
        }
        //compact test and add
        private static bool AddIf(List<Token> tokens,Match match,string name,TokenType tkt)
        {
            Group gr = match.Groups[name];
            if (gr.Success)
            {
                tokens.Add(new Token(tkt, gr.Value, null, match.Index));
                return true;
            }
            return false;
        }

        // regex that match any of the tokens
        // and asign it to it own named capture group
        // its absurdly not optimal but it make the code very compact
        // and easily scallable.
        // ordering is important, especially keywords, they have to come
        // before identifiers so that they are parsed as keyword rather
        // than as identifiers
        private readonly static Regex pattern = new(@"
[\n\t ]* #catching any whitespace
(?:
      (?<comment>//)| #catch //
        (?<asign>o/)| # catch o/
       (?<const>>w<)| # catch >w<
        (?<var>>\.<)| # catch >.<
      (?<equal>\\o/)| # catch \o/
     (?<equless>_o/)| # catch _o/
    (?<equmore>\\o_)| # catch \o_
        (?<less>/o/)| # catch /o/
      (?<more>\\o\\)| # catch \o\
    (?<notequal>_o_)| # catch _o_
      (?<add>\+\.\+)| # catch +.+
        (?<sub>-\.-)| # catch -.-
      (?<mul>\+\.\*)| # catch +.*
       (?<div>-\.\*)| # catch -.*
        (?<mod>%\.%)| # catch %.%
         (?<or>v\.v)| # catch v.v
        (?<and>&\.&)| # catch &.&
           (?<not>~)| # catch ~
        (?<if>\^u\^)| # catch ^u^
      (?<else>\^e\^)| # catch ^e^
       (?<for>\^o\^)| # catch ^o^
     (?<while>\^w\^)| # catch ^w^
          (?<fun>:D)| # catch :D
          (?<rtn>c:)| # catch c:
    (?<openblock>:>)| # catch :>
    (?<closblock><:)| # catch <:
      (?<openpar>\()| # catch (
      (?<clospar>\))| # catch )
           (?<eol>;)| # catch ;
      (?<num>[0-9]+)| # catch a number literal
(?<str>""[^""\n]*"")| # catch a string literal of any length that cannot extend on multiple lines
# keywords
(?: 
       (?<true>twue)| # catch twue 
     (?<false>fawse)| # catch fawse
     (?<print>pwint)  # catch pwint
)
#(prevent trailing valid var identifier character)
(?=[^0-9a-zA-Z])|
  
(?<iden>[a-zA-Z][a-zA-Z0-9]+)| # catch an identifier
       (?<invalid>.)| # catch an a single character that was unable to be processed
)
",RegexOptions.IgnorePatternWhitespace);

    }
}