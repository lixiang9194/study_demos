package stone.ast;

import stone.Environment.Environment;

import java.util.List;

public class ArrayLiteral extends ASTList {
    public ArrayLiteral(List<ASTree> list) { super(list); }
    public int size() { return numChildren(); }

    @Override
    public Object eval(Environment env) {
        int s = numChildren();
        Object[] res = new Object[s];
        int i = 0;
        for (ASTree t: this)
            res[i++] = ((ASTree)t).eval(env);
        return res;
    }
}
