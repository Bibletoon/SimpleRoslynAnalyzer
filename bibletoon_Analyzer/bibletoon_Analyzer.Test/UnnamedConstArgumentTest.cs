using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using VerifyCS = bibletoon_Analyzer.Test.CSharpCodeFixVerifier<
    bibletoon_Analyzer.UnnamedConstArgumentAnalyzer,
    bibletoon_Analyzer.UnnamedConstArgCodeFixProvider>;

namespace bibletoon_Analyzer.Test
{
    [TestClass]
    public class UnnamedConstArgumentTest
    {
        [TestMethod]
        public async Task TestEmptyCode()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [TestMethod]
        public async Task TestDiagnostic()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Aboba
    {   
        public void Xd(long argA, int argB, int argC) {}

        public void Zd() {
            Func<int, int> aboba = (a) => { return a + 2; };
            aboba.Invoke(12);
        }
       
    }
}";

            var expected = new DiagnosticResult(UnnamedConstArgumentAnalyzer.Id, DiagnosticSeverity.Warning)
                .WithLocation(17, 26);

            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
        
        [TestMethod]
        public async Task TestCodeFix()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Aboba
    {   
        public void Xd(long argA, int argB, int argC) {}

        public void Zd() {
            int arg = 2000;
            Xd(1000, arg, argC: 3000);
        }
       
    }
}";
            
            var fix = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class Aboba
    {   
        public void Xd(long argA, int argB, int argC) {}

        public void Zd() {
            int arg = 2000;
            Xd(argA: 1000, arg, argC: 3000);
        }
       
    }
}";

            var diagnosticResult = new DiagnosticResult(UnnamedConstArgumentAnalyzer.Id, DiagnosticSeverity.Warning)
                .WithLocation(17, 16);
            await VerifyCS.VerifyCodeFixAsync(test, diagnosticResult, fix);
        }
    }
}