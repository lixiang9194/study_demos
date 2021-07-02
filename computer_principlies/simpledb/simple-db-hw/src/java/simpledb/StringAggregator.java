package simpledb;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.NoSuchElementException;
import java.util.Map.Entry;

/**
 * Knows how to compute some aggregate over a set of StringFields.
 */
public class StringAggregator implements Aggregator {

    private static final long serialVersionUID = 1L;
    private int gbField;
    private int aField;
    private Map<Field, Integer> aggrCount;
    private int count;
    private TupleDesc td;
    private List<Tuple> tuples;

    /**
     * Aggregate constructor
     * @param gbfield the 0-based index of the group-by field in the tuple, or NO_GROUPING if there is no grouping
     * @param gbfieldtype the type of the group by field (e.g., Type.INT_TYPE), or null if there is no grouping
     * @param afield the 0-based index of the aggregate field in the tuple
     * @param what aggregation operator to use -- only supports COUNT
     * @throws IllegalArgumentException if what != COUNT
     */

    public StringAggregator(int gbfield, Type gbfieldtype, int afield, Op what) {
        if (what != Op.COUNT) {
            throw new IllegalArgumentException("string aggregator only support count");
        }
        this.gbField = gbfield;
        this.aField = afield;
        this.tuples = new ArrayList<>();

        if (gbfield == NO_GROUPING) {
            this.td = new TupleDesc(new Type[]{Type.INT_TYPE});
        } else {
            this.aggrCount = new HashMap<>();
            this.td = new TupleDesc(new Type[]{gbfieldtype, Type.INT_TYPE});
        }
    }

    /**
     * Merge a new tuple into the aggregate, grouping as indicated in the constructor
     * @param tup the Tuple containing an aggregate field and a group-by field
     */
    public void mergeTupleIntoGroup(Tuple tup) {
        Field af = tup.getField(aField);
        if (af.toString().isEmpty()) return;

        if (gbField == NO_GROUPING) {
            count++;
            return;
        }

        Field gbf = tup.getField(gbField);
        Integer oc = aggrCount.get(gbf);
        if (oc == null) oc = 0;
        aggrCount.put(gbf, ++oc);
    }

    /**
     * Create a OpIterator over group aggregate results.
     *
     * @return a OpIterator whose tuples are the pair (groupVal,
     *   aggregateVal) if using group, or a single (aggregateVal) if no
     *   grouping. The aggregateVal is determined by the type of
     *   aggregate specified in the constructor.
     */
    public OpIterator iterator() {
        return new OpIterator() {
            private Iterator<Tuple> it;

            @Override
            public void rewind() throws DbException, TransactionAbortedException {
                it = tuples.iterator();
            }

            @Override
            public void open() throws DbException, TransactionAbortedException {
                if (gbField == NO_GROUPING) {
                    Tuple t = new Tuple(td);
                    t.setField(0, new IntField(count));
                    tuples.add(t);
                } else {
                    for (Entry<Field, Integer> e: aggrCount.entrySet()) {
                        Tuple t = new Tuple(td);
                        t.setField(0, e.getKey());
                        t.setField(1, new IntField(e.getValue()));
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
