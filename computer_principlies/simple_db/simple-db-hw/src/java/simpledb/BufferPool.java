package simpledb;

import java.io.*;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * BufferPool manages the reading and writing of pages into memory from
 * disk. Access methods call into it to retrieve pages, and it fetches
 * pages from the appropriate location.
 * <p>
 * The BufferPool is also responsible for locking;  when a transaction fetches
 * a page, BufferPool checks that the transaction has the appropriate
 * locks to read/write the page.
 * 
 * @Threadsafe, all fields are final
 */
public class BufferPool {
    /** Bytes per page, including header. */
    private static final int DEFAULT_PAGE_SIZE = 4096;

    private static int pageSize = DEFAULT_PAGE_SIZE;
    
    /** Default number of pages passed to the constructor. This is used by
    other classes. BufferPool should use the numPages argument to the
    constructor instead. */
    public static final int DEFAULT_PAGES = 50;

    private Page[] pages;
    private Map<PageId, Integer> pageInds;
    private List<Integer> freePageInds;
    private LockManager lockManager;

    /**
     * Creates a BufferPool that caches up to numPages pages.
     *
     * @param numPages maximum number of pages in this buffer pool.
     */
    public BufferPool(int numPages) {
        this.pages = new Page[numPages];
        this.pageInds = new HashMap<>();
        this.freePageInds = new LinkedList<>();
        for (int i = 0; i < numPages; i++) {
            this.freePageInds.add(i);
        }
        this.lockManager = new LockManager();
    }
    public static int getPageSize() {
      return pageSize;
    }
    
    // THIS FUNCTION SHOULD ONLY BE USED FOR TESTING!!
    public static void setPageSize(int pageSize) {
    	BufferPool.pageSize = pageSize;
    }
    
    // THIS FUNCTION SHOULD ONLY BE USED FOR TESTING!!
    public static void resetPageSize() {
    	BufferPool.pageSize = DEFAULT_PAGE_SIZE;
    }

    /**
     * Retrieve the specified page with the associated permissions.
     * Will acquire a lock and may block if that lock is held by another
     * transaction.
     * <p>
     * The retrieved page should be looked up in the buffer pool.  If it
     * is present, it should be returned.  If it is not present, it should
     * be added to the buffer pool and returned.  If there is insufficient
     * space in the buffer pool, a page should be evicted and the new page
     * should be added in its place.
     *
     * @param tid the ID of the transaction requesting the page
     * @param pid the ID of the requested page
     * @param perm the requested permissions on the page
     */
    public  Page getPage(TransactionId tid, PageId pid, Permissions perm)
        throws TransactionAbortedException, DbException {
        lockPage(tid, pid, perm);

        if (pageInds.containsKey(pid)) {
            return pages[pageInds.get(pid)];
        }

        Page page = Database.getCatalog().getDatabaseFile(pid.getTableId()).readPage(pid);

        int ind = getFreePageIndex();
        pageInds.put(pid, ind);
        pages[ind] = page;

        return page;
    }


    protected synchronized int getFreePageIndex() throws DbException {
        if (freePageInds.isEmpty()) {
            evictPage();
        }
        Integer ind = freePageInds.remove(0);
        return ind;
    }

    /**
     * Releases the lock on a page.
     * Calling this is very risky, and may result in wrong behavior. Think hard
     * about who needs to call this and why, and why they can run the risk of
     * calling it.
     *
     * @param tid the ID of the transaction requesting the unlock
     * @param pid the ID of the page to unlock
     */
    public  void releasePage(TransactionId tid, PageId pid) {
        lockManager.unLockPage(tid, pid);
    }

    /**
     * Release all locks associated with a given transaction.
     *
     * @param tid the ID of the transaction requesting the unlock
     */
    public void transactionComplete(TransactionId tid) throws IOException {
        transactionComplete(tid, true);
    }

    public void lockPage(TransactionId tid, PageId pid, Permissions perm) throws TransactionAbortedException {
        lockManager.lockPage(tid, pid, perm);
    }

    /** Return true if the specified transaction has a lock on the specified page */
    public boolean holdsLock(TransactionId tid, PageId p) {
        return lockManager.hasLock(tid, p);
    }

    /**
     * Commit or abort a given transaction; release all locks associated to
     * the transaction.
     *
     * @param tid the ID of the transaction requesting the unlock
     * @param commit a flag indicating whether we should commit or abort
     */
    public void transactionComplete(TransactionId tid, boolean commit)
        throws IOException {
        lockManager.unLock(tid);
    }

    /**
     * Add a tuple to the specified table on behalf of transaction tid.  Will
     * acquire a write lock on the page the tuple is added to and any other 
     * pages that are updated (Lock acquisition is not needed for lab2). 
     * May block if the lock(s) cannot be acquired.
     * 
     * Marks any pages that were dirtied by the operation as dirty by calling
     * their markDirty bit, and adds versions of any pages that have 
     * been dirtied to the cache (replacing any existing versions of those pages) so 
     * that future requests see up-to-date pages. 
     *
     * @param tid the transaction adding the tuple
     * @param tableId the table to add the tuple to
     * @param t the tuple to add
     */
    public void insertTuple(TransactionId tid, int tableId, Tuple t)
        throws DbException, IOException, TransactionAbortedException {
        DbFile f = Database.getCatalog().getDatabaseFile(tableId);
        List<Page> dirtyPages = f.insertTuple(tid, t);
        for (Page p : dirtyPages) {
            p.markDirty(true, tid);

            //测试用例中有直接调用 page 方法插入的行为，因此进行缓存
            Integer ind = pageInds.get(p.getId());
            if (ind == null) {
                ind = getFreePageIndex();
                pageInds.put(p.getId(), ind);
            }
            pages[ind] = p;
        }
    }

    /**
     * Remove the specified tuple from the buffer pool.
     * Will acquire a write lock on the page the tuple is removed from and any
     * other pages that are updated. May block if the lock(s) cannot be acquired.
     *
     * Marks any pages that were dirtied by the operation as dirty by calling
     * their markDirty bit, and adds versions of any pages that have 
     * been dirtied to the cache (replacing any existing versions of those pages) so 
     * that future requests see up-to-date pages. 
     *
     * @param tid the transaction deleting the tuple.
     * @param t the tuple to delete
     */
    public  void deleteTuple(TransactionId tid, Tuple t)
        throws DbException, IOException, TransactionAbortedException {
        int tableId = t.getRecordId().getPageId().getTableId();
        DbFile f = Database.getCatalog().getDatabaseFile(tableId);
        List<Page> dirtyPages = f.deleteTuple(tid, t);
        for (Page p : dirtyPages) {
            p.markDirty(true, tid);
        }
    }

    /**
     * Flush all dirty pages to disk.
     * NB: Be careful using this routine -- it writes dirty data to disk so will
     *     break simpledb if running in NO STEAL mode.
     */
    public synchronized void flushAllPages() throws IOException {
        Iterator<PageId> it = pageInds.keySet().iterator();
        while (it.hasNext()) {
            flushPage(it.next());
        }

    }

    /** Remove the specific page id from the buffer pool.
        Needed by the recovery manager to ensure that the
        buffer pool doesn't keep a rolled back page in its
        cache.
        
        Also used by B+ tree files to ensure that deleted pages
        are removed from the cache so they can be reused safely
    */
    public synchronized void discardPage(PageId pid) {
        if (pageInds.containsKey(pid)) {
            Integer ind = pageInds.remove(pid);
            freePageInds.add(ind);
        }
    }

    /**
     * Flushes a certain page to disk
     * @param pid an ID indicating the page to flush
     */
    private synchronized  void flushPage(PageId pid) throws IOException {
        Integer ind = pageInds.get(pid);
        Page p = pages[ind];

        // append an update record to the log, with
        // a before-image and after-image.
        if (ind != null & pages[ind].isDirty() != null) {
            TransactionId dirtier = p.isDirty();
            Database.getLogFile().logWrite(dirtier, p.getBeforeImage(), p);
            Database.getLogFile().force();

            Database.getCatalog().getDatabaseFile(pid.getTableId()).writePage(p);
        }
    }

    /** Write all pages of the specified transaction to disk.
     */
    public synchronized  void flushPages(TransactionId tid) throws IOException {
        Iterator<PageLock> it = lockManager.iterator(tid);
        while(it.hasNext()) {
            PageLock lock = it.next();
            PageId pid = lock.getPageId();
            Integer ind = pageInds.get(pid);

            if (ind != null && pages[ind].isDirty() != null) {
                pages[ind].setBeforeImage();
                flushPage(pid);
                pages[ind].markDirty(false, null);
            }
        }
    }

    /**
     * Discards a page from the buffer pool.
     * Flushes the page to disk to ensure dirty pages are updated on disk.
     */
    private synchronized  void evictPage() throws DbException {
        Page ep = null;
        for (Page p : pages) {
            if (p.isDirty() == null) {
                ep = p;
                break;
            }
        }

        if (ep == null) {
            throw new DbException("no more clean buffer");
        }

        Integer ind = pageInds.remove(ep.getId());
        freePageInds.add(ind);
    }

}
