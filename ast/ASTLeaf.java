package stone.ast;

import stone.Environment.Environment;
import stone.exception.StoneException;
import stone.token.Token;

import java.util.ArrayList;
import java.util.Iterator;

public class ASTLeaf extends ASTree {
    private static ArrayList<ASTree> empty = new ArrayList<>();
    protected Token token;
    public ASTLeaf(Token t) { token = t; }

    @Override
    public ASTree child(int i) {
        throw new IndexOutOfBoundsException();
    }

    @Override
    public int numChildren() {
        return 0;
    }

    @Override
    public Iterator<ASTree> children() {
        return empty.iterator();
    }

    public String toString() {
        return token.getText();
    }

    @Override
    public String location() {
        return "at line " + token.getLineNumber();
    }

    public Token token() {
        return token;
    }

    @Override
    public Object eval(Environment env) {
        throw new StoneException("cannot eval: " + toString(), this);
    }
}