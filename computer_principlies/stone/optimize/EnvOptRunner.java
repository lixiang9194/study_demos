package stone.optimize;

import javassist.gluonj.util.Loader;

public class EnvOptRunner {
    public static void main(String[] args) throws Throwable {
        Loader.run(EnvOptInterpreter.class, args, EnvOptimizer.class);
    }
}
