using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = bibletoon_Analyzer.Test.CSharpCodeFixVerifier<
    bibletoon_Analyzer.UnoverridenToStringMethodCallAnalyzer,
    bibletoon_Analyzer.UnoverridenToStringMethodCallCodeFixProvider>;

namespace bibletoon_Analyzer.Test
{
    [TestClass]
    public class UnoverridenToStringMethodCallTest
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
                    class Sus {
                        public void Lol() {
                            var z = new Amogus();
                            var b = z.ToString();
                            var k = new Sus();
                            var a = k.ToString();
                        }
                    }

                    public class Amogus {
	                public override string ToString() {
		                return ""Aboba"";
                        }
                    }
                }";

            var expected = new DiagnosticResult(UnoverridenToStringMethodCallAnalyzer.Id, DiagnosticSeverity.Warning)
                .WithLocation(16, 37)
                .WithMessage("Method ToString is not declared in class Sus");

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
                    class Sus {
                        public void Lol() {
                            var z = new Amogus();
                            var b = z.ToString();
                            var k = new Sus();
                            var a = k.ToString();
                        }
                    }

                    public class Amogus {
	                public override string ToString() {
		                return ""Aboba"";
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
                    class Sus
                    {
                        public void Lol()
                        {
                            var z = new Amogus();
                            var b = z.ToString();
                            var k = new Sus();
                            var a = k.ToString();
                        }

                        public override string ToString()
                        {
                            StringBuilder resultSb = new StringBuilder("""");
                return resultSb.ToString();

                }
                    }

                    public class Amogus
                    {
                        public override string ToString()
                        {
                            return ""Aboba"";
                        }
                    }
                }";

            var expected = new DiagnosticResult(UnoverridenToStringMethodCallAnalyzer.Id, DiagnosticSeverity.Warning)
                .WithSpan(16, 37, 16, 49)
                .WithArguments("Sus");

            await VerifyCS.VerifyCodeFixAsync(test, expected, fix);
        }
    }
}