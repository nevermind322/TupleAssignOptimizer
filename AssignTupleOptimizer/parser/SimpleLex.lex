%using QUT.Gppg;
%using System.Linq;
%using AssignTupleOptimizer;

%namespace TupleParser

local 	[a-z]
nonLocal   [A-Z] 

%%

{local}  { 
  yylval.sVal = new Symbol(yytext);
  return (int) Tokens.Local;
}

{nonLocal}  { 
  yylval.sVal = new Symbol(yytext);
  yylval.sVal.fromOuterScope = true;
  return (int) Tokens.NonLocal;
}

"=" { return (int)Tokens.ASSIGN; }
"(" { return (int)Tokens.LPAREN; }
")" { return (int)Tokens.RPAREN; }
"," { return (int)Tokens.COLUMN; }

[^ \t\r\n] {
	LexError();
}

%{
  yylloc = new LexLocation(tokLin, tokCol, tokELin, tokECol);
%}

%%

public override void yyerror(string format, params object[] args) // обработка синтаксических ошибок
{
  var ww = args.Skip(1).Cast<string>().ToArray();
  string errorMsg = string.Format("({0},{1}): Встречено {2}, а ожидалось {3}", yyline, yycol, args[0], string.Join(" или ", ww));
  throw new Exception(errorMsg);
}

public void LexError()
{
  string errorMsg = string.Format("({0},{1}): Неизвестный символ {2}", yyline, yycol, yytext);
  throw new Exception(errorMsg);
}

class ScannerHelper 
{

  static ScannerHelper() 
  {
  }
  
}
