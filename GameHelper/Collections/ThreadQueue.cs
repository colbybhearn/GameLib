using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameHelper.Collections
{
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

        public void Enqueue(T o)
        {
            lock(this)
            {
                base.Enqueue(o);
            }
        }

        public T Dequeue()
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
