package stone.test;

import stone.BasicInterpreter;
import stone.ClosureParser;
import stone.Environment.NestedEnv;
import stone.Lexer;
import stone.Environment.Natives;

public class NativeRunner {
    public static void main(String[] args) throws Throwable {
        Lexer lexer = new Lexer(new CodeDialog());
        BasicInterpreter bi = new BasicInterpreter(lexer, new ClosureParser(), new Natives().environment(new NestedEnv()));
        bi.run();
    }
}