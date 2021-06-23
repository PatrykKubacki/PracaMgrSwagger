using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Resonator
{
    public class BinaryCircularBuffer
    {
        private int capacity;   // buffer size;
        private int head;       // index of byte which is first to be read
        private int tail;       // index of byte which is first to be written
        private byte[] buffer;
        private bool allowOverflow;

        public BinaryCircularBuffer(int capacity)
            : this(capacity, false)
        {
        }

        public BinaryCircularBuffer(int capacity, bool allowOverflow)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity must be greater than or equal to zero.",
                    "capacity");

            this.capacity = capacity;
            head = 0;
            tail = 0;
            buffer = new byte[capacity];
            AllowOverflow = allowOverflow;
        }

        public bool AllowOverflow
        {
            get { return allowOverflow; }
            set { allowOverflow = value; }
        }

        public int Capacity
        {
            get { return capacity; }
        }

        // Returns number of bytes currently available in the buffer
        public int Count
        {
            get {
                if (tail >= head)
                    return tail - head;
                else
                    return capacity - head + tail;
                    }
        }

        // Makes the buffer empty
        public void Clear()
        {
            head = 0;
            tail = 0;
        }

        public int Put(byte[] src)
        {
            return Put(src, 0, src.Length);
        }

        public int Put(byte[] src, int offset, int count)
        {
            int realCount = AllowOverflow ? count : Math.Min(count, capacity - Count);
            int srcIndex = offset;
            for (int i = 0; i < realCount; i++, srcIndex++)
            {
                buffer[tail++] = src[srcIndex];
                if (tail == capacity)
                    tail = 0;
            }
            return realCount;
        }

        public void Put(byte item)
        {
            if (!AllowOverflow && Count == capacity)
                throw new InternalBufferOverflowException("Buffer is full.");

            buffer[tail++] = item;
            if (tail == capacity)
                tail = 0;
            
        }

        public void PutFromRS232(RS232 rs)
        {
            int length;
            int bytesToCopy;
            int btc;  // bytesToCopy in current iteration

            if (rs.IsOpen)
            {
                length = rs.BytesToRead;

                if (AllowOverflow)
                    bytesToCopy = length;
                else
                    bytesToCopy = Math.Min(length, capacity - Count - 1);

                while (bytesToCopy > 0)
                {
                    btc = Math.Min(bytesToCopy, capacity - tail);
                    if (rs.IsOpen) 
                        rs.Read(buffer, tail, btc);
                    tail += btc;
                    if (tail >= capacity)
                        tail = 0;
                    bytesToCopy -= btc;
                }
            }
        }

        public byte Get()
        {
            if (Count == 0)
                throw new InvalidOperationException("Buffer is empty.");

            byte item = buffer[head];
            head++;
            if (head == capacity)
                head = 0;
            
            return item;
        }

        public byte this [int i]
        {
            get { return buffer[(head + buffer.Length + i) % capacity]; }
        }

        public void RemoveNBytes(int n)
        {
              head = (head + Math.Min(Count, n)) % capacity;
        }

        public void Remove1Byte()
        {
            head = (head + Math.Min(Count, 1)) % capacity;
        }

        public bool IsFull
        {
            get { return Count == capacity - 1;}
        }

        public void CopyTo(int index, byte[] array, int arrayIndex, int count)
        {
            if (count > Count)
                throw new ArgumentOutOfRangeException("count",
                    "count cannot be greater than the buffer size.");

            int bufferIndex = (head+index)%capacity;
            for (int i = index; i < count+index; i++, arrayIndex++)
            {
                array[arrayIndex] = buffer[bufferIndex];
                bufferIndex++;
                if (bufferIndex == capacity)
                    bufferIndex = 0;
            }
        }

        // This code was usefull only for debuging of protocol parser implementation
        public void previewLast10Bytes()
        {
            byte[] buf = new byte[100];
            int i;

            for (i = 0; i < 100; i++)
                if (i - 30 < this.Count)
                    buf[i] = this[i - 30];
                else
                    buf[i] = 0;

            i++;
        }
    }
}
