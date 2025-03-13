using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private CalculatorCore calculator;
        private MemoryManager memoryManager;
        private UserSettings settings;
        private BaseConverter baseConverter;
        private bool isInProgrammerMode = false;
        private int currentBase = 10; // 2 for binary, 8 for octal, 10 for decimal, 16 for hex

        public string DisplayText { get; private set; } = "0";

        public MainWindow()
        {
            InitializeComponent();

            calculator = new CalculatorCore();
            memoryManager = new MemoryManager();
            settings = new UserSettings();
            baseConverter = new BaseConverter();

            DataContext = this;

            // Load user settings
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load and apply digit grouping setting
            bool useDigitGrouping = settings.GetUseDigitGrouping();
            DigitGroupingMenuItem.IsChecked = useDigitGrouping;

            // Load and apply calculator mode
            string calculatorMode = settings.GetCalculatorMode();
            if (calculatorMode == "Programmer")
            {
                ProgrammerMenuItem.IsChecked = true;
                StandardMenuItem.IsChecked = false;
                SwitchToProgrammerMode();
            }
            else
            {
                StandardMenuItem.IsChecked = true;
                ProgrammerMenuItem.IsChecked = false;
                SwitchToStandardMode();
            }

            // Load and apply number base in programmer mode
            if (isInProgrammerMode)
            {
                int savedBase = settings.GetNumberBase();
                SetNumberBase(savedBase);
            }

            // Load operator precedence setting
            bool useOperatorPrecedence = settings.GetUseOperatorPrecedence();
            OperatorPrecedenceMenuItem.IsChecked = useOperatorPrecedence;
            calculator.UseOperatorPrecedence = useOperatorPrecedence;

            // force a refresh of the display to apply initial settings
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void UpdateDisplay(string text)
        {
            DisplayText = text;

            // Update data binding
            DataContext = null;
            DataContext = this;

            // Force a layout update
            //DisplayTextBlock.Text = text;

            if (isInProgrammerMode)
            {
                UpdateBaseDisplays(text);
            }
        }

        private void UpdateBaseDisplays(string text)
        {
            try
            {
                // Parse the current text according to current base
                long value;

                // Important: Use the calculator's current value instead of trying to parse the display text
                value = (long)calculator.GetCurrentValue();

                // Update displays for different bases
                HexDisplayTextBlock.Text = baseConverter.ToBase(value, 16);
                DecDisplayTextBlock.Text = baseConverter.ToBase(value, 10);
                OctDisplayTextBlock.Text = baseConverter.ToBase(value, 8);
                BinDisplayTextBlock.Text = baseConverter.ToBase(value, 2);
            }
            catch (Exception)
            {
                // If there's an error in conversion, show default values
                HexDisplayTextBlock.Text = "0x0";
                DecDisplayTextBlock.Text = "0";
                OctDisplayTextBlock.Text = "00";
                BinDisplayTextBlock.Text = "0000";
            }
        }

        private void SwitchToProgrammerMode()
        {
            isInProgrammerMode = true;
            ProgrammerPanel.Visibility = Visibility.Visible;
            HexPanel.Visibility = Visibility.Visible;

            // Set the default or saved base
            int savedBase = settings.GetNumberBase();
            SetNumberBase(savedBase);

            // Save the setting
            settings.SetCalculatorMode("Programmer");
        }

        private void SwitchToStandardMode()
        {
            isInProgrammerMode = false;
            ProgrammerPanel.Visibility = Visibility.Collapsed;
            HexPanel.Visibility = Visibility.Collapsed;

            // Always use base 10 in standard mode
            currentBase = 10;

            // Save the setting
            settings.SetCalculatorMode("Standard");
        }

        private void SetNumberBase(int numberBase)
        {
            // current display value before switching
            string currentDisplay = DisplayText;

            // update current base
            int previousBase = currentBase;
            currentBase = numberBase;

            // Update radio buttons
            HexRadioButton.IsChecked = (numberBase == 16);
            DecRadioButton.IsChecked = (numberBase == 10);
            OctRadioButton.IsChecked = (numberBase == 8);
            BinRadioButton.IsChecked = (numberBase == 2);

            // Save the setting
            settings.SetNumberBase(numberBase);

            // Update display to show the current value in the selected base
            //UpdateDisplay(calculator.GetCurrentValue().ToString());
            
            //convert the current value to the new base
            if(!string.IsNullOrEmpty(currentDisplay) && currentDisplay != "0")
            {
                try
                {
                    // Convert from prev base to decimal
                    long decimalValue;

                    if(previousBase == 10)
                    {
                        // if coming from decimal, just parse the value
                        if(double.TryParse(currentDisplay, out double value))
                        {
                            decimalValue = (long)value;
                        }
                        else
                        {
                            decimalValue = 0;
                        }
                    }
                    else
                    {
                        // otherwise, use the base converter
                        string cleanValue = CleanNumberForBase(currentDisplay, previousBase);
                        decimalValue = baseConverter.FromBase(cleanValue, previousBase);
                    }
                    // Set the value in calculator
                    calculator.SetValue(decimalValue);
                }
                catch
                {
                    // If conversion fails, reset to 0
                    calculator.Clear();
                }
            }

            // update the display
            UpdateDisplay(calculator.GetDisplayValue());
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

        // Event handlers for number buttons
        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string number = button.Content.ToString();

            // Check if the digit is valid for the current base
            if (!IsValidForCurrentBase(number))
                return;

            // Add this line to see the current value before appending the digit
            string beforeValue = calculator.GetDisplayValue();

            calculator.AppendDigit(number);

            // Add this line to see the value after appending the digit
            string afterValue = calculator.GetDisplayValue();

            // If these values are the same, there's a problem in AppendDigit

            // Make sure we're calling UpdateDisplay
            UpdateDisplay(calculator.GetDisplayValue());

            // Let's also try direct assignment to make absolutely sure the UI updates
            DisplayText = calculator.GetDisplayValue();
            DataContext = null;
            DataContext = this;
        }

        private void HexButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentBase != 16)
                return;

            Button button = (Button)sender;
            string digit = button.Content.ToString();

            // For hex digits A-F, we need to handle them specially
            calculator.AppendDigit(digit);

            // Update base displays
            UpdateBaseDisplays(DisplayText);
        }

        private bool IsValidForCurrentBase(string digit)
        {
            if (int.TryParse(digit, out int value))
            {
                switch (currentBase)
                {
                    case 2: // Binary
                        return value == 0 || value == 1;
                    case 8: // Octal
                        return value >= 0 && value < 8;
                    case 10: // Decimal
                        return value >= 0 && value <= 9;
                    case 16: // Hexadecimal
                        return true; // All digits 0-9 are valid for hex
                }
            }
            return true; // Non-digit values (like ".")
        }

        // Basic operations
        private void Addition_Click(object sender, RoutedEventArgs e)
        {
            calculator.SetOperation("+");
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Subtraction_Click(object sender, RoutedEventArgs e)
        {
            calculator.SetOperation("-");
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Multiplication_Click(object sender, RoutedEventArgs e)
        {
            calculator.SetOperation("*");
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Division_Click(object sender, RoutedEventArgs e)
        {
            calculator.SetOperation("/");
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            calculator.Calculate();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        // Additional operations
        private void PercentButton_Click(object sender, RoutedEventArgs e)
        {
            calculator.Percentage();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Reciprocal_Click(object sender, RoutedEventArgs e)
        {
            calculator.Reciprocal();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Square_Click(object sender, RoutedEventArgs e)
        {
            calculator.Square();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void SquareRoot_Click(object sender, RoutedEventArgs e)
        {
            calculator.SquareRoot();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void PlusMinus_Click(object sender, RoutedEventArgs e)
        {
            calculator.Negate();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            if (currentBase == 10) // Only allow decimal point in base 10
            {
                calculator.AppendDecimal();
                UpdateDisplay(calculator.GetDisplayValue());
            }
        }

        // Clear operations
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            calculator.Clear();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void ClearEntry_Click(object sender, RoutedEventArgs e)
        {
            calculator.ClearEntry();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            calculator.Backspace();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        // Memory operations
        private void MemoryStore_Click(object sender, RoutedEventArgs e)
        {
            double value = calculator.GetCurrentValue();
            memoryManager.StoreValue(value);
        }

        private void MemoryRecall_Click(object sender, RoutedEventArgs e)
        {
            if (memoryManager.HasMemory)
            {
                double value = memoryManager.RecallValue();
                calculator.SetValue(value);
                UpdateDisplay(calculator.GetDisplayValue());
            }
        }

        private void MemoryClear_Click(object sender, RoutedEventArgs e)
        {
            memoryManager.ClearMemory();
        }

        private void MemoryAdd_Click(object sender, RoutedEventArgs e)
        {
            double value = calculator.GetCurrentValue();
            memoryManager.AddToMemory(value);
        }

        private void MemorySubtract_Click(object sender, RoutedEventArgs e)
        {
            double value = calculator.GetCurrentValue();
            memoryManager.SubtractFromMemory(value);
        }

        private void MemoryList_Click(object sender, RoutedEventArgs e)
        {
            // Fill the memory list box
            MemoryListBox.Items.Clear();
            foreach (double value in memoryManager.GetMemoryValues())
            {
                MemoryListBox.Items.Add(value.ToString(CultureInfo.CurrentCulture));
            }

            // Show the popup
            MemoryPopup.IsOpen = true;
        }

        private void MemoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MemoryListBox.SelectedItem != null)
            {
                string selectedValue = MemoryListBox.SelectedItem.ToString();
                if (double.TryParse(selectedValue, out double value))
                {
                    calculator.SetValue(value);
                    UpdateDisplay(calculator.GetDisplayValue());
                }

                MemoryPopup.IsOpen = false;
            }
        }

        private void ClearMemory_Click(object sender, RoutedEventArgs e)
        {
            memoryManager.ClearMemory();
            MemoryPopup.IsOpen = false;
        }

        // Additional methods for arithmetic operations to support keyboard shortcuts
        private void Percentage()
        {
            calculator.Percentage();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        // Base selection in programmer mode
        private void HexBaseButton_Click(object sender, RoutedEventArgs e)
        {
            SetNumberBase(16);
        }

        private void DecBaseButton_Click(object sender, RoutedEventArgs e)
        {
            SetNumberBase(10);
        }

        private void OctBaseButton_Click(object sender, RoutedEventArgs e)
        {
            SetNumberBase(8);
        }

        private void BinBaseButton_Click(object sender, RoutedEventArgs e)
        {
            SetNumberBase(2);
        }

        // Menu event handlers
        private void StandardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StandardMenuItem.IsChecked = true;
            ProgrammerMenuItem.IsChecked = false;
            SwitchToStandardMode();
        }

        private void ProgrammerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProgrammerMenuItem.IsChecked = true;
            StandardMenuItem.IsChecked = false;
            SwitchToProgrammerMode();
        }

        private void DigitGroupingMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            settings.SetUseDigitGrouping(true);

            // manually force a refresh of the display to apply grouping
            //string currentValue = DisplayText;

            //// force data binding to refresh completely
            //DataContext = null;
            //DisplayText = currentValue;
            //DataContext = this;

            //// also update the base displays
            //if (isInProgrammerMode)
            //{
            //    UpdateBaseDisplays(currentValue);
            //}

            // Refresh display to apply grouping
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void DigitGroupingMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            settings.SetUseDigitGrouping(false);

            // Manually force a refresh of the display to remove grouping
            string currentValue = DisplayText;

            //// Force data binding to refresh completely
            //DataContext = null;
            //DisplayText = currentValue;
            //DataContext = this;

            //// Also update any base displays as needed
            //if (isInProgrammerMode)
            //{
            //    UpdateBaseDisplays(currentValue);
            //}

            // Refresh display to remove grouping
            UpdateDisplay(calculator.GetDisplayValue());
        }

        // method to ensure you're getting the correct display value
        private void RefreshDisplay()
        {
            // This ensures we're using the calculator's current state
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void OperatorPrecedenceMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            calculator.UseOperatorPrecedence = true;
            settings.SetUseOperatorPrecedence(true);
        }

        private void OperatorPrecedenceMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            calculator.UseOperatorPrecedence = false;
            settings.SetUseOperatorPrecedence(false);
        }

        private void CutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Implementation using strings instead of control's built-in copy/paste
            Clipboard.SetText(DisplayText);
            calculator.Clear();
            UpdateDisplay(calculator.GetDisplayValue());
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Implementation using strings instead of control's built-in copy/paste
            Clipboard.SetText(DisplayText);
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Implementation using strings instead of control's built-in copy/paste
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                // Try to parse as a number and set as the current value
                if (double.TryParse(text, out double value))
                {
                    calculator.SetValue(value);
                    UpdateDisplay(calculator.GetDisplayValue());
                }
            }
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Calculator\nImplemented by Antonio Vicas\nGroup: 10LF333",
                            "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Keyboard input handling
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle number keys
            if (e.Key >= Key.D0 && e.Key <= Key.D9 && !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                string number = (e.Key - Key.D0).ToString();
                if (IsValidForCurrentBase(number))
                {
                    calculator.AppendDigit(number);
                    UpdateDisplay(calculator.GetDisplayValue());
                }
                e.Handled = true;
            }
            // Handle numpad
            else if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                string number = (e.Key - Key.NumPad0).ToString();
                if (IsValidForCurrentBase(number))
                {
                    calculator.AppendDigit(number);
                    UpdateDisplay(calculator.GetDisplayValue());
                }
                e.Handled = true;
            }
            // Handle operators
            else if (e.Key == Key.Add || (e.Key == Key.OemPlus && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift)))
            {
                calculator.SetOperation("+");
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                calculator.SetOperation("-");
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            else if (e.Key == Key.Multiply)
            {
                calculator.SetOperation("*");
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            else if (e.Key == Key.Divide || e.Key == Key.OemQuestion)
            {
                calculator.SetOperation("/");
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            // Handle equals keys
            else if (e.Key == Key.Enter || e.Key == Key.OemPlus)
            {
                calculator.Calculate();
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            // Handle decimal point
            else if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            {
                if (currentBase == 10) // Only allow decimal point in base 10
                {
                    calculator.AppendDecimal();
                    UpdateDisplay(calculator.GetDisplayValue());
                }
                e.Handled = true;
            }
            // Handle backspace
            else if (e.Key == Key.Back)
            {
                calculator.Backspace();
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            // Handle escape for clear
            else if (e.Key == Key.Escape)
            {
                calculator.Clear();
                UpdateDisplay(calculator.GetDisplayValue());
                e.Handled = true;
            }
            // Handle cut/copy/paste keyboard shortcuts
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    CopyMenuItem_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.X)
                {
                    CutMenuItem_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    PasteMenuItem_Click(sender, e);
                    e.Handled = true;
                }
            }
            // Handle hex keys for programmer mode
            else if (isInProgrammerMode && currentBase == 16)
            {
                if (e.Key >= Key.A && e.Key <= Key.F)
                {
                    string hexDigit = ((char)(e.Key)).ToString();
                    calculator.AppendDigit(hexDigit);
                    UpdateDisplay(calculator.GetDisplayValue());
                    e.Handled = true;
                }
            }
        }
    }
}