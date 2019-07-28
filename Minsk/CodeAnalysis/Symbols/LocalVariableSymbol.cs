﻿namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name, isReadOnly, type)
        {

        }

        public override SymbolKind Kind => SymbolKind.LocalVariable;
    }
}
