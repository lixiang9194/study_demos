package stone.ast;

import stone.Environment.Environment;

import java.util.List;

public class DefStmnt extends ASTList {
    public DefStmnt(List<ASTree> c) { super(c); }
    public String name() { return ((ASTLeaf)child(0)).token.getText(); }
    public ParameterList parameters() { return (ParameterList)child(1); }
    public BlockStmnt body() { return (BlockStmnt)child(2); }
    public String toString() { return "(def " + name() + " " + parameters() + " " + body() + ")"; }

    @Override
    public Object eval(Environment env) {
        env.putNew(name(), new Function(parameters(), body(), env));
        return name();
    }
}
