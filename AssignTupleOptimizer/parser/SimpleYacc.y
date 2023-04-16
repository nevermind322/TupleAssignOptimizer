%{
    public List<Symbol> left;
    public List<Symbol> right;
    public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) { }
%}

%output = TupleParser.cs

%union { 
			public Symbol sVal;
            public List<Symbol> tupleVal;
       }

%using System.IO;
%using AssignTupleOptimizer;

%namespace TupleParser

%start progr

%token RPAREN LPAREN ASSIGN COLUMN

%token <sVal> Local NonLocal 

%type <sVal> var 
%type <tupleVal>  tuple vars

%%

progr : tuple ASSIGN tuple {left = $1; right = $3;} 
    ;

tuple : LPAREN vars RPAREN {$$ = $2;}
    ;

vars : var {$$ = new List<Symbol>(){$1};}
    | vars COLUMN var {$1.Add($3); $$ = $1;}
    ;

var : Local {$$ = $1;}
    | NonLocal {$$ = $1;}
    ;

%%
