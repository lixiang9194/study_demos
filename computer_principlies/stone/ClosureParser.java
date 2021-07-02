package stone;

import stone.ast.Closure;
import static stone.parser.Parser.rule;

public class ClosureParser extends FuncParser {
    public ClosureParser() {
        primary.insertChoice(rule(Closure.class)
                .sep("fun").ast(paramList).ast(block));
    }
}
