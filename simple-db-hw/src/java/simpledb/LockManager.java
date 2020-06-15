package simpledb;

import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class LockManager {

    private Map<PageId, PageLock> pageLocks;
    private Map<TransactionId, List<PageLock>> transLocks;

    public LockManager() {
        pageLocks = new ConcurrentHashMap<>();
        transLocks = new ConcurrentHashMap<>();
    }


    public void lockPage(TransactionId tid, PageId pid, Permissions perm) throws TransactionAbortedException {
        //加页锁
        PageLock lock = pageLocks.get(pid);
        if (lock == null) {
            synchronized (this) {
                lock = pageLocks.get(pid);
                if (lock == null) {
                    lock = new PageLock(pid);
                    pageLocks.put(pid, lock);
                }
            }
        }

        lock.lock(tid, perm);

        //维护 tid 拥有的锁list
        List<PageLock> tls = transLocks.get(tid);
        if (tls == null) {
            synchronized (this) {
                if (tls == null) {
                    tls = new LinkedList<>();
                    transLocks.put(tid, tls);
                }
            }
        }
        synchronized (tls) {
            if (!tls.contains(lock)) {
                tls.add(lock);
            }
        }
    }

    public void unLock(TransactionId tid) {
        //查询tid 拥有的锁
        List<PageLock> tls = transLocks.get(tid);
        if (tls == null) {
            return;
        }

        Iterator<PageLock> it = tls.iterator();
        //释放所有锁
        while (it.hasNext()) {
            PageLock lock = it.next();
            lock.unlock(tid);
        }
        //清除事务
        transLocks.remove(tid);
    }


    public void unLockPage(TransactionId tid, PageId pid) {
        //释放页锁
        PageLock lock = pageLocks.get(pid);
        lock.unlock(tid);

        //更新 tid 拥有的锁 list
        List<PageLock> tls = transLocks.get(tid);
        tls.remove(lock);
    }

    public boolean hasLock(TransactionId tid, PageId pid) {
        List<PageLock> tls = transLocks.get(tid);
        if (tls == null) {
            return false;
        }
        PageLock lock = pageLocks.get(pid);
        return tls.contains(lock);
    }

    public List<PageLock> getLocks(TransactionId tid) {
        List<PageLock> tls = transLocks.get(tid);
        if (tls == null) {
            tls = new LinkedList<>();
        }
        return tls;
    }

    public Iterator<PageLock> iterator(TransactionId tid) {
        List<PageLock> tls = transLocks.get(tid);
        if (tls == null) {
            tls = new LinkedList<>();
        }
        return tls.iterator();
    }
}