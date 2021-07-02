package stone.ast;

import stone.Environment.Environment;

import java.util.List;

public class ParameterList extends ASTList {
    public ParameterList(List<ASTree> c) { super(c); }
    public String name(int i) { return ((ASTLeaf)child(i)).token.getText(); }
    public int size() { return numChildren(); }

    public void eval(Environment env, int index, Object value) {
        ((Environment)env).putNew(name(index), value);
    }
}
