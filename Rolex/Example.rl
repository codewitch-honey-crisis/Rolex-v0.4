@options namespace="RolexDemo"
integerLiteral%= '(0x[0-9A-Fa-f]{1,16}|([0-9]+))([Uu][Ll]?|[Ll][Uu]?)?'
floatLiteral= '(([0-9]+)(\.[0-9]+)?([Ee][\+\-]?[0-9]+)?[DdMmFf]?)|((\.[0-9]+)([Ee][\+\-]?[0-9]+)?[DdMmFf]?)'
stringLiteral= '\"(\\([\\\"\'abfnrtv0]|[0-7]{3}|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|U[0-9A-Fa-f]{8})|[^"])*\"'
whitespace<hidden>='[ \t\r\n\v\f]+'
lineComment<hidden>='//[^\n]*'
blockComment<hidden,blockEnd="*/">="/*"
identifier='[_[:IsLetter:]][_[:IsLetterOrDigit:]]*'