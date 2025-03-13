using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calculator
{
    public class CalculatorCore
    {
        private double currentValue = 0;
        private double pendingValue = 0;
        private string currentInput = "0";
        private string pendingOperation = "";
        private bool isNewInput = true;
        private bool hasCalculated = false;
        private List<string> operationHistory = new List<string>();
        private List<double> valueHistory = new List<double>();

        public bool UseOperatorPrecedence { get; set; } = false;

        public double GetCurrentValue()
        {
            return currentValue;
        }

        public string GetDisplayValue()
        {
            return currentInput;
        }

        public void SetValue(double value)
        {
            currentValue = value;
            currentInput = value.ToString(CultureInfo.CurrentCulture);
            isNewInput = true;
        }

        public void AppendDigit(string digit)
        {
            if (hasCalculated)
            {
                // If we just calculated a result and now entering a new number, clear everything
                Clear();
                hasCalculated = false;
            }

            if (isNewInput)
            {
                currentInput = digit;
                isNewInput = false;
            }
            else
            {
                // Don't allow multiple leading zeros
                if (currentInput == "0" && digit == "0")
                    return;

                // Replace single zero with the new digit
                if (currentInput == "0" && digit != ".")
                    currentInput = digit;
                else
                    currentInput += digit;
            }

            // Parse the input as a double
            //if (double.TryParse(currentInput, out double value))
            //{
            //    currentValue = value;
            //}

            // pentru b 16 avem nevoie de special handling
            try
            {
                currentValue = double.Parse(currentInput);
            }
            catch
            {
                // Handle hex digits (A-F) for base 16
                if (currentInput.Length > 0)
                {
                    try
                    {
                        // Try to convert from hex to decimal if we're in base 16
                        // This assumes you have a way to know the current base
                        BaseConverter converter = new BaseConverter();
                        currentValue = converter.FromBase(currentInput, 16);
                    }
                    catch
                    {
                        // If conversion fails, keep the current value
                        // This prevents errors from invalid inputs
                    }
                }
            }
        }

        public void SetInputBase(int inputBase, string displayValue)
        {
            // This method will be called when changing bases in programmer mode
            if (string.IsNullOrEmpty(displayValue) || displayValue == "0")
                return;

            try
            {
                // Convert the current display value to a number in the previous base
                long value;

                if (inputBase == 10)
                {
                    // If we're in decimal mode, parse as double to handle decimals
                    if (double.TryParse(displayValue, out double decimalValue))
                        value = (long)decimalValue;
                    else
                        value = 0;
                }
                else
                {
                    // For other bases, use the BaseConverter
                    BaseConverter converter = new BaseConverter();
                    string cleanValue = CleanNumberForBase(displayValue, inputBase);
                    value = converter.FromBase(cleanValue, inputBase);
                }

                // Set the calculated value
                currentValue = value;
                currentInput = value.ToString();
                isNewInput = true;
            }
            catch (Exception)
            {
                // If conversion fails, reset to 0
                Clear();
            }
        }

        private string CleanNumberForBase(string input, int inputBase)
        {
            // Clean up the input based on the base
            switch (inputBase)
            {
                case 2: // Binary
                    return input.Replace(" ", "");
                case 8: // Octal
                    return input.StartsWith("0") ? input.Substring(1) : input;
                case 16: // Hex
                    return input.StartsWith("0x") ? input.Substring(2) : input;
                default:
                    return input;
            }
        }

        public void AppendDecimal()
        {
            if (hasCalculated)
            {
                // If we just calculated a result and now adding decimal, treat as "0."
                Clear();
                currentInput = "0.";
                isNewInput = false;
                hasCalculated = false;
                return;
            }

            if (isNewInput)
            {
                currentInput = "0.";
                isNewInput = false;
            }
            else if (!currentInput.Contains("."))
            {
                currentInput += ".";
            }
        }

        public void SetOperation(string operation)
        {
            // Handle cascading operations
            if (!string.IsNullOrEmpty(pendingOperation) && !isNewInput)
            {
                // Perform the pending operation before setting the new one
                Calculate();
            }

            pendingValue = currentValue;
            pendingOperation = operation;
            isNewInput = true; // This flag is important - it tells the AppendDigit method to start a new number
            hasCalculated = false;

            if (UseOperatorPrecedence)
            {
                // Store operation and value for precedence calculations
                operationHistory.Add(operation);
                valueHistory.Add(currentValue);
            }
        }

        public void Calculate()
        {
            if (UseOperatorPrecedence && operationHistory.Count > 0)
            {
                CalculateWithPrecedence();
                return;
            }

            if (string.IsNullOrEmpty(pendingOperation) || isNewInput)
                return;

            double rightOperand = currentValue;

            switch (pendingOperation)
            {
                case "+":
                    currentValue = pendingValue + rightOperand;
                    break;
                case "-":
                    currentValue = pendingValue - rightOperand;
                    break;
                case "*":
                    currentValue = pendingValue * rightOperand;
                    break;
                case "/":
                    if (rightOperand == 0)
                    {
                        currentInput = "Cannot divide by zero";
                        pendingOperation = "";
                        isNewInput = true;
                        hasCalculated = true;
                        return;
                    }
                    currentValue = pendingValue / rightOperand;
                    break;
                case "%":
                    currentValue = pendingValue % rightOperand;
                    break;
            }

            currentInput = currentValue.ToString(CultureInfo.CurrentCulture);
            pendingOperation = "";
            isNewInput = true;
            hasCalculated = true;
        }

        private void CalculateWithPrecedence()
        {
            // Ensure we have the final value
            valueHistory.Add(currentValue);

            // Create a copy of the operations and values
            var operations = new List<string>(operationHistory);
            var values = new List<double>(valueHistory);

            // First pass: handle multiplication and division
            for (int i = 0; i < operations.Count; i++)
            {
                if (operations[i] == "*" || operations[i] == "/" || operations[i] == "%")
                {
                    double result;
                    if (operations[i] == "*")
                    {
                        result = values[i] * values[i + 1];
                    }
                    else if (operations[i] == "/")
                    {
                        if (values[i + 1] == 0)
                        {
                            currentInput = "Cannot divide by zero";
                            Clear();
                            return;
                        }
                        result = values[i] / values[i + 1];
                    }
                    else // %
                    {
                        result = values[i] % values[i + 1];
                    }

                    // Replace the values with the result
                    values[i] = result;

                    // Remove the used value and operation
                    values.RemoveAt(i + 1);
                    operations.RemoveAt(i);

                    // Adjust index
                    i--;
                }
            }

            // Second pass: handle addition and subtraction
            double finalResult = values[0];
            for (int i = 0; i < operations.Count; i++)
            {
                if (operations[i] == "+")
                {
                    finalResult += values[i + 1];
                }
                else if (operations[i] == "-")
                {
                    finalResult -= values[i + 1];
                }
            }

            // Set the result
            currentValue = finalResult;
            currentInput = finalResult.ToString(CultureInfo.CurrentCulture);

            // Clear the operation history
            operationHistory.Clear();
            valueHistory.Clear();

            isNewInput = true;
            hasCalculated = true;
        }

        public void Percentage()
        {
            if (string.IsNullOrEmpty(pendingOperation))
            {
                // If no operation is pending, calculate percentage of the current value
                currentValue = currentValue / 100;
            }
            else
            {
                // Calculate percentage based on the pending value
                switch (pendingOperation)
                {
                    case "+":
                    case "-":
                        currentValue = pendingValue * (currentValue / 100);
                        break;
                    case "*":
                    case "/":
                        currentValue = currentValue / 100;
                        break;
                }
            }

            currentInput = currentValue.ToString(CultureInfo.CurrentCulture);
            isNewInput = true;
        }

        public void Reciprocal()
        {
            if (currentValue == 0)
            {
                currentInput = "Cannot divide by zero";
                isNewInput = true;
                return;
            }

            currentValue = 1 / currentValue;
            currentInput = currentValue.ToString(CultureInfo.CurrentCulture);
            isNewInput = true;
        }

        public void Square()
        {
            currentValue = currentValue * currentValue;
            currentInput = currentValue.ToString(CultureInfo.CurrentCulture);
            isNewInput = true;
        }

        public void SquareRoot()
        {
            if (currentValue < 0)
            {
                currentInput = "Invalid input";
                isNewInput = true;
                return;
            }

            currentValue = Math.Sqrt(currentValue);
            currentInput = currentValue.ToString(CultureInfo.CurrentCulture);
            isNewInput = true;
        }

        public void Negate()
        {
            currentValue = -currentValue;
            if (currentInput.StartsWith("-"))
                currentInput = currentInput.Substring(1);
            else if (currentInput != "0")
                currentInput = "-" + currentInput;
        }

        public void Clear()
        {
            currentValue = 0;
            pendingValue = 0;
            currentInput = "0";
            pendingOperation = "";
            isNewInput = true;
            hasCalculated = false;

            // Clear operation history
            operationHistory.Clear();
            valueHistory.Clear();
        }

        public void ClearEntry()
        {
            currentValue = 0;
            currentInput = "0";
            isNewInput = true;

            // Don't clear the pending operation
        }

        public void Backspace()
        {
            if (isNewInput || currentInput.Length <= 1)
            {
                currentInput = "0";
                currentValue = 0;
                isNewInput = true;
                return;
            }

            currentInput = currentInput.Substring(0, currentInput.Length - 1);

            if (currentInput == "-" || currentInput == "")
            {
                currentInput = "0";
                currentValue = 0;
            }
            else
            {
                if (double.TryParse(currentInput, out double value))
                {
                    currentValue = value;
                }
            }
        }
    }
}
