package stone.ast;

import stone.Environment.Environment;
import stone.exception.StoneException;
import java.util.List;

public class Arguments extends Postfix {
    public Arguments(List<ASTree> c) { super(c); }
    public int size() { return numChildren(); }

    @Override
    public Object eval(Environment callerEnv, Object value) {
        if (!(value instanceof Function))
            throw new StoneException("bad function", this);
        Function func = (Function)value;
        ParameterList params = func.parameters();
        if (size() != params.size())
            throw new StoneException("bad number of arguments", this);
        Environment newEnv = func.makeEnv();
        int num = 0;
        for (ASTree a: this)
            ((ParameterList)params).eval(newEnv, num++, ((ASTree)a).eval(callerEnv));
        return ((BlockStmnt)func.body()).eval(newEnv);
    }
}
