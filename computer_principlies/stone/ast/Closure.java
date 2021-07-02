package stone.ast;

import stone.Environment.Environment;

import java.util.List;

public class Closure extends ASTList {
    public Closure(List<ASTree> c) { super(c); }

    public ParameterList parameters() { return (ParameterList)child(0); }

    public BlockStmnt body() { return (BlockStmnt)child(1); }

    public String toString() {
        return "(fun" + parameters() + " " + body() + ")";
    }

    public Object eval(Environment env) {
        return new Function(parameters(), body(), env);
    }
}
