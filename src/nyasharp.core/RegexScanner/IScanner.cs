using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nyasharp.Scanner
{
    public interface IScanner
    {
        List<Token> ScanTokens(string source);

    }


}