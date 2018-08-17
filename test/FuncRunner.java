package stone.test;

import stone.BasicInterpreter;
import stone.Environment.NestedEnv;
import stone.FuncParser;
import stone.Lexer;

public class FuncRunner {
    public static void main(String[] args) throws Throwable {
        Lexer lexer = new Lexer(new CodeDialog());
        BasicInterpreter bi = new BasicInterpreter(lexer, new FuncParser(), new NestedEnv());
        bi.run();
    }
}
