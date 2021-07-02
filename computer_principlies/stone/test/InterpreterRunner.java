package stone.test;

import stone.BasicInterpreter;
import stone.BasicParser;
import stone.Environment.NestedEnv;
import stone.Lexer;
import stone.exception.ParseException;

public class InterpreterRunner {
    public static void main(String[] args) throws ParseException {
        Lexer lexer = new Lexer(new CodeDialog());
        BasicInterpreter bi = new BasicInterpreter(lexer, new BasicParser(), new NestedEnv());
        bi.run();
    }
}
