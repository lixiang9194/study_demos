package stone.ast;

import stone.Environment.Environment;
import stone.exception.StoneException;
import java.util.List;

public class Arguments extends Postfix {
    public Arguments(List<ASTree> c) { super(c); }
    public int size() { return numChildren(); }

    @Override
    public Object eval(Environment callerEnv, Object value) {
        if (!(value instanceof NativeFunction))
            return originEval(callerEnv, value);
        else
            return nativeEval(callerEnv, value);
    }

    public Object originEval(Environment callerEnv, Object value) {
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

    public Object nativeEval(Environment callerEnv, Object value) {
        NativeFunction func = (NativeFunction)value;
        int nparams = func.numOfParameters();
        if (size() != nparams)
            throw new StoneException("bad number of arguments", this);
        Object[] args = new Object[nparams];
        int num = 0;
        for (ASTree a: this) {
            ASTree ae = (ASTree)a;
            args[num++] = ae.eval(callerEnv);
        }
        return func.invoke(args, this);
    }
}
