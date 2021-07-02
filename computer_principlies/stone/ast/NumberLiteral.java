package stone.ast;

import stone.Environment.Environment;
import stone.token.Token;

public class NumberLiteral extends ASTLeaf {
    public NumberLiteral(Token t) { super(t);}
    public int value() { return token().getNumber(); }

    @Override
    public Object eval(Environment env) {
        return value();
    }
}
