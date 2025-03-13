using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class MemoryManager
    {
        private List<double> memoryValues = new List<double>();
        private double currentMemoryValue = 0;

        public bool HasMemory => memoryValues.Count > 0;

        public void StoreValue(double value)
        {
            memoryValues.Add(value);
            currentMemoryValue = value;
        }

        public double RecallValue()
        {
            return currentMemoryValue;
        }

        public void ClearMemory()
        {
            memoryValues.Clear();
            currentMemoryValue = 0;
        }

        public void AddToMemory(double value)
        {
            if (memoryValues.Count == 0)
            {
                // If memory is empty, store the value
                StoreValue(value);
            }
            else
            {
                // Add to the last value in memory
                currentMemoryValue += value;

                // Update the last value in the list
                if (memoryValues.Count > 0)
                {
                    memoryValues[memoryValues.Count - 1] = currentMemoryValue;
                }
            }
        }

        public void SubtractFromMemory(double value)
        {
            if (memoryValues.Count == 0)
            {
                // If memory is empty, store the negative value
                StoreValue(-value);
            }
            else
            {
                // Subtract from the last value in memory
                currentMemoryValue -= value;

                // Update the last value in the list
                if (memoryValues.Count > 0)
                {
                    memoryValues[memoryValues.Count - 1] = currentMemoryValue;
                }
            }
        }

        public List<double> GetMemoryValues()
        {
            // Return a copy of the list
            return new List<double>(memoryValues);
        }
    }
}
