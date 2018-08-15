package stone.token;

public class IdToken extends Token {
    private String text;
    public IdToken(int line, String id) {
        super(line);
        text = id;
    }
    public boolean isIdentifier() {
        return true;
    }
    public String getText() {
        return text;
    }
}
