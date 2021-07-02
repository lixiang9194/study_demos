package stone;

import stone.ast.*;
import stone.parser.Parser;
import static stone.parser.Parser.rule;

public class FuncParser extends BasicParser {
    Parser param = rule().identifier(reserved);
    Parser params = rule(ParameterList.class)
            .ast(param).repeat(rule().sep(",").ast(param));
    Parser paramList = rule().sep("(").maybe(params).sep(")");
    Parser def = rule(DefStmnt.class)
            .sep("def").identifier(reserved).ast(paramList).ast(block);
    Parser args = rule(Arguments.class)
            .ast(expr).repeat(rule().sep(",").ast(expr));
    Parser postfix = rule().sep("(").maybe(args).sep(")");

    //支持数组
    Parser elements = rule(ArrayLiteral.class)
            .ast(expr).repeat(rule().sep(",").ast(expr));

    public FuncParser() {
        reserved.add(")");
        reserved.add("]");
        primary.repeat(postfix);
        simple.option(args);
        program.insertChoice(def);
        primary.insertChoice(rule().sep("[").maybe(elements).sep("]"));
        postfix.insertChoice(rule(ArrayRef.class).sep("[").ast(expr).sep("]"));
    }
}
