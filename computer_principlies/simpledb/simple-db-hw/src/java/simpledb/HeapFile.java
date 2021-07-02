package simpledb;

import java.io.*;
import java.util.*;

/**
 * HeapFile is an implementation of a DbFile that stores a collection of tuples
 * in no particular order. Tuples are stored on pages, each of which is a fixed
 * size, and the file is simply a collection of those pages. HeapFile works
 * closely with HeapPage. The format of HeapPages is described in the HeapPage
 * constructor.
 * 
 * @see simpledb.HeapPage#HeapPage
 * @author Sam Madden
 */
public class HeapFile implements DbFile {

    private File dbFile;
    private RandomAccessFile reader;
    private TupleDesc td;

    /**
     * Constructs a heap file backed by the specified file.
     * 
     * @param f
     *            the file that stores the on-disk backing store for this heap
     *            file.
     */
    public HeapFile(File f, TupleDesc td) {
        this.dbFile = f;
        try {
            this.reader = new RandomAccessFile(f, "rw");
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        }

        this.td = td;
    }

    /**
     * Returns the File backing this HeapFile on disk.
     * 
     * @return the File backing this HeapFile on disk.
     */
    public File getFile() {
        return dbFile;
    }

    /**
     * Returns an ID uniquely identifying this HeapFile. Implementation note:
     * you will need to generate this tableid somewhere to ensure that each
     * HeapFile has a "unique id," and that you always return the same value for
     * a particular HeapFile. We suggest hashing the absolute file name of the
     * file underlying the heapfile, i.e. f.getAbsoluteFile().hashCode().
     * 
     * @return an ID uniquely identifying this HeapFile.
     */
    public int getId() {
        return dbFile.getAbsoluteFile().hashCode();
    }

    /**
     * Returns the TupleDesc of the table stored in this DbFile.
     * 
     * @return TupleDesc of this DbFile.
     */
    public TupleDesc getTupleDesc() {
        return td;
    }

    // see DbFile.java for javadocs
    public Page readPage(PageId pid) {
        int pageSize = BufferPool.getPageSize();
        int pgNo = pid.getPageNumber();
        Page page = null;

        try {
            byte[] data = new byte[pageSize];
            reader.seek(pgNo * pageSize);
            reader.read(data);
            page = new HeapPage((HeapPageId) pid, data);
        } catch (IOException e) {
            e.printStackTrace();
        }
        return page;
    }

    // see DbFile.java for javadocs
    public void writePage(Page page) throws IOException {
        int pageSize = BufferPool.getPageSize();
        int pgNo = page.getId().getPageNumber();
        reader.seek(pgNo * pageSize);
        reader.write(page.getPageData());
    }

    private synchronized HeapPage addPage(TransactionId tid) throws IOException, TransactionAbortedException,
            DbException {
        HeapPageId pid = new HeapPageId(getId(), numPages());
        Database.getBufferPool().lockPage(tid, pid, Permissions.READ_WRITE);
        byte[] data = HeapPage.createEmptyPageData();
        reader.seek(reader.length());
        reader.write(data);
        Page page = Database.getBufferPool().getPage(tid, pid, Permissions.READ_WRITE);
        return (HeapPage)page;
    }

    /**
     * Returns the number of pages in this HeapFile.
     */
    public int numPages() {
        return (int)dbFile.length() / BufferPool.getPageSize();
    }

    // see DbFile.java for javadocs
    public ArrayList<Page> insertTuple(TransactionId tid, Tuple t)
            throws DbException, IOException, TransactionAbortedException {
        HeapPage page = null;
        HeapPageId pid;
        int pgNo = 0;

        for (; pgNo<numPages(); pgNo++) {
            pid = new HeapPageId(getId(), pgNo);
            page = (HeapPage)Database.getBufferPool().getPage(tid, pid, Permissions.READ_ONLY);
            if (page.getNumEmptySlots() > 0) {
                Database.getBufferPool().lockPage(tid, pid, Permissions.READ_WRITE);
                if (page.getNumEmptySlots() == 0) {
                    Database.getBufferPool().releasePage(tid, pid);
                    continue;
                }
                break;
            }
            Database.getBufferPool().releasePage(tid, pid);
            page = null;
        }

        if (page == null) {
            page = addPage(tid);
        }

        page.insertTuple(t);
        page.markDirty(true, tid);
        return new ArrayList<>(Arrays.asList(page));
    }

    // see DbFile.java for javadocs
    public ArrayList<Page> deleteTuple(TransactionId tid, Tuple t) throws DbException,
            TransactionAbortedException {
        PageId pid = t.getRecordId().getPageId();
        HeapPage page = (HeapPage)Database.getBufferPool().getPage(tid, pid, Permissions.READ_WRITE);
        page.deleteTuple(t);
        return new ArrayList<>(Arrays.asList(page));
    }

    // see DbFile.java for javadocs
    public DbFileIterator iterator(TransactionId tid) {
        return new DbFileIterator(){
            private int pgNo;
            private Iterator<Tuple> pgTps;

            @Override
            public void rewind() throws DbException, TransactionAbortedException {
                this.open();
            }

            @Override
            public void open() throws DbException, TransactionAbortedException {
                this.pgNo = 0;
                PageId pid = new HeapPageId(getId(), pgNo);
                HeapPage page = (HeapPage)Database.getBufferPool().getPage(tid, pid, Permissions.READ_ONLY);
                this.pgTps = page.iterator();
            }

            @Override
            public Tuple next() throws DbException, TransactionAbortedException, NoSuchElementException {
                if (!this.hasNext()) {
                    throw new NoSuchElementException("no more tuples in file");
                }
                return this.pgTps.next();
            }

            @Override
            public boolean hasNext() throws DbException, TransactionAbortedException {
                if (this.pgTps == null) {
                    return false;
                }
                if (this.pgTps.hasNext()) {
                    return true;
                }
                if (++pgNo < numPages()) {
                    PageId pid = new HeapPageId(getId(), pgNo);
                    HeapPage page = (HeapPage)Database.getBufferPool().getPage(tid, pid, Permissions.READ_ONLY);
                    this.pgTps = page.iterator();
                    return hasNext();
                }

                return false;
            }

            @Override
            public void close() {
                this.pgNo = 0;
                this.pgTps = null;
            }
        };
    }

}

