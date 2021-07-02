package stone.optimize;

import static javassist.gluonj.GluonJ.revise;
import javassist.gluonj.*;
import stone.Environment.Environment;
import stone.Environment.Symbols;
import stone.ast.*;
import stone.exception.StoneException;
import stone.token.Token;
import java.util.List;

@Reviser
public class EnvOptimizer {
    @Reviser
    public static interface EnvEx extends Environment {
        Symbols symbols();
        void put(int nest, int index, Object value);
        Object get(int nest, int index);
        void putNew(String name, Object value);
        Environment where(String name);
    }

    @Reviser
    public static abstract class ASTreeEx extends ASTree {
        public void lookup(Symbols syms) {}
    }

    @Reviser
    public static class ASTListEx extends ASTList {
        public ASTListEx(List<ASTree> c) { super(c); }
        public void lookup(Symbols syms) {
            for (ASTree t: this) {
                ((ASTreeEx)t).lookup(syms);
            }
        }
    }

    @Reviser
    public static class DefStmntEx extends DefStmnt {
        protected int index, size;
        public DefStmntEx(List<ASTree> c) { super(c); }
        public void lookup(Symbols syms) {
            index = syms.putNew(name());
            size = ClosureEx.lookup(syms, parameters(), body());
        }
        public Object eval(Environment env) {
            ((EnvEx)env).put(0, index, new OptFunction(parameters(), body(), env, size));
            return name();
        }
    }

    @Reviser
    public static class ClosureEx extends Closure {
        protected int size = -1;
        public ClosureEx(List<ASTree> c) { super(c); }
        public void lookup(Symbols syms) {
            size = lookup(syms, parameters(), body());
        }
        public Object eval(Environment env) {
            return new OptFunction(parameters(), body(), env, size);
        }
        public static int lookup(Symbols syms, ParameterList params, BlockStmnt body) {
            Symbols newSyms = new Symbols(syms);
            ((ParamsEx)params).lookup(newSyms);
            ((ASTreeEx)revise(body)).lookup(newSyms);
            return newSyms.size();
        }
    }

    @Reviser
    public static class ParamsEx extends ParameterList {
        protected int[] offsets = null;
        public ParamsEx(List<ASTree> c) { super(c); }
        public void lookup(Symbols syms) {
            int s = size();
            offsets = new int[s];
            for (int i= 0; i < s; i++)
                offsets[i] = syms.putNew(name(i));
        }
        public void eval(Environment env, int index, Object value) {
            ((EnvEx)env).put(0, offsets[index], value);
        }
    }

    @Reviser
    public static class NameEx extends Name {
        protected static final int UNKNOWN = -1;
        protected int nest, index;

        public NameEx(Token t) {
            super(t);
            index = UNKNOWN;
        }

        public void lookup(Symbols syms) {
            Symbols.Location loc = syms.get(name());
            if (loc == null)
                throw new StoneException("undefined name: " + name(), this);
            else {
                nest = loc.nest;
                index = loc.index;
            }
        }

        public void lookupForAssign(Symbols syms) {
            Symbols.Location loc = syms.put(name());
            nest = loc.nest;
            index = loc.index;
        }

        @Override
        public Object eval(Environment env) {
            if (index == UNKNOWN)
                return env.get(name());
            else
                return ((EnvEx) env).get(nest, index);
        }

        public void evalForAssign(Environment env, Object value) {
            if (index == UNKNOWN)
                env.put(name(), value);
            else
                ((EnvEx) env).put(nest, index, value);
        }
    }

    @Reviser
    public static class BinaryEx extends BinaryExpr {
        public BinaryEx(List<ASTree> c) { super(c); }
        public void lookup(Symbols syms) {
            ASTree left = left();
            if ("=".equals(operator())) {
                if (left instanceof Name) {
                    ((NameEx)left).lookupForAssign(syms);
                    ((ASTreeEx)right()).lookup(syms);
                    return;
                }
            }
            ((ASTreeEx)left).lookup(syms);
            ((ASTreeEx)right()).lookup(syms);
        }
       @Override
       protected Object computeAssign(Environment env, Object rvalue) {
            ASTree l = left();
            if (l instanceof Name) {
                ((NameEx)l).evalForAssign(env, rvalue);
                return rvalue;
            }
            else
                return super.computeAssign(env, rvalue);
        }
    }
}
