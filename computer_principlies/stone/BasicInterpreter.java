package stone;

import stone.Environment.Environment;
import stone.ast.ASTree;
import stone.ast.NullStmnt;
import stone.exception.ParseException;
import stone.test.CodeDialog;
import stone.token.Token;

public class BasicInterpreter {
    private Lexer lexer;
    private BasicParser bp;
    private Environment env;
    public BasicInterpreter(Lexer lexer, BasicParser bp, Environment env) {
        this.lexer = lexer;
        this.bp = bp;
        this.env = env;
    }

    public void run() throws ParseException {
        while (lexer.peek(0) != Token.EOF) {
            ASTree t = bp.parse(lexer);
            if (!(t instanceof NullStmnt)) {
                Object r = ((ASTree)t).eval(env);
                System.out.println("=> " + r);
            }
        }
    }
}
