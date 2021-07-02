package simpledb;

import java.io.Serializable;
import java.util.*;

/**
 * TupleDesc describes the schema of a tuple.
 */
public class TupleDesc implements Serializable {

    /**
     * A help class to facilitate organizing the information of each field
     * */
    public static class TDItem implements Serializable {

        private static final long serialVersionUID = 1L;

        /**
         * The type of the field
         * */
        public final Type fieldType;
        
        /**
         * The name of the field
         * */
        public final String fieldName;

        public TDItem(Type t, String n) {
            this.fieldName = n;
            this.fieldType = t;
        }

        public String toString() {
            return fieldName + "(" + fieldType + ")";
        }
    }

    /**
     * @return
     *        An iterator which iterates over all the field TDItems
     *        that are included in this TupleDesc
     * */
    public Iterator<TDItem> iterator() {
        return new Iterator<TupleDesc.TDItem>(){
            private int id = 0;

            @Override
            public TDItem next() {
                if (!hasNext()) {
                    throw new NoSuchElementException("no more items in tupleDesc");
                }
                return tdItems[id++];
            }

            @Override
            public boolean hasNext() {
                if (id < numFields()) {
                    return true;
                }
                return false;
            }
        };
    }

    private static final long serialVersionUID = 1L;
    private TDItem[] tdItems;

    /**
     * Create a new TupleDesc with typeAr.length fields with fields of the
     * specified types, with associated named fields.
     * 
     * @param typeAr
     *            array specifying the number of and types of fields in this
     *            TupleDesc. It must contain at least one entry.
     * @param fieldAr
     *            array specifying the names of the fields. Note that names may
     *            be null.
     */
    public TupleDesc(Type[] typeAr, String[] fieldAr) {
        int size = typeAr.length;
        this.tdItems = new TDItem[size];

        for (int i=0; i<size; i++) {
            this.tdItems[i] = new TDItem(typeAr[i], fieldAr[i]);
        }
    }

    /**
     * Constructor. Create a new tuple desc with typeAr.length fields with
     * fields of the specified types, with anonymous (unnamed) fields.
     * 
     * @param typeAr
     *            array specifying the number of and types of fields in this
     *            TupleDesc. It must contain at least one entry.
     */
    public TupleDesc(Type[] typeAr) {
        this(typeAr, new String[typeAr.length]);
    }

    /**
     * @return the number of fields in this TupleDesc
     */
    public int numFields() {
        return this.tdItems.length;
    }

    protected TDItem getTDItem(int i) throws NoSuchElementException {
        if (i>=this.numFields()) {
            throw new NoSuchElementException("no item" + i + "in tupleDesc");
        }
        return this.tdItems[i];
    }

    /**
     * Gets the (possibly null) field name of the ith field of this TupleDesc.
     * 
     * @param i
     *            index of the field name to return. It must be a valid index.
     * @return the name of the ith field
     * @throws NoSuchElementException
     *             if i is not a valid field reference.
     */
    public String getFieldName(int i) throws NoSuchElementException {
        return this.getTDItem(i).fieldName;
    }

    /**
     * Gets the type of the ith field of this TupleDesc.
     * 
     * @param i
     *            The index of the field to get the type of. It must be a valid
     *            index.
     * @return the type of the ith field
     * @throws NoSuchElementException
     *             if i is not a valid field reference.
     */
    public Type getFieldType(int i) throws NoSuchElementException {
        return this.getTDItem(i).fieldType;
    }

    /**
     * Find the index of the field with a given name.
     * 
     * @param name
     *            name of the field.
     * @return the index of the field that is first to have the given name.
     * @throws NoSuchElementException
     *             if no field with a matching name is found.
     */
    public int fieldNameToIndex(String name) throws NoSuchElementException {
        if (name == null) {
            throw new NoSuchElementException();
        }

        for (int i=0; i<this.tdItems.length; i++) {
            if (name.equals(this.tdItems[i].fieldName)) {
                return i;
            }
        }
        throw new NoSuchElementException("no item" + name + "in tupleDesc");
    }

    /**
     * @return The size (in bytes) of tuples corresponding to this TupleDesc.
     *         Note that tuples from a given TupleDesc are of a fixed size.
     */
    public int getSize() {
        int size = 0;
        for(int i=0; i<this.tdItems.length; i++) {
            size += this.tdItems[i].fieldType.getLen();
        }
        return size;
    }

    /**
     * Merge two TupleDescs into one, with td1.numFields + td2.numFields fields,
     * with the first td1.numFields coming from td1 and the remaining from td2.
     * 
     * @param td1
     *            The TupleDesc with the first fields of the new TupleDesc
     * @param td2
     *            The TupleDesc with the last fields of the TupleDesc
     * @return the new TupleDesc
     */
    public static TupleDesc merge(TupleDesc td1, TupleDesc td2) {
        int size1 = td1.numFields();
        int size2 = td2.numFields();
        int size = size1 + size2;

        Type[] typeAr = new Type[size];
        String[] nameAr = new String[size];

        for(int i = 0; i<size1; i++) {
            typeAr[i] = td1.getFieldType(i);
            nameAr[i] = td1.getFieldName(i);
        }
        for(int i = 0; i<size2; i++) {
            typeAr[i+size1] = td2.getFieldType(i);
            nameAr[i+size1] = td2.getFieldName(i);
        }

        TupleDesc tuple = new TupleDesc(typeAr, nameAr);
        return tuple;
    }

    /**
     * Compares the specified object with this TupleDesc for equality. Two
     * TupleDescs are considered equal if they have the same number of items
     * and if the i-th type in this TupleDesc is equal to the i-th type in o
     * for every i.
     * 
     * @param o
     *            the Object to be compared for equality with this TupleDesc.
     * @return true if the object is equal to this TupleDesc.
     */

    public boolean equals(Object o) {
        if (!(o instanceof TupleDesc)) {
            return false;
        }

        TupleDesc t = (TupleDesc)o;
        if (this.numFields() != t.numFields()) {
            return false;
        }

        for (int i=0; i<this.numFields(); i++) {
            if (this.getFieldType(i) != t.getFieldType(i)) {
                return false;
            }
        }

        return true;
    }

    public int hashCode() {
        // If you want to use TupleDesc as keys for HashMap, implement this so
        // that equal objects have equals hashCode() results
        throw new UnsupportedOperationException("unimplemented");
    }

    /**
     * Returns a String describing this descriptor. It should be of the form
     * "fieldType[0](fieldName[0]), ..., fieldType[M](fieldName[M])", although
     * the exact format does not matter.
     * 
     * @return String describing this descriptor.
     */
    public String toString() {
        String str = "";

        for (TDItem t: this.tdItems) {
            str += t + ", ";
        }

        return str.substring(0, str.length()-2);
    }
}
