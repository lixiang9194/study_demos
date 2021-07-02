package simpledb;

public class FieldHistogram {

    public void addValue(Field f) {
        if (f.getType().equals(Type.INT_TYPE)) {
            addValue(((IntField)f).getValue());
        } else {
            addValue(((StringField)f).getValue());
        }
    }

    public double estimateSelectivity(Predicate.Op op, Field f) {
        if (f.getType().equals(Type.INT_TYPE)) {
            return estimateSelectivity(op, ((IntField)f).getValue());
        } else {
            return estimateSelectivity(op, ((StringField)f).getValue());
        }
    }


    public double estimateSelectivity(Predicate.Op op, int v) {
        throw new UnsupportedOperationException();
    }
    public double estimateSelectivity(Predicate.Op op, String v) {
        throw new UnsupportedOperationException();
    }

    public void addValue(int v) {
        throw new UnsupportedOperationException();
    }

    public void addValue(String s) {
        throw new UnsupportedOperationException();
    }

}