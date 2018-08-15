package stone.ast;

import stone.token.Token;

public class StringLiteral extends ASTLeaf{
    public StringLiteral(Token t) { super(t); }
    public String value() { return token.getText(); }
}
