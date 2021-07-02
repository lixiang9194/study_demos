package simpledb;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.NoSuchElementException;
import java.util.Map.Entry;

/**
 * Knows how to compute some aggregate over a set of IntFields.
 */
public class IntegerAggregator implements Aggregator {

    private static final long serialVersionUID = 1L;
    private int gbField;
    private int aField;
    private Op what;
    private Map<Field, Integer> aggrAns;
    private Map<Field, Integer> aggrCount;
    private int ans;
    private int count;
    private TupleDesc td;
    private List<Tuple> tuples;

    /**
     * Aggregate constructor
     * 
     * @param gbfield
     *            the 0-based index of the group-by field in the tuple, or
     *            NO_GROUPING if there is no grouping
     * @param gbfieldtype
     *            the type of the group by field (e.g., Type.INT_TYPE), or null
     *            if there is no grouping
     * @param afield
     *            the 0-based index of the aggregate field in the tuple
     * @param what
     *            the aggregation operator
     */

    public IntegerAggregator(int gbfield, Type gbfieldtype, int afield, Op what) {
        this.gbField = gbfield;
        this.aField = afield;
        this.what = what;
        this.tuples = new ArrayList<>();

        if (gbfield == NO_GROUPING) {
            this.td = new TupleDesc(new Type[]{Type.INT_TYPE});
        } else {
            this.aggrAns = new HashMap<>();
            this.aggrCount = new HashMap<>();
            this.td = new TupleDesc(new Type[]{gbfieldtype, Type.INT_TYPE});
        }
    }

    /**
     * Merge a new tuple into the aggregate, grouping as indicated in the
     * constructor
     * 
     * @param tup
     *            the Tuple containing an aggregate field and a group-by field
     */
    public void mergeTupleIntoGroup(Tuple tup) {
        int af = ((IntField)tup.getField(aField)).getValue();
        if (gbField == NO_GROUPING) {
            switch (what) {
                case MIN:
                    ans = Math.min(ans, af);
                    break;
                case MAX:
                    ans = Math.max(ans, af);
                    break;
                case COUNT:
                    ans += 1;
                    break;
                case AVG:
                    count += 1;
                case SUM:
                    ans += af;
                    break;
                default:
                    throw new UnsupportedOperationException("no supported aggregate operation");
            }
        } else {
            Field gbf = tup.getField(gbField);
            Integer aAns = aggrAns.get(gbf);
            Integer aCount = aggrCount.get(gbf);
            if (aAns == null) {
                if (what == Op.COUNT) {
                    aAns = 1;
                } else {
                    aAns = af;
                    aCount = 1;
                }
            } else {
                switch (what) {
                    case MIN:
                        aAns = Math.min(aAns, af);
                        break;
                    case MAX:
                        aAns = Math.max(aAns, af);
                        break;
                    case COUNT:
                        aAns += 1;
                        break;
                    case AVG:
                        aCount += 1;
                    case SUM:
                        aAns += af;
                        break;
                    default:
                        throw new UnsupportedOperationException("no supported aggregate operation");
                }
            }
            aggrAns.put(gbf, aAns);
            if (what == Op.AVG) {
                aggrCount.put(gbf, aCount);
            }
        }
    }

    /**
     * Create a OpIterator over group aggregate results.
     * 
     * @return a OpIterator whose tuples are the pair (groupVal, aggregateVal)
     *         if using group, or a single (aggregateVal) if no grouping. The
     *         aggregateVal is determined by the type of aggregate specified in
     *         the constructor.
     */
    public OpIterator iterator() {
        return new OpIterator(){
            private Iterator<Tuple> it;

            @Override
            public void rewind() throws DbException, TransactionAbortedException {
                it = tuples.iterator();
            }

            @Override
            public void open() throws DbException, TransactionAbortedException {
                if (gbField == NO_GROUPING) {
                    if (what == Op.AVG) {
                        ans /= count;
                    }
                    Tuple t = new Tuple(td);
                    t.setField(0, new IntField(ans));
                    tuples.add(t);
                } else {
                    for (Entry<Field, Integer> e: aggrAns.entrySet()) {
                        Field f = e.getKey();
                        Integer aAns = e.getValue();
                        if (what == Op.AVG) {
                            Integer aCount = aggrCount.get(f);
                            aAns /= aCount;
                        }
                        Tuple t = new Tuple(td);
                        t.setField(0, f);
                        t.setField(1, new IntField(aAns));
                        tuples.add(t);
                    }
                }
                it = tuples.iterator();
            }

            @Override
            public Tuple next() throws DbException, TransactionAbortedException, NoSuchElementException {
                return it.next();
            }

            @Override
            public boolean hasNext() throws DbException, TransactionAbortedException {
                return it.hasNext();
            }

            @Override
            public TupleDesc getTupleDesc() {
                return td;
            }

            @Override
            public void close() {
                it = null;
            }
        };
    }

}
