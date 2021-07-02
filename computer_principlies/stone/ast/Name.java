package stone.ast;

import stone.Environment.Environment;
import stone.exception.StoneException;
import stone.token.Token;

public class Name extends ASTLeaf {
    public Name (Token t) { super(t); }
    public String name() { return token.getText(); }

    @Override
    public Object eval(Environment env) {
        Object value = env.get(name());
        if (value == null)
            throw new StoneException("undefined name: " + name(), this);
        else
            return value;
    }
}
