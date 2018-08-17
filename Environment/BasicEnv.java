package stone.Environment;

import java.util.HashMap;

public abstract class BasicEnv implements Environment{
    protected HashMap<String, Object> values;
    public BasicEnv() {
        values = new HashMap<>();
    }
    public void put(String name, Object value) {
        values.put(name, value);
    }
    public Object get(String name) {
        return values.get(name);
    }

}
