@echo off
REM generate resource resX file and copy to destination
REM csc GenerateResource.cs
rem GenerateResource.exe
rem move Content.resx ..\IncludeResources
rem del GenerateResource.exe

REM generate a fresh copy of parser.cs
gppg /gplex /nolines gplex.y
move parser.cs ..\

REM generate a fresh copy of Scanner.cs
gplex gplex.lex
move Scanner.cs ..\

if not exist GplexBuffers.cs goto finish
move GplexBuffers.cs ..\

:finish
REM Ended

