package stone.ast;

import stone.Environment.Environment;

import java.util.List;

public class WhileStmnt extends ASTList {
    public WhileStmnt(List<ASTree> c) { super((c)); }
    public ASTree condition() { return child(0); }
    public ASTree body() { return child(1); }
    public String toString() {
        return "(while " + condition() + " " + body() + ")";
    }

    @Override
    public Object eval(Environment env) {
        Object result = 0;
        for (;;) {
            Object c = ((ASTree)condition()).eval(env);
            if (c instanceof Integer && ((Integer)c).intValue() == FALSE)
                return result;
            else
                result = ((ASTree)body()).eval(env);
        }
    }
}
