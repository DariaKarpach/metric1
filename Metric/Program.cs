using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace Metric
{
    class Program
    {

        static string code;

        static void deleteLiterals()
    {
        string literalRegularExpression = @"((\'.*\')|(//.*|{[^}]*}))";
        string elseRegularExpression = @"\s*\bend\b\s*\belse\b\s*\bbegin\b";
        string replace = "";
        code = Regex.Replace(code, literalRegularExpression, replace);
        code = Regex.Replace(code, elseRegularExpression, replace, RegexOptions.IgnoreCase);
    }

       static int countingIf()
       {
           int resultIfCount = 0;
           string ifRegularExpression = @"\bif\b";

           foreach (Match Count in Regex.Matches(code, ifRegularExpression,  RegexOptions.IgnoreCase | RegexOptions.Singleline))
               resultIfCount++;

           return resultIfCount;
       }

       static int countingNesting(string textForAnalys)
       {
           string [] linesOfCode = textForAnalys.Split('\r') ;
           var stackForCounting = new Stack<string>();
           int maximalNesting = 0;
           int currentNesting = 0;
          
           for ( int countLines = 0; countLines < linesOfCode.Length; countLines++)
           {
               string [] wordsOfLine = linesOfCode[countLines].Split(new Char [] {' ', '\n', '\r'});
               for (int arrayCount = 0; arrayCount<wordsOfLine.Length; arrayCount++)
               {
                   if (wordsOfLine[arrayCount] == "if" | wordsOfLine[arrayCount] == "for" | wordsOfLine[arrayCount] == "while" | wordsOfLine[arrayCount] == "repeat" | wordsOfLine[arrayCount] == "else")
                   {
                       if (wordsOfLine[arrayCount] == "if") currentNesting++;
                       stackForCounting.Push(wordsOfLine[arrayCount]);
                   }
                   if (stackForCounting.Count != 0)
                   {
                       if (wordsOfLine[arrayCount] == "end;" & (stackForCounting.Peek() == "if"))
                       {
                           stackForCounting.Pop();
                           currentNesting--;
                        }
                       else
                       if (wordsOfLine[arrayCount] == "end;" & (stackForCounting.Peek() != "if"))
                           stackForCounting.Pop();

                       if (wordsOfLine[arrayCount] == "end")
                           for (int stackElements = stackForCounting.Count; stackElements > 1; stackElements--)
                           {
                               if (stackForCounting.Peek() == "if") currentNesting--;
                               stackForCounting.Pop();
                           }
                   }
                   if (currentNesting > maximalNesting)
                       maximalNesting = currentNesting;
               }
           }
           return maximalNesting ;
       }

       static int deleteVariableDeclarations()
       {
           List<string> listWithTypes = new List<string>();

           string identifierRegularExpression = @"[a-z_]\w*(?=\s*\:\s*|\s*,\s*)";
           string search = @"(\bvar\b|\btype\b|\bconst\b)(.*?)(?=\b(begin|var|type|const|procedure|function)\b)";
           string typeRegularExpression = @"(?<=\:\s)([a-z_]\w*)|([a-z_]\w*)(?=\s*\=)";
           string constantRegularExpression = @"(?<=const)\s*([a-z_]\w*)";
           string replace = "";

           bool flagForCycle = true;

           while (flagForCycle)
           {
               Match findVarSection = Regex.Match(code, search, RegexOptions.Singleline | RegexOptions.IgnoreCase);

               string section = findVarSection.Value;
               string [] split  = section.Split(';');

               code = code.Replace(section, replace);
              for (int arrayCount = 0; arrayCount < split.Length; arrayCount++)
              {
                  Match identifier = Regex.Match(split[arrayCount], identifierRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                  Match type = Regex.Match(split[arrayCount], typeRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                  Match constant = Regex.Match(split[arrayCount], constantRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                  if (identifier.Value != "")
                      code = Regex.Replace(code, @"\b" + identifier.Value + @"\b", replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                  if (type.Value != "") 
                      listWithTypes.Add(type.Value); 
                  if (constant.Value != "")
                      code = Regex.Replace(code, @"\b" + constant.Value + @"\b", replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);
              }
              
                   int varCount = 0;

                   foreach (Match endOfText in Regex.Matches(code, @"\b(var|type|const)\b", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                       varCount++;
                   if (varCount == 0) 
                       flagForCycle = false;
           }
           return listWithTypes.Count;
       }

       static int deleteKeywords()
       {
           string keywordRegularExpression = @"((\b(for|while|repeat|break|continue|if|case|goto|with|in)\b))";
           string additionalKeywordRegularExpression = @"((\b(uses)\b.*?(?=\;))\b(do|until|begin|end|then|downto|to|of)\b|(\b(function|procedure)\b.*?(?=\;))|(\b(uses)\b.*?(?=\;)))";
           string replace = "";
           int keywordCount = 0;

           foreach (Match count in Regex.Matches(code, keywordRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase))
               keywordCount++;

           code = Regex.Replace(code, keywordRegularExpression, replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);
           code = Regex.Replace(code, additionalKeywordRegularExpression, replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);

           return keywordCount ;
       }

       static int countingSubroutineCall()
       {
           int subroutineCallCount = 0;
           string subroutineCallRegularExpression = @"[a-z_]\w*(?=\()";

           foreach (Match count in Regex.Matches(code, subroutineCallRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase))
               subroutineCallCount++;

           return subroutineCallCount;
       }

       static int countingOperators()
       {
           int resultCount = 0;
           resultCount = deleteVariableDeclarations() + deleteKeywords();
           string operatorRegularExpression = @"[\+\-\/\*@^]|\b(mod|or|xor|not|shl|shr|and|div)\b|(\.\w*)|(\:\=)";
           string replace = "";
           foreach (Match count in Regex.Matches(code, operatorRegularExpression, RegexOptions.Singleline | RegexOptions.IgnoreCase))
               resultCount++;
           code = Regex.Replace(code, operatorRegularExpression, replace, RegexOptions.Singleline | RegexOptions.IgnoreCase);

           resultCount = resultCount + countingSubroutineCall();

           return resultCount;
       }

        static void Main(string[] args)
        {
            StreamReader fileWithCode = new StreamReader("d:\\code.txt");
            code = fileWithCode.ReadToEnd();
            fileWithCode.Close();
            
           deleteLiterals();

           int ifCount = countingIf();
           Console.Write("The number of conditional operators: ");
           Console.WriteLine( ifCount);

           int maximalNesting = countingNesting(code);
           Console.Write("Maximal nesting: ");
            Console.WriteLine(maximalNesting);

            int operatorCount = countingOperators();
            Console.Write("The number of operators: ");
            Console.WriteLine(operatorCount);


            double saturation = (double)ifCount/ operatorCount;
            Console.Write("Saturation : ");
            Console.WriteLine("{0:0.###}", saturation);

                       
            Console.ReadLine();     
        
        }
    }
}
