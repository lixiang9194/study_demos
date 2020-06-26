package simpledb;

import simpledb.Predicate.Op;

/** A class to represent a fixed-width histogram over a single integer-based field.
 */
public class IntHistogram extends FieldHistogram {

    private int buckets;
    private int min;
    private int max;
    private int[] arr;
    private int sum;
    private double width;

    /**
     * Create a new IntHistogram.
     * 
     * This IntHistogram should maintain a histogram of integer values that it receives.
     * It should split the histogram into "buckets" buckets.
     * 
     * The values that are being histogrammed will be provided one-at-a-time through the "addValue()" function.
     * 
     * Your implementation should use space and have execution time that are both
     * constant with respect to the number of values being histogrammed.  For example, you shouldn't 
     * simply store every value that you see in a sorted list.
     * 
     * @param buckets The number of buckets to split the input value into.
     * @param min The minimum integer value that will ever be passed to this class for histogramming
     * @param max The maximum integer value that will ever be passed to this class for histogramming
     */
    public IntHistogram(int buckets, int min, int max) {
        //此处增加了最大值、最小值两边的两个桶，以便估算时落入相应桶
        arr = new int[buckets + 2];
        this.buckets = buckets;
        this.min = min;
        this.max = max;
        width = (max - min) / (double)buckets;
    }

    private int getIndex(int v)
    {
        if (v > max) {
            return buckets + 1;
        } else if (v == max) {
            return buckets;
        } else if (v < min) {
            return 0;
        }
        int index = buckets * (v - min) / (max - min) + 1;
        return index;
    }
    /**
     * Add a value to the set of values that you are keeping a histogram of.
     * @param v Value to add to the histogram
     */
    public void addValue(int v) {
        int index = getIndex(v);
        arr[index]++;
        sum++;
    }

    /**
     * Estimate the selectivity of a particular predicate and operand on this table.
     * 
     * For example, if "op" is "GREATER_THAN" and "v" is 5, 
     * return your estimate of the fraction of elements that are greater than 5.
     * 
     * @param op Operator
     * @param v Value
     * @return Predicted selectivity of this particular operator and value
     */
    public double estimateSelectivity(Predicate.Op op, int v) {
        switch (op) {
            case EQUALS:
            case GREATER_THAN:
            case LESS_THAN:
                return estimateSingleSelectivity(op, v);
            case LESS_THAN_OR_EQ:
                return estimateSingleSelectivity(Op.EQUALS, v) + estimateSingleSelectivity(Op.LESS_THAN, v);
            case GREATER_THAN_OR_EQ:
                return estimateSingleSelectivity(Op.EQUALS, v) + estimateSingleSelectivity(Op.GREATER_THAN, v);
            case NOT_EQUALS:
                return 1 - estimateSingleSelectivity(Op.EQUALS, v);
            default:
                break;
        }

        return 0;
    }

    private double estimateSingleSelectivity(Predicate.Op op, int v) {
        //我们估算实际是按整数宽度分布估算的，但桶过多时宽度可能小于 1，需要取 1 处理
        double divdeWidth = width > 1 ? width : 1;
        int index = getIndex(v);
        int high = arr[index];
        double select = 0;
        switch (op) {
            case EQUALS:
                select = high /  divdeWidth;
                break;
            case GREATER_THAN:
                double wid = (min + width * index) - (v + 1);
                wid = wid > 0 ? wid : 0;
                select = high * wid / divdeWidth;
                for(int i = index+1; i<buckets+1; i++) {
                    select += arr[i];
                }
                break;
            case LESS_THAN:
                double widd = (v - 1) - (min + width * (index - 1));
                widd = widd > 0 ? widd : 0;
                select = high * widd / divdeWidth;
                for(int i = index-1; i>=0; i--) {
                    select += arr[i];
                }
                break;
            default:
                break;
        }

        return select / sum;
    }
    
    /**
     * @return
     *     the average selectivity of this histogram.
     *     
     *     This is not an indispensable method to implement the basic
     *     join optimization. It may be needed if you want to
     *     implement a more efficient optimization
     * */
    public double avgSelectivity()
    {
        // some code goes here
        return 1.0;
    }
    
    /**
     * @return A string describing this histogram, for debugging purposes
     */
    public String toString() {
        double left = min - width;
        String s = "total: " + sum + "\n";
        for (int i : arr) {
            s += left + ": " + i + "\t";
            left += width;
        }
        return s + "\n";
    }
}
