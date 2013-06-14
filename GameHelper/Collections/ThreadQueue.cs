using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameHelper.Collections
{
    /* Warning!
     * Do not store a ThreadQueue in a Queue
     * If you do this and call Enqueue or Dequeue, it will call Queue.Enqueue and Queue.Dequeue
     * And NOT call the Thread Safe ThreadQueue.Enqueue and Dequeue.
     */
    
    public class ThreadQueue<T> : Queue<T>
    {
        public ThreadQueue()
            : base()
        {
        }

        new public int Count
        {
            get
            {
                lock (this)
                {
                    return base.Count;
                }
            }
        }

        new public void Enqueue(T o)
        {
            lock(this)
            {
                base.Enqueue(o);
            }
        }

        new public T Dequeue()
        {
            lock (this)
            {
                return base.Dequeue();
            }
        }

        public T DequeueUnsafe()
        {
            return base.Dequeue();
        }

        public void EnqueueUnsafe(T o)
        {
            base.Enqueue(o);
        }

    }
}
