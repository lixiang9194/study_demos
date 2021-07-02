package simpledb;

import java.util.LinkedList;
import java.util.List;
import java.util.Random;

public class PageLock {

    private long LOCK_TIMEOUT_MIN = 200;    //millseconds
    private long LOCK_TIMEOUT_MAX = 500;
    private PageId pid;
    private List<TransactionId> readTids = new LinkedList<>();
    private TransactionId writeTid;
    private Random rand = new Random(System.currentTimeMillis());

    public PageLock(PageId pid) {
        this.pid = pid;
    }

    public PageId getPageId() {
        return pid;
    }

    public boolean isWrite(TransactionId tid) {
        return tid.equals(writeTid);
    }

    public synchronized void lock(TransactionId tid, Permissions perm) throws TransactionAbortedException {
        try {
            if (perm.equals(Permissions.READ_ONLY)) {
                lockRead(tid);
            } else {
                lockWrite(tid);
            }
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public synchronized void unlock(TransactionId tid) {
        if (tid.equals(writeTid)) {
            writeTid = null;
        } else {
            readTids.remove(tid);
        }
        notifyAll();
    }

    private synchronized void lockRead(TransactionId tid) throws InterruptedException, TransactionAbortedException {
        //可重入
        if (readTids.contains(tid)) {
            return;
        }

        //锁降级
        if (tid.equals(writeTid) && readTids.isEmpty()) {
            return;
        }

        //锁超时
        long lockTimeOut = LOCK_TIMEOUT_MIN + (LOCK_TIMEOUT_MAX - LOCK_TIMEOUT_MIN) / 100 * rand.nextInt(100);
        long expires = System.currentTimeMillis() + lockTimeOut;

        //有事务正在写或排队写，排队等待，防止写饥饿
        while (writeTid != null) {
            wait(lockTimeOut);
            if (System.currentTimeMillis() >= expires) {
                throw new TransactionAbortedException();
            }
        }

        //加读锁
        readTids.add(tid);
    }

    private synchronized void lockWrite(TransactionId tid) throws InterruptedException, TransactionAbortedException {
        //可重入
        if (tid.equals(writeTid)) {
            return;
        }

        long lockTimeOut = LOCK_TIMEOUT_MIN + (LOCK_TIMEOUT_MAX - LOCK_TIMEOUT_MIN) / 100 * rand.nextInt(100);
        long expires = System.currentTimeMillis() + lockTimeOut;

        //等待写事务结束
        while (writeTid != null) {
            wait(lockTimeOut);
            if (System.currentTimeMillis() >= expires) {
                throw new TransactionAbortedException();
            }
        }

        //预占写锁，阻止后续事务加读锁，防止写饥饿
        writeTid = tid;

        lockTimeOut = LOCK_TIMEOUT_MIN + (LOCK_TIMEOUT_MAX - LOCK_TIMEOUT_MIN) / 100 * rand.nextInt(100);
        expires = System.currentTimeMillis() + lockTimeOut;

        //等待读事务完成
        while (!readTids.isEmpty()) {
            //锁升级
            if (readTids.size() == 1 && readTids.contains(tid)) {
                readTids.remove(tid);
                return;
            }
            wait(lockTimeOut);
            if (System.currentTimeMillis() >= expires) {
                writeTid = null;
                throw new TransactionAbortedException();
            }
        }
    }

}