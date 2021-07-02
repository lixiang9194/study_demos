package stone.optimize;

import stone.ClosureParser;
import stone.BasicParser;
import stone.Environment.Environment;
import stone.Environment.Natives;
import stone.Environment.ResizableArrayEnv;
import stone.Lexer;
import stone.ast.ASTree;
import stone.ast.NullStmnt;
import stone.exception.ParseException;
import stone.test.CodeDialog;
import stone.token.Token;

public class EnvOptInterpreter {

    public static void main(String[] args) throws ParseException {
        run(new ClosureParser(), new Natives().environment(new ResizableArrayEnv()));
    }

    public static void run(BasicParser bp, Environment env) throws ParseException {
        Lexer lexer = new Lexer(new CodeDialog());
        while (lexer.peek(0) != Token.EOF) {
            ASTree t = bp.parse(lexer);
            if (!(t instanceof NullStmnt)) {
                ((EnvOptimizer.ASTreeEx)t).lookup(
                        ((EnvOptimizer.EnvEx)env).symbols());
            Object r = ((ASTree)t).eval(env);
            System.out.println("=> " + r);
            }
        }
    }
}
