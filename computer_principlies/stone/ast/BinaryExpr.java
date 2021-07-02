package stone.ast;

import stone.Environment.Environment;
import stone.exception.StoneException;
import java.util.List;

public class BinaryExpr extends ASTList {
    public BinaryExpr(List<ASTree> c) { super(c); }
    public ASTree left() { return child(0); }
    public String operator() {
        return ((ASTLeaf)child(1)).token.getText();
    }
    public ASTree right() { return child(2); }

    @Override
    public Object eval(Environment env) {
        String op = operator();
        if ("=".equals(op)) {
            Object right = ((ASTree)right()).eval(env);
            return computeAssign(env, right);
        }
        else {
            Object left = ((ASTree)left()).eval(env);
            Object right = ((ASTree)right()).eval(env);
            return computeOp(left, op, right);
        }
    }

    protected Object computeAssign(Environment env, Object rvalue) {
        ASTree left = left();
        if (left instanceof PrimaryExpr) {
                PrimaryExpr p = (PrimaryExpr)left;
                if (p.hasPostfix(0) && p.postfix(0) instanceof ArrayRef) {
                    Object a = ((PrimaryExpr)left).evalSubExpr(env, 1);
                    if (a instanceof Object[]) {
                        ArrayRef aref = (ArrayRef)p.postfix(0);
                        Object index = ((ASTree)aref.index()).eval(env);
                        if (index instanceof Integer) {
                            ((Object[])a)[(Integer)index] = rvalue;
                            return rvalue;
                        }
                    }
                    throw new StoneException("bad array access", this);
                }
                else if (p.hasPostfix(0) && p.postfix(0) instanceof Dot) {
                    Object t = ((PrimaryExpr)left).evalSubExpr(env, 1);
                    if (t instanceof StoneObject)
                        return setField((StoneObject)t, (Dot)p.postfix(0), rvalue);
                }
            }
        if (left instanceof Name) {
            env.put(((Name)left).name(), rvalue);
            return rvalue;
        }
        else
            throw new StoneException("bad assignment", this);
    }

    protected Object computeOp(Object left, String op, Object right) {
        if (left instanceof Integer && right instanceof Integer)
            return computeNumber((Integer)left, op, (Integer)right);
        else if (op.equals("+"))
            return String.valueOf(left) + String.valueOf(right);
        else if (op.equals("==")) {
            if (left == null)
                return right == null ? TRUE : FALSE;
            else
                return left.equals(right) ? TRUE : FALSE;
        }
        else
            throw new StoneException("Bad type", this);
    }
    protected Object computeNumber(Integer left, String op, Integer right) {
        int a = left.intValue();
        int b = right.intValue();
        if (op.equals("+"))
            return a + b;
        else if (op.equals("-"))
            return a - b;
        else if (op.equals("*"))
            return a * b;
        else if (op.equals("/"))
            return a / b;
        else if (op.equals("%"))
            return a % b;
        else if (op.equals("=="))
            return a == b ? TRUE : FALSE;
        else if (op.equals(">"))
            return a > b ? TRUE : FALSE;
        else if (op.equals("<"))
            return a < b ? TRUE : FALSE;
        else
            throw new StoneException("bad operator", this);
    }

    protected Object setField(StoneObject obj, Dot expr, Object rvalue) {
        String name = expr.name();
        try {
            obj.write(name, rvalue);
            return rvalue;
        } catch (StoneObject.AccessException e) {
            throw new StoneException("bad member access " + location() + ": " + name);
        }
    }
}
