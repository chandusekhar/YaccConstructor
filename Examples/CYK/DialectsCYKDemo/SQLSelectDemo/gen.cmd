..\..\..\yc\YaccConstructor.exe -c ExpandEbnf -c ExpandMeta -c ExpandInnerAlt -c ExpandTopLevelAlt -c ExpandBrackets  -c "ReplaceLiterals KW_%%s" -c Linearize -c ToCNF -g CYKGenerator -i mssql.yrd
