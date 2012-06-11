using System;
using System.Collections.Generic;
using System.Threading;

namespace FxBackupLib
{
	public class LimitedQueue<T>
	{		
      // The actual queue
      private Queue<T> _internalQueue = new Queue<T>();

      // Event used to trigger to readers that an item has been queued
      [NonSerialized]
      private AutoResetEvent _evt = new AutoResetEvent(false);

      // We lock on the objects above in the following ways:
      // lock( _internalQueue )
      //      Used by readers (dequeue-ers) only.  This ensures that only
      //      one reader is ever waiting on the event.
      //
      // lock ( _evt )
      //      Used by readers (dequeue-ers) and writers (enqueue-ers).
      //      This ensures that the state of the event is always
      //      consistent with the queue:
      //          Signaled ==> queue has an item
      //          Cleared  ==> queue is empty

      public bool Contains(T item)
      {
         // Locking is pointless here
         return _internalQueue.Contains(item);
      }

      public int Count
      {
         // Locking is pointless here
         get { return _internalQueue.Count; }
      }

      public void Enqueue(T item)
      {
         lock ( _evt )
         {
            _internalQueue.Enqueue(item);
            _evt.Set();
         }
      }

      public T Dequeue()
      {
         T item;
         if ( TryDequeue(out item) )
         {
            return item;
         }
         else
         {
            throw new InvalidOperationException("Queue is empty");
         }
      }

      public bool TryDequeue(out T item)
      {
         return WaitAndDequeue(0, out item);
      }

      // Note:  all dequeue functions go thru here
      public bool WaitAndDequeue(int timeout, out T item)
      {
         item = default(T);

         // Let only one reader in to wait and watch the event
         lock ( _internalQueue )
         {
            // If the queue is populated, no need to wait
            lock ( _evt )
            {
               if ( _internalQueue.Count > 0 )
               {
                  _evt.Reset();
                  item = _internalQueue.Dequeue();
                  return true;
               }
            }

            // Queue is empty.  Wait on the event; if it signals,
            // then something has been queued
            if ( _evt.WaitOne(timeout, false) )
            {
               item = _internalQueue.Dequeue();
               return true;
            }
            else
            {
               // Nothing queued while we waited; we timed out
               return false;
            }
         }
      }

      public void Clear ()
		{
			// Make sure these are locked in the same order as WaitAndDequeue,
			// otherwise it will introduce a deadlock
			lock (_internalQueue) {
				lock (_evt) {
					_internalQueue.Clear ();
					_evt.Reset ();
				}
			}
		}
	}


}