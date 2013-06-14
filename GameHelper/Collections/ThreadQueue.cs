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
            : base ()
        {
        }

        public int myCount
        {
            get
            {
                int c = 0;
                lock (this)
                {
                    c= this.Count;
                }
                return c;
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
            T o;
            lock (this)
            {
                o = base.Dequeue();
            }
            return o;
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
