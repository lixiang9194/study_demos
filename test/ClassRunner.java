package stone.test;

import stone.BasicInterpreter;
import stone.ClassParser;
import stone.ClosureParser;
import stone.Environment.Natives;
import stone.Environment.NestedEnv;
import stone.Lexer;

public class ClassRunner {
    public static void main(String[] args) throws Throwable {
        Lexer lexer = new Lexer(new CodeDialog());
        BasicInterpreter bi = new BasicInterpreter(lexer, new ClassParser(), new Natives().environment(new NestedEnv()));
        bi.run();
    }
}
/*
class Position {
	x = y = 0
	def move (nx, ny) {
		x = nx; y = ny;
	}
}
p = Position.new
p.move(3,4)
p.x = 10
print p.x + p.y
 */