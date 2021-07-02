package stone.test;

import stone.BasicInterpreter;
import stone.ClosureParser;
import stone.Environment.NestedEnv;
import stone.Lexer;

public class ClosureRunner {
    public static void main(String[] args) throws Throwable {
        Lexer lexer = new Lexer(new CodeDialog());
        BasicInterpreter bi = new BasicInterpreter(lexer, new ClosureParser(), new NestedEnv());
        bi.run();
    }
}
/*
测试用例
def fib (n)  {
	if n < 2  {
		n
	} else {
		fib(n-1) + fib(n-2)
	}
}
t = currentTime()
fib(33)
print currentTime() - t + "msec"
*/
